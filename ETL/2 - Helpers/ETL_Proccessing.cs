using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using MySql.Data;
using MySql.Data.Common;
using MySql.Data.Types;
using MySql.Data.MySqlClient;
using DataAccessLayer_NET_Framework_;
using System;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ETL._2___Helpers
{
    public class ETL_Proccessing
    {
        #region Declares
        string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string defaultCreatedUpdatedBy = "1";

        int iInsertCount = 0;
        int iInsertErrorCount = 0;
        Logging logging = new Logging();

        public static DataView vMasterCodes = new DataView();

        public static DataView dvRefDutyStatus = new DataView();
        public static DataView dvRefOrganizationTypes = new DataView();
        public static DataView dvRefItemTypes = new DataView();
        public static DataView dvLocationTypes = new DataView();
        public static DataView dvRefOwnerTypes = new DataView();
        public static DataView dvRefNibrsOffenseCodes = new DataView();
        public static DataView dvRefUcrOffenseStatusCodes = new DataView();
        public static DataView dvRefUcrSummaryOffenseCodes = new DataView();

        #endregion

        #region ELT Proccessing

        // NOTES: Most need some finishing. Routines needing extensive work have comments below. Search for 'SCB TODO' for all TODO's.
        // Needs to be done, not even started:
        public bool ETL_10_Atrributes(OracleDAL oracleDAL, MySqlDAL mySqlDAL, out int InsertedRows, out int ErroredRows)
        {
            InsertedRows = 0;
            ErroredRows = 0;

            try
            {

                string insertStatement =
                    "INSERT INTO migration_attributes (source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date, created_by, updated_date, updated_by, " +
                    "source_attribute_id, parent_source_attribute_id, parent_attribute_type, attribute_type, display_abbreviation, display_value) VALUES (";

                string selectStatement = "";
                string insertValues = "";
                string fullInsertStatement = "";

                int attributeTypeID = 0;

                // SCB TODO: Verify where to get the attributes.  From PRIORS Reference values tab of DD spreadsheet?  Need Oracle access to see.
                for (int i = 0; i < vMasterCodes.Count; i++)
                {
                    attributeTypeID = 0;

                    selectStatement = "SELECT at1.id, at1.name, at1.parent_attribute_type_id, at2.name " +
                        "FROM migration.ref_migration_attribute_type at1 " +
                        "LEFT JOIN migration.ref_migration_attribute_type at2 ON at2.id = at1.parent_attribute_type_id " +
                        "WHERE at1.name = '" + vMasterCodes[i][0].ToString() + "'";

                MySqlDataReader dr = mySqlDAL.ExecuteDataReader(selectStatement);
                    if (dr.HasRows)
                    {
                        insertValues = "";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        // Excel columns: Table Id, Value, Description, Agency
                        insertValues += "'" + vMasterCodes[i][1].ToString() + "', "; // source_attribute_id  - 32  Unique to type



                        while (dr.Read())
                        {
                            attributeTypeID = (int)dr[0];
                            if (dr[2].ToString() != "")
                            {
                                insertValues += "'" + dr[2].ToString() + "'"; // parent_source_attribute_id
                            }
                            else
                            {
                                insertValues += "null, ";  // parent_source_attribute_id
                            }
                            if (dr[3].ToString() != "")
                            {
                                insertValues += "'" + dr[3].ToString() + "'";  // parent_attribute_type
                            }
                            else
                            {
                                insertValues += "null, ";  // parent_attribute_type
                            }
                        }
                        dr.Close();



                        //insertValues += parentAttributeTypeID + ", ";  // parent_source_attribute_id
                        //insertValues += parentAttributeType + ", ";    // parent_attribute_type

                        insertValues += "'" + vMasterCodes[i][0].ToString() + "', "; // attribute_type       - 128 "The mark43 attribute type"
                        //
                        insertValues += "'" + vMasterCodes[i][1].ToString() + "', "; // display_abbreviation - 15
                        insertValues += "'" + vMasterCodes[i][2].ToString() + "')"; // display_value - 256

                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(fullInsertStatement))
                        {
                            InsertedRows += 1;
                        }
                        else
                        {
                            ErroredRows += 1;
                        }
                    }
                    dr.Dispose();
                } // for Master Code dataview loop
                logging.WriteReportDataEntry("Attributes", InsertedRows, ErroredRows);
                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_10_Atrributes. " + ex.Message);
                return false;
            }
        }
        // Confirm the source of Excel file:
        public bool ETL_20_OffenseCodes(OracleDAL oracleDAL, MySqlDAL mySqlDAL, string offenseCodeExcelPath)
        {
            // As of 5-9-2018 - I have an Excel spreadsheet with PRIORS Offense Codes. Is this the source? On form there is a text box for Excel file.
            // As of 5-10-2018 - Teri sent updated DB and this inserted into the DB.
            try
            {
                string insertStatement = "";
                // NOTE: Paths with spaces are acceptable.
                string excelConnString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + offenseCodeExcelPath + ";" +
                                         @"Extended Properties='Excel 8.0;HDR=Yes;'";

                using (OleDbConnection connection = new OleDbConnection(excelConnString))
                {
                    connection.Open(); 
                    OleDbCommand command = new OleDbCommand("select * from [Sheet1$]", connection);
                    using (OleDbDataReader dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string Code = dr[0].ToString();
                            string Name = dr[1].ToString();
                            string Agency = dr[7].ToString(); // SHARED, SMP (Salem), etc

                            if (Agency.ToString().ToLower() == "shared" || Agency.ToString().ToLower() == "smp")
                            {
                                insertStatement = "INSERT INTO migration_offense_codes (created_date, created_by, updated_date, updated_by, " +
                                    "source_offense_code_id, offense_name, active_date) " +
                                    "VALUES ('" + currentDateTime + "', '" + defaultCreatedUpdatedBy + "', '" + currentDateTime + "', '" + 
                                    defaultCreatedUpdatedBy + "', '" + Code + "', '" + Name + "', '" + currentDateTime + "')";

                                if (mySqlDAL.ExecuteNonQuery(insertStatement))
                                {
                                    iInsertCount += 1;
                                }
                                else
                                {
                                    iInsertErrorCount = +1;
                                }
                            }

                        }
                    }
                }
                logging.WriteReportDataEntry("Offense codes", iInsertCount, iInsertErrorCount);
                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //    "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Offense codes copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_20_OffenseCodes. " + ex.Message);
                return false;
            }
        }
        public bool ETL_40_Users(OracleDAL oracleDAL, MySqlDAL mySqlDAL, out int InsertedRows, out int ErroredRows)
        {
            try
            {
                // Oracle tables:
                // EMPLOYEE
                // MASTER_CODES

                // SCB TODO: 1) Use a CASE around RMS_LOCKED? 2) Lookup for duty_status_value in ref_migration_duty_status??  refDutyStatus should be populated
                string selectStatement = "SELECT emp.CREATE_DATE, emp.CDCREATE_OPERID, emp.UPDATED_DATE, emp.CDOPERID, " +
                    "emp.SEQNUM, emp.FIRST_NAME, emp.SURNAME, emp.MIDDLE_NAME, emp.DOB, emp.SEX, mc.CODE_DESCRIPTION, " +
                    "emp.RANK, emp.RES_PHONE, emp.EMAIL, emp.BADGE_ID, emp.FOREIGN_SEQNUM, emp.EMPLOYEE_STATUS, emp.AGENCY, " + 
                    //"emp.RMS_LOCKED" +
                    "CASE WHEN UPPER(emp.RMS_LOCKED) = 'Y' THEN 1 ELSE 0 END " +
                    "FROM EMPLOYEE emp";
                // JOIN ??
                //" FROM EMPLOYEE emp LEFT JOIN MASTER_CODES mc ON UPPER(emp.RANK) = UPPER(mc.CODE_VALUE) AND UPPER(mc.TABLE_ID) = 'RANK'";

                string insertStatement = "INSERT INTO migration_users (source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_user_id, first_name, last_name, middle_name, date_of_birth, sex_attr_code, rank_attr_value, rank_attr_code, " +
                    "phone_number, primary_email, badge_number, external_cad_id, duty_status_value, department_agency_name, " +
                    "is_disabled) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                int iDutyStatus = 0;
                string sDutyStatus = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + FormatDateTimeForMySQL(dr["emp.CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["emp.CDCREATE_OPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["emp.UPDATED_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["emp.CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["emp.SEQNUM"].ToString() + "', ";  // varchar
                        insertValues += "'" + dr["emp.FIRST_NAME"].ToString() + "', ";
                        insertValues += "'" + dr["emp.SURNAME"].ToString() + "', ";
                        insertValues += "'" + dr["emp.MIDDLE_NAME"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["emp.DOB"].ToString()) + "', ";
                        insertValues += "'" + dr["emp.SEX"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("RANK", "", dr["p.RANK"].ToString()) + "', ";
                        insertValues += "'" + dr["emp.RANK"].ToString() + "', ";

                        insertValues += "'" + dr["emp.RES_PHONE"].ToString() + "', ";
                        insertValues += "'" + dr["emp.EMAIL"].ToString() + "', ";
                        insertValues += "'" + dr["emp.BADGE_ID"].ToString() + "', ";
                        insertValues += "'" + dr["emp.FOREIGN_SEQNUM"].ToString() + "', ";


                        // SCB TODO: Ref Lookup EXAMPLE! Use emp.EMPLOYEE_STATUS to lookup in refDutyStatus and populate duty_status_value
                        iDutyStatus = (int)dr["emp.EMPLOYEE_STATUS"];
                        dvRefDutyStatus.RowFilter = "id = " + iDutyStatus;
                        sDutyStatus = dvRefDutyStatus[0][1].ToString();
                        insertValues += "'" + sDutyStatus + "', ";


                        insertValues += "'" + dr["emp.AGENCY"].ToString() + "', ";

                        insertValues += "'" + dr["emp.RMS_LOCKED"].ToString() + "')"; // tinyint - CASE Statement above to convert this. Correct?

                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount += 1;
                        }
                    }
                }
                InsertedRows = iInsertCount;
                ErroredRows = iInsertErrorCount;
                logging.WriteReportDataEntry("Users", iInsertCount, iInsertErrorCount);
                return true;
            }
            catch (Exception ex)
            {
                InsertedRows = iInsertCount;
                ErroredRows = iInsertErrorCount;
                logging.WriteEvent("Error in ETL_40_Users. " + ex.Message);
                return false;
            }
        }
        public bool ETL_50_Locations(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // INCIDENT

                string selectStatement = "SELECT COUNTY FROM INCIDENT";

                string insertStatement = "INSERT INTO migration_locations (created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_location_id, source_location_type, administrative_area_level_2) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";
            
                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        // SCB TODO: What values for these 2:
                        insertValues += "'" + 1 + "', ";  // source_location_id ??
                        insertValues += "'" + ' ' + "', ";  // source_location_type ??

                        insertValues += "'" + dr["COUNTY"].ToString() + "')"; // Last field, close with right parenthese..

                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(fullInsertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Locations", iInsertCount, iInsertErrorCount);
                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Locations copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_50_Locations. " + ex.Message);
                return false;
            }
        }
        // Needs additional work:
        public bool ETL_60_Names(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle tables:
                // PERSON
                // MASTER_CODES

                string selectStatement = "SELECT p.CREATE_DATE, p.CDCREATE_OPERID, p.UPDATE_DATE, p.CDOPERID, p.SEQNUM, " +
                    "p.FIRST_NAME, p.MIDDLE_NAME, p.SURNAME, p.SUFFIX, p.PREFIX, p.MONIKER, p.SSN, p.LICENSE, " +
                    "p.LIC_ENDORSEMENTS, p.DOB, mc.CODE_DESCRIPTION, p.BIRTH_PLACE, p.DECEASED, p.REMARKS, p.WEIGHT, p.TO_WEIGHT, " +
                    "p.HEIGHT, p.TO_HEIGHT, p.CITIZENSHIP, p.MARITAL, p.FBI_ID, p.STATE_ID, p.NCIC_PRINT, p.BUILD, p.EYE, p.SEX, " +
                    "p.ETHNICITY, p.RACE, p.COMPLEXION, p.RES_PHONE, p,BUS_PHONE, p.CELL_PHONE, p.EMAIL, p.EMANCIPATION_DATE, " +
                    "p.HAIR_STYLE, p.HAIR_LENGTH, p.HAIR, p.FACIAL_HAIR, p.ALRT_NOTES, p.SMT, p.RECORD_TYPE, p.SURNAME, p.BUSINESS_TYPE, " +
                    "p.TEETH" +
                    "FROM PERSON p";
                //"FROM PERSON p LEFT JOIN MASTER_CODES mc ON p.BIRTH_PLACE = mc.CODE_DESCRIPTION WHERE UPPER(mc.TABLE_ID) = 'STATE'";

                // SCB TODO: No insert fields for p.SMT - these go right after caution_attr_code -> end of 11th line below.
                string insertStatement = "INSERT INTO migration_names (created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_name_id, source_master_name_id, source_owner_id, source_owner_type, " +
                    "first_name, middle_name, last_name, suffix, title, nickname_1, ssn, drivers_license_number, " +
                    "drivers_license_endorsement_attr_code, date_of_birth, birth_state_attr_value, birth_state_attr_code, date_of_death, " +
                    "details, weight, weight_range_min, weight_range_max, height, height_range_min, height_range_max, " +
                    "citizenship_attr_value, citizenship_attr_code, marital_status_attr_value, marital_status_attr_code, " +
                    "fbi_ucn, state_id_number, fingerprint_id, build_attr_value, build_attr_code, eye_color_attr_value, eye_color_attr_code, " +
                    "sex_attr_value, sex_attr_code, ethnicity_attr_value, ethnicity_attr_code, race_attr_value, race_attr_code, " +
                    "skin_tone_attr_value, skin_tone_attr_code, phone_number_home, phone_number_work, phone_number_mobile, email_home, " +
                    "date_of_emancipation, hair_style_attr_value, hair_style_attr_code, hair_length_attr_value, hair_length_attr_code, " +
                    "hair_color_attr_value, hair_color_attr_code, facial_hair_type_attr_value, facial_hair_type_attr_code, caution_attr_code, " +
                    "type, organization_name, organization_type_global_attr, physical_characteristic_teeth_attr_code, physical_characteristic_teeth_attr_value" + 
                    ") VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";
                string semiColonList = "";
                List<string> listOfValues = null;
                string sValue = "";
                int iPos = 0;
                string sValueLeft = "";
                string sValueRight = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        // MySQL retrieves and displays DATETIME values in 'YYYY-MM-DD HH:MM:SS' format

                        insertValues += "'" + FormatDateTimeForMySQL(dr["p.CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["p.CDCREATE_OPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["p.UPDATED_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["p.CDOPERID"].ToString() + "', ";
                        insertValues += "'" + dr["p.SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["p.SEQNUM"].ToString() + "', ";

                        // SCB TODO: What values for these 2:
                        insertValues += "'" + 1 + "', ";  // source_owner_id - non-nullable
                        insertValues += "'" + ' ' + "', ";  // source_owner_type - non-nullable

                        insertValues += "'" + dr["p.FIRST_NAME"].ToString() + "', ";
                        insertValues += "'" + dr["p.MIDDLE_NAME"].ToString() + "', ";
                        insertValues += "'" + dr["p.SURNAME"].ToString() + "', ";
                        insertValues += "'" + dr["p.SUFFIX"].ToString() + "', ";
                        insertValues += "'" + dr["p.PREFIX"].ToString() + "', ";
                        insertValues += "'" + dr["p.MONIKER"].ToString() + "', ";
                        insertValues += "'" + dr["p.SSN"].ToString() + "', ";
                        insertValues += "'" + dr["p.LICENSE"].ToString() + "', ";
                        insertValues += "'" + dr["p.LIC_ENDORSEMENTS"].ToString() + "', ";
                        insertValues += "'" + dr["p.DOB"].ToString() + "', ";
                        insertValues += "'" + dr["mc.CODE_DESCRIPTION"].ToString() + "', ";

                        // These match up with nnnn_values & nnnn_code respectively, but sometimes (TEETH) code is first - BE CAREFUL & TEST
                        insertValues += "'" + GetMasterCodeValue("STATE", "", dr["p.BIRTH_PLACE"].ToString()) + "', ";
                        insertValues += "'" + dr["p.BIRTH_PLACE"].ToString() + "', ";

                        insertValues += "'" + dr["p.DECEASED"].ToString() + "', ";
                        insertValues += "'" + dr["p.REMARKS"].ToString() + "', ";
                        insertValues += "'" + dr["p.WEIGHT"].ToString() + "', ";
                        insertValues += "'" + dr["p.WEIGHT"].ToString() + "', ";  // weight min
                        insertValues += "'" + dr["p.TO_WEIGHT"].ToString() + "', ";
                        insertValues += "'" + dr["p.HEIGHT"].ToString() + "', ";
                        insertValues += "'" + dr["p.HEIGHT"].ToString() + "', ";  // height min
                        insertValues += "'" + dr["p.TO_HEIGHT"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("NCIC-Country", "", dr["p.CITIZENSHIP"].ToString()) + "', ";
                        insertValues += "'" + dr["p.CITIZENSHIP"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("MARITAL", "", dr["p.MARITAL"].ToString()) + "', ";
                        insertValues += "'" + dr["p.MARITAL"].ToString() + "', ";

                        insertValues += "'" + dr["p.FBI_ID"].ToString() + "', ";
                        insertValues += "'" + dr["p.STATE_ID"].ToString() + "', ";
                        insertValues += "'" + dr["p.NCIC_PRINT"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("BUILD", "", dr["p.BUILD"].ToString()) + "', ";
                        insertValues += "'" + dr["p.BUILD"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("EYECOL", "", dr["p.EYE"].ToString()) + "', ";
                        insertValues += "'" + dr["p.EYE"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("SEX", "", dr["p.SEX"].ToString()) + "', ";
                        insertValues += "'" + dr["p.SEX"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("ETHNIC", "", dr["p.ETHNICITY"].ToString()) + "', ";
                        insertValues += "'" + dr["p.ETHNICITY"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("RACE", "", dr["p.RACE"].ToString()) + "', ";
                        insertValues += "'" + dr["p.RACE"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("COMPLEX", "", dr["p.COMPLEXION"].ToString()) + "', ";
                        insertValues += "'" + dr["p.COMPLEXION"].ToString() + "', ";

                        insertValues += "'" + dr["p.RES_PHONE"].ToString() + "', ";
                        insertValues += "'" + dr["p.BUS_PHONE"].ToString() + "', ";
                        insertValues += "'" + dr["p.CELL_PHONE"].ToString() + "', ";
                        insertValues += "'" + dr["p.EMAIL"].ToString() + "', ";

                        insertValues += "'" + FormatDateTimeForMySQL(dr["p.EMANCIPATION_DATE"].ToString()) + "', ";

                        insertValues += "'" + GetMasterCodeValue("HAIR STYLE", "", dr["p.HAIR_STYLE"].ToString()) + "', ";
                        insertValues += "'" + dr["p.HAIR_STYLE"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("HAIRLENGTH", "", dr["p.HAIR_LENGTH"].ToString()) + "', ";
                        insertValues += "'" + dr["p.HAIR_LENGTH"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("HAIRCOL", "", dr["p.HAIR"].ToString()) + "', ";
                        insertValues += "'" + dr["p.HAIR"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("FACIAL HAIR", "", dr["p.FACIAL_HAIR"].ToString()) + "', ";
                        insertValues += "'" + dr["p.FACIAL_HAIR"].ToString() + "', ";

                        insertValues += "'" + dr["p.ALRT_NOTES"].ToString() + "', ";


                        // NOTES From Adam Kinch:
                        // First, the PERSON.SMT code must be parsed into separate values, then each value can be sub - queried in the Master Codes table.
                        // "SQL FILTER: table_id = 'SMT' AND code_value = [[parsed SMT code]]"
                        // Multipick field. Each SMT code is separated by a ";" character   
                        // ...and if a description is included, it is after the code, separated by a ":" character. 
                        // Example: ;TAT L CHK:Bald Eagle;PRCD NOSE:;SC BACK:Surgical Scar;MC BEHAVIO:Bi-Polar disorder

                        // INTO: physical_characteristic_appearance_attr_value & physical_characteristic_appearance_attr_code
                        // AND behavioral_characteristic_attr_value & behavioral_characteristic_attr_code

                        semiColonList = dr["p.SMT"].ToString();
                        listOfValues = semiColonList.Split(';').ToList<string>();
                        foreach (string s in listOfValues)
                        {
                            iPos = s.IndexOf(":");
                            sValueLeft = s.Substring(0, iPos);
                            sValueRight = s.Substring(iPos + 1, s.Length - iPos - 1);

                            sValue = GetMasterCodeValue("SMT", "", sValueLeft);

                            // SCB TODO: Now what? We don't want multiple names rows for all these.
                        }



                        insertValues += "'" + dr["p.RECORD_TYPE"].ToString() + "', ";
                        insertValues += "'" + dr["p.SURNAME"].ToString() + "', ";
                        insertValues += "'" + dr["p.BUSINESS_TYPE"].ToString() + "', ";

                        insertValues += "'" + dr["p.TEETH"].ToString() + "', ";
                        insertValues += "'" + GetMasterCodeValue("TEETH", "", dr["p.TEETH"].ToString()) + "', ";




                        // SCB TODO: Finish up:
                        // More lookups, but have questions before continuing. Also see note on last field, RESIDENT. May come from different table.


                        insertValues += dr[""].ToString() + "')"; // Last field, close with right parenthese..

                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Names", iInsertCount, iInsertErrorCount);

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Names. " + ex.Message);
                return false;
            }
        }
        public bool ETL_60_Name_Report_Links(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // INVLOVEMENTS

                // SCB TODO: These 3 were in DD (EVENTID, CITATION_ID, INTID) assigned to source_report_event_number . Using EVENTID for now
                string selectStatement = "SELECT UPDATE_DATE, CDOPERID, " +
                    "EVENTID, PERSON_SEQNUM, BOOKING_ID, EVENTID, RECORD_TYPE, " + 
                    "INVOLVEMENT, LEOKA_ACT, CODE_DESCRIPTION, LEOKA_ASSIGN FROM table";

                string insertStatement = "INSERT INTO migration_name_report_links " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_report_event_number, source_name_id, source_arrest_id, source_offense_id, name_type, " +
                    "report_type, leoka_activity_attr_code, leoka_activity_attr_value, leoka_assignment_attr_code) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["PERSON_SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["BOOKING_ID"].ToString() + "', ";
                        insertValues += "'" + dr["EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["RECORD_TYPE"].ToString() + "', ";
                        insertValues += "'" + dr["INVOLVEMENT"].ToString() + "', ";
                        insertValues += "'" + dr["LEOKA_ACT"].ToString() + "', ";
                        insertValues += "'" + dr["CODE_DESCRIPTION"].ToString() + "', ";
                        insertValues += "'" + dr["LEOKA_ASSIGN"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("LEOKAASSIGN", "", dr["LEOKA_ASSIGN"].ToString()) + "')";


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Name report links", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Name report links copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_60_Name_Report_Links. " + ex.Message);
                return false;
            }
        }
        public bool ETL_70_Reports(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            // SCB TODO: Different report types come from the INCIDENT table - how do we differentiate?
            try
            {
                // Oracle table:
                // INCIDENT

                // SCB TODO: Source for responding_officer_id has 2 sources: REP_OFFICER, OFFICER_NAME. Using REP_OFFICER for now
                // ALSO assist_officer_id1 has 2 sources: ASSIST_OFFICER1, ASSIST_OFFICER1_NAME. Using ASSIST_OFFICER1 for now.
                string selectStatement = "SELECT CREATE_DATE, CDCREATE_OPERID, UPDATE_DATE, CDOPERID, " +
                    "EVENTID, LOCATION, NOTES, OCCURR_DATE, CLEARED, REP_OFFICER, ASSIST_OFFICER1, " +
                    "DEPT_CASE_DISPO, DEPT_CASE_DISPO_DATE FROM INCIDENT";

                string insertStatement = "INSERT INTO migration_reports " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_report_event_number, source_location_id, narrative, event_start, event_end, responding_officer_id, " +
                    "assist_officer_id1, case_status_attr_value, case_status_attr_code, case_status_date, reporting_party_name_id, " +
                    "is_domestic_violence) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + FormatDateTimeForMySQL(dr["CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["CDCREATE_OPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["LOCATION"].ToString() + "', ";
                        insertValues += "'" + dr["NOTES"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["OCCURR_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["CLEARED"].ToString() + "', ";
                        insertValues += "'" + dr["REP_OFFICER"].ToString() + "', ";
                        insertValues += "'" + dr["ASSIST_OFFICER1"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("DEPSTATUS", dr["DEPT_CASE_DISPO"].ToString(), "") + "', ";
                        insertValues += "'" + dr["DEPT_CASE_DISPO"].ToString() + "', ";

                        insertValues += "'" + FormatDateTimeForMySQL(dr["DEPT_CASE_DISPO_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["NAME"].ToString() + "', ";
                        insertValues += "'" + dr["DOMESTIC"].ToString() + "')"; // Last field, close with right parenthese..


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Reports", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Reports copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_70_Reports. " + ex.Message);
                return false;
            }
        }
        public bool ETL_70_1_Reports_Arrests(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // ARREST

                string selectStatement = "SELECT CREATE_DATE, CDCREATE_OPERID, SEQNUM, EVENTID, BOOKING_ID, PERSON_SEQNUM, ARREST_DATE, ARREST_OFFICER, " +
                    "ARREST_LOCATION, ARMED_WITH, AGENCY FROM ARREST ar LEFT JOIN AGENCY_INFO ai ON ai.AGENCY_MNEMONIC = ar.AGENCY";
                // SCB TODO: Check the JOIN above - needed??

                string insertStatement = "INSERT INTO migration_arrests " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_arrest_id, source_report_event_number, arrest_number, defendant_id, arrest_date, arresting_officer_id, " +
                    "source_arrest_location_id, arrestee_was_armed_with1_attr_value, arrestee_was_armed_with1_attr_code, " +
                    "arresting_agency_attr_value, arresting_agency_attr_code) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + FormatDateTimeForMySQL(dr["ar.CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["ar.CDCREATE_OPERID"].ToString() + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += dr["ar.SEQNUM"].ToString() + "', ";
                        insertValues += dr["ar.EVENTID"].ToString() + "', ";
                        insertValues += dr["ar.BOOKING_ID"].ToString() + "', ";
                        insertValues += dr["ar.PERSON_SEQNUM"].ToString() + "', ";
                        insertValues += FormatDateTimeForMySQL(dr["ar.ARREST_DATE"].ToString()) + "', ";
                        insertValues += dr["ar.ARREST_OFFICER"].ToString() + "', ";
                        insertValues += dr["ar.ARREST_LOCATION"].ToString() + "', ";


                        insertValues += "'" + GetMasterCodeValue("WEAPONINV", dr["ar.ARMED_WITH"].ToString(), "") + "', ";
                        insertValues += "'" + dr["ar.ARMED_WITH"].ToString() + "', ";

                        // SCB TODO: Check if data is correct, from DD:
                        //AGENCY_INFO.AGENCY_NAME - SQL_FILTERS: AGENCY_INFO.AGENCY_MNEMONIC = ARREST.AGENCY
                        insertValues += "'" + dr["ai.AGENCY_MNEMONIC"].ToString() + "', ";


                        insertValues += "'" + dr["ar.AGENCY"].ToString() + "')";


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Reports - Arrests", iInsertCount, iInsertErrorCount);
                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Arrests copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_70_1_Reports_Arrests. " + ex.Message);
                return false;
            }
        }
        public bool ETL_70_2_Reports_Charges(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // INVOLVEMENTS

                string selectStatement = "SELECT CREATE_DATE, CDOPERID, BOOKING_DISPLAY_ROW, BOOKING_ID, OFFENSE_ID, " +
                    "ARREST_TYPE, OFFENSE_DESCR, JUVENILE_INVOLVED, COUNTS " +
                    "FROM INVOLVEMENTS " + 
                    //" inv JOIN ARREST ar ON inv.booking_id = ar.booking_id AND inv.eventid = ar.eventid" + 
                    " WHERE UPPER(involve_type) IN ('ARRESTED','SUMMONED')";
                // SCB TODO: DD also mentions: involvements.booking_id = arrest.booking_id AND involvements.eventid = arrest.eventid. Use JOIN above??

                string insertStatement = "INSERT INTO migration_charges " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "offense_order, source_arrest_id, source_offense_id, arrest_type, charge_code_name, " +
                    "juvenile_disposition_attr_value, juvenile_disposition_attr_code, charge_count) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + FormatDateTimeForMySQL(dr["CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["CDOPERID"].ToString() + "', ";
                        //insertValues += FormatDateTimeForMySQL(dr["UPDATE_DATE"].ToString()) + "', ";
                        //insertValues += dr["CDOPERID"].ToString() + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += dr["BOOKING_DISPLAY_ROW"].ToString() + "', ";
                        insertValues += dr["BOOKING_ID"].ToString() + "', ";
                        insertValues += dr["OFFENSE_ID"].ToString() + "', ";
                        insertValues += dr["ARREST_TYPE"].ToString() + "', ";
                        insertValues += dr["OFFENSE_DESCR"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("UCRJUVDISPO", dr["JUVENILE_INVOLVED"].ToString(), "") + "', ";
                        insertValues += "'" + dr["JUVENILE_INVOLVED"].ToString() + "')";


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Reports - Charges", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Charges copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_70_2_Reports_Charges. " + ex.Message);
                return false;
            }
        }
        // Needs to be completed, many MASTER_CODES lookups:
        public bool ETL_70_3_Reports_Offenses(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            // Oracle table:
            // 1 - INVOLVEMENTS
            // 2 - INCIDENT (Location)

            string selectStatement = "SELECT inv.UPDATE_DATE, inv.CDOPERID, inv.EVENTID, inv.OFFENSE_ID, inc.LOCATION, inv.OFFENSE_DESCR, " +
                "inv.IBR, inv.OFFENSE_UCR, inv.COMMITTED, inv.GRDINDEX1 FROM INVOLVEMENTS inv " +
                "WHERE LCASE(inv.Involve_type) IN ('victim', 'offense')";  // USE WHERE ??

            //"JOIN INCIDENT inc ON ?? ";
            // FROM INVOLVEMENTS inv JOIN INCIDENT inc ON (EVENTID or ??" +
            // WHERE LCASE(inv.Involve_type) IN ('victim', 'offense')"; // ??

            string insertStatement = "INSERT INTO migration_offenses " +
                "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                "created_date_utc, created_by, updated_date_utc, updated_by, " +
                "source_report_event_number, source_offense_id, source_offense_location_id, offense_code_name, " + 
                "nibrs_code, ucr_summary_code, was_completed, offense_order ) VALUES (";

            string insertValues = "";
            string fullInsertStatement = "";

            try
            {
                string semiColonList = "";
                List<string> listOfValues = null;
                string sValue = "";
                //int iPos = 0;
                //string sValueLeft = "";
                //string sValueRight = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        // SCB TODO: 1) Test this and 2) Use or delete - not sure it looks neater.
                        //insertValues = AppendInsertStatement(insertValues, currentDateTime, true);
                        //insertValues = AppendInsertStatement(insertValues, defaultCreatedUpdatedBy, false);
                        //insertValues = AppendInsertStatement(insertValues, FormatDateTimeForMySQL(dr["UPDATE_DATE"].ToString()).ToString(), true);
                        //insertValues = AppendInsertStatement(insertValues, dr["CDOPERID"].ToString(), false);

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["OFFENSE_ID"].ToString() + "', ";
                        insertValues += "'" + dr["LOCATION"].ToString() + "', ";  // SCB TODO: In PRIORS this is Location Type. See DD for notes.
                        insertValues += "'" + dr["OFFENSE_DESCR"].ToString() + "', ";
                        insertValues += "'" + dr["IBR"].ToString() + "', ";
                        insertValues += "'" + dr["OFFENSE_UCR"].ToString() + "', ";
                        insertValues += "'" + dr["COMMITTED"].ToString() + "', ";
                        insertValues += "'" + dr["GRDINDEX1"].ToString() + "', ";

                        // Seperate semi-colon delimited values:
                        semiColonList = dr["inv.AGG_ASS_HOM_CIRC"].ToString();
                        listOfValues = semiColonList.Split(';').ToList<string>();
                        foreach (string s in listOfValues)
                        {
                            sValue = GetMasterCodeValue("AAHCIRC", s, "");  // Code Value, not Code Description
                            // Now what? We don't want multiple names rows for all these. Append back with semi-colons and insert into 1 field?
                        }

                        // From Names, they were further seperated by colons.
                        //foreach (string s in listOfValues)
                        //{
                        //    iPos = s.IndexOf(":");
                        //    sValueLeft = s.Substring(0, iPos);
                        //    sValueRight = s.Substring(iPos + 1, s.Length - iPos - 1);

                        //    sValue = GetMasterCodeValue("AAHCIRC", "", sValueLeft);
                        //}

                        // SCB TOOD: Etc, left off here.  Here come the many MASTER_CODES lookups...  See notes below.
                        // Don't proceed further until we know what to do with these delimited values.


                        //   A - For negligent_manslaughter_attr_value :
                        //      INVOLVEMENTS.AGG_ASS_HOM_CIRC has semi-colon seperated values. 
                        //          Individually, use these to look up MASTER_CODES where table_id = 'AAHCIRC' AND code_value = [[parsed agg aslt circ code]]
                        //   B - For justifiable_homicide_attr_value :
                        //      INVOLVEMENTS.JUST_HOM_CIRC has semi-colon seperated values. 
                        //          Individually, use these to look up MASTER_CODES where table_id = 'ADDJUSTHOM' AND code_value = [[parsed just aslt circ code]]
                        //   C - For weapon_or_force_involved1_attr_value :
                        //      INVOLVEMENTS.OFF_WEAPON_USED1 has semi-colon seperated values.
                        //          Individually, use these to look up MASTER_CODES where table_id = 'WEAPON/FORCE' AND code_value = [[parsed weapon used code]]
                        // Like A above:
                        //   D - For homicide_circumstance_attr_value :
                        //       INVOLVEMENTS.AGG_ASS_HOM_CIRC has semi-colon seperated values.
                        //          Individually, use these to look up MASTER_CODES where table_id = 'AAHCIRC' AND code_value = [[parsed agg aslt circ code]]
                        //   E - For criminal_activity_category_attr_value :
                        //       INVOLVEMENTS.CRIMINAL_ACTIVITY1 has semi-colon seperated values.
                        //          Individually, use these to look up MASTER_CODES where table_id = 'TYPECRIM' AND code_value = [[parsed criminal activity code]]
                        //   F - For bias_motivation1_attr_value :
                        //       Look up MASTER_CODES where table_id = 'BIAS MOTIVATION' AND code_value = INVOLVEMENTS.BIAS_MOTIVATION
                        // Like D above:
                        //   G - For aggravated_assault_circumstance1_attr_value :
                        //       INVOLVEMENTS.AGG_ASS_HOM_CIRC has semi-colon seperated values.
                        //          Individually, use these to look up MASTER_CODES where table_id = 'AAHCIRC' AND code_value = [[parsed agg aslt circ code]]
                        //   H - For point_of_entry_attr_value :
                        //       INCIDENT.POINT_OF_ENTRY has semi-colon seperated values.
                        //          Individually, use these to look up MASTER_CODES where table_id = 'POINTOFENTRY' AND code_value = [[parsed P.O.E. code]]
                        //   I - For method_of_entry_attr_value :
                        //       INCIDENT.METHOD_OF_ENTRY has semi-colon seperated values.
                        //          Individually, use these to look up MASTER_CODES where table_id = 'METHODOFENTRY' AND code_value = [[parsed M.O.E. code]]
                        //   J - For suspect_actions_attr_value :
                        //       INVOLVEMENTS.SUSPECT_ACTIONS has semi-colon seperated values.
                        //          Individually, use these to look up MASTER_CODES where table_id = 'SUSPECTACTIONS' AND code_value = [[parsed suspect actions code]]
                        //   K - For force_used_attr_value :
                        //       INCIDENT.USE_OF_FORCE_CODES has semi-colon seperated values.
                        //          Individually, use these to look up MASTER_CODES where table_id = 'USEOFFORCE' AND code_value = [[parsed use of force code]]


                        //insertValues += "'" + dr[""].ToString() + "', ";

                        //insertValues += "'" + GetMasterCodeValue("", dr[""].ToString(), "") + "', ";
                        //insertValues += "'" + dr[""].ToString() + "')";


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }

                logging.WriteReportDataEntry("Reports - Charges", iInsertCount, iInsertErrorCount);

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_70_3_Reports_Offenses. " + ex.Message);
                return false;
            }
        }
        // Needs a little work:
        public bool ETL_70_4_Reports_FieldContacts(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // FIELDINTERVIEWS, FI_PERSON, VEHICLE

                string selectStatement = "SELECT fi.CREATE_DATE, fi.CDCREATE_OPERID, fi.UPDATE_DATE, fi.CDOPERID, " +
                    "fi.INTID, fi.PERSON_SEQNUM, fi.LOCATION, fi.INTID, fi.ASSOCIATION_TYPE, fi.FIELDINT_OFFICER " + // , VEHICLE
                    "FROM FIELDINTERVIEWS fi LEFT JOIN FI_PERSON fip ON fi.INTID = fip.INTDID " +   
                    "JOIN VEHICLE v ON fi.INTID = v.INTID";  // SCB TODO: Check JOIN here.

                string insertStatement = "INSERT INTO migration_field_contacts " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, source_report_event_number, subject1_id, " +
                    "vehicle1_id, contact_location_id, source_field_contact_id, subject1_role_attr_code, source_submitted_by) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + FormatDateTimeForMySQL(dr["CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["CDOPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";


                        insertValues += dr["INTID"].ToString() + "', ";

                        insertValues += dr["FI_PERSON.PERSON_SEQNUM"].ToString() + "', ";
                        // SCB TODO: From DD: If there are multiple people, there will be multiple records in the FI_PERSON table 
                        // linked to the FIELDINTERVIEWS record.
                        // "SQL FILTERS: FIELDINTERVIEWS.INTID = FI_PERSON.INTID"

                        //insertValues += dr["VEHICLE.VEHICLE_SEQNUM???"].ToString() + "', ";  // Field NOT specified in DD
                        // SCB TODO: From DD: If there are multiple vehicles, there will be multiple records in the VEHICLE table 
                        // linked ot the FIELDINTERVIEWS record.
                        // "SQL FILTERS: FIELDINTERVIEWS.INTID = VEHICLE.INTID"

                        insertValues += "'" + dr["LOCATION"].ToString() + "', ";
                        insertValues += "'" + dr["INTID"].ToString() + "', ";
                        insertValues += "'" + dr["ASSOCIATION_TYPE"].ToString() + "', ";
                        insertValues += "'" + dr["FIELDINT_OFFICER"].ToString() + "')";


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Reports - Field contacts", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Field contacts copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_70_4_Reports_FieldContacts. " + ex.Message);
                return false;
            }
        }
        public bool ETL_70_5_Reports_MissingPersons(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // INCIDENT, INVOLVEMENTS

                string selectStatement = "SELECT inc.CREATE_DATE, inc.CDCREATE_OPERID, inc.UPDATE_DATE, inc.CDOPERID, " +
                    "inv.EVENTID, inv.SEQNUM, inv.INVOLVE_TYPE, inv.PERSON_SEQNUM, inc.DEPT_CASE_DISPO " + 
                    "FROM INCIDENT inc LEFT JOIN INVOLVEMENTS inv ON inc.EVENTID = inv.EVENTID " + // SCB TODO: Check JOIN Here. EVENTID?? SEQNUM??
                    "WHERE upper(inv.Involve_Type) IN ('RUNAWAY', ',MISSING','SUSP-MISSING')";  

                string insertStatement = "INSERT INTO migration_missing_persons " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, source_report_event_number, source_missing_person_id, " +
                    "missing_person_type_attr_code, source_missing_person_report_id, global_case_closure_status_attr) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + FormatDateTimeForMySQL(dr["inc.CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["inc.CDOPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["inc.UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["inc.CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["inv.EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["inv.SEQNUM"].ToString() + "',";
                        insertValues += "'" + dr["inv.INVOLVE_TYPE"].ToString() + "',";
                        insertValues += "'" + dr["inv.PERSON_SEQNUM"].ToString() + "',";
                        insertValues += "'" + dr["inc.DEPT_CASE_DISPO"].ToString() + "')";


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Reports - Mising persons", iInsertCount, iInsertErrorCount);

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_70_5_Reports_MissingPersons. " + ex.Message);
                return false;
            }
        }
        public bool ETL_70_6_Reports_Impounds(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // INCIDENT

                string selectStatement = "SELECT CREATE_DATE, CDCREATE_OPERID, UPDATE_DATE, CDOPERID, EVENTID, SEQNUM FROM INCIDENT";

                string insertStatement = "INSERT INTO migration_report_impounds " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, source_report_event_number, impound_date, source_impound_id) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + FormatDateTimeForMySQL(dr["inc.CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["inc.CDOPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["inc.UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["inc.CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["EVENTID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["SEQNUM"].ToString() + "')";

                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Reports - Impounds", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Impounds copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_70_6_Reports_Impounds. " + ex.Message);
                return false;
            }
        }
        public bool ETL_70_7_Reports_Additional_Information(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // ATTACHMENTS

                string selectStatement = "SELECT CREATE_DATE, CDCREATE_OPERID, UPDATE_DATE, CDOPERID, " +
                    "SEQNUM, EVENTID, NARRATIVE, ATTACH_OFFICER, REVIEWED_BY FROM ATTACHMENTS";
                // SCB TODO: Which one: ATTACH_OFFICER or OFFICER_NAME

                string insertStatement = "INSERT INTO migration_additional_information " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_additional_information_id, source_report_event_number, narrative, source_submitted_by, source_approved_by) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + FormatDateTimeForMySQL(dr["inc.CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["inc.CDOPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["inc.UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["inc.CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["NARRATIVE"].ToString() + "', ";
                        insertValues += "'" + dr["ATTACH_OFFICER"].ToString() + "', ";
                        insertValues += "'" + dr["REVIEWED_BY"].ToString() + "', ";


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Reports - Additional information", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Additional Information copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_70_7_Reports_Additional_Information. " + ex.Message);
                return false;
            }
        }
        public bool ETL_70_8_Reports_Citation_Charges(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // INVOLVEMENTS & CITATION
                // From DD: involvements.citation_id = citation.citation_id AND involvements.involvement = 'Citation'

                string selectStatement = "SELECT in.UPDATE_DATE, in.CDOPERID, in.CITATION_ID, in.OFFENSE_DESCR, in.SEQNUM" +
                    "FROM INVOLVEMENTS in JOIN CITATION ci ON in.citation_id = ci.citation_id WHERE UPPER(n.involvement) = 'CITATION'";
                // SCB TODO: Check JOIN

                string insertStatement = "INSERT INTO migration_citation_charges " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_citation_id, offense_code_name, offense_sequence_number) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["in.UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["in.CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["in.CITATION_ID"].ToString() + "', ";
                        insertValues += "'" + dr["in.OFFENSE_DESCR"].ToString() + "', ";
                        insertValues += "'" + dr["in.SEQNUM"].ToString() + "')";

                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Reports - Citation charges", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Citation charges copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_70_8_Reports_Citation_Charges. " + ex.Message);
                return false;
            }
        }
        public bool ETL_70_9_Reports_Traffic_Crash(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // INCIDENT & INVOLVEMENTS
                // SCB TODO: Both REP_OFFICER, OFFICER_NAME in DD, which one? Using REP_OFFICER for now.
                string selectStatement = "SELECT inc.CREATE_DATE, inc.CDCREATE_OPERID, inc.UPDATE_DATE, inc.CDOPERID, " +
                    "inc.LOCATION, inc.EVENTID, inc.REP_OFFICER, inc.REVIEWED_BY FROM INCIDENT inc ";
                // SCB TODO: JOIN INVOLVEMENTS ON ?? to get PERSON_SEQNUM for subject1_id

                string insertStatement = "INSERT INTO migration_traffic_crash " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, source_traffic_crash_location_id, " +
                    "source_report_event_number, source_submitted_by, source_approved_by) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + FormatDateTimeForMySQL(dr["inc.CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["inc.CDCREATE_OPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["inc.UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["inc.CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["inc.LOCATION"].ToString() + "', ";
                        insertValues += "'" + dr["inc.EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["inc.REP_OFFICER"].ToString() + "', ";
                        insertValues += "'" + dr["inc.REVIEWED_BY"].ToString() + "')";

                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Reports - Traffic crash incidents", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Traffic crash incidents copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_70_9_Reports_Traffic_Crash. " + ex.Message);
                return false;
            }
        }
        public bool ETL_80_Items(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle tables:
                // PROPERTY, VEHICLE, MASTER_CODES

                string selectStatement = "SELECT p.UPDATE_DATE, p.CDOPERID, p.SEQNUM, p.CATEGORY, p.DESCRIPTION, p.COLOR, " +
                    "p.SERIAL_NUM, p.MAKE, p.MODEL, " +
                    "v.LIC_NUM, v.VIN, v.VEH_MAKE, v.VEH_MODEL, v.VEH_YEAR, v.VEH_LIC_STATE, v.EXPIRY_DATE, v.VEH_STYLE, v.INSURANCE_COMPANY, " +
                    "p.MAKE, p.SIZE_CALIBER, p.BARREL_LENGTH, p.COLOR, p.STORAGE_LOCATION, p.AISLE, " +
                    "p.DRUG_WEIGHT, p.DRUG_WEIGHT_UNIT, p.SEQNUM, p.PERSON_SEQNUM, p.EVENTID, p.SEIZE_LOCATION" +
                    "v.VEH_TOW_COMPANY, v.IMP_LOCATION" +
                    " FROM PROPERTY p JOIN VEHICLE v ON p.NNN = v.NNN";  // SCB TODO: What to JOIN on here.

                string insertStatement = "INSERT INTO migration_items " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_item_id, source_master_item_id, item_category_attr_value, item_category_attr_code, description, " +
                    "primary_color_attr_value, primary_color_attr_code, serial_number, item_make, item_model, " +
                    "vehicle_tag, vehicle_vin_number, vehicle_vehicle_make_code, vehicle_vehicle_model_code, vehicle_year_of_manufacture, " +
                    "vehicle_registration_state_attr_code, vehicle_registration_year, vehicle_body_style_attr_value, vehicle_body_style_attr_code, " +
                    "vehicle_insurance_provider_name, vehicle_insurance_policy_number, " +
                    "firearm_firearm_make_attr_code, firearm_caliber, firearm_barrel_length, firearm_finish_attr_code, " +
                    "storage_facility, storage_location, towing_company, towing_location, " +
                    "quantity, measurement_units_attr_code, sequence_number, item_owner_name_source_id, source_owner_report_event_number, " + 
                    "property_recovered_location_source_id) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["p.UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["p.CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["p.SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["p.SEQNUM"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("PROPCAT", "", dr["p.CATEGORY"].ToString()) + "', ";
                        insertValues += "'" + dr["p.CATEGORY"].ToString() + "', ";

                        insertValues += "'" + dr["DESCRIPTION"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("VEHCOLOR", "", dr["p.COLOR"].ToString()) + "', ";
                        insertValues += "'" + dr["p.COLOR"].ToString() + "', ";

                        insertValues += "'" + dr["p.SERIAL_NUM"].ToString() + "', ";
                        insertValues += "'" + dr["p.MAKE"].ToString() + "', ";
                        insertValues += "'" + dr["p.MODEL"].ToString() + "', ";

                        insertValues += "'" + dr["v.LIC_NUM"].ToString() + "', ";
                        insertValues += "'" + dr["v.VIN"].ToString() + "', ";
                        insertValues += "'" + dr["v.VEH_MAKE"].ToString() + "', ";
                        insertValues += "'" + dr["v.VEH_MODEL"].ToString() + "', ";
                        insertValues += "'" + dr["v.VEH_YEAR"].ToString() + "', ";
                        insertValues += "'" + dr["v.VEH_LIC_STATE"].ToString() + "', ";
                        insertValues += "'" + dr["v.EXPIRY_DATE"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("VEH_STYLE", "", dr["v.VEH_STYLE"].ToString()) + "', ";
                        insertValues += "'" + dr["v.VEH_STYLE"].ToString() + "', ";

                        // SCB TODO: vehicle_insurance_provider_name  & vehicle_insurance_policy_number are both stored in VEHICLE.INSURANCE_COMPANY
                        // Parse?? - Assigning to both for now.
                        insertValues += "'" + dr["v.INSURANCE_COMPANY"].ToString() + "', ";
                        insertValues += "'" + dr["v.INSURANCE_COMPANY"].ToString() + "', ";

                        insertValues += "'" + dr["p.MAKE"].ToString() + "', ";
                        insertValues += "'" + dr["p.SIZE_CALIBER"].ToString() + "', ";
                        insertValues += "'" + dr["p.BARREL_LENGTH"].ToString() + "', ";
                        insertValues += "'" + dr["p.COLOR"].ToString() + "', ";
                        insertValues += "'" + dr["p.STORAGE_LOCATION"].ToString() + "', ";
                        insertValues += "'" + dr["p.AISLE"].ToString() + "', ";

                        insertValues += "'" + dr["v.VEH_TOW_COMPANY"].ToString() + "', ";
                        insertValues += "'" + dr["v.IMP_LOCATION"].ToString() + "', ";

                        insertValues += "'" + dr["p.DRUG_WEIGHT"].ToString() + "', ";
                        insertValues += "'" + dr["p.DRUG_WEIGHT_UNIT"].ToString() + "', ";
                        insertValues += "'" + dr["p.SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["p.PERSON_SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["p.EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["p.SEIZE_LOCATION"].ToString() + "')";


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Items", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Items copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Items. " + ex.Message);
                return false;
            }
        }
        public bool ETL_90_1_Evidence_Items(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // PROPERTY

                string selectStatement = "SELECT UPDATE_DATE, CDOPERID, SEQNUM, EVENTID, FROM_OFFICER, PROPID, PROP_TAG" +
                    "FROM PROPERTY";

                string insertStatement = "INSERT INTO migration_evidence_items " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, source_report_event_number, responsible_officer_source_user_id, " +
                    "identifier1_description, source_evidence_item_id" +
                    "source_master_item_id, source_barcode_value) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["inc.UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["inc.CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["FROM_OFFICER"].ToString() + "', ";
                        insertValues += "'" + dr["PROPID"].ToString() + "', ";
                        insertValues += "'" + dr["SEQNUM"].ToString() + "', ";  // SCB TODO: or PROPID, see DD.
                        insertValues += "'" + dr["PROP_TAG"].ToString() + "')";


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Evidence items", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Evidence items copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_90_1_Evidence_Items. " + ex.Message);
                return false;
            }
        }
        public bool ETL_90_2_Evidence_Chain(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // PROPERTY_STATUS & PROPERTY

                string selectStatement = "SELECT ps.UPDATE_DATE, ps.CDOPERID, ps.SEQNUM, ps.PROPERTY_SEQNUM, p.EVENTID, " +
                    "ps.STATUS, ps.STATUS_DATE, ps.TO_OFFICER, ps.COMMENTS, ps.STORAGE_LOCATION, ps.AISLE" +
                    "FROM PROPERTY_STATUS ps LEFT JOIN PROPERTY p ON ps.property_seqnum = p.seqnum";

                string insertStatement = "INSERT INTO migration_evidence_chain_events " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_chain_event_id, source_evidence_item_id, source_report_event_number, " +
                    "chain_event_type_name, chain_event_date, received_by_name, chain_event_details, " +
                    "storage_location_facility_name, storage_location_shelf_name) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["ps.UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["ps.CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["ps.SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["ps.PROPERTY_SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["ps.STATUS"].ToString() + "', ";
                        insertValues += "'" + dr["ps.STATUS_DATE"].ToString() + "', ";
                        insertValues += "'" + dr["ps.TO_OFFICER"].ToString() + "', ";
                        insertValues += "'" + dr["ps.COMMENTS"].ToString() + "', ";
                        insertValues += "'" + dr["ps.STORAGE_LOCATION"].ToString() + "', ";
                        insertValues += "'" + dr["ps.AISLE"].ToString() + "')";


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Evidence chain events", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Evidence chain events copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_90_2_Evidence_Chain. " + ex.Message);
                return false;
            }
        }
        public bool ETL_110_Cases(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // NOTE: Removed SEQNUM > id. Check, but id is probbaly an identity, and SEQNUM is assigned below.
                string selectStatement = "SELECT CREATE_DATE, CDCREATE_OPERID, UPDATE_DATE, CDOPERID, " +
                    "SEQNUM, EVENTID, CASE_NAME, ASSIGNED_DIVISION, " +
                    "DUE_DATE, DATE_ASSIGNED, SOLV_CHK4, SOLV_CHK6, DEPT_CASE_DISPO, " +
                    "DEPT_CASE_DISPO_DATE, OFFICER_ASSIGNED, ASSIGNED_BY FROM INCIDENT";

                string insertStatement =
                    "INSERT INTO migration_cases (source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date, created_by, updated_date, updated_by," +
                    "source_case_id, local_id, title, assigned_personnel_unit_attr_value, assigned_personnel_unit_attr_code, " +
                    "due_date, assigned_date, sf_suspect_named, sf_witness_to_offense, status_attr_value, status_attr_code, " +
                    "status_date, assignee_source_user_id, assignee_updated_by) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + FormatDateTimeForMySQL(dr["CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["CDCREATE_OPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["UPDATED_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["CDOPERID"].ToString() + "', ";
                        insertValues += "'" + dr["SEQNUM"].ToString() + "', ";  // varchar

                        insertValues += "'" + FormatDateTimeForMySQL(dr["CREATE_DATE"].ToString()) + "', "; 
                        insertValues += "'" + dr["CDCREATE_OPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["UPDATED_DATE"].ToString()) + "', "; 
                        insertValues += "'" + dr["CDOPERID"].ToString() + "', ";

                        insertValues += "'" + dr["SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["CASE_NAME"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("BUREAU", dr["ASSIGNED_DIVISION"].ToString(), "") + "', ";
                        insertValues += "'" + dr["ASSIGNED_DIVISION"].ToString() + "', ";

                        insertValues += "'" + FormatDateTimeForMySQL(dr["DUE_DATE"].ToString()) + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["DATE_ASSIGNED"].ToString()) + "', ";
                        insertValues += "'" + dr["SOLV_CHK4"].ToString() + "', ";
                        insertValues += "'" + dr["SOLV_CHK6"].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("DEPSTATUS", dr["DEPT_CASE_DISPO"].ToString(), "") + "', ";
                        insertValues += "'" + dr["DEPT_CASE_DISPO"].ToString() + "', ";

                        insertValues += "'" + FormatDateTimeForMySQL(dr["DEPT_CASE_DISPO_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["OFFICER_ASSIGNED"].ToString() + "', ";
                        insertValues += "'" + dr["ASSIGNED_BY"].ToString() + "')";

                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Cases", iInsertCount, iInsertErrorCount);

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_110_Cases. " + ex.Message);
                return false;
            }
        }
        public bool ETL_110_Case_Notes(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // CASE_LOG

                string selectStatement = "SELECT UPDATE_DATE, CDOPERID, NOTE_DATE, " +
                    "EVENTID, NARRATIVE, SUBJECT, OFFICER FROM CASE_LOG";

                string insertStatement = "INSERT INTO migration_case_notes " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_case_id, content, title, author_source_id) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["UPDATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["CDOPERID"].ToString() + "', ";

                        // NOTE: NOTE_DATE Defaultes to current D/T but Office can over-ride
                        insertValues += "'" + FormatDateTimeForMySQL(dr["NOTE_DATE"].ToString()) + "', "; 
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["NARRATIVE"].ToString() + "', ";
                        insertValues += "'" + dr["SUBJECT"].ToString() + "', ";
                        insertValues += "'" + dr["OFFICER"].ToString() + "')";


                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("Case notes", iInsertCount, iInsertErrorCount);

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Case notes copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_110_Case_Notes. " + ex.Message);
                return false;
            }
        }
        public bool ETL_120_Attachments(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // ATTACHMENTS
                // SCB TODO: Excel had these two ATTACH_OFFICER, OFFICER_NAME assigned to source_submitted_by. Which one?

                string selectStatement = "SELECT CREATE_DATE, CDCREATE_OPERID, UPDATE_DATE, CDOPERID, SEQNUM, EVENTID, NARRATIVE, " +
                                         "ATTACH_OFFICER, REVIEWED_BY FROM ATTACHMENTS";

                string insertStatement = "INSERT INTO migration_additional_information " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_additional_information_id, source_report_event_number, narrative, source_submitted_by, source_approved_by) VALUES (";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + dr["CREATE_DATE"].ToString() + "', ";
                        insertValues += "'" + dr["CDCREATE_OPERID"].ToString() + "', ";
                        insertValues += "'" + dr["UPDATE_DATE"].ToString() + "', ";
                        insertValues += "'" + dr["CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += "'" + dr["SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["EVENTID"].ToString() + "', ";
                        insertValues += "'" + dr["NARRATIVE"].ToString() + "', ";
                        insertValues += "'" + dr["ATTACH_OFFICER"].ToString() + "', "; // NOTE: See above
                        insertValues += "'" + dr["REVIEWED_BY"].ToString() + "')"; // Last field, close with right parenthese..

                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }
                logging.WriteReportDataEntry("EAttachments", iInsertCount, iInsertErrorCount);

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_120_Attachments. " + ex.Message);
                return false;
            }
        }

        // Remaining: Reports (Custodial Evidence), Legacy Details (Other), Case Notes??
        // migration_person_employment_histories ??
        // migration_person_identifying_marks ??
        // migration_person_injuries ??
        // migration_report_statuses ??

        public bool ETL_Starter(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle table:
                // 

                string selectStatement = "SELECT CREATE_DATE, CDCREATE_OPERID, UPDATE_DATE, CDOPERID, " +
                    "FROM table";

                string insertStatement = "INSERT INTO migration_ " +
                    "(source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by) VALUES ('";

                string insertValues = "";
                string fullInsertStatement = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += FormatDateTimeForMySQL(dr["CREATE_DATE"].ToString()) + "', ";
                        insertValues += dr["CDCREATE_OPERID"].ToString() + "', ";
                        insertValues += FormatDateTimeForMySQL(dr["UPDATE_DATE"].ToString()) + "', ";
                        insertValues += dr["CDOPERID"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";


                        // Use these as templates:

                        insertValues += dr[""].ToString() + "', ";

                        insertValues += "'" + GetMasterCodeValue("STATE", "", dr["BIRTH_PLACE"].ToString()) + "', ";
                        insertValues += "'" + dr["BIRTH_PLACE"].ToString() + "', ";

                        insertValues += FormatDateTimeForMySQL(dr["CREATE_DATE"].ToString()) + "', ";

                        insertValues += dr[""].ToString() + "')";  // Finish up with a closing parentheses.



                        fullInsertStatement = insertStatement + insertValues;

                        if (mySqlDAL.ExecuteNonQuery(insertStatement))
                        {
                            iInsertCount += 1;
                        }
                        else
                        {
                            iInsertErrorCount = +1;
                        }
                    }
                }

                logging.WriteReportDataEntry("Data entity", iInsertCount, iInsertErrorCount);

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Starter. " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Helpers
        public string AppendInsertStatement(string InsertStatement, string ValueToAppend, bool SurroundWithQuotes, bool LastField = false)
        { // SCB TODO: Delete if not using, and aren't using currently.
            string ResultingStatement = "";
            if (SurroundWithQuotes)
                ResultingStatement = InsertStatement + "'" + ValueToAppend + "'";
            else
                ResultingStatement = InsertStatement + ValueToAppend;

            if (LastField)
                ResultingStatement += ")";
            else
                ResultingStatement += ", ";

            return ResultingStatement;
        }
        public DateTime FormatDateTimeForMySQL(string incomingDateTime)
        {
            // MySQL retrieves and displays DATETIME values in 'YYYY-MM-DD HH:MM:SS' format
            return DateTime.Parse(incomingDateTime);
        }
        public bool BuildMasterCodeDataView(string filePath)
        {
            try
            {
                string sqlQuery = "Select * From [PRIORS Reference Values$]";
                DataSet ds = new DataSet();
                string constring = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=YES;\"";
                OleDbConnection con = new OleDbConnection(constring + "");
                OleDbDataAdapter da = new OleDbDataAdapter(sqlQuery, con);
                da.Fill(ds);
                DataTable dt = ds.Tables[0];
                vMasterCodes = new DataView(dt);
                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in MasterTableCreate. " + ex.Message);
                return false;
            }

        }
        public bool BuildReferenceDataViews(MySqlDAL mySqlDAL)
        {
            try
            {
                string selectStatement = "";

                selectStatement = "SELECT id, name FROM migration.ref_migration_duty_status";
                MySqlDataAdapter adapter = new MySqlDataAdapter(selectStatement, mySqlDAL.MySqlConn);
                DataSet ds = new DataSet();
                adapter.Fill(ds, "Lookups");
                dvRefDutyStatus = ds.Tables["Lookups"].DefaultView;
                adapter = null;

                selectStatement = "SELECT id, name, description FROM migration.ref_global_organization_types";
                adapter = new MySqlDataAdapter(selectStatement, mySqlDAL.MySqlConn);
                ds = new DataSet();
                adapter.Fill(ds, "Lookups");
                dvRefOrganizationTypes = ds.Tables["Lookups"].DefaultView;
                adapter = null;

                selectStatement = "SELECT id, name, description FROM migration.ref_migration_item_type";
                adapter = new MySqlDataAdapter(selectStatement, mySqlDAL.MySqlConn);
                ds = new DataSet();
                adapter.Fill(ds, "Lookups");
                dvRefItemTypes = ds.Tables["Lookups"].DefaultView;
                adapter = null;

                selectStatement = "SELECT id, location_type FROM migration.ref_migration_location_types";
                adapter = new MySqlDataAdapter(selectStatement, mySqlDAL.MySqlConn);
                ds = new DataSet();
                adapter.Fill(ds, "Lookups");
                dvLocationTypes = ds.Tables["Lookups"].DefaultView;
                adapter = null;

                selectStatement = "SELECT id, owner_type FROM migration.ref_migration_owner_types";
                adapter = new MySqlDataAdapter(selectStatement, mySqlDAL.MySqlConn);
                ds = new DataSet();
                adapter.Fill(ds, "Lookups");
                dvRefOwnerTypes = ds.Tables["Lookups"].DefaultView;
                adapter = null;

                selectStatement = "SELECT code, name FROM migration.ref_nibrs_offense_codes";
                adapter = new MySqlDataAdapter(selectStatement, mySqlDAL.MySqlConn);
                ds = new DataSet();
                adapter.Fill(ds, "Lookups");
                dvRefNibrsOffenseCodes = ds.Tables["Lookups"].DefaultView;
                adapter = null;

                selectStatement = "SELECT code, description FROM migration.ref_ucr_offense_status_codes";
                adapter = new MySqlDataAdapter(selectStatement, mySqlDAL.MySqlConn);
                ds = new DataSet();
                adapter.Fill(ds, "Lookups");
                dvRefUcrOffenseStatusCodes = ds.Tables["Lookups"].DefaultView;
                adapter = null;

                selectStatement = "SELECT code, name FROM migration.ref_ucr_summary_offense_codes";
                adapter = new MySqlDataAdapter(selectStatement, mySqlDAL.MySqlConn);
                ds = new DataSet();
                adapter.Fill(ds, "Lookups");
                dvRefUcrSummaryOffenseCodes = ds.Tables["Lookups"].DefaultView;
                adapter = null;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in BuildReferenceDataViews. " + ex.Message);
                return false;
            }
            return true;
        }
        public string GetMasterCodeValue(string tableId, string tableCode = "", string tableDescription = "", string agency = "")
        {
            try
            {
                DataView dvSearch = vMasterCodes;
                string sValue = "";

                if ((tableDescription == "" & agency == "") | (tableDescription != "" & agency != ""))
                {
                    sValue = "MC:PARAM ERROR";
                }
                else
                { 
                    if (tableDescription == "")
                    { 
                        if (agency == "")
                        { 
                            dvSearch.RowFilter = "TABLE_ID = '" + tableId + "' AND [CODE VALUE] = '" + tableCode + "'";
                        }
                        else
                        { 
                            dvSearch.RowFilter = "TABLE_ID = '" + tableId + "' AND [CODE VALUE] = '" + tableCode + "' AND AGENCY = '" + agency + "'";
                        }
                        sValue = dvSearch[0][2].ToString();
                    }
                    else // 
                    { 
                        if (agency == "")
                        { 
                            dvSearch.RowFilter = "TABLE_ID = '" + tableId + "' AND [CODE DESCRIPTION] = '" + tableDescription + "'";
                        }
                        else
                        {
                            dvSearch.RowFilter = "TABLE_ID = '" + tableId + "' AND [CODE DESCRIPTION] = '" + tableDescription + "' AND AGENCY = '" + agency + "'";
                         }
                        sValue = dvSearch[0][1].ToString();
                    }
                }
                return sValue;
            }
            catch (Exception)
            {
                return "MC:NOT FOUND";
            }
        }
        #endregion
    }
}
