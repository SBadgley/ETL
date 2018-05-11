using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using MySql.Data.MySqlClient;
using DataAccessLayer_NET_Framework_;
using System;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Linq;

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
        #endregion

        #region ETL_10_Atrributes
        public bool ETL_10_Atrributes(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // SCB TODO: Where to get the attributes??  MASTER_CODES.CODE_DESCRIPTION?? From spreadsheet, appears maybe so.  Need Oracle access to see.

                string selectStatement = "SELECT ?? FROM ??";

                string insertStatement = 
                    "INSERT INTO migration_attributes (source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "created_date, created_by, updated_date, updated_by, " + 
                    "source_attribute_id, attribute_type, display_abbreviation, display_value) VALUES ";

                string insertValues = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        // MySQL retrieves and displays DATETIME values in 'YYYY-MM-DD HH:MM:SS' format

                        insertValues += "'" + FormatDateTimeForMySQL(dr["CREATE_DATE"].ToString()) + "', "; // Defaults to current DT, so remove?
                        insertValues += "'" + dr[".CDCREATE_OPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["UPDATED_DATE"].ToString()) + "', "; // Defaults to current DT, so remove?
                        insertValues += "'" + dr["CDOPERID"].ToString() + "', ";

                        insertValues += "'" + dr[""].ToString() + "', ";  

                        insertValues += dr[""].ToString() + ")"; // Last field, close with right parenthese..

                        insertStatement += insertValues;

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
                mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                    "VALUES ('" + currentDateTime + "', 'Attributes copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Atrributes. " + ex.Message);
                return false;
            }
        }
        #endregion
        #region ETL_20_OffenseCodes
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
                // Older Excel Provider:
                //string excelConnString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\sbadgley\Documents\Documentation\Mark43\PRIORS_Offense_Codes.xlsx;" +
                //         @"Extended Properties='Excel 8.0;HDR=Yes;'";

                using (OleDbConnection connection = new OleDbConnection(excelConnString))
                {
                    connection.Open();  // Table not in expected format.
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
                                    "VALUES ('" + currentDateTime + "', '" + defaultCreatedUpdatedBy + "', '" + currentDateTime + "', '" + defaultCreatedUpdatedBy + 
                                    "', '" + Code + "', '" + Name + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";

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
                mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                    "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Offense codes copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_OffenseCodes. " + ex.Message);
                return false;
            }
        }
        #endregion
        #region ETL_30_Offenses
        public bool ETL_30_Offenses(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            //Logging logging = new Logging();
            try
            {
                //string selectStatement = "SELECT * FROM INVOLVEMENTS inv JOIN INCIDENT inc ON " +
                //    " WHERE LCASE(inv.Involve_type) IN ('victim', 'offense')"; // ??

                // NOTES:
                // source_report_event_number = inv.EVENTID
                // source_offense_id = inv.OFFENSE_ID
                // source_offense_location_id = inc.LOCATION

                // Oracle Offense tables:
                // 1 - INVOLVEMENTS
                // 2 - INCIDENT (Location)
                // 3 - MASTER_CODES.CODE_DESCRIPTION :
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


                //if (mySqlDAL.ExecuteNonQuery(insertStatement))
                //{
                //    iInsertCount += 1;
                //}
                //else
                //{
                //    iInsertErrorCount = +1;
                //}

                //mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                //    "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Offenses copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");


                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Offenses. " + ex.Message);
                return false;
            }
        }
        #endregion
        #region ETL_40_Users
        public bool ETL_40_Users(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            try
            {
                // Oracle tables:
                // EMPLOYEE
                // MASTER_CODES

                // SCB TODO: Use a CASE around RMS_LOCKED?
                string selectStatement = "SELECT emp.CREATE_DATE, emp.CDCREATE_OPERID, emp.UPDATED_DATE, emp.CDOPERID, " +
                    "emp.SEQNUM, emp.FIRST_NAME, emp.SURNAME, emp.MIDDLE_NAME, emp.DOB, emp.SEX, mc.CODE_DESCRIPTION, " +
                    "emp.RANK, emp.RES_PHONE, emp.EMAIL, emp.BADGE_ID, emp.FOREIGN_SEQNUM, emp.EMPLOYEE_STATUS, emp.AGENCY, emp.RMS_LOCKED" +
                    " FROM EMPLOYEE emp";
                //" FROM EMPLOYEE emp LEFT JOIN MASTER_CODES mc ON UPPER(emp.RANK) = UPPER(mc.CODE_VALUE) AND UPPER(mc.TABLE_ID) = 'RANK'";

                string insertStatement = "INSERT INTO migration_users (source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "source_user_id, first_name, last_name, middle_name, date_of_birth, sex_attr_code, rank_attr_value, rank_attr_code, " +
                    "phone_number, primary_email, badge_number, external_cad_id, duty_status_value, department_agency_name, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by, is_disabled) VALUES (";

                string insertValues = "";
                string sValue = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        // MySQL retrieves and displays DATETIME values in 'YYYY-MM-DD HH:MM:SS' format

                        insertValues += "'" + FormatDateTimeForMySQL(dr["emp.CREATE_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["emp.CDCREATE_OPERID"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["emp.UPDATED_DATE"].ToString()) + "', ";
                        insertValues += "'" + dr["emp.CDOPERID"].ToString() + "', ";
                        insertValues += "'" + dr["emp.SEQNUM"].ToString() + "', ";  // varchar
                        insertValues += "'" + dr["emp.FIRST_NAME"].ToString() + "', ";
                        insertValues += "'" + dr["emp.SURNAME"].ToString() + "', ";
                        insertValues += "'" + dr["emp.MIDDLE_NAME"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["emp.DOB"].ToString()) + "', ";
                        insertValues += "'" + dr["emp.SEX"].ToString() + "', ";


                        sValue = GetMasterCodeValue("RANK", "", dr["p.RANK"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["emp.RANK"].ToString() + "', ";

                        insertValues += "'" + dr["emp.RES_PHONE"].ToString() + "', ";
                        insertValues += "'" + dr["emp.EMAIL"].ToString() + "', ";
                        insertValues += "'" + dr["emp.BADGE_ID"].ToString() + "', ";
                        insertValues += "'" + dr["emp.FOREIGN_SEQNUM"].ToString() + "', ";
                        insertValues += "'" + dr["emp.EMPLOYEE_STATUS"].ToString() + "', ";
                        insertValues += "'" + dr["emp.AGENCY"].ToString() + "', ";

                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        insertValues += dr["emp.RMS_LOCKED"].ToString() + ")"; // tinyint - Last field, close with right parenthese..

                        insertStatement += insertValues;

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
                mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                    "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Users copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Users. " + ex.Message);
                return false;
            }
        }
        #endregion
        #region ETL_50_Locations
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

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";
                        insertValues += "'" + currentDateTime + "', ";
                        insertValues += "'" + defaultCreatedUpdatedBy + "', ";

                        // SCB TODO: What values for thesde 2:
                        insertValues += "'" + 1 + "', ";  // source_location_id ??
                        insertValues += "'" + ' ' + "', ";  // source_location_type ??

                        insertValues += dr["COUNTY"].ToString() + ")"; // Last field, close with right parenthese..

                        insertStatement += insertValues;

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

                mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                        "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Locations copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Locations. " + ex.Message);
                return false;
            }
        }
        #endregion
        #region ETL_60_Names
        public bool ETL_60_Names(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            //Logging logging = new Logging();
            try
            {
                // Oracle tables:
                // PERSON
                // MASTER_CODES - Many, many lookups.  Need to find efficient way...

                string selectStatement = "SELECT p.CREATE_DATE, p.CDCREATE_OPERID, p.UPDATE_DATE, p.CDOPERID, p.SEQNUM, " +
                    "p.FIRST_NAME, p.MIDDLE_NAME, p.SURNAME, p.SUFFIX, p.PREFIX, p.MONIKER, p.SSN, p.LICENSE, " +
                    "p.LIC_ENDORSEMENTS, p.DOB, mc.CODE_DESCRIPTION, p.BIRTH_PLACE, p.DECEASED, p.REMARKS, p.WEIGHT, p.TO_WEIGHT, " +
                    "p.HEIGHT, p.TO_HEIGHT, p.CITIZENSHIP, p.MARITAL, p.FBI_ID, p.STATE_ID, p.NCIC_PRINT, p.BUILD, p.EYE, p.SEX, " +
                    "p.ETHNICITY, p.RACE, p.COMPLEXION, p.RES_PHONE, p,BUS_PHONE, p.CELL_PHONE, p.EMAIL, p.EMANCIPATION_DATE, " +
                    "p.HAIR_STYLE, p.HAIR_LENGTH, p.HAIR, p.FACIAL_HAIR, p.ALRT_NOTES, p.SMT, p.RECORD_TYPE, p.SURNAME, p.BUSINESS_TYPE, " +
                    "p.TEETH" +
                    "FROM PERSON p";
                //"FROM PERSON p LEFT JOIN MASTER_CODES mc ON p.BIRTH_PLACE = mc.CODE_DESCRIPTION WHERE UPPER(mc.TABLE_ID) = 'STATE'";

                // SCB TODO: No insert fields for p.SMT - these go right afteer caution_attr_code -> end of 11th line below.
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

                        // SCB TODO: What values for thesde 2:
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

                        sValue = GetMasterCodeValue("STATE", "", dr["p.BIRTH_PLACE"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.BIRTH_PLACE"].ToString() + "', ";

                        insertValues += "'" + dr["p.DECEASED"].ToString() + "', ";
                        insertValues += "'" + dr["p.REMARKS"].ToString() + "', ";
                        insertValues += "'" + dr["p.WEIGHT"].ToString() + "', ";
                        insertValues += "'" + dr["p.WEIGHT"].ToString() + "', ";  // weight min
                        insertValues += "'" + dr["p.TO_WEIGHT"].ToString() + "', ";
                        insertValues += "'" + dr["p.HEIGHT"].ToString() + "', ";
                        insertValues += "'" + dr["p.HEIGHT"].ToString() + "', ";  // height min
                        insertValues += "'" + dr["p.TO_HEIGHT"].ToString() + "', ";

                        sValue = GetMasterCodeValue("NCIC-Country", "", dr["p.CITIZENSHIP"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.CITIZENSHIP"].ToString() + "', ";

                        sValue = GetMasterCodeValue("MARITAL", "", dr["p.MARITAL"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.MARITAL"].ToString() + "', ";

                        insertValues += "'" + dr["p.FBI_ID"].ToString() + "', ";
                        insertValues += "'" + dr["p.STATE_ID"].ToString() + "', ";
                        insertValues += "'" + dr["p.NCIC_PRINT"].ToString() + "', ";

                        sValue = GetMasterCodeValue("BUILD", "", dr["p.BUILD"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.BUILD"].ToString() + "', ";

                        sValue = GetMasterCodeValue("EYECOL", "", dr["p.EYE"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.EYE"].ToString() + "', ";

                        sValue = GetMasterCodeValue("SEX", "", dr["p.SEX"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.SEX"].ToString() + "', ";

                        sValue = GetMasterCodeValue("ETHNIC", "", dr["p.ETHNICITY"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.ETHNICITY"].ToString() + "', ";

                        sValue = GetMasterCodeValue("RACE", "", dr["p.RACE"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.RACE"].ToString() + "', ";

                        sValue = GetMasterCodeValue("COMPLEX", "", dr["p.COMPLEXION"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.COMPLEXION"].ToString() + "', ";

                        insertValues += "'" + dr["p.RES_PHONE"].ToString() + "', ";
                        insertValues += "'" + dr["p.BUS_PHONE"].ToString() + "', ";
                        insertValues += "'" + dr["p.CELL_PHONE"].ToString() + "', ";
                        insertValues += "'" + dr["p.EMAIL"].ToString() + "', ";

                        insertValues += "'" + FormatDateTimeForMySQL(dr["p.EMANCIPATION_DATE"].ToString()) + "', ";

                        sValue = GetMasterCodeValue("HAIR STYLE", "", dr["p.HAIR_STYLE"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.HAIR_STYLE"].ToString() + "', ";

                        sValue = GetMasterCodeValue("HAIRLENGTH", "", dr["p.HAIR_LENGTH"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.HAIR_LENGTH"].ToString() + "', ";

                        sValue = GetMasterCodeValue("HAIRCOL", "", dr["p.HAIR"].ToString());
                        insertValues += "'" + sValue + "', ";
                        insertValues += "'" + dr["p.HAIR"].ToString() + "', ";

                        sValue = GetMasterCodeValue("FACIAL HAIR", "", dr["p.FACIAL_HAIR"].ToString());
                        insertValues += "'" + sValue + "', ";
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

                        sValue = GetMasterCodeValue("TEETH", "", dr["p.TEETH"].ToString());
                        insertValues += "'" + sValue + "', ";




                        // SCB TODO: Finish up:
                        // More lookups, but have questions before continuing. Also see note on last field, RESIDENT. May come from different table.


                        insertValues += dr[""].ToString() + ")"; // Last field, close with right parenthese..

                        insertStatement += insertValues;

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

                mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                    "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Names copied.', '" + iInsertCount + " rows copied, " + iInsertErrorCount + " rows had errors.', '')");

                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Names. " + ex.Message);
                return false;
            }
        }
        #endregion
        // Remaining: Reports, Items, Evidence, Cases, Attachments, Legacy Details (Other)
        #region Helpers
        public DateTime FormatDateTimeForMySQL(string incomingDateTime)
        {
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
        #region LoopTesting
        public bool ELT_Stage_1()
        {
            Logging logging = new Logging();
            try
            {
                //int iTableRowCount = 0;
                string selectStatement = "";
                string insertStatement = "";
                string insertStatementFINAL = "";
                int iCurSeq1 = 0;
                int iCurSeq2 = 0;
                //int iCurSeq3 = 0;
                int iRow = 0;

                //string oracleConnString = ConfigurationManager.AppSettings.Get("OracleSourceConnString"); // null
                //string oracleConnString = ConfigurationManager.AppSettings["OracleSourceConnString"]; // null
                //string oracleConnString = ConfigurationManager.ConnectionStrings["OracleSourceConnString"].ToString(); // Error
                //string oracleConnString = "";

                const string oracleConnString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521)) (CONNECT_DATA=(SERVICE_NAME=PROD))); User Id=userId;Password=password;";
                OracleDAL oracleDAL = new OracleDAL(oracleConnString);

                //const string mySqlConnString = "Server = localhost; UID = sbadgley; password = 2010Camaro!ZL1UPED";

                MySqlDAL mySqlSELECT = new MySqlDAL()  // mySqlConnString
                {
                    //LoggingLevel = 2,
                    //UserID = "sbadgley",
                    //Password = "2010Camaro!ZL1UPED",
                    //DatabaseName = "migration"
                };


                MySqlDAL mySqlINSERT = new MySqlDAL() // mySqlConnString
                {
                    //LoggingLevel = 2,
                    //UserID = "sbadgley",
                    //Password = "2010Camaro!ZL1UPED",
                    //DatabaseName = "migration"
                };

                MySqlDAL mySql = new MySqlDAL() // mySqlConnString
                {
                    //LoggingLevel = 2,
                    //UserID = "sbadgley",
                    //Password = "2010Camaro!ZL1UPED",
                    //DatabaseName = "migration"
                };

                //Dictionaries dicts = new Dictionaries();

                using (MySqlDataReader drSELECT = mySqlSELECT.ExecuteDataReader("SELECT SequenceLevel1, SequenceLevel2, SequenceLevel3, SourceSchema, " + 
                    "SourceDatabase, SourceTable, SourceField, TargetServer, TargetDatabase, TargetTable, TargetField, SelectOverride, Transformation " + 
                    "FROM migration.etl_mapping ORDER BY SequenceLevel1, SequenceLevel2, SequenceLevel3;"))
                {
                    if (drSELECT.HasRows)
                    {
                    //selectStatement = "SELECT " + drSELECT["SourceField"].ToString() + ", ";
                    //insertStatement = "INSERT INTO " + drSELECT["TargetTable"].ToString() + "(" + drSELECT["TargetField"].ToString() + ", ";

                    while (drSELECT.Read())
                        {
                            if (drSELECT["SelectOverride"].ToString() != "")
                            {
                                selectStatement = drSELECT["SelectOverride"].ToString();
                            }

                            // For now, treat Seq 1 and DB, Seq 2 as table and Seq 3 as field. So, when Seq 1 OR Seq 2 changes:
                            // It's either the first row of data OR we have a new table to process.
                            if (((int)drSELECT["SequenceLevel1"] != iCurSeq1 | (int)drSELECT["SequenceLevel2"] != iCurSeq2) & iRow != 0)
                            {
                                iCurSeq1 = (int)drSELECT["SequenceLevel1"];
                                iCurSeq2 = (int)drSELECT["SequenceLevel2"];

                                // Run the INSERTS
                                // Remove ", " on end and finish statements.
                                selectStatement = selectStatement.Substring(0, selectStatement.Length - 2) + " FROM " + drSELECT["SourceTable"].ToString();
                                insertStatement = insertStatement.Substring(0, insertStatement.Length - 2) + ") VALUES (";

                                //RunLevel1Inserts()

                                using (MySqlDataReader drSELECTData = mySqlSELECT.ExecuteDataReader(selectStatement))
                                {
                                    if (drSELECTData.HasRows)
                                    {
                                        while (drSELECTData.Read())
                                        {
                                            insertStatementFINAL = insertStatement;


                                            for (int iCol = 0; iCol < drSELECTData.FieldCount; iCol++)
                                            {
                                                insertStatementFINAL += "'" + drSELECTData[iCol] + "', ";
                                            }

                                            insertStatementFINAL += insertStatementFINAL.Substring(0, insertStatementFINAL.Length - 2) + ")";

                                            mySql.ExecuteNonQuery(insertStatementFINAL);
                                        }
                                    }
                                }
                                if (drSELECT["SelectOverride"].ToString() != "")
                                {
                                    selectStatement = drSELECT["SelectOverride"].ToString();
                                }
                                else
                                {
                                    selectStatement = "SELECT " + drSELECT["SourceField"].ToString() + ", ";
                                }
                                insertStatement = "INSERT INTO " + drSELECT["TargetTable"].ToString() + "(" + drSELECT["TargetField"].ToString() + ", ";

                            }
                            else if (iRow == 0)
                            {
                                iCurSeq1 = (int)drSELECT["SequenceLevel1"];
                                iCurSeq2 = (int)drSELECT["SequenceLevel2"];

                                if (drSELECT["SelectOverride"].ToString() != "")
                                {
                                    selectStatement = drSELECT["SelectOverride"].ToString();
                                }
                                else
                                {
                                    selectStatement = "SELECT " + drSELECT["SourceField"].ToString() + ", ";
                                }
                                insertStatement = "INSERT INTO " + drSELECT["TargetTable"].ToString() + "(" + drSELECT["TargetField"].ToString() + ", ";
                            }
                            else  // Sequences didn't change, concatenate more fields.
                            {
                                selectStatement += drSELECT["SourceField"].ToString() + ", ";
                                insertStatement += drSELECT["TargetField"].ToString() + ", ";
                            }
                            iRow = +1;

                        } // Reading main sequence select
                    }  // Sequence select had rows.




                    // SCB TODO: Move this and identiacl code above into routine. This is just to test..
                    // SCB TODO: Change reader to Oracle
                    // Remove ", " on end and finish statements.
                    selectStatement = selectStatement.Substring(0, selectStatement.Length - 2) + " FROM " + drSELECT["SourceTable"].ToString();
                    insertStatement = insertStatement.Substring(0, insertStatement.Length - 2) + ") VALUES (";

                    //RunLevel1Inserts()

                    using (MySqlDataReader drSELECTData = mySqlSELECT.ExecuteDataReader(selectStatement))
                    {
                        if (drSELECTData.HasRows)
                        {
                            while (drSELECTData.Read())
                            {
                                insertStatementFINAL = insertStatement;


                                for (int iCol = 0; iCol < drSELECTData.FieldCount; iCol++)
                                {
                                    insertStatementFINAL += "'" + drSELECTData[iCol] + "', ";
                                }

                                insertStatementFINAL += insertStatementFINAL.Substring(0, insertStatementFINAL.Length - 2) + ")";

                                mySql.ExecuteNonQuery(insertStatementFINAL);
                            }
                        }
                    }




                }  // using


                // TESTING BELOW ======================================================================================
                // SCB TODO: Replace with actual table names.
                //using (OracleDataReader dr = oracleDAL.ExecuteReader("SELECT EmpID, FirstName, LastName FROM Employee"))
                //{ 
                //    if (dr.HasRows)
                //    {
                //        while (dr.Read())
                //        {
                //            mySql.ExecuteNonQuery("INSERT INTO EmployeeTable (LegacyId, FirstName, LastName) " +
                //                "VALUES (" + dr[0] + ", '" + dr[1].ToString() + "', '" + dr[2].ToString() + "')");

                //            iTableRowCount += 1;


                //            //dicts.AddToArrestTypesDictionary(1, "");

                //        }
                //    }
                //}
                //logging.WriteEvent(iTableRowCount + " rows inserted into the EmployeeTable table.");
                //logging.WriteReportEntry("Employee table complete.", iTableRowCount + " rows inserted into the EmployeeTable table.", "");


                // ==== And finally  =============================================================================

                mySql.Close();

                return true;
            }
            catch (System.Exception ex)
            {
                logging.WriteEvent("Error in ETL_Stage_1. ERROR: " + ex.Message);
                return false;
            }
        }
        #endregion
    }
}
