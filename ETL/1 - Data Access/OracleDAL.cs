using System;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;  
using Oracle.ManagedDataAccess.Types;
using System.Data;  // CommandType
using ETL._2___Helpers; // Logging

namespace DataAccessLayer_NET_Framework_
{
    public class OracleDAL
    {
        #region Variables
        const string LoggingFileName = "\\OracleDALLog.txt";
        Logging logging = new Logging();

        public static OracleDAL objOracleDAL = null;
        public OracleConnection objOracleConnection = null;

        public string SqlCommandProperty { get; set; } = "";
        public int CommandTimeoutOverride { get; set; } = 30;
        public enum EnumLoggingLevel { NoLogging = 0, MinimalLogging = 1, FullLogging = 2 };
        #endregion
        public OracleDAL(string connectionString)
        {
            OracleConnection objOracleConnection = new OracleConnection(connectionString);
            if (OracleConnection.IsAvailable)  
            {
                objOracleConnection.Open();
            }
            else
            {
                logging.WriteEvent("Oracle connection is not available.");
            }
        }
        public OracleDataReader ExecuteReader(string executeStatement = "", bool logCount = false)
        {
            if (SqlCommandProperty != "")
            {
                executeStatement = SqlCommandProperty;  // Allow over-ride
                SqlCommandProperty = "";  // Clear it out so it's not used again
            }
            OracleCommand objSqlCommand = new OracleCommand(executeStatement, objOracleConnection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = CommandTimeoutOverride
            };
            try
            {
                OracleDataReader dr = objSqlCommand.ExecuteReader();
                logging.WriteEvent("ExecuteReader called (Part 1). Statement = " + executeStatement);
                if (logCount)
                {
                    int numberOfRows = 0;
                    if (dr.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(dr);
                        numberOfRows = dt.Rows.Count;
                    }
                    logging.WriteEvent("ExecuteReader called (Part 2). Rows returned = " + numberOfRows);
                }
                logging.WriteEvent("----------------------------------------------------------");

                return dr;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ExecuteReader. " + ex.Message);
                return null;
            }
        }
        public int ExecuteScalar(string executeStatement = "")
        {
            if (SqlCommandProperty != "")
            {
                executeStatement = SqlCommandProperty;  // Allow over-ride
                SqlCommandProperty = "";  // Clear it out so it's not used again
            }
            OracleCommand oracleSqlCommand = new OracleCommand(executeStatement, objOracleConnection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = CommandTimeoutOverride
            };
            object oScalarValue = -1;
            int iScalarInt = -1;
            try
            {
                oScalarValue = oracleSqlCommand.ExecuteScalar();
                iScalarInt = Convert.ToInt32(oScalarValue);
                return iScalarInt;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ExecuteScalar. " + ex.Message);
                return -1;
            }
        }
        public void ExecuteStoredProcedure(string storedProcedure, OracleParameter[] param)
        {
            OracleCommand oraCmd = new OracleCommand();
            oraCmd.CommandType = CommandType.StoredProcedure;
            oraCmd.CommandText = storedProcedure;
            oraCmd.Connection = objOracleConnection;

            if (param != null)
            {
                oraCmd.Parameters.AddRange(param);
            }
            try
            {
                oraCmd.ExecuteNonQuery(); 
            }
            catch (Exception ex) 
            {
                logging.WriteEvent("Error in ExecuteStoredProcedure. " + ex.Message);
            }
        }
        public void CloseConnection()
        {
            try
            {
                objOracleConnection.Close();
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error closing Oracle connection. " + ex.Message);
            }
        }
    }
}
