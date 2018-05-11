using System;
using System.Data;

using System.Configuration;

using ETL._2___Helpers;
using MySql.Data.MySqlClient;

using ETL;

namespace DataAccessLayer_NET_Framework_
{
    public class MySqlDAL
    {
        Logging logging = new Logging();

        public static MySqlDAL mySqlDAL = null;
        private MySqlConnection mySqlConn = new MySqlConnection();
        public static string mySqlConnectionString = "";

        public MySqlConnection MySqlConn { get => mySqlConn; set => mySqlConn = value; }

        public MySqlDAL(string connectionString = "")
        {
            mySqlConnectionString = ConfigurationManager.AppSettings["MySql_FullConnString"];
            MySqlConn = new MySqlConnection(mySqlConnectionString);
        }

        public static MySqlDAL GetInstance()
        {
            if (mySqlDAL == null)
            {
                mySqlDAL = new MySqlDAL(); 
            }
            return mySqlDAL;
        }

        public bool OpenConnection()
        {
            if (MySqlConn.ConnectionString.ToString() == "")
                return false;

            if (MySqlConn == null) 
            {
                mySqlConnectionString = ConfigurationManager.AppSettings["MySql_FullConnString"];
                MySqlConn = new MySqlConnection(mySqlConnectionString);
                MySqlConn.Open();
            }
            else if (MySqlConn.State == ConnectionState.Closed)
            {
                MySqlConn.Open();
            }
            return true;
        }

        public bool ExecuteNonQuery(string executeString)
        {
            if (OpenConnection())
            {
                MySqlCommand mySqlComm = new MySqlCommand
                {
                    CommandType = CommandType.Text,
                    CommandText = executeString,
                    Connection = MySqlConn
                };
                try
                {
                    mySqlComm.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    logging.WriteEvent("Error in ExecuteNonQuery. " + ex.Message);
                    return false;
                }
            }
            else
            {
                logging.WriteEvent("Error in ExecuteNonQuery. Could not connect to database.");
            }
        }

        public MySqlDataReader ExecuteDataReader(string executeString)
        {
            if (OpenConnection())
            {
                MySqlCommand mySqlComm = new MySqlCommand
                {
                    CommandType = CommandType.Text,
                    CommandText = executeString,
                    Connection = MySqlConn
                };

                try
                {
                    MySqlDataReader dr = mySqlComm.ExecuteReader();
                    return dr;
                }
                catch (Exception ex)
                {
                    logging.WriteEvent("Error in ExecuteDataReader. " + ex.Message);
                    return null;
                }
            }
            else
            {
                logging.WriteEvent("Error in ExecuteDataReader. Could not connect to database.");
                return null;
            }
        }

        public void Close()
        {
            MySqlConn.Close();
        }
    }
}
