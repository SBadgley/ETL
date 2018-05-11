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
        const string LoggingFileName = "\\OracleDALLog.txt";
        Logging logging = new Logging();

        public static OracleDAL objOracleDAL = null;
        public OracleConnection objOracleConnection = null;

        //public string ConnectionStringProperty { get; set; } = "";
        public string SqlCommandProperty { get; set; } = "";
        public int CommandTimeoutOverride { get; set; } = 30;
        //public int AppEnvironment { get; set; } = 1;
        //public int LoggingLevel { get; set; } = 1;

        public enum EnumLoggingLevel { NoLogging = 0, MinimalLogging = 1, FullLogging = 2 };

        public OracleDAL(string connectionString)
        {
            ////ConnectionStringProperty = ConfigurationManager.AppSettings.Get("OracleSourceConnString");
            //LoggingLevel = (int)EnumLoggingLevel.FullLogging;

            //ConnectionStringProperty = ConfigurationManager.AppSettings["OracleConnectionString"];

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

        //public static OracleDAL GetInstance(string DB)  // Used??
        //{
        //    if (objOracleDAL == null)
        //    {
        //        OracleDAL objOracleDAL = new OracleDAL(DB);
        //    }
        //    return objOracleDAL;
        //}

        //public void OpenConnection() // Not currently used.
        //{
        //    if (objOracleConnection.State != ConnectionState.Open)
        //    {
        //        objOracleConnection.Open();
        //    }
        //}
        //public void CloseConnection() // Not currently used.
        //{
        //    if (objOracleConnection.State == ConnectionState.Open)
        //    {
        //        objOracleConnection.Close();
        //    }
        //}


        public OracleDataReader ExecuteReader(string executeStatement = "")
        {
            //OpenConnection();

            if (SqlCommandProperty != "")
            {
                executeStatement = SqlCommandProperty;
                SqlCommandProperty = "";
            }

            OracleCommand objSqlCommand = new OracleCommand(executeStatement, objOracleConnection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = CommandTimeoutOverride
            };
            try
            {
                OracleDataReader dr = objSqlCommand.ExecuteReader();
                logging.WriteEvent("ExecuteReader called. Statement = " + executeStatement);
                return dr;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ExecuteReader. " + ex.Message);
                return null;
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
    }
}
