using System;
using System.Windows.Forms;
using DataAccessLayer_NET_Framework_;
using ETL._2___Helpers;
using System.Configuration;

using System.Collections.Generic;

namespace ETL
{
    public partial class ETLController : Form
    {
        #region Variables
        Logging logging = new Logging();
        public string mySqlConnString = "";
        public string oracleConnString = "";
        public List<string> listOfMigrationTables = new List<string>();
        public List<string> listOfAgencies = new List<string>();

        int iInsertedRows = 0;
        int iErroredRows = 0;
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
            #region Initilize

            ListBoxInfo.Items.Add("==========  CONVERSION COMMENCED ==========");
            logging.WriteEvent("==========  CONVERSION COMMENCED ==========");
            logging.WriteReportEntry("==========  CONVERSION COMMENCED ==========", "==========  CONVERSION COMMENCED ==========", "");

            BuildTableList();
            #endregion

            #region Sanity Check
            if (FormSetupSeemsCompleted() == false)
            {  // Routine will alert user to any problems, so don't need to do here too.
                return;
            }

            if (TablesAreClear() == false)
            {
                DialogResult response = MessageBox.Show("Existing data found. Continue with data conversion?", "Attention!", MessageBoxButtons.YesNo);
                if (response != DialogResult.Yes)
                {
                    ListBoxInfo.Items.Add("Existing data found, user aborted.");
                    return;
                }
                ListBoxInfo.Items.Add("Existing data found, user continued.");
            }
            #endregion

            #region Data Access Connections

            logging.WriteEvent("Oracle Connection String used:" + oracleConnString);
            logging.WriteEvent("MySql Connection String used:" + mySqlConnString);

            OracleDAL oracleDAL = null;
            MySqlDAL mySqlDAL = null;

            try
            {
                oracleDAL = new OracleDAL(oracleConnString);
            }
            catch (Exception)  // ex
            {
                // SCB TODO: Revisit Oracle connection once we have DB, and put the MessageBox back:
                //ListBoxInfo.Items.Add("Error creating Oracle DAL. Error: " + ex.Message);
                //logging.WriteEvent("Error creating Oracle DAL. Error: " + ex.Message);
                //MessageBox.Show("Error creating Oracle DAL. Error: " + ex.Message);
                //return;
            }

            try
            {
                mySqlDAL = new MySqlDAL(mySqlConnString);
            }
            catch (Exception ex)
            {
                ListBoxInfo.Items.Add("Error creating MySql DAL. Error: " + ex.Message);
                logging.WriteEvent("Error creating MySql DAL. Error: " + ex.Message);
                MessageBox.Show("Error creating MySql DAL. Error: " + ex.Message);
                return;
            }

            ListBoxInfo.Items.Add("Connections to Databases successful.");

            #endregion

            // At this point, form completeness has been checked as best as can be AND the DB connections have been successful,
            // procees with the ETL process.
            ETL_Proccessing etlProcessing = new ETL_Proccessing();

            #region Populate lookups
            ListBoxInfo.Items.Add("Building Master Codes DataView for lookups...");

            if (!etlProcessing.BuildMasterCodeDataView(txtDataDictionaryFilePath.Text, mySqlDAL))
            {
                logging.WriteEvent("Could not build MasterCode view. Check that the Excel file exists and that it is not opened.");
                ListBoxInfo.Items.Add("Could not build MasterCode view. Check that the Excel file exists and that it is not opened.");
                return;
            }

            ListBoxInfo.Items.Add("Building Reference DataViews for lookups...");

            if (!etlProcessing.BuildReferenceDataViews(mySqlDAL))
            {
                logging.WriteEvent("Could not build Reference Table views.");
                ListBoxInfo.Items.Add("Could not build Reference Table views.");
                return;
            }
            listOfAgencies.Clear();
            if (ChkLBAgencies.CheckedItems.Count != 0)
            {
                for (int x = 0; x <= ChkLBAgencies.CheckedItems.Count - 1; x++)
                {
                    listOfAgencies.Add(ChkLBAgencies.CheckedItems[x].ToString());
                }
            }
            #endregion

            #region ETL Processes

