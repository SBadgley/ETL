using System;
using System.IO;
using DataAccessLayer_NET_Framework_;

//using ETL;

namespace ETL._2___Helpers
{
    public class Logging 
    {
        public void WriteEvent(string InformationToLog)
        {
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ETL_Debugging.txt", InformationToLog);
        }
        public void WriteReportEntry(string Summary, string Details, string Notes)
        {
            MySqlDAL mySqlDAL = new MySqlDAL("");

            mySqlDAL.ExecuteNonQuery("INSERT INTO ETL_Results (DateTime, ResultSummary, ResultDetail, Notes) " + 
                "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Summary + "', '" + Details + "', '" + Notes + "')");
        }
    }
}
