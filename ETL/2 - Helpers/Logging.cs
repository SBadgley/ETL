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
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ETL_Debugging.txt", 
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + InformationToLog + Environment.NewLine + Environment.NewLine);
        }
        public void WriteReportEntry(string Summary, string Details, string Notes)
        {
            MySqlDAL mySqlDAL = new MySqlDAL();

            mySqlDAL.ExecuteNonQuery("INSERT INTO ETL_Results (EventTime, ResultSummary, ResultDetail, Notes) " + 
                "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Summary + "', '" + Details + "', '" + Notes + "')");
        }
        public void WriteReportDataEntry(string DataEntity, int SuccessfulRows, int ErrorRows, string Notes = "")
        {
            MySqlDAL mySqlDAL = new MySqlDAL();

            if (ErrorRows == 0)
            { 
            mySqlDAL.ExecuteNonQuery("INSERT INTO ETL_Results (EventTime, ResultSummary, ResultDetail, Notes) " +
                "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + SuccessfulRows + " " + DataEntity + " rows copied.', '" + Notes + "')");
            }
            else
            {
                mySqlDAL.ExecuteNonQuery("INSERT INTO ETL_Results (EventTime, ResultSummary, ResultDetail, Notes) " +
                    "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + SuccessfulRows + " " + DataEntity + " rows copied.', '" + ErrorRows + " rows had errors.', '" + Notes + "')");
            }
        }
    }
}
