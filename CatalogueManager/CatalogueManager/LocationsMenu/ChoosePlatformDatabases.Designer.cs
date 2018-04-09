using ReusableUIComponents;

namespace CatalogueManager.LocationsMenu
{
    partial class ChoosePlatformDatabases
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
            this.label1 = new System.Windows.Forms.Label();
            this.tbCatalogueConnectionString = new ReusableUIComponents.ConnectionStringTextBox();
            this.btnSaveAndClose = new System.Windows.Forms.Button();
            this.gbSqlServer = new System.Windows.Forms.GroupBox();
            this.pReferenceADataExport = new System.Windows.Forms.Panel();
            this.tbDataExportManagerConnectionString = new ReusableUIComponents.ConnectionStringTextBox();
            this.btnCheckDataExportManager = new System.Windows.Forms.Button();
            this.pReferenceACatalogue = new System.Windows.Forms.Panel();
            this.btnCheckCatalogue = new System.Windows.Forms.Button();
            this.btnCreateNewDataExportManagerDatabase = new System.Windows.Forms.Button();
            this.btnCreateNewCatalogue = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.btnCreateSuite = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbSuiteServer = new System.Windows.Forms.TextBox();
            this.tbDatabasePrefix = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.pCreateAllPlatformDatabases = new System.Windows.Forms.Panel();
            this.gbSqlServer.SuspendLayout();
            this.pReferenceADataExport.SuspendLayout();
            this.pReferenceACatalogue.SuspendLayout();
            this.pCreateAllPlatformDatabases.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(83, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Catalogue:";
            // 
            // tbCatalogueConnectionString
            // 
            this.tbCatalogueConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCatalogueConnectionString.DatabaseType = ReusableLibraryCode.DatabaseType.MicrosoftSQLServer;
            this.tbCatalogueConnectionString.ForeColor = System.Drawing.Color.Black;
            this.tbCatalogueConnectionString.Location = new System.Drawing.Point(3, 5);
            this.tbCatalogueConnectionString.Name = "tbCatalogueConnectionString";
            this.tbCatalogueConnectionString.Size = new System.Drawing.Size(951, 20);
            this.tbCatalogueConnectionString.TabIndex = 1;
            this.tbCatalogueConnectionString.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbCatalogueConnectionString_KeyUp);
            // 
            // btnSaveAndClose
            // 
            this.btnSaveAndClose.Location = new System.Drawing.Point(479, 224);
            this.btnSaveAndClose.Name = "btnSaveAndClose";
            this.btnSaveAndClose.Size = new System.Drawing.Size(274, 23);
            this.btnSaveAndClose.TabIndex = 10;
            this.btnSaveAndClose.Text = "Save and Close";
            this.btnSaveAndClose.UseVisualStyleBackColor = true;
            this.btnSaveAndClose.Click += new System.EventHandler(this.btnSaveAndClose_Click);
            // 
            // gbSqlServer
            // 
            this.gbSqlServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSqlServer.Controls.Add(this.pReferenceADataExport);
            this.gbSqlServer.Controls.Add(this.pReferenceACatalogue);
            this.gbSqlServer.Controls.Add(this.btnCreateNewDataExportManagerDatabase);
            this.gbSqlServer.Controls.Add(this.btnCreateNewCatalogue);
            this.gbSqlServer.Controls.Add(this.label8);
            this.gbSqlServer.Controls.Add(this.label1);
            this.gbSqlServer.Location = new System.Drawing.Point(6, 85);
            this.gbSqlServer.Name = "gbSqlServer";
            this.gbSqlServer.Size = new System.Drawing.Size(1248, 131);
            this.gbSqlServer.TabIndex = 8;
            this.gbSqlServer.TabStop = false;
            this.gbSqlServer.Text = "Connection Strings (SQL Server)";
            // 
            // pReferenceADataExport
            // 
            this.pReferenceADataExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pReferenceADataExport.Controls.Add(this.tbDataExportManagerConnectionString);
            this.pReferenceADataExport.Controls.Add(this.btnCheckDataExportManager);
            this.pReferenceADataExport.Location = new System.Drawing.Point(165, 73);
            this.pReferenceADataExport.Name = "pReferenceADataExport";
            this.pReferenceADataExport.Size = new System.Drawing.Size(957, 49);
            this.pReferenceADataExport.TabIndex = 9;
            // 
            // tbDataExportManagerConnectionString
            // 
            this.tbDataExportManagerConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDataExportManagerConnectionString.DatabaseType = ReusableLibraryCode.DatabaseType.MicrosoftSQLServer;
            this.tbDataExportManagerConnectionString.ForeColor = System.Drawing.Color.Black;
            this.tbDataExportManagerConnectionString.Location = new System.Drawing.Point(3, 3);
            this.tbDataExportManagerConnectionString.Name = "tbDataExportManagerConnectionString";
            this.tbDataExportManagerConnectionString.Size = new System.Drawing.Size(951, 20);
            this.tbDataExportManagerConnectionString.TabIndex = 5;
            // 
            // btnCheckDataExportManager
            // 
            this.btnCheckDataExportManager.Location = new System.Drawing.Point(3, 26);
            this.btnCheckDataExportManager.Name = "btnCheckDataExportManager";
            this.btnCheckDataExportManager.Size = new System.Drawing.Size(64, 23);
            this.btnCheckDataExportManager.TabIndex = 7;
            this.btnCheckDataExportManager.Text = "Check";
            this.btnCheckDataExportManager.UseVisualStyleBackColor = true;
            this.btnCheckDataExportManager.Click += new System.EventHandler(this.btnCheckDataExportManager_Click);
            // 
            // pReferenceACatalogue
            // 
            this.pReferenceACatalogue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pReferenceACatalogue.Controls.Add(this.tbCatalogueConnectionString);
            this.pReferenceACatalogue.Controls.Add(this.btnCheckCatalogue);
            this.pReferenceACatalogue.Location = new System.Drawing.Point(165, 19);
            this.pReferenceACatalogue.Name = "pReferenceACatalogue";
            this.pReferenceACatalogue.Size = new System.Drawing.Size(957, 48);
            this.pReferenceACatalogue.TabIndex = 8;
            // 
            // btnCheckCatalogue
            // 
            this.btnCheckCatalogue.Location = new System.Drawing.Point(3, 25);
            this.btnCheckCatalogue.Name = "btnCheckCatalogue";
            this.btnCheckCatalogue.Size = new System.Drawing.Size(64, 23);
            this.btnCheckCatalogue.TabIndex = 3;
            this.btnCheckCatalogue.Text = "Check";
            this.btnCheckCatalogue.UseVisualStyleBackColor = true;
            this.btnCheckCatalogue.Click += new System.EventHandler(this.btnCheckCatalogue_Click);
            // 
            // btnCreateNewDataExportManagerDatabase
            // 
            this.btnCreateNewDataExportManagerDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewDataExportManagerDatabase.Location = new System.Drawing.Point(1128, 71);
            this.btnCreateNewDataExportManagerDatabase.Name = "btnCreateNewDataExportManagerDatabase";
            this.btnCreateNewDataExportManagerDatabase.Size = new System.Drawing.Size(101, 23);
            this.btnCreateNewDataExportManagerDatabase.TabIndex = 6;
            this.btnCreateNewDataExportManagerDatabase.Text = "Create New...";
            this.btnCreateNewDataExportManagerDatabase.UseVisualStyleBackColor = true;
            this.btnCreateNewDataExportManagerDatabase.Click += new System.EventHandler(this.btnCreateNewDataExportManagerDatabase_Click);
            // 
            // btnCreateNewCatalogue
            // 
            this.btnCreateNewCatalogue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewCatalogue.Location = new System.Drawing.Point(1128, 20);
            this.btnCreateNewCatalogue.Name = "btnCreateNewCatalogue";
            this.btnCreateNewCatalogue.Size = new System.Drawing.Size(101, 23);
            this.btnCreateNewCatalogue.TabIndex = 2;
            this.btnCreateNewCatalogue.Text = "Create New...";
            this.btnCreateNewCatalogue.UseVisualStyleBackColor = true;
            this.btnCreateNewCatalogue.Click += new System.EventHandler(this.btnCreateNewCatalogue_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(48, 76);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(111, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Data Export Manager:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 248);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Result:";
            // 
            // checksUI1
            // 
            this.checksUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checksUI1.Location = new System.Drawing.Point(6, 264);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(1257, 347);
            this.checksUI1.TabIndex = 0;
            // 
            // btnCreateSuite
            // 
            this.btnCreateSuite.Location = new System.Drawing.Point(454, 3);
            this.btnCreateSuite.Name = "btnCreateSuite";
            this.btnCreateSuite.Size = new System.Drawing.Size(211, 23);
            this.btnCreateSuite.TabIndex = 5;
            this.btnCreateSuite.Text = "Create RDMP Database Suite...";
            this.btnCreateSuite.UseVisualStyleBackColor = true;
            this.btnCreateSuite.Click += new System.EventHandler(this.btnCreateSuite_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(170, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "1. Create New Platform Databases";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "2. Use Existing";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Server:";
            // 
            // tbSuiteServer
            // 
            this.tbSuiteServer.Location = new System.Drawing.Point(61, 4);
            this.tbSuiteServer.Name = "tbSuiteServer";
            this.tbSuiteServer.Size = new System.Drawing.Size(143, 20);
            this.tbSuiteServer.TabIndex = 2;
            this.tbSuiteServer.Text = "localhost\\sqlexpress";
            // 
            // tbDatabasePrefix
            // 
            this.tbDatabasePrefix.Location = new System.Drawing.Point(306, 5);
            this.tbDatabasePrefix.Name = "tbDatabasePrefix";
            this.tbDatabasePrefix.Size = new System.Drawing.Size(143, 20);
            this.tbDatabasePrefix.TabIndex = 4;
            this.tbDatabasePrefix.Text = "RDMP_";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(215, 8);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(85, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Database Prefix:";
            // 
            // pCreateAllPlatformDatabases
            // 
            this.pCreateAllPlatformDatabases.Controls.Add(this.tbSuiteServer);
            this.pCreateAllPlatformDatabases.Controls.Add(this.tbDatabasePrefix);
            this.pCreateAllPlatformDatabases.Controls.Add(this.btnCreateSuite);
            this.pCreateAllPlatformDatabases.Controls.Add(this.label6);
            this.pCreateAllPlatformDatabases.Controls.Add(this.label5);
            this.pCreateAllPlatformDatabases.Location = new System.Drawing.Point(25, 29);
            this.pCreateAllPlatformDatabases.Name = "pCreateAllPlatformDatabases";
            this.pCreateAllPlatformDatabases.Size = new System.Drawing.Size(676, 27);
            this.pCreateAllPlatformDatabases.TabIndex = 12;
            // 
            // ChoosePlatformDatabases
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1266, 615);
            this.Controls.Add(this.pCreateAllPlatformDatabases);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checksUI1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.gbSqlServer);
            this.Controls.Add(this.btnSaveAndClose);
            this.Name = "ChoosePlatformDatabases";
            this.Text = "Configure DataManagementPlatform Core Databases";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ChooseDatabase_KeyUp);
            this.gbSqlServer.ResumeLayout(false);
            this.gbSqlServer.PerformLayout();
            this.pReferenceADataExport.ResumeLayout(false);
            this.pReferenceADataExport.PerformLayout();
            this.pReferenceACatalogue.ResumeLayout(false);
            this.pReferenceACatalogue.PerformLayout();
            this.pCreateAllPlatformDatabases.ResumeLayout(false);
            this.pCreateAllPlatformDatabases.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private ConnectionStringTextBox tbCatalogueConnectionString;
        private System.Windows.Forms.Button btnSaveAndClose;
        private System.Windows.Forms.GroupBox gbSqlServer;
        private System.Windows.Forms.Label label2;
        private ConnectionStringTextBox tbDataExportManagerConnectionString;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnCreateNewDataExportManagerDatabase;
        private System.Windows.Forms.Button btnCreateNewCatalogue;
        private System.Windows.Forms.Button btnCheckDataExportManager;
        private System.Windows.Forms.Button btnCheckCatalogue;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.Button btnCreateSuite;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbSuiteServer;
        private System.Windows.Forms.TextBox tbDatabasePrefix;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel pCreateAllPlatformDatabases;
        private System.Windows.Forms.Panel pReferenceACatalogue;
        private System.Windows.Forms.Panel pReferenceADataExport;
    }
}