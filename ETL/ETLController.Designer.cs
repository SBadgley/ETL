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
            this.chkAttributes = new System.Windows.Forms.CheckBox();
            this.chkOffenseCodes = new System.Windows.Forms.CheckBox();
            this.chkLookupTables = new System.Windows.Forms.CheckBox();
            this.chkUsers = new System.Windows.Forms.CheckBox();
            this.chkLocations = new System.Windows.Forms.CheckBox();
            this.chkNames = new System.Windows.Forms.CheckBox();
            this.chkReports = new System.Windows.Forms.CheckBox();
            this.chkItems = new System.Windows.Forms.CheckBox();
            this.chkEvidence = new System.Windows.Forms.CheckBox();
            this.chkCases = new System.Windows.Forms.CheckBox();
            this.chkAttachments = new System.Windows.Forms.CheckBox();
            this.chkLegacyOther = new System.Windows.Forms.CheckBox();
            this.btnRUNMigration = new System.Windows.Forms.Button();
            this.openOffenseExcelFile = new System.Windows.Forms.OpenFileDialog();
            this.txtOffenseExcelFile = new System.Windows.Forms.TextBox();
            this.btnSelectOffenseExcel = new System.Windows.Forms.Button();
            this.gbDataEntities.SuspendLayout();
            this.SuspendLayout();
            // 
            // ListBoxInfo
            // 
            this.ListBoxInfo.FormattingEnabled = true;
            this.ListBoxInfo.ItemHeight = 16;
            this.ListBoxInfo.Location = new System.Drawing.Point(27, 496);
            this.ListBoxInfo.Name = "ListBoxInfo";
            this.ListBoxInfo.Size = new System.Drawing.Size(808, 196);
            this.ListBoxInfo.TabIndex = 0;
            // 
            // gbDataEntities
            // 
            this.gbDataEntities.Controls.Add(this.btnSelectOffenseExcel);
            this.gbDataEntities.Controls.Add(this.txtOffenseExcelFile);
            this.gbDataEntities.Controls.Add(this.chkLegacyOther);
            this.gbDataEntities.Controls.Add(this.chkAttachments);
            this.gbDataEntities.Controls.Add(this.chkCases);
            this.gbDataEntities.Controls.Add(this.chkEvidence);
            this.gbDataEntities.Controls.Add(this.chkItems);
            this.gbDataEntities.Controls.Add(this.chkReports);
            this.gbDataEntities.Controls.Add(this.chkNames);
            this.gbDataEntities.Controls.Add(this.chkLocations);
            this.gbDataEntities.Controls.Add(this.chkUsers);
            this.gbDataEntities.Controls.Add(this.chkLookupTables);
            this.gbDataEntities.Controls.Add(this.chkOffenseCodes);
            this.gbDataEntities.Controls.Add(this.chkAttributes);
            this.gbDataEntities.Location = new System.Drawing.Point(27, 41);
            this.gbDataEntities.Name = "gbDataEntities";
            this.gbDataEntities.Size = new System.Drawing.Size(808, 426);
            this.gbDataEntities.TabIndex = 10;
            this.gbDataEntities.TabStop = false;
            this.gbDataEntities.Text = "Data Entities";
            // 
            // chkAttributes
            // 
            this.chkAttributes.AutoSize = true;
            this.chkAttributes.Checked = true;
            this.chkAttributes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAttributes.Location = new System.Drawing.Point(34, 72);
            this.chkAttributes.Name = "chkAttributes";
            this.chkAttributes.Size = new System.Drawing.Size(90, 21);
            this.chkAttributes.TabIndex = 13;
            this.chkAttributes.Text = "Attributes";
            this.chkAttributes.UseVisualStyleBackColor = true;
            // 
            // chkOffenseCodes
            // 
            this.chkOffenseCodes.AutoSize = true;
            this.chkOffenseCodes.Checked = true;
            this.chkOffenseCodes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOffenseCodes.Location = new System.Drawing.Point(34, 99);
            this.chkOffenseCodes.Name = "chkOffenseCodes";
            this.chkOffenseCodes.Size = new System.Drawing.Size(122, 21);
            this.chkOffenseCodes.TabIndex = 14;
            this.chkOffenseCodes.Text = "Offense codes";
            this.chkOffenseCodes.UseVisualStyleBackColor = true;
            // 
            // chkLookupTables
            // 
            this.chkLookupTables.AutoSize = true;
            this.chkLookupTables.Checked = true;
            this.chkLookupTables.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLookupTables.Location = new System.Drawing.Point(34, 29);
            this.chkLookupTables.Name = "chkLookupTables";
            this.chkLookupTables.Size = new System.Drawing.Size(119, 21);
            this.chkLookupTables.TabIndex = 12;
            this.chkLookupTables.Text = "Lookup tables";
            this.chkLookupTables.UseVisualStyleBackColor = true;
            // 
            // chkUsers
            // 
            this.chkUsers.AutoSize = true;
            this.chkUsers.Checked = true;
            this.chkUsers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUsers.Location = new System.Drawing.Point(34, 126);
            this.chkUsers.Name = "chkUsers";
            this.chkUsers.Size = new System.Drawing.Size(67, 21);
            this.chkUsers.TabIndex = 15;
            this.chkUsers.Text = "Users";
            this.chkUsers.UseVisualStyleBackColor = true;
            // 
            // chkLocations
            // 
            this.chkLocations.AutoSize = true;
            this.chkLocations.Location = new System.Drawing.Point(34, 153);
            this.chkLocations.Name = "chkLocations";
            this.chkLocations.Size = new System.Drawing.Size(91, 21);
            this.chkLocations.TabIndex = 16;
            this.chkLocations.Text = "Locations";
            this.chkLocations.UseVisualStyleBackColor = true;
            // 
            // chkNames
            // 
            this.chkNames.AutoSize = true;
            this.chkNames.Location = new System.Drawing.Point(34, 180);
            this.chkNames.Name = "chkNames";
            this.chkNames.Size = new System.Drawing.Size(74, 21);
            this.chkNames.TabIndex = 17;
            this.chkNames.Text = "Names";
            this.chkNames.UseVisualStyleBackColor = true;
            // 
            // chkReports
            // 
            this.chkReports.AutoSize = true;
            this.chkReports.Location = new System.Drawing.Point(34, 207);
            this.chkReports.Name = "chkReports";
            this.chkReports.Size = new System.Drawing.Size(80, 21);
            this.chkReports.TabIndex = 18;
            this.chkReports.Text = "Reports";
            this.chkReports.UseVisualStyleBackColor = true;
            // 
            // chkItems
            // 
            this.chkItems.AutoSize = true;
            this.chkItems.Location = new System.Drawing.Point(34, 234);
            this.chkItems.Name = "chkItems";
            this.chkItems.Size = new System.Drawing.Size(63, 21);
            this.chkItems.TabIndex = 19;
            this.chkItems.Text = "Items";
            this.chkItems.UseVisualStyleBackColor = true;
            // 
            // chkEvidence
            // 
            this.chkEvidence.AutoSize = true;
            this.chkEvidence.Location = new System.Drawing.Point(34, 261);
            this.chkEvidence.Name = "chkEvidence";
            this.chkEvidence.Size = new System.Drawing.Size(88, 21);
            this.chkEvidence.TabIndex = 20;
            this.chkEvidence.Text = "Evidence";
            this.chkEvidence.UseVisualStyleBackColor = true;
            // 
            // chkCases
            // 
            this.chkCases.AutoSize = true;
            this.chkCases.Location = new System.Drawing.Point(34, 288);
            this.chkCases.Name = "chkCases";
            this.chkCases.Size = new System.Drawing.Size(69, 21);
            this.chkCases.TabIndex = 21;
            this.chkCases.Text = "Cases";
            this.chkCases.UseVisualStyleBackColor = true;
            // 
            // chkAttachments
            // 
            this.chkAttachments.AutoSize = true;
            this.chkAttachments.Location = new System.Drawing.Point(34, 315);
            this.chkAttachments.Name = "chkAttachments";
            this.chkAttachments.Size = new System.Drawing.Size(108, 21);
            this.chkAttachments.TabIndex = 22;
            this.chkAttachments.Text = "Attachments";
            this.chkAttachments.UseVisualStyleBackColor = true;
            // 
            // chkLegacyOther
            // 
            this.chkLegacyOther.AutoSize = true;
            this.chkLegacyOther.Location = new System.Drawing.Point(34, 358);
            this.chkLegacyOther.Name = "chkLegacyOther";
            this.chkLegacyOther.Size = new System.Drawing.Size(126, 21);
            this.chkLegacyOther.TabIndex = 23;
            this.chkLegacyOther.Text = "Legacy (Other)";
            this.chkLegacyOther.UseVisualStyleBackColor = true;
            // 
            // btnRUNMigration
            // 
            this.btnRUNMigration.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRUNMigration.Location = new System.Drawing.Point(890, 53);
            this.btnRUNMigration.Name = "btnRUNMigration";
            this.btnRUNMigration.Size = new System.Drawing.Size(192, 38);
            this.btnRUNMigration.TabIndex = 11;
            this.btnRUNMigration.Text = "RUN Migration";
            this.btnRUNMigration.UseVisualStyleBackColor = true;
            this.btnRUNMigration.Click += new System.EventHandler(this.btnRUNMigration_Click);
            // 
            // txtOffenseExcelFile
            // 
            this.txtOffenseExcelFile.Location = new System.Drawing.Point(162, 99);
            this.txtOffenseExcelFile.Name = "txtOffenseExcelFile";
            this.txtOffenseExcelFile.Size = new System.Drawing.Size(528, 22);
            this.txtOffenseExcelFile.TabIndex = 24;
            // 
            // btnSelectOffenseExcel
            // 
            this.btnSelectOffenseExcel.Location = new System.Drawing.Point(696, 93);
            this.btnSelectOffenseExcel.Name = "btnSelectOffenseExcel";
            this.btnSelectOffenseExcel.Size = new System.Drawing.Size(84, 34);
            this.btnSelectOffenseExcel.TabIndex = 25;
            this.btnSelectOffenseExcel.Text = "Select";
            this.btnSelectOffenseExcel.UseVisualStyleBackColor = true;
            this.btnSelectOffenseExcel.Click += new System.EventHandler(this.btnSelectOffenseExcel_Click);
            // 
            // ETLController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1332, 770);
            this.Controls.Add(this.btnRUNMigration);
            this.Controls.Add(this.gbDataEntities);
            this.Controls.Add(this.ListBoxInfo);
            this.MaximizeBox = false;
            this.Name = "ETLController";
            this.Text = "ETC Controller";
            this.gbDataEntities.ResumeLayout(false);
            this.gbDataEntities.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ListBox ListBoxInfo;
        private System.Windows.Forms.GroupBox gbDataEntities;
        private System.Windows.Forms.CheckBox chkNames;
        private System.Windows.Forms.CheckBox chkLocations;
        private System.Windows.Forms.CheckBox chkUsers;
        private System.Windows.Forms.CheckBox chkLookupTables;
        private System.Windows.Forms.CheckBox chkOffenseCodes;
        private System.Windows.Forms.CheckBox chkAttributes;
        private System.Windows.Forms.CheckBox chkLegacyOther;
        private System.Windows.Forms.CheckBox chkAttachments;
        private System.Windows.Forms.CheckBox chkCases;
        private System.Windows.Forms.CheckBox chkEvidence;
        private System.Windows.Forms.CheckBox chkItems;
        private System.Windows.Forms.CheckBox chkReports;
        private System.Windows.Forms.Button btnRUNMigration;
        private System.Windows.Forms.Button btnSelectOffenseExcel;
        private System.Windows.Forms.TextBox txtOffenseExcelFile;
        private System.Windows.Forms.OpenFileDialog openOffenseExcelFile;
    }
}

