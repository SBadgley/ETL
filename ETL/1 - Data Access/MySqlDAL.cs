using System;
using System.Data;

using System.Configuration;

using ETL._2___Helpers;
using MySql.Data.MySqlClient;

namespace DataAccessLayer_NET_Framework_
{
    public class MySqlDAL
    {
        Logging logging = new Logging();
        //const string LoggingFileName = "\\MySqlDALLog.txt";

        public static MySqlDAL mySqlDAL = null;
        private MySqlConnection mySqlConn = new MySqlConnection();

        public string DBServer { get => _dBServer; set => _dBServer = value; }
        public string DBPort { get => _dBPort; set => _dBPort = value; }
        public string DatabaseName { get => _databaseName; set => _databaseName = value; }
        public string UserID { get => _userID; set => _userID = value; }
        public string Password { get => _password; set => _password = value; }

        public MySqlConnection Connection { get => _connection; private set => _connection = value; }

        private static MySqlDAL _instance = null;
        private string _dBServer = "localhost";
        private string _dBPort = "3306";
        private string _databaseName = string.Empty;
        private string _userID;
        private string _password;

        private MySqlConnection _connection = null;


        public string ConnectionStringProperty { get; set; } = "";
        public string SqlCommandProperty { get; set; } = "";
        public int CommandTimeoutOverride { get; set; } = 30;
        public int AppEnvironment { get; set; } = 1;
        public int LoggingLevel { get; set; } = 1;
        public MySqlConnection MySqlConn { get => mySqlConn; set => mySqlConn = value; }

        public enum EnumLoggingLevel { NoLogging = 0, MinimalLogging = 1, FullLogging = 2 };

        //private MySqlDAL()
        //{
        //}

        public MySqlDAL(string connectionString = "")
        {
            // Working
            _dBServer = ConfigurationManager.AppSettings["MySQL_ServerName"];
            _databaseName = ConfigurationManager.AppSettings["MySql_Database"];
            //_dBPort = ConfigurationManager.AppSettings["MySql_Port"];
            _userID = ConfigurationManager.AppSettings["MySql_UserId"];
            _password = ConfigurationManager.AppSettings["MySql_Password"];

            System.Data.SqlClient.SqlConnectionStringBuilder MySqlConnString = new System.Data.SqlClient.SqlConnectionStringBuilder();
            MySqlConnString["Server"] = DBServer;
            MySqlConnString["database"] = DatabaseName;
            //MySqlConnString["port"] = DBPort;
            MySqlConnString["UID"] = UserID;
            MySqlConnString["password"] = Password;


            MySqlConn = new MySqlConnection(MySqlConnString.ToString());
            LoggingLevel = (int)EnumLoggingLevel.FullLogging;
        }

        //public MySqlDAL(string connectionString)
        //{
        //    MySqlConn = new MySqlConnection(connectionString);
        //    LoggingLevel = (int)EnumLoggingLevel.FullLogging;
        //}

        public static MySqlDAL GetInstance()  // string DB
        {
            if (mySqlDAL == null)
            {
                mySqlDAL = new MySqlDAL();  // DB
            }
            return mySqlDAL;
        }
        public static MySqlDAL Instance()
        {
            if (_instance == null)
                _instance = new MySqlDAL();
            return _instance;
        }

        public bool OpenConnection()
        {
            if (String.IsNullOrEmpty(DatabaseName) |  String.IsNullOrEmpty(UserID) | String.IsNullOrEmpty(Password))
                return false;

            if (MySqlConn == null)
            {
                //MySqlConnectionStringBuilder MySqlConnStrBuilder = new MySqlConnectionStringBuilder();
                //MySqlConnStrBuilder.ConnectionString = ("Server=localhost; database={0}; UID={1}; password={2}", DatabaseName, UserID, Password);

                string connstring = string.Format("Server={0}; database={1}; port={2}; UID={3}; password={4}",
                                                   DBServer, DatabaseName, DBPort, UserID, Password);
                mySqlConn.ConnectionString = connstring;
                MySqlConn.Open();
            }
            else if (MySqlConn.State == ConnectionState.Closed)
            {
                string connstring = string.Format("Server={0}; database={1}; port={2}; UID={3}; password={4}",
                                                   DBServer, DatabaseName, DBPort, UserID, Password);
                MySqlConn.ConnectionString = connstring;
                MySqlConn.Open();
            }

            return true;
        }

        public void ExecuteNonQuery(string executeString)
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
                }
                catch (Exception ex)
                {
                    logging.WriteEvent("Error in ExecuteNonQuery. " + ex.Message);
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
