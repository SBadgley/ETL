using System;
using System.IO;
using DataAccessLayer_NET_Framework_;

using ETL;

namespace ETL._2___Helpers
{
    public class Logging 
    {
        public void WriteEvent(string InformationToLog)
        // string LoggingFileName, string strLogItem, int LoggingLevel = 2, string PartialLogName = "(ETL)"
        {
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ETL_Debugging.txt", InformationToLog);
            //WriteToListBox(InformationToLog);

            //string TextToLog = DateTime.Now + ": " + PartialLogName + " - " + strLogItem + Environment.NewLine;
            //switch (LoggingLevel)
            //{
            //    case 0:
            //        break;
            //    case 1:
            //        File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "", InformationToLog);
            //        break;
            //    case 2:
            //        File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "", InformationToLog);
            //        break;
            //    default:
            //        break;
            //}
        }

        public void WriteReportEntry(string Summary, string Details, string Notes)
        {
            //File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ETL_Report.txt", Details);

            //SQLServerDAL sqlServerDAL = new SQLServerDAL("");
            MySqlDAL mySqlDAL = new MySqlDAL("")
            {
                LoggingLevel = 2,
                UserID = "sbadgley",
                Password = "2010Camaro!ZL1UPED",
                DatabaseName = "migration"
            };

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
