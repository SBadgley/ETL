using System.Collections.Generic;
using MySql.Data.MySqlClient;

using DataAccessLayer_NET_Framework_;

namespace ETL._2___Helpers
{
    public class Dictionaries
    {
        Logging logging = new Logging();
        const string LogFileName = "\\Dictionaries.txt";

        public Dictionary<int, string> dictArrestTypes = new Dictionary<int, string>();
        public Dictionary<int, string> dictItemTypes = new Dictionary<int, string>();

        public void AddToArrestTypesDictionary(int keyInt, string valueString)
        {
            dictArrestTypes.Add(keyInt, valueString);
        }
        public string GetFromArrestTypesDictionary(int keyInt)
        {
            dictArrestTypes.TryGetValue(keyInt, out string valueOut);
            return valueOut;
        }

        public void AddToItemTypesDictionary(int keyInt, string valueString)
        {
            dictItemTypes.Add(keyInt, valueString);
        }
        public string GetFromItemTypesDictionary(int keyInt)
        {
            dictItemTypes.TryGetValue(keyInt, out string valueOut);
            return valueOut;
        }

        public bool BuildDictionaries()
        {
            // SCB TODO: Remove all this if not using.
            try
            {
                MySqlDAL mySqlDAL = new MySqlDAL("");

                Dictionaries dicts = new Dictionaries();

                MySqlDataReader dr = mySqlDAL.ExecuteDataReader("SELECT id, name, description FROM ref_migration_item_type");
                if (dr != null && dr.HasRows)
                {
                    while (dr.Read())
                    {
                        dicts.AddToItemTypesDictionary(dr.GetInt32(0), dr[1].ToString());
                    }
                }
                mySqlDAL.Close();

                dr = mySqlDAL.ExecuteDataReader("SELECT id, arrest_type FROM ref_migration_arrest_types");
                if (dr != null && dr.HasRows)
                {
                    while (dr.Read())
                    {
                        dicts.AddToArrestTypesDictionary(dr.GetInt32(0), dr[1].ToString());
                    }
                }
                mySqlDAL.Close();

                logging.WriteEvent("BuildDictionaries completed.");
                return true;
            }
            catch (System.Exception ex)
            {
                logging.WriteEvent("Error in BuildDictionaries: " + ex.Message);
                return false;
            }
        }
    }
}
