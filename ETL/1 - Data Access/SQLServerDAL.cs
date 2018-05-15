using System;
using System.Data;
using System.Data.SqlClient;

using ETL._2___Helpers;  // Logging

namespace DataAccessLayer_NET_Framework_
{
    public class SQLServerDAL
    {
        const string LoggingFileName = "\\OracleDALLog.txt";
        Logging logging = new Logging();

        public static SQLServerDAL objSqlServerDAL = null;
        public SqlConnection objSqlConnection = null;

        public string ConnectionStringProperty { get; set; } = "";
        public string SqlCommandProperty { get; set; } = "";
        public int CommandTimeoutOverride { get; set; } = 30;
        public int AppEnvironment { get; set; } = 1;

        public SQLServerDAL(string connectionString)
        {
            ConnectionStringProperty = connectionString;
            //SqlConnection objSqlConnection = new SqlConnection(ConnectionStringProperty);
            //if (objSqlConnection.State == ConnectionState.Closed)
            //{
            //    objSqlConnection.Open();
            //}
        }

        public static SQLServerDAL GetInstance(string DB)
        {
            if (objSqlServerDAL == null)
            {
                objSqlServerDAL = new SQLServerDAL(DB);
            }
            return objSqlServerDAL;
        }

        public void OpenConnection()
        {
            try
            {
                SqlConnection objSqlConnection = new SqlConnection(ConnectionStringProperty);
                if (objSqlConnection.State == ConnectionState.Open)
                {
                    objSqlConnection.Close();
                    objSqlConnection.Open();
                }
                else if (objSqlConnection.State == ConnectionState.Closed)
                {
                    objSqlConnection.Open();
                }
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in OpenConnection. " + ex.Message);
                throw ex;
            }
        }

        public void Dispose()
        {
            try
            {
                if (objSqlConnection.State != ConnectionState.Closed)
                {
                    this.objSqlConnection.Close();
                    this.objSqlConnection.Dispose();
                }
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in Dispose. " + ex.Message);
                throw ex;
            }
        }

        public int ExecuteNonQuery(string executeStatement = "")
        {
            OpenConnection();

            if (SqlCommandProperty != "")
            {
                executeStatement = SqlCommandProperty;
                SqlCommandProperty = "";
            }

            SqlCommand objSqlCommand = new SqlCommand(executeStatement, objSqlConnection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = CommandTimeoutOverride
            };

            //SqlTransaction sqlTransaction = null;

            try
            {
                int i = objSqlCommand.ExecuteNonQuery();
                // sqlTransaction.Commit();
                logging.WriteEvent("ExecuteNonQuery called. Statement = " + executeStatement);
                return i;
            }
            catch (Exception ex)
            {
                //sqlTransaction.Rollback();

                string errorStr = ex.Message;
                logging.WriteEvent("Error in ExecuteNonQuery. " + ex.Message);
                return 0;
            }
        }

        public SqlDataReader ExecuteReader(string executeStatement = "")
        {
            OpenConnection();

            if (SqlCommandProperty != "")
            {
                executeStatement = SqlCommandProperty;
                SqlCommandProperty = "";
            }

            SqlCommand objSqlCommand = new SqlCommand(executeStatement, objSqlConnection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = CommandTimeoutOverride
            };
            try
            {
                SqlDataReader dr = objSqlCommand.ExecuteReader();
                logging.WriteEvent("ExecuteReader called.Statement = " + executeStatement);
                return dr;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ExecuteReader. " + ex.Message);
                return null;
            }
        }

        public bool ExecuteStoredProcedure(string StoredProcedureName, SqlParameter[] Parameters = null)
        {
            try
            {
                OpenConnection();

                SqlCommand objSqlCommand = new SqlCommand()
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = CommandTimeoutOverride
                };
                objSqlCommand.Connection = objSqlConnection;
                objSqlCommand.CommandText = StoredProcedureName;
                objSqlCommand.CommandType = CommandType.StoredProcedure;
                if (Parameters != null)
                {
                    foreach (var parameter in Parameters)
                    {
                        objSqlCommand.Parameters.Add(parameter);
                    }
                }
                objSqlCommand.ExecuteNonQuery();
                logging.WriteEvent("ExecuteStoredProcedure called. Procedure called = " + StoredProcedureName);
                return true;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in ExecuteStoredProcedure. " + ex.Message);
                return false;
            }
        }

        public DataSet ExecuteGetDataSet(string executeStatement = "")
        {
            OpenConnection();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();

            if (SqlCommandProperty != "")
            {
                executeStatement = SqlCommandProperty;
                SqlCommandProperty = "";
            }

            try
            {
                SqlDataAdapter sda = new SqlDataAdapter(executeStatement, objSqlConnection);
                sda.Fill(ds);
                logging.WriteEvent("ExecuteGetDataSet called. Statement = " + executeStatement);
                return ds;

            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in GetDataSet. " + ex.Message);
                return null;
            }
        }

        public int ExecuteScalar(string executeStatement = "")
        {
            OpenConnection();

            if (SqlCommandProperty != "")
            {
                executeStatement = SqlCommandProperty;
                SqlCommandProperty = "";
            }
            SqlCommand objSqlCommand = new SqlCommand(executeStatement, objSqlConnection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = CommandTimeoutOverride
            };
            objSqlCommand.CommandText = executeStatement;

            try
            {
                object scalarObj;
                int scalarInt = -1;
                scalarObj = objSqlCommand.ExecuteScalar();
                scalarInt = (int)scalarObj; // If this fails, see MySqlDAL for Convert.Int... statement
                logging.WriteEvent("ExecuteScalar called. Statement = " + executeStatement);
                return scalarInt;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error in GetScalar. " + ex.Message);
                return -1;
            }
        }
    }
}
