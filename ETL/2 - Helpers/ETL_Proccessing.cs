using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

using MySql.Data.MySqlClient;

using DataAccessLayer_NET_Framework_;

using System.Configuration;
using System;

using System.Data.OleDb;

namespace ETL._2___Helpers
{
    public class ETL_Proccessing
    {
        public bool ETL_Atrributes(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            Logging logging = new Logging();
            try
            {
                // Where to get the attributes??  MASTER_CODES.CODE_DESCRIPTION?? From spreadsheet, appears maybe so.  Need Oracle access to see.


                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Atrributes. " + ex.Message);
                return false;
            }
        }

        public bool ETL_OffenseCodes(OracleDAL oracleDAL, MySqlDAL mySqlDAL, string offenseCodeExcelPath)
        {
            // As of 5-9-2018 - I have an Excel spreadsheet with PRIORS Offense Codes. Is this the source? On form there is a text box for Excel file.
            Logging logging = new Logging();
            try
            {
                // SCB TODO: What is the 1) destination table, 2) fields, 3) description, 4) check other columns (Deactivated?), 5) more??
                // Table: ref_nibrs_offense_codes:
                // code char(3) not nullable
                // name varchar(128) not nullable
                // description varchar(512) not nullable

                string insertStatement = "";
                int iRows = 0;

                // txtOffenseExcelFile.Text

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

                            string Agency = dr[7].ToString(); // SHARED, SMP (Salem)

                            if (Agency.ToString().ToLower() == "shared" || Agency.ToString().ToLower() == "smp")
                            {
                                insertStatement = "INSERT INTO migration_offense_codes (code, name, description) " + 
                                    "VALUES ('" + Code + "', '" + Name + "', '" + Name + "')";

                                iRows += 1;
                                // SCB TODO: migration_offense_codes table doen't exist in my current DB/Schema. So insert commented out.
                                //mySqlDAL.ExecuteNonQuery(insertStatement);
                            }

                        }
                    }
                }
                mySqlDAL.ExecuteNonQuery("INSERT INTO etl_results(EventTime, ResultSummary, ResultDetail, Notes) " +
                    "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'Offecnse codes copied.', '" + iRows + " rows copied.', '')");


                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_OffenseCodes. " + ex.Message);
                return false;
            }
        }

        public bool ETL_Offenses(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            Logging logging = new Logging();
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



                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Offenses. " + ex.Message);
                return false;
            }
        }

        public bool ETL_Users(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            Logging logging = new Logging();
            try
            {
                // Oracle tables:
                // EMPLOYEE
                // MASTER_CODES

                string selectStatement = "SELECT emp.CREATE_DATE, emp.CDCREATE_OPERID, emp.UPDATED_DTAE, emp.CDOPERID, " +
                    "emp.SEQNUM, emp.FIRST_NAME, emp.SURNAME, emp.MIDDLE_NAME, emp.DOB, emp.SEX, mc.CODE_DESCRIPTION, " +
                    "emp.RANK, emp.RES_PHONE, emp.EMAIL, emp.BADGE_ID, emp.FOREIGN_SEQNUM, emp.EMPLOYEE_STATUS, emp.AGENCY, emp.RMS_LOCKED" +
                    " FROM EMPLOYEE emp LEFT JOIN MASTER_CODES mc ON UPPER(emp.RANK) = UPPER(mc.CODE_VALUE) AND UPPER(mc.TABLE_ID) = 'RANK'";

                string insertStatement = "INSERT INTO migration_users (source_created_date, source_created_by, source_updated_date, source_updated_by, " +
                    "source_user_id, first_name, last_name, middle_name, date_of_birth, sex_attr_code, rank_attr_value, rank_attr_code, " +
                    "phone_number, primary_email, badge_number, external_cad_id, duty_status_value, department_agency_name, is_disabled, " +
                    "created_date_utc, created_by, updated_date_utc, updated_by) VALUES (";

                // SCB TODO: Additional, non-nullable, migration table fields with no source. Using "source..." values for now, KEEP?:
                // created_date_utc
                // created_by  BigInt(20)
                // updated_date_utc
                // updated_by  BigInt(20)

                string insertValues = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        // MySQL retrieves and displays DATETIME values in 'YYYY-MM-DD HH:MM:SS' format

                        insertValues += "'" + FormatDateTimeForMySQL(dr["source_created_date"].ToString()) + "', ";
                        insertValues += "'" + dr["source_created_by"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["source_updated_date"].ToString()) + "', ";
                        insertValues += "'" + dr["source_updated_by"].ToString() + "', ";
                        insertValues += "'" + dr["source_user_id"].ToString() + "', ";  // varchar
                        insertValues += "'" + dr["first_name"].ToString() + "', ";
                        insertValues += "'" + dr["last_name"].ToString() + "', ";
                        insertValues += "'" + dr["middle_name"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["date_of_birth"].ToString()) + "', ";
                        insertValues += "'" + dr["sex_attr_code"].ToString() + "', ";
                        insertValues += "'" + dr["rank_attr_value"].ToString() + "', ";
                        insertValues += "'" + dr["rank_attr_code"].ToString() + "', ";
                        insertValues += "'" + dr["phone_number"].ToString() + "', ";
                        insertValues += "'" + dr["primary_email"].ToString() + "', ";
                        insertValues += "'" + dr["badge_number"].ToString() + "', ";
                        insertValues += "'" + dr["external_cad_id"].ToString() + "', ";
                        insertValues += "'" + dr["duty_status_value"].ToString() + "', ";
                        insertValues += "'" + dr["department_agency_name"].ToString() + "', ";

                        insertValues += "'" + FormatDateTimeForMySQL(dr["source_created_date"].ToString()) + "', ";
                        insertValues += "'" + dr["source_created_by"].ToString() + "', ";
                        insertValues += "'" + FormatDateTimeForMySQL(dr["source_updated_date"].ToString()) + "', ";
                        insertValues += "'" + dr["source_updated_by"].ToString() + "', ";

                        insertValues += dr["is_disabled"].ToString() + ")"; // tinyint - Last field, close with right parenthese..

                        insertStatement += insertValues;

                        mySqlDAL.ExecuteNonQuery(insertStatement);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Users. " + ex.Message);
                return false;
            }
        }

        public bool ETL_Locations(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            Logging logging = new Logging();
            try
            {
                // Oracle table:
                // INCIDENT

                string selectStatement = "SELECT COUNTY FROM INCIDENT";

                string insertStatement = "INSERT INTO migration_locations (created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_location_id, source_location_type, administrative_area_level_2) VALUES (";

                // non-nullable, migration table fields with no source:
                // created_date_utc
                // created_by  BigInt(20)
                // updated_date_utc
                // updated_by  BigInt(20)
                // source_location_id
                // source_location_type

                string insertValues = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        // MySQL retrieves and displays DATETIME values in 'YYYY-MM-DD HH:MM:SS' format

                        // SCB TODO: Finish up insertValues here:
                        insertValues += "'', ";

                        insertValues += dr["COUNTY"].ToString() + ")"; // Last field, close with right parenthese..

                        insertStatement += insertValues;

                        mySqlDAL.ExecuteNonQuery(insertStatement);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Locations. " + ex.Message);
                return false;
            }
        }

        public bool ETL_Names(OracleDAL oracleDAL, MySqlDAL mySqlDAL)
        {
            Logging logging = new Logging();
            try
            {
                // Oracle tables:
                // PERSON
                // MASTER_CODES

                string selectStatement = "SELECT * FROM PERSON";

                string insertStatement = "INSERT INTO migration_names (created_date_utc, created_by, updated_date_utc, updated_by, " +
                    "source_name_id, source_master_name_id, source_owner_id, source_owner_type, " + 
                    ") VALUES (";

                // non-nullable, migration table fields with no source:
                // created_date_utc
                // created_by  BigInt(20)
                // updated_date_utc
                // updated_by  BigInt(20)
                // source_name_id  VarChar(32)
                // source_master_name_id  BigInt(20)
                // source_owner_id
                // source_owner_type

                string insertValues = "";

                OracleDataReader dr = oracleDAL.ExecuteReader(selectStatement);
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        // MySQL retrieves and displays DATETIME values in 'YYYY-MM-DD HH:MM:SS' format

                        // SCB TODO: Finish up insertValues here:

                        insertValues += dr[""].ToString() + ")"; // Last field, close with right parenthese..

                        insertStatement += insertValues;

                        mySqlDAL.ExecuteNonQuery(insertStatement);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ETL_Names. " + ex.Message);
                return false;
            }
        }

        public DateTime FormatDateTimeForMySQL(string incomingDateTime)
        {
            return DateTime.Parse(incomingDateTime);
        }
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
                    LoggingLevel = 2,
                    UserID = "sbadgley",
                    Password = "2010Camaro!ZL1UPED",
                    DatabaseName = "migration"
                };


                MySqlDAL mySqlINSERT = new MySqlDAL() // mySqlConnString
                {
                    LoggingLevel = 2,
                    UserID = "sbadgley",
                    Password = "2010Camaro!ZL1UPED",
                    DatabaseName = "migration"
                };

                MySqlDAL mySql = new MySqlDAL() // mySqlConnString
                {
                    LoggingLevel = 2,
                    UserID = "sbadgley",
                    Password = "2010Camaro!ZL1UPED",
                    DatabaseName = "migration"
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
