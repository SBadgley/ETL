using System;
using System.IO;
using DataAccessLayer_NET_Framework_;

using ETL;

namespace ETL._2___Helpers
{
    public class Logging 
    {
        public void WriteEvent(string InformationToLog)
        {
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ETL_Debugging.txt", InformationToLog);
            //WriteToListBox(InformationToLog);
        }

        public void WriteReportEntry(string Summary, string Details, string Notes)
        {
            MySqlDAL mySqlDAL = new MySqlDAL("");

            mySqlDAL.ExecuteNonQuery("INSERT INTO ETL_Results (DateTime, ResultSummary, ResultDetail, Notes) " + 
                "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Summary + "', '" + Details + "', '" + Notes + "')");

            //mySqlDAL.ExecuteNonQuery("INSERT INTO ETL_Mapping..ETL_Results (DateTime, ResultSummary, ResultDetail, Notes) " +
            //    "VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + Summary + "', '" + Details + "', '" + Notes + "')");

        }

        public void WriteToListBox(string InformationToLog)
        {
            // SCB TODO:  Won't show...
            try
            {
                ETLController etl = new ETLController();
                etl.ListBoxInfo.Items.Add(DateTime.Now + ": " + InformationToLog);
                //etl.ListBoxInfo.Hide();
                //etl.ListBoxInfo.Show();
                //etl.ListBoxInfo.Refresh();
                //etl.ListBoxInfo.Parent.Refresh();
                //etl.ListBoxInfo.Hide();
                //etl.ListBoxInfo.Show();
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }
    }
}
