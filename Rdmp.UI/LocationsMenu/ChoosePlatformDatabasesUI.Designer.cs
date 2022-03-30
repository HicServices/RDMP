using System;
using Rdmp.UI.SimpleControls;


namespace Rdmp.UI.LocationsMenu
{
    partial class ChoosePlatformDatabasesUI
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
            this.tbCatalogueConnectionString = new Rdmp.UI.SimpleControls.ConnectionStringTextBox();
            this.btnSaveAndClose = new System.Windows.Forms.Button();
            this.gbUseExisting = new System.Windows.Forms.GroupBox();
            this.btnBack2 = new System.Windows.Forms.Button();
            this.pReferenceADataExport = new System.Windows.Forms.Panel();
            this.btnCreateYamlFile = new System.Windows.Forms.Button();
            this.btnBrowseForDataExport = new System.Windows.Forms.Button();
            this.tbDataExportManagerConnectionString = new Rdmp.UI.SimpleControls.ConnectionStringTextBox();
            this.btnCheckDataExportManager = new System.Windows.Forms.Button();
            this.pReferenceACatalogue = new System.Windows.Forms.Panel();
            this.btnBrowseForCatalogue = new System.Windows.Forms.Button();
            this.btnCheckCatalogue = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checksUI1 = new Rdmp.UI.ChecksUI.ChecksUI();
            this.btnCreateSuite = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tbSuiteServer = new System.Windows.Forms.TextBox();
            this.tbDatabasePrefix = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnCreateNew = new System.Windows.Forms.Button();
            this.pChooseOption = new System.Windows.Forms.Panel();
            this.btnUseExisting = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.gbCreateNew = new System.Windows.Forms.GroupBox();
            this.gbSqlAuthentication = new System.Windows.Forms.GroupBox();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.gbExampleDatasets = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.tbPeopleCount = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tbRowCount = new System.Windows.Forms.TextBox();
            this.cbCreateExampleDatasets = new System.Windows.Forms.CheckBox();
            this.tbSeed = new System.Windows.Forms.TextBox();
            this.tbOtherKeywords = new System.Windows.Forms.TextBox();
            this.tbCreateDatabaseTimeout = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btnBack1 = new System.Windows.Forms.Button();
            this.pResults = new System.Windows.Forms.Panel();
            this.gbUseExisting.SuspendLayout();
            this.pReferenceADataExport.SuspendLayout();
            this.pReferenceACatalogue.SuspendLayout();
            this.pChooseOption.SuspendLayout();
            this.gbCreateNew.SuspendLayout();
            this.gbSqlAuthentication.SuspendLayout();
            this.gbExampleDatasets.SuspendLayout();
            this.pResults.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(97, 25);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Catalogue:";
            // 
            // tbCatalogueConnectionString
            // 
            this.tbCatalogueConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCatalogueConnectionString.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            this.tbCatalogueConnectionString.ForeColor = System.Drawing.Color.Black;
            this.tbCatalogueConnectionString.Location = new System.Drawing.Point(84, 8);
            this.tbCatalogueConnectionString.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbCatalogueConnectionString.Name = "tbCatalogueConnectionString";
            this.tbCatalogueConnectionString.Size = new System.Drawing.Size(937, 23);
            this.tbCatalogueConnectionString.TabIndex = 1;
            this.tbCatalogueConnectionString.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbCatalogueConnectionString_KeyUp);
            // 
            // btnSaveAndClose
            // 
            this.btnSaveAndClose.Location = new System.Drawing.Point(537, 167);
            this.btnSaveAndClose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSaveAndClose.Name = "btnSaveAndClose";
            this.btnSaveAndClose.Size = new System.Drawing.Size(320, 27);
            this.btnSaveAndClose.TabIndex = 10;
            this.btnSaveAndClose.Text = "Save and Close";
            this.btnSaveAndClose.UseVisualStyleBackColor = true;
            this.btnSaveAndClose.Click += new System.EventHandler(this.btnSaveAndClose_Click);
            // 
            // gbUseExisting
            // 
            this.gbUseExisting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbUseExisting.Controls.Add(this.btnBack2);
            this.gbUseExisting.Controls.Add(this.pReferenceADataExport);
            this.gbUseExisting.Controls.Add(this.btnSaveAndClose);
            this.gbUseExisting.Controls.Add(this.pReferenceACatalogue);
            this.gbUseExisting.Controls.Add(this.label8);
            this.gbUseExisting.Controls.Add(this.label1);
            this.gbUseExisting.Location = new System.Drawing.Point(7, 14);
            this.gbUseExisting.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbUseExisting.Name = "gbUseExisting";
            this.gbUseExisting.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbUseExisting.Size = new System.Drawing.Size(1226, 201);
            this.gbUseExisting.TabIndex = 8;
            this.gbUseExisting.TabStop = false;
            this.gbUseExisting.Text = "Connect to existing Platform Databases (Enter Connection Strings)";
            this.gbUseExisting.Visible = false;
            // 
            // btnBack2
            // 
            this.btnBack2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBack2.Location = new System.Drawing.Point(7, 167);
            this.btnBack2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBack2.Name = "btnBack2";
            this.btnBack2.Size = new System.Drawing.Size(88, 27);
            this.btnBack2.TabIndex = 10;
            this.btnBack2.Text = "<< Back";
            this.btnBack2.UseVisualStyleBackColor = true;
            this.btnBack2.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // pReferenceADataExport
            // 
            this.pReferenceADataExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pReferenceADataExport.Controls.Add(this.btnCreateYamlFile);
            this.pReferenceADataExport.Controls.Add(this.btnBrowseForDataExport);
            this.pReferenceADataExport.Controls.Add(this.tbDataExportManagerConnectionString);
            this.pReferenceADataExport.Controls.Add(this.btnCheckDataExportManager);
            this.pReferenceADataExport.Location = new System.Drawing.Point(192, 93);
            this.pReferenceADataExport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pReferenceADataExport.Name = "pReferenceADataExport";
            this.pReferenceADataExport.Size = new System.Drawing.Size(1027, 60);
            this.pReferenceADataExport.TabIndex = 9;
            // 
            // btnCreateYamlFile
            // 
            this.btnCreateYamlFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateYamlFile.Location = new System.Drawing.Point(914, 32);
            this.btnCreateYamlFile.Name = "btnCreateYamlFile";
            this.btnCreateYamlFile.Size = new System.Drawing.Size(108, 23);
            this.btnCreateYamlFile.TabIndex = 9;
            this.btnCreateYamlFile.Text = "Save as yaml...";
            this.btnCreateYamlFile.UseVisualStyleBackColor = true;
            this.btnCreateYamlFile.Click += new System.EventHandler(this.btnCreateYamlFile_Click);
            // 
            // btnBrowseForDataExport
            // 
            this.btnBrowseForDataExport.Location = new System.Drawing.Point(4, 3);
            this.btnBrowseForDataExport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBrowseForDataExport.Name = "btnBrowseForDataExport";
            this.btnBrowseForDataExport.Size = new System.Drawing.Size(75, 27);
            this.btnBrowseForDataExport.TabIndex = 8;
            this.btnBrowseForDataExport.Text = "Browse...";
            this.btnBrowseForDataExport.UseVisualStyleBackColor = true;
            this.btnBrowseForDataExport.Click += new System.EventHandler(this.btnBrowseForDataExport_Click);
            // 
            // tbDataExportManagerConnectionString
            // 
            this.tbDataExportManagerConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDataExportManagerConnectionString.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            this.tbDataExportManagerConnectionString.ForeColor = System.Drawing.Color.Black;
            this.tbDataExportManagerConnectionString.Location = new System.Drawing.Point(84, 3);
            this.tbDataExportManagerConnectionString.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbDataExportManagerConnectionString.Name = "tbDataExportManagerConnectionString";
            this.tbDataExportManagerConnectionString.Size = new System.Drawing.Size(938, 23);
            this.tbDataExportManagerConnectionString.TabIndex = 5;
            // 
            // btnCheckDataExportManager
            // 
            this.btnCheckDataExportManager.Location = new System.Drawing.Point(4, 30);
            this.btnCheckDataExportManager.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCheckDataExportManager.Name = "btnCheckDataExportManager";
            this.btnCheckDataExportManager.Size = new System.Drawing.Size(75, 27);
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
            this.pReferenceACatalogue.Controls.Add(this.btnBrowseForCatalogue);
            this.pReferenceACatalogue.Controls.Add(this.btnCheckCatalogue);
            this.pReferenceACatalogue.Location = new System.Drawing.Point(192, 22);
            this.pReferenceACatalogue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pReferenceACatalogue.Name = "pReferenceACatalogue";
            this.pReferenceACatalogue.Size = new System.Drawing.Size(1027, 66);
            this.pReferenceACatalogue.TabIndex = 8;
            // 
            // btnBrowseForCatalogue
            // 
            this.btnBrowseForCatalogue.Location = new System.Drawing.Point(4, 6);
            this.btnBrowseForCatalogue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBrowseForCatalogue.Name = "btnBrowseForCatalogue";
            this.btnBrowseForCatalogue.Size = new System.Drawing.Size(75, 27);
            this.btnBrowseForCatalogue.TabIndex = 3;
            this.btnBrowseForCatalogue.Text = "Browse...";
            this.btnBrowseForCatalogue.UseVisualStyleBackColor = true;
            this.btnBrowseForCatalogue.Click += new System.EventHandler(this.btnBrowseForCatalogue_Click);
            // 
            // btnCheckCatalogue
            // 
            this.btnCheckCatalogue.Location = new System.Drawing.Point(4, 32);
            this.btnCheckCatalogue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCheckCatalogue.Name = "btnCheckCatalogue";
            this.btnCheckCatalogue.Size = new System.Drawing.Size(75, 27);
            this.btnCheckCatalogue.TabIndex = 3;
            this.btnCheckCatalogue.Text = "Check";
            this.btnCheckCatalogue.UseVisualStyleBackColor = true;
            this.btnCheckCatalogue.Click += new System.EventHandler(this.btnCheckCatalogue_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(56, 88);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(121, 15);
            this.label8.TabIndex = 4;
            this.label8.Text = "Data Export Manager:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 10);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "Result:";
            // 
            // checksUI1
            // 
            this.checksUI1.AllowsYesNoToAll = true;
            this.checksUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checksUI1.Location = new System.Drawing.Point(4, 29);
            this.checksUI1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(867, 441);
            this.checksUI1.TabIndex = 0;
            // 
            // btnCreateSuite
            // 
            this.btnCreateSuite.Location = new System.Drawing.Point(108, 144);
            this.btnCreateSuite.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCreateSuite.Name = "btnCreateSuite";
            this.btnCreateSuite.Size = new System.Drawing.Size(75, 27);
            this.btnCreateSuite.TabIndex = 2;
            this.btnCreateSuite.Text = "Create";
            this.btnCreateSuite.UseVisualStyleBackColor = true;
            this.btnCreateSuite.Click += new System.EventHandler(this.btnCreateSuite_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(54, 30);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 15);
            this.label5.TabIndex = 1;
            this.label5.Text = "Server:";
            // 
            // tbSuiteServer
            // 
            this.tbSuiteServer.Location = new System.Drawing.Point(108, 27);
            this.tbSuiteServer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbSuiteServer.Name = "tbSuiteServer";
            this.tbSuiteServer.Size = new System.Drawing.Size(166, 23);
            this.tbSuiteServer.TabIndex = 0;
            this.tbSuiteServer.Text = "localhost\\sqlexpress";
            // 
            // tbDatabasePrefix
            // 
            this.tbDatabasePrefix.Location = new System.Drawing.Point(108, 56);
            this.tbDatabasePrefix.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbDatabasePrefix.Name = "tbDatabasePrefix";
            this.tbDatabasePrefix.Size = new System.Drawing.Size(166, 23);
            this.tbDatabasePrefix.TabIndex = 1;
            this.tbDatabasePrefix.Text = "RDMP_";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 59);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 15);
            this.label6.TabIndex = 3;
            this.label6.Text = "Database Prefix:";
            // 
            // btnCreateNew
            // 
            this.btnCreateNew.Location = new System.Drawing.Point(4, 42);
            this.btnCreateNew.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCreateNew.Name = "btnCreateNew";
            this.btnCreateNew.Size = new System.Drawing.Size(259, 27);
            this.btnCreateNew.TabIndex = 13;
            this.btnCreateNew.Text = "I want to create new Platform Databases";
            this.btnCreateNew.UseVisualStyleBackColor = true;
            this.btnCreateNew.Click += new System.EventHandler(this.btnCreateNew_Click);
            // 
            // pChooseOption
            // 
            this.pChooseOption.Controls.Add(this.btnUseExisting);
            this.pChooseOption.Controls.Add(this.btnCreateNew);
            this.pChooseOption.Controls.Add(this.label7);
            this.pChooseOption.Location = new System.Drawing.Point(330, 347);
            this.pChooseOption.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pChooseOption.Name = "pChooseOption";
            this.pChooseOption.Size = new System.Drawing.Size(758, 115);
            this.pChooseOption.TabIndex = 14;
            // 
            // btnUseExisting
            // 
            this.btnUseExisting.Location = new System.Drawing.Point(270, 42);
            this.btnUseExisting.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnUseExisting.Name = "btnUseExisting";
            this.btnUseExisting.Size = new System.Drawing.Size(471, 27);
            this.btnUseExisting.TabIndex = 13;
            this.btnUseExisting.Text = "Our organisation already has RDMP Platform Databases that I want to connect to";
            this.btnUseExisting.UseVisualStyleBackColor = true;
            this.btnUseExisting.Click += new System.EventHandler(this.btnUseExisting_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 9);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(199, 15);
            this.label7.TabIndex = 0;
            this.label7.Text = "Which best describes your situation?";
            // 
            // gbCreateNew
            // 
            this.gbCreateNew.Controls.Add(this.cbCreateExampleDatasets);
            this.gbCreateNew.Controls.Add(this.gbSqlAuthentication);
            this.gbCreateNew.Controls.Add(this.gbExampleDatasets);
            this.gbCreateNew.Controls.Add(this.tbOtherKeywords);
            this.gbCreateNew.Controls.Add(this.tbCreateDatabaseTimeout);
            this.gbCreateNew.Controls.Add(this.label14);
            this.gbCreateNew.Controls.Add(this.label13);
            this.gbCreateNew.Controls.Add(this.label9);
            this.gbCreateNew.Controls.Add(this.tbSuiteServer);
            this.gbCreateNew.Controls.Add(this.btnBack1);
            this.gbCreateNew.Controls.Add(this.tbDatabasePrefix);
            this.gbCreateNew.Controls.Add(this.label5);
            this.gbCreateNew.Controls.Add(this.btnCreateSuite);
            this.gbCreateNew.Controls.Add(this.label6);
            this.gbCreateNew.Location = new System.Drawing.Point(559, 252);
            this.gbCreateNew.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbCreateNew.Name = "gbCreateNew";
            this.gbCreateNew.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbCreateNew.Size = new System.Drawing.Size(888, 180);
            this.gbCreateNew.TabIndex = 15;
            this.gbCreateNew.TabStop = false;
            this.gbCreateNew.Text = "Create New Platform Databases";
            this.gbCreateNew.Visible = false;
            // 
            // gbSqlAuthentication
            // 
            this.gbSqlAuthentication.Controls.Add(this.tbUsername);
            this.gbSqlAuthentication.Controls.Add(this.label3);
            this.gbSqlAuthentication.Controls.Add(this.label4);
            this.gbSqlAuthentication.Controls.Add(this.tbPassword);
            this.gbSqlAuthentication.Location = new System.Drawing.Point(281, 25);
            this.gbSqlAuthentication.Name = "gbSqlAuthentication";
            this.gbSqlAuthentication.Size = new System.Drawing.Size(237, 76);
            this.gbSqlAuthentication.TabIndex = 15;
            this.gbSqlAuthentication.TabStop = false;
            this.gbSqlAuthentication.Text = "Sql Authentication*";
            // 
            // tbUsername
            // 
            this.tbUsername.Location = new System.Drawing.Point(80, 17);
            this.tbUsername.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(149, 23);
            this.tbUsername.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 22);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Username:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 45);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Password:";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(80, 42);
            this.tbPassword.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(149, 23);
            this.tbPassword.TabIndex = 4;
            this.tbPassword.UseSystemPasswordChar = true;
            // 
            // gbExampleDatasets
            // 
            this.gbExampleDatasets.Controls.Add(this.label10);
            this.gbExampleDatasets.Controls.Add(this.label12);
            this.gbExampleDatasets.Controls.Add(this.tbPeopleCount);
            this.gbExampleDatasets.Controls.Add(this.label11);
            this.gbExampleDatasets.Controls.Add(this.tbRowCount);
            this.gbExampleDatasets.Controls.Add(this.tbSeed);
            this.gbExampleDatasets.Enabled = false;
            this.gbExampleDatasets.Location = new System.Drawing.Point(536, 33);
            this.gbExampleDatasets.Name = "gbExampleDatasets";
            this.gbExampleDatasets.Size = new System.Drawing.Size(230, 100);
            this.gbExampleDatasets.TabIndex = 14;
            this.gbExampleDatasets.TabStop = false;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(51, 25);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 15);
            this.label10.TabIndex = 10;
            this.label10.Text = "Seed:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 49);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(82, 15);
            this.label12.TabIndex = 10;
            this.label12.Text = "Person Count:";
            // 
            // tbPeopleCount
            // 
            this.tbPeopleCount.Location = new System.Drawing.Point(94, 46);
            this.tbPeopleCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbPeopleCount.Name = "tbPeopleCount";
            this.tbPeopleCount.Size = new System.Drawing.Size(129, 23);
            this.tbPeopleCount.TabIndex = 11;
            this.tbPeopleCount.TextChanged += new System.EventHandler(this.Tb_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(20, 74);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(69, 15);
            this.label11.TabIndex = 10;
            this.label11.Text = "Row Count:";
            // 
            // tbRowCount
            // 
            this.tbRowCount.Location = new System.Drawing.Point(94, 71);
            this.tbRowCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbRowCount.Name = "tbRowCount";
            this.tbRowCount.Size = new System.Drawing.Size(129, 23);
            this.tbRowCount.TabIndex = 11;
            this.tbRowCount.TextChanged += new System.EventHandler(this.Tb_TextChanged);
            // 
            // cbCreateExampleDatasets
            // 
            this.cbCreateExampleDatasets.AutoSize = true;
            this.cbCreateExampleDatasets.Location = new System.Drawing.Point(543, 33);
            this.cbCreateExampleDatasets.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbCreateExampleDatasets.Name = "cbCreateExampleDatasets";
            this.cbCreateExampleDatasets.Size = new System.Drawing.Size(118, 19);
            this.cbCreateExampleDatasets.TabIndex = 9;
            this.cbCreateExampleDatasets.Text = "Example Datasets";
            this.cbCreateExampleDatasets.UseVisualStyleBackColor = true;
            this.cbCreateExampleDatasets.CheckedChanged += new System.EventHandler(this.cbCreateExampleDatasets_CheckedChanged);
            // 
            // tbSeed
            // 
            this.tbSeed.Location = new System.Drawing.Point(94, 22);
            this.tbSeed.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbSeed.Name = "tbSeed";
            this.tbSeed.Size = new System.Drawing.Size(129, 23);
            this.tbSeed.TabIndex = 11;
            this.tbSeed.Text = "500";
            this.tbSeed.TextChanged += new System.EventHandler(this.Tb_TextChanged);
            // 
            // tbOtherKeywords
            // 
            this.tbOtherKeywords.Location = new System.Drawing.Point(108, 115);
            this.tbOtherKeywords.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbOtherKeywords.Name = "tbOtherKeywords";
            this.tbOtherKeywords.Size = new System.Drawing.Size(410, 23);
            this.tbOtherKeywords.TabIndex = 13;
            // 
            // tbCreateDatabaseTimeout
            // 
            this.tbCreateDatabaseTimeout.Location = new System.Drawing.Point(108, 85);
            this.tbCreateDatabaseTimeout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbCreateDatabaseTimeout.Name = "tbCreateDatabaseTimeout";
            this.tbCreateDatabaseTimeout.Size = new System.Drawing.Size(90, 23);
            this.tbCreateDatabaseTimeout.TabIndex = 13;
            this.tbCreateDatabaseTimeout.Text = "30";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(10, 118);
            this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(94, 15);
            this.label14.TabIndex = 12;
            this.label14.Text = "Other Keywords:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(10, 88);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(91, 15);
            this.label13.TabIndex = 12;
            this.label13.Text = "Create Timeout:";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(318, 156);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(564, 15);
            this.label9.TabIndex = 8;
            this.label9.Text = "(* Username and Password are Optional.  If omitted then Integrated Security will " +
    "be used - recommended)";
            // 
            // btnBack1
            // 
            this.btnBack1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBack1.Location = new System.Drawing.Point(8, 144);
            this.btnBack1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBack1.Name = "btnBack1";
            this.btnBack1.Size = new System.Drawing.Size(88, 27);
            this.btnBack1.TabIndex = 5;
            this.btnBack1.Text = "<< Back";
            this.btnBack1.UseVisualStyleBackColor = true;
            this.btnBack1.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // pResults
            // 
            this.pResults.Controls.Add(this.checksUI1);
            this.pResults.Controls.Add(this.label2);
            this.pResults.Location = new System.Drawing.Point(14, 223);
            this.pResults.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pResults.Name = "pResults";
            this.pResults.Size = new System.Drawing.Size(874, 473);
            this.pResults.TabIndex = 16;
            // 
            // ChoosePlatformDatabasesUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1477, 710);
            this.Controls.Add(this.gbCreateNew);
            this.Controls.Add(this.pChooseOption);
            this.Controls.Add(this.gbUseExisting);
            this.Controls.Add(this.pResults);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ChoosePlatformDatabasesUI";
            this.Text = "Platform Databases";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ChooseDatabase_KeyUp);
            this.gbUseExisting.ResumeLayout(false);
            this.gbUseExisting.PerformLayout();
            this.pReferenceADataExport.ResumeLayout(false);
            this.pReferenceADataExport.PerformLayout();
            this.pReferenceACatalogue.ResumeLayout(false);
            this.pReferenceACatalogue.PerformLayout();
            this.pChooseOption.ResumeLayout(false);
            this.pChooseOption.PerformLayout();
            this.gbCreateNew.ResumeLayout(false);
            this.gbCreateNew.PerformLayout();
            this.gbSqlAuthentication.ResumeLayout(false);
            this.gbSqlAuthentication.PerformLayout();
            this.gbExampleDatasets.ResumeLayout(false);
            this.gbExampleDatasets.PerformLayout();
            this.pResults.ResumeLayout(false);
            this.pResults.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private ConnectionStringTextBox tbCatalogueConnectionString;
        private System.Windows.Forms.Button btnSaveAndClose;
        private System.Windows.Forms.GroupBox gbUseExisting;
        private System.Windows.Forms.Label label2;
        private ConnectionStringTextBox tbDataExportManagerConnectionString;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnCheckDataExportManager;
        private System.Windows.Forms.Button btnCheckCatalogue;
        private ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.Button btnCreateSuite;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbSuiteServer;
        private System.Windows.Forms.TextBox tbDatabasePrefix;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel pReferenceACatalogue;
        private System.Windows.Forms.Panel pReferenceADataExport;
        private System.Windows.Forms.Button btnBrowseForCatalogue;
        private System.Windows.Forms.Button btnCreateNew;
        private System.Windows.Forms.Panel pChooseOption;
        private System.Windows.Forms.Button btnUseExisting;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox gbCreateNew;
        private System.Windows.Forms.Button btnBack1;
        private System.Windows.Forms.Button btnBack2;
        private System.Windows.Forms.Button btnBrowseForDataExport;
        private System.Windows.Forms.Panel pResults;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox cbCreateExampleDatasets;
        private System.Windows.Forms.TextBox tbSeed;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbRowCount;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tbPeopleCount;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btnCreateYamlFile;
        private System.Windows.Forms.GroupBox gbExampleDatasets;
        private System.Windows.Forms.TextBox tbOtherKeywords;
        private System.Windows.Forms.TextBox tbCreateDatabaseTimeout;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox gbSqlAuthentication;
    }
}