            if (chkAttributes.Checked)
            {
                ListBoxInfo.Items.Add("Processing Attributes...");
                ListBoxInfo.Refresh();

                if (!etlProcessing.ETL_10_Atrributes(oracleDAL, mySqlDAL, listOfAgencies, out iInsertedRows, out iErroredRows))
                {
                    DialogResult response = MessageBox.Show("An error occurred during Attribute data loading. Continue?", "Attention", MessageBoxButtons.YesNo);
                    if (response == DialogResult.No)
                    {
                        logging.WriteEvent("Error occurred during Attribute data loading, user aborted.");
                        return;
                    }
                }
                if (iErroredRows == 0)
                {
                    ListBoxInfo.Items.Add("Attributes completed. " + iInsertedRows + " rows inserted.");
                }
                else
                {
                    ListBoxInfo.Items.Add("Attributes completed. " + iInsertedRows + " rows inserted, " + iErroredRows + " rows had errors.");
                }
            }
            if (chkOffenseCodes.Checked)
            {
                ListBoxInfo.Items.Add("Processing Offense Codes...");
                ListBoxInfo.Refresh();

                if (!etlProcessing.ETL_20_OffenseCodes(oracleDAL, mySqlDAL, listOfAgencies, txtOffenseExcelFile.Text, out iInsertedRows, out iErroredRows))
                {
                    DialogResult response = MessageBox.Show("An error occurred during Offense Codes data loading. Continue?", "Attention", MessageBoxButtons.YesNo);
                    if (response == DialogResult.No)
                    {
                        logging.WriteEvent("Error occurred during Offense Codes data loading, user aborted.");
                        return;
                    }
                }
                if (iErroredRows == 0)
                {
                    ListBoxInfo.Items.Add("Offense Codes completed. " + iInsertedRows + " rows inserted.");
                }
                else
                {
                    ListBoxInfo.Items.Add("Offense Codes completed. " + iInsertedRows + " rows inserted, " + iErroredRows + " rows had errors.");
                }
            }
            if (chkUsers.Checked)
            {
                ListBoxInfo.Items.Add("Processing Users...");
                // SCB TODO: Do the same with the other routines (out counts)? I think so, given time.
                if (!etlProcessing.ETL_40_Users(oracleDAL, mySqlDAL, out iInsertedRows, out iErroredRows))
                {
                    // SCB TODO: Do the same with the other routines (MessageBox)?  Errors are logged to file but not readily appearant.
                    DialogResult response = MessageBox.Show("An error occurred during User data loading. Continue?", "Attention", MessageBoxButtons.YesNo);
                    if (response == DialogResult.No)
                    {
                        logging.WriteEvent("Error occurred during User data loading, user aborted.");
                        return;
                    }
                }
                if (iErroredRows == 0)
                {
                    ListBoxInfo.Items.Add("Users completed. " + iInsertedRows + " rows inserted.");
                }
                else
                {
                    ListBoxInfo.Items.Add("Users completed. " + iInsertedRows + " rows inserted, " + iErroredRows + " rows had errors.");
                }
            }
            if (chkLocations.Checked)
            {
                ListBoxInfo.Items.Add("Processing Locations...");
                etlProcessing.ETL_50_Locations(oracleDAL, mySqlDAL);
            }
            if (chkNames.Checked)
            {
                ListBoxInfo.Items.Add("Processing Names...");
                etlProcessing.ETL_60_Names(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Name Report Links...");
                etlProcessing.ETL_60_Name_Report_Links(oracleDAL, mySqlDAL);
            }
            if (chkReports.Checked)
            {
                ListBoxInfo.Items.Add("Processing Reports...");
                etlProcessing.ETL_70_Reports(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Reports_Arrests...");
                etlProcessing.ETL_70_1_Reports_Arrests(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Reports_Charges...");
                etlProcessing.ETL_70_2_Reports_Charges(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Reports_Offenses...");
                etlProcessing.ETL_70_3_Reports_Offenses(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Reports_FieldContacts...");
                etlProcessing.ETL_70_4_Reports_FieldContacts(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Reports_MissingPersons...");
                etlProcessing.ETL_70_5_Reports_MissingPersons(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Reports_Impounds...");
                etlProcessing.ETL_70_6_Reports_Impounds(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Reports_AdditionalInfo...");
                etlProcessing.ETL_70_7_Reports_Additional_Information(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Reports_CitationCharges...");
                etlProcessing.ETL_70_8_Reports_Citation_Charges(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Reports_TrafficCrashes...");
                etlProcessing.ETL_70_9_Reports_Traffic_Crash(oracleDAL, mySqlDAL);
            }
            if (chkItems.Checked)
            {
                ListBoxInfo.Items.Add("Processing Items...");
                etlProcessing.ETL_80_Items(oracleDAL, mySqlDAL);
            }
            if (chkEvidence.Checked)
            {
                ListBoxInfo.Items.Add("Processing Evidence Items...");
                etlProcessing.ETL_90_1_Evidence_Items(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Evidence Chain Items...");
                etlProcessing.ETL_90_2_Evidence_Chain(oracleDAL, mySqlDAL);
            }
            if (chkCases.Checked)
            {
                ListBoxInfo.Items.Add("Processing Cases...");
                etlProcessing.ETL_110_Cases(oracleDAL, mySqlDAL);
                ListBoxInfo.Items.Add("Processing Case Notes...");
                etlProcessing.ETL_110_Case_Notes(oracleDAL, mySqlDAL);
            }
            if (chkLegacyAttachments.Checked)
            {
                ListBoxInfo.Items.Add("Processing Attachments...");
                etlProcessing.ETL_120_Attachments(oracleDAL, mySqlDAL);
            }
            // Etc...

            try
            {
                oracleDAL.CloseConnection();
            }
            catch (Exception)
            {
            } 
            mySqlDAL.Close();

            ListBoxInfo.Items.Add("==========  CONVERSION COMPLETED ==========");
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

            if (ChkLBAgencies.CheckedItems.Count == 0 & chkAttributes.Checked) 
            {
                MessageBox.Show("Attributes is checked, but no Agencies are checked.");
                return false;
            }
            return true;
        }
        private bool TablesAreClear()
        {
            string selectStatement = "";
            int tableCount = 0;
            MySqlDAL mySqlDAL = new MySqlDAL(mySqlConnString);

            foreach (string s in listOfMigrationTables)
            {
                selectStatement = "SELECT COUNT(*) FROM migration." + s;
                tableCount = mySqlDAL.ExecuteScalar(selectStatement);
                if (tableCount > 0)
                {
                    ListBoxInfo.Items.Add("POSSIBLE Problem - Table " + s.ToString() + " has data.");
                }

            }
            return true;
        }

        private bool ClearTables()
        {
            string sqlStatement = "";
            bool nonQueryExecuted = false;
            bool allExecuted = true;
            MySqlDAL mySqlDAL = new MySqlDAL(mySqlConnString);

            foreach (string s in listOfMigrationTables)
            {
                sqlStatement = "DELETE FROM migration." + s;
                nonQueryExecuted = mySqlDAL.ExecuteNonQuery(sqlStatement);
                if (!nonQueryExecuted)
                {
                    allExecuted = false;
                    ListBoxInfo.Items.Add("Failed to clear table " + s.ToString());
                }
            }
            return allExecuted;
        }

        private void ClearETLTables()
        {
            MySqlDAL mySqlDAL = new MySqlDAL(mySqlConnString);
            bool nonQueryExecuted = false;
            string sqlStatement = "DELETE FROM migration.ETL_Results";
            nonQueryExecuted = mySqlDAL.ExecuteNonQuery(sqlStatement);
            if (nonQueryExecuted)
                ListBoxInfo.Items.Add("ETL_Results table cleared.");
            else
                ListBoxInfo.Items.Add("Failed to clear table ETL_Results.");

            nonQueryExecuted = false;
            sqlStatement = "DELETE FROM etl_attributes";
            nonQueryExecuted = mySqlDAL.ExecuteNonQuery(sqlStatement);
            if (nonQueryExecuted)
                ListBoxInfo.Items.Add("etl_attributes table cleared.");
            else
                ListBoxInfo.Items.Add("Failed to clear table etl_attributes.");
        }

        private void BuildTableList()
        {
            // Used to 1) check if there is any data in these tables and 2) to clear ETL Staging tables.

            listOfMigrationTables.Add("migration_attributes"); 
            listOfMigrationTables.Add("migration_offense_codes");
            listOfMigrationTables.Add("migration_users");
            listOfMigrationTables.Add("migration_locations");
            listOfMigrationTables.Add("migration_names");
            listOfMigrationTables.Add("migration_name_report_links");
            listOfMigrationTables.Add("migration_reports");
            listOfMigrationTables.Add("migration_arrests");
            listOfMigrationTables.Add("migration_charges");
            listOfMigrationTables.Add("migration_offenses"); 
            listOfMigrationTables.Add("migration_field_contacts");
            listOfMigrationTables.Add("migration_missing_persons");
            listOfMigrationTables.Add("migration_report_impounds");
            listOfMigrationTables.Add("migration_additional_information");
            listOfMigrationTables.Add("migration_citation_charges");
            listOfMigrationTables.Add("migration_traffic_crash");
            listOfMigrationTables.Add("migration_items");
            listOfMigrationTables.Add("migration_evidence_items");
            listOfMigrationTables.Add("migration_evidence_chain_events");
            listOfMigrationTables.Add("migration_cases");
            listOfMigrationTables.Add("migration_case_notes");
            listOfMigrationTables.Add("migration_attachments");
            listOfMigrationTables.Add("migration_additional_information");
            //listOfMigrationTables.Add("migration_legacy_details"); // Not used yet
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
        private void btnClearTables_Click(object sender, EventArgs e)
        {
            if (ClearTables())
            {
                MessageBox.Show("Tables have been cleared.");
            }
            else
            {
                MessageBox.Show("One or more tables WAS NOT cleared.");
            }
        }
        private void btnClearETLTable_Click(object sender, EventArgs e)
        {
            ClearETLTables();
        }
        #endregion

    }
}
