using System;
using System.Windows.Forms;
using DataAccessLayer_NET_Framework_;
using ETL._2___Helpers;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Collections.Generic;

namespace ETL
{
    public partial class ETLController : Form
    {
        #region Variables
        Logging logging = new Logging();
        public string mySqlConnString = "";
        public string oracleConnString = "";
        #endregion
        #region Class Initialization
        public ETLController()
        {
            InitializeComponent();

            try
            {
                txtMySqlConnString.Text = ConfigurationManager.AppSettings["MySql_FullConnString"];
                mySqlConnString = txtMySqlConnString.Text;
                txtOracleConnString.Text = ConfigurationManager.AppSettings["OracleConnectionString"];
                oracleConnString = txtOracleConnString.Text;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error retrieving a connection string. " + ex.Message);
                txtMySqlConnString.Text = "Error";
                txtOracleConnString.Text = "Error";
            }
        }
        #endregion

        private void btnRUNMigration_Click(object sender, EventArgs e)
        {
            logging.WriteEvent("==========  CONVERSION COMMENCED ==========");
            logging.WriteReportEntry("==========  CONVERSION COMMENCED ==========", "==========  CONVERSION COMMENCED ==========", "");

            if (FormSetupSeemsCompleted() == false)
            {
                return;
            }

            #region Data Access Connections

            logging.WriteEvent("Oracle Connection String used:" + oracleConnString);
            logging.WriteEvent("MySql Connection String used:" + mySqlConnString);

            OracleDAL oracleDAL = null;
            MySqlDAL mySqlDAL = null;

            try
            {
                OracleDAL tempOracleDAL = new OracleDAL(oracleConnString);
                oracleDAL = tempOracleDAL;
            }
            catch (Exception ex)
            {
                // SCB TODO: Revisit Oracle connection once we have DB, and put this code back:
                //logging.WriteEvent("Error creating Oracle DAL. Error: " + ex.Message);
                //MessageBox.Show("Error creating Oracle DAL. Error: " + ex.Message);
                //return;
            }

            try
            {
                MySqlDAL tempMySqlDAL = new MySqlDAL();
                mySqlDAL = tempMySqlDAL;
            }
            catch (Exception ex)
            {
                logging.WriteEvent("Error creating MySql DAL. Error: " + ex.Message);
                MessageBox.Show("Error creating MySql DAL. Error: " + ex.Message);
                return;
            }

            #endregion

            #region ETL Processes

            // At this point in the routine, form completeness has been checked as best as can be AND the DB connections have been successful.
            ETL_Proccessing etlProcessing = new ETL_Proccessing();

            if (!etlProcessing.BuildMasterCodeDataView(txtDataDictionaryFilePath.Text))
            {
                logging.WriteEvent("Could not build MasterCode view. Check Excel file and make sure it is not opened.");
            }


            if (chkAttributes.Checked)
            { 
                etlProcessing.ETL_10_Atrributes(oracleDAL, mySqlDAL);
            }

            if (chkOffenseCodes.Checked)
            { 
                etlProcessing.ETL_20_OffenseCodes(oracleDAL, mySqlDAL, txtOffenseExcelFile.Text);
            }
            // ETL_30_Offenses ??
            if (chkUsers.Checked)
            {
                etlProcessing.ETL_40_Users(oracleDAL, mySqlDAL);
            }

            if (chkLocations.Checked)
            {
                etlProcessing.ETL_50_Locations(oracleDAL, mySqlDAL);
            }

            if (chkNames.Checked)
            {
                etlProcessing.ETL_60_Names(oracleDAL, mySqlDAL);
            }

            // Etc...

            logging.WriteEvent("==========  CONVERSION COMPLETED ==========");
            logging.WriteReportEntry("==========  CONVERSION COMPLETED ==========", "==========  CONVERSION COMPLETED ==========", "");

            #endregion
        }

        #region Helper routines
        /// <summary>
        /// Check form for completeness.
        /// </summary>
        /// <returns>true if all form checks pass, otherwise show reason and return false.</returns>
        private bool FormSetupSeemsCompleted()
        {
            if (chkOffenseCodes.Checked & txtOffenseExcelFile.Text == "")
            {
                MessageBox.Show("Select an Offense Code Excel file.");
                return false;
            }
            if (txtOracleConnString.Text == "" || txtMySqlConnString.Text == "")
            {
                MessageBox.Show("One or both of the connection strings could not be found. Check app.config file.");
                return false;
            }
            if (txtDataDictionaryFilePath.Text == "")
            {
                MessageBox.Show("Select the Data Dictionary Excel file.");
                return false;
            }
            return true;
        }
        #endregion
        #region Click Events
        private void btnSelectOffenseExcel_Click(object sender, EventArgs e)
        {
            openOffenseExcelFile.ShowDialog();
            txtOffenseExcelFile.Text = openOffenseExcelFile.FileName;
        }

        private void btnSelectDataDictionaryFile_Click(object sender, EventArgs e)
        {
            openOffenseExcelFile.ShowDialog();
            txtDataDictionaryFilePath.Text = openOffenseExcelFile.FileName;
        }
        #endregion
    }
}
