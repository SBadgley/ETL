using System;
using System.Data;
using System.Configuration;
using ETL._2___Helpers;
using MySql.Data.MySqlClient;

namespace DataAccessLayer_NET_Framework_
{
    public class MySqlDAL
    {
        #region Variables
        Logging logging = new Logging();
        public static MySqlDAL mySqlDAL = null;
        private MySqlConnection mySqlConn = new MySqlConnection();
        public static string mySqlConnectionString = "";
        public MySqlConnection MySqlConn { get => mySqlConn; set => mySqlConn = value; }
        #endregion
        public MySqlDAL(string connectionString)
        {
            mySqlConnectionString = connectionString; //ConfigurationManager.AppSettings["MySql_FullConnString"];
            MySqlConn = new MySqlConnection(mySqlConnectionString);
        }
        public static MySqlDAL GetInstance()
        {
            if (mySqlDAL == null)
            {
                mySqlDAL = new MySqlDAL(mySqlConnectionString); 
            }
            return mySqlDAL;
        }
        public bool OpenConnection()
        {
            if (MySqlConn.ConnectionString.ToString() == "")
                return false;

            if (MySqlConn == null) 
            {
                //mySqlConnectionString =ConfigurationManager.AppSettings["MySql_FullConnString"];
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
                return false;
            }
        }
        public int ExecuteScalar(string executeString)
        {
            if (OpenConnection())
            {
                MySqlCommand mySqlCommand = new MySqlCommand
                {
                    CommandType = CommandType.Text,
                    CommandText = executeString,
                    Connection = MySqlConn
                };
                object oScalarValue = -1;
                int iScalarInt = -1;
                try
                {
                    oScalarValue = mySqlCommand.ExecuteScalar();
                    iScalarInt = Convert.ToInt32(oScalarValue); 
                    //logging.WriteEvent("ExecuteScalar called. Statement = " + executeString);
                    return iScalarInt;


                    //iScalarValue = (int)mySqlComm.ExecuteScalar();
                    //return iScalarValue;
                }
                catch (Exception ex)
                {
                    logging.WriteEvent("Error in ExecuteNonQuery. " + ex.Message);
                    return iScalarInt;
                }
            }
            else
            {
                logging.WriteEvent("Error in ExecuteNonQuery. Could not connect to database.");
                return - 1;
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

        //public string ExecuteLookup(string executeString)
        //{
        //    if (OpenConnection())
        //    {
        //        MySqlCommand mySqlComm = new MySqlCommand
        //        {
        //            CommandType = CommandType.Text,
        //            CommandText = executeString,
        //            Connection = MySqlConn
        //        };

        //        try
        //        {
        //            MySqlDataReader dr = mySqlComm.ExecuteReader();
        //            return dr[0].ToString();
        //        }
        //        catch (Exception ex)
        //        {
        //            logging.WriteEvent("Error in ExecuteLookup. " + ex.Message);
        //            return null;
        //        }
        //    }
        //    else
        //    {
        //        logging.WriteEvent("Error in ExecuteLookup. Could not connect to database.");
        //        return null;
        //    }
        //}


        public void Close()
        {
            MySqlConn.Close();
        }
    }
}
