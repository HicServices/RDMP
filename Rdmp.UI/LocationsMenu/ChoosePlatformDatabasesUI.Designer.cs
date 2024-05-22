using System;
using Rdmp.Core;
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
            label1 = new System.Windows.Forms.Label();
            tbCatalogueConnectionString = new ConnectionStringTextBox();
            btnSaveAndClose = new System.Windows.Forms.Button();
            gbUseExisting = new System.Windows.Forms.GroupBox();
            btnBack2 = new System.Windows.Forms.Button();
            pReferenceADataExport = new System.Windows.Forms.Panel();
            btnCreateYamlFile = new System.Windows.Forms.Button();
            btnBrowseForDataExport = new System.Windows.Forms.Button();
            tbDataExportManagerConnectionString = new ConnectionStringTextBox();
            btnCheckDataExportManager = new System.Windows.Forms.Button();
            pReferenceACatalogue = new System.Windows.Forms.Panel();
            btnBrowseForCatalogue = new System.Windows.Forms.Button();
            btnCheckCatalogue = new System.Windows.Forms.Button();
            label8 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            checksUI1 = new ChecksUI.ChecksUI();
            btnCreateSuite = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            tbSuiteServer = new System.Windows.Forms.TextBox();
            tbDatabasePrefix = new System.Windows.Forms.TextBox();
            label6 = new System.Windows.Forms.Label();
            btnCreateNew = new System.Windows.Forms.Button();
            pChooseOption = new System.Windows.Forms.Panel();
            btnUseExisting = new System.Windows.Forms.Button();
            label7 = new System.Windows.Forms.Label();
            gbCreateNew = new System.Windows.Forms.GroupBox();
            cbCreateExampleDatasets = new System.Windows.Forms.CheckBox();
            cbCreateLoggingServer = new System.Windows.Forms.CheckBox();
            gbSqlAuthentication = new System.Windows.Forms.GroupBox();
            tbUsername = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            tbPassword = new System.Windows.Forms.TextBox();
            gbExampleDatasets = new System.Windows.Forms.GroupBox();
            label10 = new System.Windows.Forms.Label();
            label12 = new System.Windows.Forms.Label();
            tbPeopleCount = new System.Windows.Forms.TextBox();
            label11 = new System.Windows.Forms.Label();
            tbRowCount = new System.Windows.Forms.TextBox();
            tbSeed = new System.Windows.Forms.TextBox();
            tbOtherKeywords = new System.Windows.Forms.TextBox();
            tbCreateDatabaseTimeout = new System.Windows.Forms.TextBox();
            label14 = new System.Windows.Forms.Label();
            label13 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            btnBack1 = new System.Windows.Forms.Button();
            pResults = new System.Windows.Forms.Panel();
            btnUseYamlFile = new System.Windows.Forms.Button();
            gbUseExisting.SuspendLayout();
            pReferenceADataExport.SuspendLayout();
            pReferenceACatalogue.SuspendLayout();
            pChooseOption.SuspendLayout();
            gbCreateNew.SuspendLayout();
            gbSqlAuthentication.SuspendLayout();
            gbExampleDatasets.SuspendLayout();
            pResults.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(97, 25);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(64, 15);
            label1.TabIndex = 0;
            label1.Text = "Catalogue:";
            // 
            // tbCatalogueConnectionString
            // 
            tbCatalogueConnectionString.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbCatalogueConnectionString.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            tbCatalogueConnectionString.ForeColor = System.Drawing.Color.Black;
            tbCatalogueConnectionString.Location = new System.Drawing.Point(84, 8);
            tbCatalogueConnectionString.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbCatalogueConnectionString.Name = "tbCatalogueConnectionString";
            tbCatalogueConnectionString.Size = new System.Drawing.Size(937, 23);
            tbCatalogueConnectionString.TabIndex = 1;
            tbCatalogueConnectionString.KeyUp += tbCatalogueConnectionString_KeyUp;
            // 
            // btnSaveAndClose
            // 
            btnSaveAndClose.Location = new System.Drawing.Point(537, 167);
            btnSaveAndClose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnSaveAndClose.Name = "btnSaveAndClose";
            btnSaveAndClose.Size = new System.Drawing.Size(320, 27);
            btnSaveAndClose.TabIndex = 10;
            btnSaveAndClose.Text = "Save and Close";
            btnSaveAndClose.UseVisualStyleBackColor = true;
            btnSaveAndClose.Click += btnSaveAndClose_Click;
            // 
            // gbUseExisting
            // 
            gbUseExisting.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbUseExisting.Controls.Add(btnBack2);
            gbUseExisting.Controls.Add(pReferenceADataExport);
            gbUseExisting.Controls.Add(btnSaveAndClose);
            gbUseExisting.Controls.Add(pReferenceACatalogue);
            gbUseExisting.Controls.Add(label8);
            gbUseExisting.Controls.Add(label1);
            gbUseExisting.Location = new System.Drawing.Point(7, 14);
            gbUseExisting.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbUseExisting.Name = "gbUseExisting";
            gbUseExisting.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbUseExisting.Size = new System.Drawing.Size(1226, 201);
            gbUseExisting.TabIndex = 8;
            gbUseExisting.TabStop = false;
            gbUseExisting.Text = "Connect to existing Platform Databases (Enter Connection Strings)";
            gbUseExisting.Visible = false;
            // 
            // btnBack2
            // 
            btnBack2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnBack2.Location = new System.Drawing.Point(7, 167);
            btnBack2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBack2.Name = "btnBack2";
            btnBack2.Size = new System.Drawing.Size(88, 27);
            btnBack2.TabIndex = 10;
            btnBack2.Text = "<< Back";
            btnBack2.UseVisualStyleBackColor = true;
            btnBack2.Click += btnBack_Click;
            // 
            // pReferenceADataExport
            // 
            pReferenceADataExport.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pReferenceADataExport.Controls.Add(btnUseYamlFile);
            pReferenceADataExport.Controls.Add(btnCreateYamlFile);
            pReferenceADataExport.Controls.Add(btnBrowseForDataExport);
            pReferenceADataExport.Controls.Add(tbDataExportManagerConnectionString);
            pReferenceADataExport.Controls.Add(btnCheckDataExportManager);
            pReferenceADataExport.Location = new System.Drawing.Point(192, 93);
            pReferenceADataExport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pReferenceADataExport.Name = "pReferenceADataExport";
            pReferenceADataExport.Size = new System.Drawing.Size(1027, 60);
            pReferenceADataExport.TabIndex = 9;
            // 
            // btnCreateYamlFile
            // 
            btnCreateYamlFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnCreateYamlFile.Location = new System.Drawing.Point(914, 32);
            btnCreateYamlFile.Name = "btnCreateYamlFile";
            btnCreateYamlFile.Size = new System.Drawing.Size(108, 23);
            btnCreateYamlFile.TabIndex = 9;
            btnCreateYamlFile.Text = "Save as yaml...";
            btnCreateYamlFile.UseVisualStyleBackColor = true;
            btnCreateYamlFile.Click += btnCreateYamlFile_Click;
            // 
            // btnBrowseForDataExport
            // 
            btnBrowseForDataExport.Location = new System.Drawing.Point(4, 3);
            btnBrowseForDataExport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBrowseForDataExport.Name = "btnBrowseForDataExport";
            btnBrowseForDataExport.Size = new System.Drawing.Size(75, 27);
            btnBrowseForDataExport.TabIndex = 8;
            btnBrowseForDataExport.Text = "Browse...";
            btnBrowseForDataExport.UseVisualStyleBackColor = true;
            btnBrowseForDataExport.Click += btnBrowseForDataExport_Click;
            // 
            // tbDataExportManagerConnectionString
            // 
            tbDataExportManagerConnectionString.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbDataExportManagerConnectionString.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            tbDataExportManagerConnectionString.ForeColor = System.Drawing.Color.Black;
            tbDataExportManagerConnectionString.Location = new System.Drawing.Point(84, 3);
            tbDataExportManagerConnectionString.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbDataExportManagerConnectionString.Name = "tbDataExportManagerConnectionString";
            tbDataExportManagerConnectionString.Size = new System.Drawing.Size(938, 23);
            tbDataExportManagerConnectionString.TabIndex = 5;
            // 
            // btnCheckDataExportManager
            // 
            btnCheckDataExportManager.Location = new System.Drawing.Point(4, 30);
            btnCheckDataExportManager.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnCheckDataExportManager.Name = "btnCheckDataExportManager";
            btnCheckDataExportManager.Size = new System.Drawing.Size(75, 27);
            btnCheckDataExportManager.TabIndex = 7;
            btnCheckDataExportManager.Text = "Check";
            btnCheckDataExportManager.UseVisualStyleBackColor = true;
            btnCheckDataExportManager.Click += btnCheckDataExportManager_Click;
            // 
            // pReferenceACatalogue
            // 
            pReferenceACatalogue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pReferenceACatalogue.Controls.Add(tbCatalogueConnectionString);
            pReferenceACatalogue.Controls.Add(btnBrowseForCatalogue);
            pReferenceACatalogue.Controls.Add(btnCheckCatalogue);
            pReferenceACatalogue.Location = new System.Drawing.Point(192, 22);
            pReferenceACatalogue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pReferenceACatalogue.Name = "pReferenceACatalogue";
            pReferenceACatalogue.Size = new System.Drawing.Size(1027, 66);
            pReferenceACatalogue.TabIndex = 8;
            // 
            // btnBrowseForCatalogue
            // 
            btnBrowseForCatalogue.Location = new System.Drawing.Point(4, 6);
            btnBrowseForCatalogue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBrowseForCatalogue.Name = "btnBrowseForCatalogue";
            btnBrowseForCatalogue.Size = new System.Drawing.Size(75, 27);
            btnBrowseForCatalogue.TabIndex = 3;
            btnBrowseForCatalogue.Text = "Browse...";
            btnBrowseForCatalogue.UseVisualStyleBackColor = true;
            btnBrowseForCatalogue.Click += btnBrowseForCatalogue_Click;
            // 
            // btnCheckCatalogue
            // 
            btnCheckCatalogue.Location = new System.Drawing.Point(4, 32);
            btnCheckCatalogue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnCheckCatalogue.Name = "btnCheckCatalogue";
            btnCheckCatalogue.Size = new System.Drawing.Size(75, 27);
            btnCheckCatalogue.TabIndex = 3;
            btnCheckCatalogue.Text = "Check";
            btnCheckCatalogue.UseVisualStyleBackColor = true;
            btnCheckCatalogue.Click += btnCheckCatalogue_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(56, 88);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(121, 15);
            label8.TabIndex = 4;
            label8.Text = "Data Export Manager:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(4, 10);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(42, 15);
            label2.TabIndex = 11;
            label2.Text = "Result:";
            // 
            // checksUI1
            // 
            checksUI1.AllowsYesNoToAll = true;
            checksUI1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            checksUI1.Location = new System.Drawing.Point(4, 29);
            checksUI1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            checksUI1.Name = "checksUI1";
            checksUI1.Size = new System.Drawing.Size(867, 441);
            checksUI1.TabIndex = 0;
            // 
            // btnCreateSuite
            // 
            btnCreateSuite.Location = new System.Drawing.Point(108, 144);
            btnCreateSuite.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnCreateSuite.Name = "btnCreateSuite";
            btnCreateSuite.Size = new System.Drawing.Size(75, 27);
            btnCreateSuite.TabIndex = 2;
            btnCreateSuite.Text = "Create";
            btnCreateSuite.UseVisualStyleBackColor = true;
            btnCreateSuite.Click += btnCreateSuite_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(54, 30);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(42, 15);
            label5.TabIndex = 1;
            label5.Text = "Server:";
            // 
            // tbSuiteServer
            // 
            tbSuiteServer.Location = new System.Drawing.Point(108, 27);
            tbSuiteServer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbSuiteServer.Name = "tbSuiteServer";
            tbSuiteServer.Size = new System.Drawing.Size(166, 23);
            tbSuiteServer.TabIndex = 0;
            tbSuiteServer.Text = "localhost\\sqlexpress";
            // 
            // tbDatabasePrefix
            // 
            tbDatabasePrefix.Location = new System.Drawing.Point(108, 56);
            tbDatabasePrefix.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbDatabasePrefix.Name = "tbDatabasePrefix";
            tbDatabasePrefix.Size = new System.Drawing.Size(166, 23);
            tbDatabasePrefix.TabIndex = 1;
            tbDatabasePrefix.Text = "RDMP_";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(10, 59);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(91, 15);
            label6.TabIndex = 3;
            label6.Text = "Database Prefix:";
            // 
            // btnCreateNew
            // 
            btnCreateNew.Location = new System.Drawing.Point(4, 42);
            btnCreateNew.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnCreateNew.Name = "btnCreateNew";
            btnCreateNew.Size = new System.Drawing.Size(259, 27);
            btnCreateNew.TabIndex = 13;
            btnCreateNew.Text = "I want to create new Platform Databases";
            btnCreateNew.UseVisualStyleBackColor = true;
            btnCreateNew.Click += btnCreateNew_Click;
            // 
            // pChooseOption
            // 
            pChooseOption.Controls.Add(btnUseExisting);
            pChooseOption.Controls.Add(btnCreateNew);
            pChooseOption.Controls.Add(label7);
            pChooseOption.Location = new System.Drawing.Point(330, 347);
            pChooseOption.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pChooseOption.Name = "pChooseOption";
            pChooseOption.Size = new System.Drawing.Size(758, 115);
            pChooseOption.TabIndex = 14;
            // 
            // btnUseExisting
            // 
            btnUseExisting.Location = new System.Drawing.Point(270, 42);
            btnUseExisting.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnUseExisting.Name = "btnUseExisting";
            btnUseExisting.Size = new System.Drawing.Size(471, 27);
            btnUseExisting.TabIndex = 13;
            btnUseExisting.Text = "Our organisation already has RDMP Platform Databases that I want to connect to";
            btnUseExisting.UseVisualStyleBackColor = true;
            btnUseExisting.Click += btnUseExisting_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(4, 9);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(199, 15);
            label7.TabIndex = 0;
            label7.Text = "Which best describes your situation?";
            // 
            // gbCreateNew
            // 
            gbCreateNew.Controls.Add(cbCreateExampleDatasets);
            gbCreateNew.Controls.Add(cbCreateLoggingServer);
            gbCreateNew.Controls.Add(gbSqlAuthentication);
            gbCreateNew.Controls.Add(gbExampleDatasets);
            gbCreateNew.Controls.Add(tbOtherKeywords);
            gbCreateNew.Controls.Add(tbCreateDatabaseTimeout);
            gbCreateNew.Controls.Add(label14);
            gbCreateNew.Controls.Add(label13);
            gbCreateNew.Controls.Add(label9);
            gbCreateNew.Controls.Add(tbSuiteServer);
            gbCreateNew.Controls.Add(btnBack1);
            gbCreateNew.Controls.Add(tbDatabasePrefix);
            gbCreateNew.Controls.Add(label5);
            gbCreateNew.Controls.Add(btnCreateSuite);
            gbCreateNew.Controls.Add(label6);
            gbCreateNew.Location = new System.Drawing.Point(559, 252);
            gbCreateNew.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbCreateNew.Name = "gbCreateNew";
            gbCreateNew.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbCreateNew.Size = new System.Drawing.Size(888, 180);
            gbCreateNew.TabIndex = 15;
            gbCreateNew.TabStop = false;
            gbCreateNew.Text = "Create New Platform Databases";
            gbCreateNew.Visible = false;
            // 
            // cbCreateExampleDatasets
            // 
            cbCreateExampleDatasets.AutoSize = true;
            cbCreateExampleDatasets.Location = new System.Drawing.Point(543, 33);
            cbCreateExampleDatasets.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbCreateExampleDatasets.Name = "cbCreateExampleDatasets";
            cbCreateExampleDatasets.Size = new System.Drawing.Size(118, 19);
            cbCreateExampleDatasets.TabIndex = 9;
            cbCreateExampleDatasets.Text = "Example Datasets";
            cbCreateExampleDatasets.UseVisualStyleBackColor = true;
            cbCreateExampleDatasets.CheckedChanged += cbCreateExampleDatasets_CheckedChanged;
            // 
            // cbCreateLoggingServer
            // 
            cbCreateLoggingServer.AutoSize = true;
            cbCreateLoggingServer.Checked = true;
            cbCreateLoggingServer.CheckState = System.Windows.Forms.CheckState.Checked;
            cbCreateLoggingServer.Location = new System.Drawing.Point(543, 156);
            cbCreateLoggingServer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbCreateLoggingServer.Name = "cbCreateLoggingServer";
            cbCreateLoggingServer.Size = new System.Drawing.Size(147, 19);
            cbCreateLoggingServer.TabIndex = 9;
            cbCreateLoggingServer.Text = "Create a logging server";
            cbCreateLoggingServer.UseVisualStyleBackColor = true;
            // 
            // gbSqlAuthentication
            // 
            gbSqlAuthentication.Controls.Add(tbUsername);
            gbSqlAuthentication.Controls.Add(label3);
            gbSqlAuthentication.Controls.Add(label4);
            gbSqlAuthentication.Controls.Add(tbPassword);
            gbSqlAuthentication.Location = new System.Drawing.Point(281, 25);
            gbSqlAuthentication.Name = "gbSqlAuthentication";
            gbSqlAuthentication.Size = new System.Drawing.Size(237, 76);
            gbSqlAuthentication.TabIndex = 15;
            gbSqlAuthentication.TabStop = false;
            gbSqlAuthentication.Text = "Sql Authentication*";
            // 
            // tbUsername
            // 
            tbUsername.Location = new System.Drawing.Point(80, 17);
            tbUsername.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbUsername.Name = "tbUsername";
            tbUsername.Size = new System.Drawing.Size(149, 23);
            tbUsername.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 22);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(63, 15);
            label3.TabIndex = 6;
            label3.Text = "Username:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(15, 45);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(60, 15);
            label4.TabIndex = 6;
            label4.Text = "Password:";
            // 
            // tbPassword
            // 
            tbPassword.Location = new System.Drawing.Point(80, 42);
            tbPassword.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbPassword.Name = "tbPassword";
            tbPassword.Size = new System.Drawing.Size(149, 23);
            tbPassword.TabIndex = 4;
            tbPassword.UseSystemPasswordChar = true;
            // 
            // gbExampleDatasets
            // 
            gbExampleDatasets.Controls.Add(label10);
            gbExampleDatasets.Controls.Add(label12);
            gbExampleDatasets.Controls.Add(tbPeopleCount);
            gbExampleDatasets.Controls.Add(label11);
            gbExampleDatasets.Controls.Add(tbRowCount);
            gbExampleDatasets.Controls.Add(tbSeed);
            gbExampleDatasets.Enabled = false;
            gbExampleDatasets.Location = new System.Drawing.Point(536, 33);
            gbExampleDatasets.Name = "gbExampleDatasets";
            gbExampleDatasets.Size = new System.Drawing.Size(230, 100);
            gbExampleDatasets.TabIndex = 14;
            gbExampleDatasets.TabStop = false;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(51, 25);
            label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(35, 15);
            label10.TabIndex = 10;
            label10.Text = "Seed:";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(7, 49);
            label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(82, 15);
            label12.TabIndex = 10;
            label12.Text = "Person Count:";
            // 
            // tbPeopleCount
            // 
            tbPeopleCount.Location = new System.Drawing.Point(94, 46);
            tbPeopleCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbPeopleCount.Name = "tbPeopleCount";
            tbPeopleCount.Size = new System.Drawing.Size(129, 23);
            tbPeopleCount.TabIndex = 11;
            tbPeopleCount.TextChanged += Tb_TextChanged;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(20, 74);
            label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(69, 15);
            label11.TabIndex = 10;
            label11.Text = "Row Count:";
            // 
            // tbRowCount
            // 
            tbRowCount.Location = new System.Drawing.Point(94, 71);
            tbRowCount.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbRowCount.Name = "tbRowCount";
            tbRowCount.Size = new System.Drawing.Size(129, 23);
            tbRowCount.TabIndex = 11;
            tbRowCount.TextChanged += Tb_TextChanged;
            // 
            // tbSeed
            // 
            tbSeed.Location = new System.Drawing.Point(94, 22);
            tbSeed.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbSeed.Name = "tbSeed";
            tbSeed.Size = new System.Drawing.Size(129, 23);
            tbSeed.TabIndex = 11;
            tbSeed.Text = "500";
            tbSeed.TextChanged += Tb_TextChanged;
            // 
            // tbOtherKeywords
            // 
            tbOtherKeywords.Location = new System.Drawing.Point(108, 115);
            tbOtherKeywords.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbOtherKeywords.Name = "tbOtherKeywords";
            tbOtherKeywords.Size = new System.Drawing.Size(410, 23);
            tbOtherKeywords.TabIndex = 13;
            // 
            // tbCreateDatabaseTimeout
            // 
            tbCreateDatabaseTimeout.Location = new System.Drawing.Point(108, 85);
            tbCreateDatabaseTimeout.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbCreateDatabaseTimeout.Name = "tbCreateDatabaseTimeout";
            tbCreateDatabaseTimeout.Size = new System.Drawing.Size(90, 23);
            tbCreateDatabaseTimeout.TabIndex = 13;
            tbCreateDatabaseTimeout.Text = "30";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new System.Drawing.Point(10, 118);
            label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label14.Name = "label14";
            label14.Size = new System.Drawing.Size(94, 15);
            label14.TabIndex = 12;
            label14.Text = "Other Keywords:";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(10, 88);
            label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(91, 15);
            label13.TabIndex = 12;
            label13.Text = "Create Timeout:";
            // 
            // label9
            // 
            label9.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(318, 156);
            label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(564, 15);
            label9.TabIndex = 8;
            label9.Text = "(* Username and Password are Optional.  If omitted then Integrated Security will be used - recommended)";
            // 
            // btnBack1
            // 
            btnBack1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnBack1.Location = new System.Drawing.Point(8, 144);
            btnBack1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBack1.Name = "btnBack1";
            btnBack1.Size = new System.Drawing.Size(88, 27);
            btnBack1.TabIndex = 5;
            btnBack1.Text = "<< Back";
            btnBack1.UseVisualStyleBackColor = true;
            btnBack1.Click += btnBack_Click;
            // 
            // pResults
            // 
            pResults.Controls.Add(checksUI1);
            pResults.Controls.Add(label2);
            pResults.Location = new System.Drawing.Point(14, 223);
            pResults.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pResults.Name = "pResults";
            pResults.Size = new System.Drawing.Size(874, 473);
            pResults.TabIndex = 16;
            // 
            // btnUseYamlFile
            // 
            btnUseYamlFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnUseYamlFile.Location = new System.Drawing.Point(800, 32);
            btnUseYamlFile.Name = "btnUseYamlFile";
            btnUseYamlFile.Size = new System.Drawing.Size(108, 23);
            btnUseYamlFile.TabIndex = 10;
            btnUseYamlFile.Text = "Use yaml File";
            btnUseYamlFile.UseVisualStyleBackColor = true;
            btnUseYamlFile.Click += btnUseYamlFile_Click;
            // 
            // ChoosePlatformDatabasesUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1477, 710);
            Controls.Add(gbCreateNew);
            Controls.Add(pChooseOption);
            Controls.Add(gbUseExisting);
            Controls.Add(pResults);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ChoosePlatformDatabasesUI";
            Text = "Platform Databases";
            KeyUp += ChooseDatabase_KeyUp;
            gbUseExisting.ResumeLayout(false);
            gbUseExisting.PerformLayout();
            pReferenceADataExport.ResumeLayout(false);
            pReferenceADataExport.PerformLayout();
            pReferenceACatalogue.ResumeLayout(false);
            pReferenceACatalogue.PerformLayout();
            pChooseOption.ResumeLayout(false);
            pChooseOption.PerformLayout();
            gbCreateNew.ResumeLayout(false);
            gbCreateNew.PerformLayout();
            gbSqlAuthentication.ResumeLayout(false);
            gbSqlAuthentication.PerformLayout();
            gbExampleDatasets.ResumeLayout(false);
            gbExampleDatasets.PerformLayout();
            pResults.ResumeLayout(false);
            pResults.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.CheckBox cbCreateLoggingServer;
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
        private System.Windows.Forms.Button btnUseYamlFile;
    }
}