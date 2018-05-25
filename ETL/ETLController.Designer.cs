namespace ETL
{
    partial class ETLController
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ListBoxInfo = new System.Windows.Forms.ListBox();
            this.gbDataEntities = new System.Windows.Forms.GroupBox();
            this.ChkLBAgencies = new System.Windows.Forms.CheckedListBox();
            this.chkLegacyOther = new System.Windows.Forms.CheckBox();
            this.btnSelectOffenseExcel = new System.Windows.Forms.Button();
            this.txtOffenseExcelFile = new System.Windows.Forms.TextBox();
            this.chkLegacyAttachments = new System.Windows.Forms.CheckBox();
            this.chkAttachments = new System.Windows.Forms.CheckBox();
            this.chkCases = new System.Windows.Forms.CheckBox();
            this.chkEvidence = new System.Windows.Forms.CheckBox();
            this.chkItems = new System.Windows.Forms.CheckBox();
            this.chkReports = new System.Windows.Forms.CheckBox();
            this.chkNames = new System.Windows.Forms.CheckBox();
            this.chkLocations = new System.Windows.Forms.CheckBox();
            this.chkUsers = new System.Windows.Forms.CheckBox();
            this.chkOffenseCodes = new System.Windows.Forms.CheckBox();
            this.chkAttributes = new System.Windows.Forms.CheckBox();
            this.btnRUNMigration = new System.Windows.Forms.Button();
            this.openOffenseExcelFile = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblDataDictionaryFile = new System.Windows.Forms.Label();
            this.btnSelectDataDictionaryFile = new System.Windows.Forms.Button();
            this.txtDataDictionaryFilePath = new System.Windows.Forms.TextBox();
            this.lblMySqlConnString = new System.Windows.Forms.Label();
            this.lblOracleConnString = new System.Windows.Forms.Label();
            this.txtMySqlConnString = new System.Windows.Forms.TextBox();
            this.txtOracleConnString = new System.Windows.Forms.TextBox();
            this.btnClearTables = new System.Windows.Forms.Button();
            this.btnClearETLTable = new System.Windows.Forms.Button();
            this.AgencyFilter = new System.Windows.Forms.Label();
            this.gbDataEntities.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ListBoxInfo
            // 
            this.ListBoxInfo.FormattingEnabled = true;
            this.ListBoxInfo.ItemHeight = 16;
            this.ListBoxInfo.Location = new System.Drawing.Point(27, 582);
            this.ListBoxInfo.Name = "ListBoxInfo";
            this.ListBoxInfo.Size = new System.Drawing.Size(1145, 340);
            this.ListBoxInfo.TabIndex = 0;
            // 
            // gbDataEntities
            // 
            this.gbDataEntities.Controls.Add(this.AgencyFilter);
            this.gbDataEntities.Controls.Add(this.ChkLBAgencies);
            this.gbDataEntities.Controls.Add(this.chkLegacyOther);
            this.gbDataEntities.Controls.Add(this.btnSelectOffenseExcel);
            this.gbDataEntities.Controls.Add(this.txtOffenseExcelFile);
            this.gbDataEntities.Controls.Add(this.chkLegacyAttachments);
            this.gbDataEntities.Controls.Add(this.chkAttachments);
            this.gbDataEntities.Controls.Add(this.chkCases);
            this.gbDataEntities.Controls.Add(this.chkEvidence);
            this.gbDataEntities.Controls.Add(this.chkItems);
            this.gbDataEntities.Controls.Add(this.chkReports);
            this.gbDataEntities.Controls.Add(this.chkNames);
            this.gbDataEntities.Controls.Add(this.chkLocations);
            this.gbDataEntities.Controls.Add(this.chkUsers);
            this.gbDataEntities.Controls.Add(this.chkOffenseCodes);
            this.gbDataEntities.Controls.Add(this.chkAttributes);
            this.gbDataEntities.Location = new System.Drawing.Point(27, 224);
            this.gbDataEntities.Name = "gbDataEntities";
            this.gbDataEntities.Size = new System.Drawing.Size(1145, 352);
            this.gbDataEntities.TabIndex = 10;
            this.gbDataEntities.TabStop = false;
            this.gbDataEntities.Text = "Data Entities";
            // 
            // ChkLBAgencies
            // 
            this.ChkLBAgencies.FormattingEnabled = true;
            this.ChkLBAgencies.Items.AddRange(new object[] {
            "AVP",
            "DAP",
            "DAS",
            "GRT",
            "GVP",
            "IDP",
            "KZP",
            "LCP",
            "MMP",
            "SHARED",
            "SMP",
            "SYP",
            "TRP"});
            this.ChkLBAgencies.Location = new System.Drawing.Point(711, 109);
            this.ChkLBAgencies.Name = "ChkLBAgencies";
            this.ChkLBAgencies.Size = new System.Drawing.Size(270, 225);
            this.ChkLBAgencies.TabIndex = 27;
            // 
            // chkLegacyOther
            // 
            this.chkLegacyOther.AutoSize = true;
            this.chkLegacyOther.Location = new System.Drawing.Point(62, 323);
            this.chkLegacyOther.Name = "chkLegacyOther";
            this.chkLegacyOther.Size = new System.Drawing.Size(126, 21);
            this.chkLegacyOther.TabIndex = 26;
            this.chkLegacyOther.Text = "Legacy (Other)";
            this.chkLegacyOther.UseVisualStyleBackColor = true;
            // 
            // btnSelectOffenseExcel
            // 
            this.btnSelectOffenseExcel.Location = new System.Drawing.Point(987, 52);
            this.btnSelectOffenseExcel.Name = "btnSelectOffenseExcel";
            this.btnSelectOffenseExcel.Size = new System.Drawing.Size(84, 29);
            this.btnSelectOffenseExcel.TabIndex = 25;
            this.btnSelectOffenseExcel.Text = "Select";
            this.btnSelectOffenseExcel.UseVisualStyleBackColor = true;
            this.btnSelectOffenseExcel.Click += new System.EventHandler(this.btnSelectOffenseExcel_Click);
            // 
            // txtOffenseExcelFile
            // 
            this.txtOffenseExcelFile.Location = new System.Drawing.Point(162, 55);
            this.txtOffenseExcelFile.Name = "txtOffenseExcelFile";
            this.txtOffenseExcelFile.Size = new System.Drawing.Size(819, 22);
            this.txtOffenseExcelFile.TabIndex = 24;
            // 
            // chkLegacyAttachments
            // 
            this.chkLegacyAttachments.AutoSize = true;
            this.chkLegacyAttachments.Location = new System.Drawing.Point(62, 298);
            this.chkLegacyAttachments.Name = "chkLegacyAttachments";
            this.chkLegacyAttachments.Size = new System.Drawing.Size(160, 21);
            this.chkLegacyAttachments.TabIndex = 23;
            this.chkLegacyAttachments.Text = "Legacy (Attacments)";
            this.chkLegacyAttachments.UseVisualStyleBackColor = true;
            // 
            // chkAttachments
            // 
            this.chkAttachments.AutoSize = true;
            this.chkAttachments.Location = new System.Drawing.Point(34, 271);
            this.chkAttachments.Name = "chkAttachments";
            this.chkAttachments.Size = new System.Drawing.Size(108, 21);
            this.chkAttachments.TabIndex = 22;
            this.chkAttachments.Text = "Attachments";
            this.chkAttachments.UseVisualStyleBackColor = true;
            // 
            // chkCases
            // 
            this.chkCases.AutoSize = true;
            this.chkCases.Location = new System.Drawing.Point(34, 244);
            this.chkCases.Name = "chkCases";
            this.chkCases.Size = new System.Drawing.Size(69, 21);
            this.chkCases.TabIndex = 21;
            this.chkCases.Text = "Cases";
            this.chkCases.UseVisualStyleBackColor = true;
            // 
            // chkEvidence
            // 
            this.chkEvidence.AutoSize = true;
            this.chkEvidence.Location = new System.Drawing.Point(34, 217);
            this.chkEvidence.Name = "chkEvidence";
            this.chkEvidence.Size = new System.Drawing.Size(88, 21);
            this.chkEvidence.TabIndex = 20;
            this.chkEvidence.Text = "Evidence";
            this.chkEvidence.UseVisualStyleBackColor = true;
            // 
            // chkItems
            // 
            this.chkItems.AutoSize = true;
            this.chkItems.Location = new System.Drawing.Point(34, 190);
            this.chkItems.Name = "chkItems";
            this.chkItems.Size = new System.Drawing.Size(63, 21);
            this.chkItems.TabIndex = 19;
            this.chkItems.Text = "Items";
            this.chkItems.UseVisualStyleBackColor = true;
            // 
            // chkReports
            // 
            this.chkReports.AutoSize = true;
            this.chkReports.Location = new System.Drawing.Point(34, 163);
            this.chkReports.Name = "chkReports";
            this.chkReports.Size = new System.Drawing.Size(80, 21);
            this.chkReports.TabIndex = 18;
            this.chkReports.Text = "Reports";
            this.chkReports.UseVisualStyleBackColor = true;
            // 
            // chkNames
            // 
            this.chkNames.AutoSize = true;
            this.chkNames.Location = new System.Drawing.Point(34, 136);
            this.chkNames.Name = "chkNames";
            this.chkNames.Size = new System.Drawing.Size(74, 21);
            this.chkNames.TabIndex = 17;
            this.chkNames.Text = "Names";
            this.chkNames.UseVisualStyleBackColor = true;
            // 
            // chkLocations
            // 
            this.chkLocations.AutoSize = true;
            this.chkLocations.Location = new System.Drawing.Point(34, 109);
            this.chkLocations.Name = "chkLocations";
            this.chkLocations.Size = new System.Drawing.Size(91, 21);
            this.chkLocations.TabIndex = 16;
            this.chkLocations.Text = "Locations";
            this.chkLocations.UseVisualStyleBackColor = true;
            // 
            // chkUsers
            // 
            this.chkUsers.AutoSize = true;
            this.chkUsers.Location = new System.Drawing.Point(34, 82);
            this.chkUsers.Name = "chkUsers";
            this.chkUsers.Size = new System.Drawing.Size(67, 21);
            this.chkUsers.TabIndex = 15;
            this.chkUsers.Text = "Users";
            this.chkUsers.UseVisualStyleBackColor = true;
            // 
            // chkOffenseCodes
            // 
            this.chkOffenseCodes.AutoSize = true;
            this.chkOffenseCodes.Location = new System.Drawing.Point(34, 55);
            this.chkOffenseCodes.Name = "chkOffenseCodes";
            this.chkOffenseCodes.Size = new System.Drawing.Size(122, 21);
            this.chkOffenseCodes.TabIndex = 14;
            this.chkOffenseCodes.Text = "Offense codes";
            this.chkOffenseCodes.UseVisualStyleBackColor = true;
            // 
            // chkAttributes
            // 
            this.chkAttributes.AutoSize = true;
            this.chkAttributes.Checked = true;
            this.chkAttributes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAttributes.Location = new System.Drawing.Point(34, 28);
            this.chkAttributes.Name = "chkAttributes";
            this.chkAttributes.Size = new System.Drawing.Size(90, 21);
            this.chkAttributes.TabIndex = 13;
            this.chkAttributes.Text = "Attributes";
            this.chkAttributes.UseVisualStyleBackColor = true;
            // 
            // btnRUNMigration
            // 
            this.btnRUNMigration.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRUNMigration.Location = new System.Drawing.Point(1178, 25);
            this.btnRUNMigration.Name = "btnRUNMigration";
            this.btnRUNMigration.Size = new System.Drawing.Size(201, 38);
            this.btnRUNMigration.TabIndex = 11;
            this.btnRUNMigration.Text = "RUN Migration";
            this.btnRUNMigration.UseVisualStyleBackColor = true;
            this.btnRUNMigration.Click += new System.EventHandler(this.btnRUNMigration_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblDataDictionaryFile);
            this.groupBox1.Controls.Add(this.btnSelectDataDictionaryFile);
            this.groupBox1.Controls.Add(this.txtDataDictionaryFilePath);
            this.groupBox1.Controls.Add(this.lblMySqlConnString);
            this.groupBox1.Controls.Add(this.lblOracleConnString);
            this.groupBox1.Controls.Add(this.txtMySqlConnString);
            this.groupBox1.Controls.Add(this.txtOracleConnString);
            this.groupBox1.Location = new System.Drawing.Point(27, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1145, 193);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data Sources (Connect Strings)";
            // 
            // lblDataDictionaryFile
            // 
            this.lblDataDictionaryFile.AutoSize = true;
            this.lblDataDictionaryFile.Location = new System.Drawing.Point(32, 128);
            this.lblDataDictionaryFile.Name = "lblDataDictionaryFile";
            this.lblDataDictionaryFile.Size = new System.Drawing.Size(131, 17);
            this.lblDataDictionaryFile.TabIndex = 31;
            this.lblDataDictionaryFile.Text = "Data Dictionary file:";
            // 
            // btnSelectDataDictionaryFile
            // 
            this.btnSelectDataDictionaryFile.Location = new System.Drawing.Point(987, 143);
            this.btnSelectDataDictionaryFile.Name = "btnSelectDataDictionaryFile";
            this.btnSelectDataDictionaryFile.Size = new System.Drawing.Size(84, 29);
            this.btnSelectDataDictionaryFile.TabIndex = 30;
            this.btnSelectDataDictionaryFile.Text = "Select";
            this.btnSelectDataDictionaryFile.UseVisualStyleBackColor = true;
            this.btnSelectDataDictionaryFile.Click += new System.EventHandler(this.btnSelectDataDictionaryFile_Click);
            // 
            // txtDataDictionaryFilePath
            // 
            this.txtDataDictionaryFilePath.Location = new System.Drawing.Point(31, 148);
            this.txtDataDictionaryFilePath.Name = "txtDataDictionaryFilePath";
            this.txtDataDictionaryFilePath.Size = new System.Drawing.Size(950, 22);
            this.txtDataDictionaryFilePath.TabIndex = 29;
            // 
            // lblMySqlConnString
            // 
            this.lblMySqlConnString.AutoSize = true;
            this.lblMySqlConnString.Location = new System.Drawing.Point(28, 75);
            this.lblMySqlConnString.Name = "lblMySqlConnString";
            this.lblMySqlConnString.Size = new System.Drawing.Size(128, 17);
            this.lblMySqlConnString.TabIndex = 28;
            this.lblMySqlConnString.Text = "MySql Conn String:";
            // 
            // lblOracleConnString
            // 
            this.lblOracleConnString.AutoSize = true;
            this.lblOracleConnString.Location = new System.Drawing.Point(28, 25);
            this.lblOracleConnString.Name = "lblOracleConnString";
            this.lblOracleConnString.Size = new System.Drawing.Size(132, 17);
            this.lblOracleConnString.TabIndex = 27;
            this.lblOracleConnString.Text = "Oracle Conn String:";
            // 
            // txtMySqlConnString
            // 
            this.txtMySqlConnString.Location = new System.Drawing.Point(31, 95);
            this.txtMySqlConnString.Name = "txtMySqlConnString";
            this.txtMySqlConnString.Size = new System.Drawing.Size(1040, 22);
            this.txtMySqlConnString.TabIndex = 26;
            // 
            // txtOracleConnString
            // 
            this.txtOracleConnString.Location = new System.Drawing.Point(31, 45);
            this.txtOracleConnString.Name = "txtOracleConnString";
            this.txtOracleConnString.Size = new System.Drawing.Size(1040, 22);
            this.txtOracleConnString.TabIndex = 25;
            // 
            // btnClearTables
            // 
            this.btnClearTables.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearTables.Location = new System.Drawing.Point(1178, 824);
            this.btnClearTables.Name = "btnClearTables";
            this.btnClearTables.Size = new System.Drawing.Size(204, 38);
            this.btnClearTables.TabIndex = 13;
            this.btnClearTables.Text = "Clear Migration Tables";
            this.btnClearTables.UseVisualStyleBackColor = true;
            this.btnClearTables.Click += new System.EventHandler(this.btnClearTables_Click);
            // 
            // btnClearETLTable
            // 
            this.btnClearETLTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearETLTable.Location = new System.Drawing.Point(1178, 884);
            this.btnClearETLTable.Name = "btnClearETLTable";
            this.btnClearETLTable.Size = new System.Drawing.Size(204, 38);
            this.btnClearETLTable.TabIndex = 14;
            this.btnClearETLTable.Text = "Clear ETL Table(s)";
            this.btnClearETLTable.UseVisualStyleBackColor = true;
            this.btnClearETLTable.Click += new System.EventHandler(this.btnClearETLTable_Click);
            // 
            // AgencyFilter
            // 
            this.AgencyFilter.AutoSize = true;
            this.AgencyFilter.Location = new System.Drawing.Point(708, 89);
            this.AgencyFilter.Name = "AgencyFilter";
            this.AgencyFilter.Size = new System.Drawing.Size(270, 17);
            this.AgencyFilter.TabIndex = 28;
            this.AgencyFilter.Text = "Agency Filter (Attributes, Offense Codes):";
            // 
            // ETLController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1394, 939);
            this.Controls.Add(this.btnClearETLTable);
            this.Controls.Add(this.btnClearTables);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnRUNMigration);
            this.Controls.Add(this.gbDataEntities);
            this.Controls.Add(this.ListBoxInfo);
            this.MaximizeBox = false;
            this.Name = "ETLController";
            this.Text = "ETC Controller";
            this.gbDataEntities.ResumeLayout(false);
            this.gbDataEntities.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ListBox ListBoxInfo;
        private System.Windows.Forms.GroupBox gbDataEntities;
        private System.Windows.Forms.CheckBox chkNames;
        private System.Windows.Forms.CheckBox chkLocations;
        private System.Windows.Forms.CheckBox chkUsers;
        private System.Windows.Forms.CheckBox chkOffenseCodes;
        private System.Windows.Forms.CheckBox chkAttributes;
        private System.Windows.Forms.CheckBox chkLegacyAttachments;
        private System.Windows.Forms.CheckBox chkAttachments;
        private System.Windows.Forms.CheckBox chkCases;
        private System.Windows.Forms.CheckBox chkEvidence;
        private System.Windows.Forms.CheckBox chkItems;
        private System.Windows.Forms.CheckBox chkReports;
        private System.Windows.Forms.Button btnRUNMigration;
        private System.Windows.Forms.Button btnSelectOffenseExcel;
        private System.Windows.Forms.TextBox txtOffenseExcelFile;
        private System.Windows.Forms.OpenFileDialog openOffenseExcelFile;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblMySqlConnString;
        private System.Windows.Forms.Label lblOracleConnString;
        public System.Windows.Forms.TextBox txtMySqlConnString;
        public System.Windows.Forms.TextBox txtOracleConnString;
        private System.Windows.Forms.Button btnSelectDataDictionaryFile;
        private System.Windows.Forms.TextBox txtDataDictionaryFilePath;
        private System.Windows.Forms.Label lblDataDictionaryFile;
        private System.Windows.Forms.CheckBox chkLegacyOther;
        private System.Windows.Forms.Button btnClearTables;
        private System.Windows.Forms.Button btnClearETLTable;
        private System.Windows.Forms.CheckedListBox ChkLBAgencies;
        private System.Windows.Forms.Label AgencyFilter;
    }
}

