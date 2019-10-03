using System;
using Rdmp.UI.SimpleControls;
using ReusableUIComponents;

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
            this.tbCatalogueConnectionString = new ConnectionStringTextBox();
            this.btnSaveAndClose = new System.Windows.Forms.Button();
            this.gbUseExisting = new System.Windows.Forms.GroupBox();
            this.btnBack2 = new System.Windows.Forms.Button();
            this.pReferenceADataExport = new System.Windows.Forms.Panel();
            this.btnBrowseForDataExport = new System.Windows.Forms.Button();
            this.tbDataExportManagerConnectionString = new ConnectionStringTextBox();
            this.btnCheckDataExportManager = new System.Windows.Forms.Button();
            this.pReferenceACatalogue = new System.Windows.Forms.Panel();
            this.btnBrowseForCatalogue = new System.Windows.Forms.Button();
            this.btnCheckCatalogue = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.checksUI1 = new ChecksUI.ChecksUI();
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
            this.tbSeed = new System.Windows.Forms.TextBox();
            this.tbRowCount = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.tbPeopleCount = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.cbCreateExampleDatasets = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBack1 = new System.Windows.Forms.Button();
            this.pResults = new System.Windows.Forms.Panel();
            this.gbUseExisting.SuspendLayout();
            this.pReferenceADataExport.SuspendLayout();
            this.pReferenceACatalogue.SuspendLayout();
            this.pChooseOption.SuspendLayout();
            this.gbCreateNew.SuspendLayout();
            this.pResults.SuspendLayout();
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
            this.tbCatalogueConnectionString.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            this.tbCatalogueConnectionString.ForeColor = System.Drawing.Color.Black;
            this.tbCatalogueConnectionString.Location = new System.Drawing.Point(72, 7);
            this.tbCatalogueConnectionString.Name = "tbCatalogueConnectionString";
            this.tbCatalogueConnectionString.Size = new System.Drawing.Size(804, 20);
            this.tbCatalogueConnectionString.TabIndex = 1;
            this.tbCatalogueConnectionString.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbCatalogueConnectionString_KeyUp);
            // 
            // btnSaveAndClose
            // 
            this.btnSaveAndClose.Location = new System.Drawing.Point(460, 145);
            this.btnSaveAndClose.Name = "btnSaveAndClose";
            this.btnSaveAndClose.Size = new System.Drawing.Size(274, 23);
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
            this.gbUseExisting.Location = new System.Drawing.Point(6, 12);
            this.gbUseExisting.Name = "gbUseExisting";
            this.gbUseExisting.Size = new System.Drawing.Size(1051, 174);
            this.gbUseExisting.TabIndex = 8;
            this.gbUseExisting.TabStop = false;
            this.gbUseExisting.Text = "Connect to existing Platform Databases (Enter Connection Strings)";
            this.gbUseExisting.Visible = false;
            // 
            // btnBack2
            // 
            this.btnBack2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBack2.Location = new System.Drawing.Point(6, 145);
            this.btnBack2.Name = "btnBack2";
            this.btnBack2.Size = new System.Drawing.Size(75, 23);
            this.btnBack2.TabIndex = 10;
            this.btnBack2.Text = "<< Back";
            this.btnBack2.UseVisualStyleBackColor = true;
            this.btnBack2.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // pReferenceADataExport
            // 
            this.pReferenceADataExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pReferenceADataExport.Controls.Add(this.btnBrowseForDataExport);
            this.pReferenceADataExport.Controls.Add(this.tbDataExportManagerConnectionString);
            this.pReferenceADataExport.Controls.Add(this.btnCheckDataExportManager);
            this.pReferenceADataExport.Location = new System.Drawing.Point(165, 81);
            this.pReferenceADataExport.Name = "pReferenceADataExport";
            this.pReferenceADataExport.Size = new System.Drawing.Size(880, 52);
            this.pReferenceADataExport.TabIndex = 9;
            // 
            // btnBrowseForDataExport
            // 
            this.btnBrowseForDataExport.Location = new System.Drawing.Point(3, 3);
            this.btnBrowseForDataExport.Name = "btnBrowseForDataExport";
            this.btnBrowseForDataExport.Size = new System.Drawing.Size(64, 23);
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
            this.tbDataExportManagerConnectionString.Location = new System.Drawing.Point(72, 3);
            this.tbDataExportManagerConnectionString.Name = "tbDataExportManagerConnectionString";
            this.tbDataExportManagerConnectionString.Size = new System.Drawing.Size(805, 20);
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
            this.pReferenceACatalogue.Controls.Add(this.btnBrowseForCatalogue);
            this.pReferenceACatalogue.Controls.Add(this.btnCheckCatalogue);
            this.pReferenceACatalogue.Location = new System.Drawing.Point(165, 19);
            this.pReferenceACatalogue.Name = "pReferenceACatalogue";
            this.pReferenceACatalogue.Size = new System.Drawing.Size(880, 57);
            this.pReferenceACatalogue.TabIndex = 8;
            // 
            // btnBrowseForCatalogue
            // 
            this.btnBrowseForCatalogue.Location = new System.Drawing.Point(3, 5);
            this.btnBrowseForCatalogue.Name = "btnBrowseForCatalogue";
            this.btnBrowseForCatalogue.Size = new System.Drawing.Size(64, 23);
            this.btnBrowseForCatalogue.TabIndex = 3;
            this.btnBrowseForCatalogue.Text = "Browse...";
            this.btnBrowseForCatalogue.UseVisualStyleBackColor = true;
            this.btnBrowseForCatalogue.Click += new System.EventHandler(this.btnBrowseForCatalogue_Click);
            // 
            // btnCheckCatalogue
            // 
            this.btnCheckCatalogue.Location = new System.Drawing.Point(3, 28);
            this.btnCheckCatalogue.Name = "btnCheckCatalogue";
            this.btnCheckCatalogue.Size = new System.Drawing.Size(64, 23);
            this.btnCheckCatalogue.TabIndex = 3;
            this.btnCheckCatalogue.Text = "Check";
            this.btnCheckCatalogue.UseVisualStyleBackColor = true;
            this.btnCheckCatalogue.Click += new System.EventHandler(this.btnCheckCatalogue_Click);
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
            this.label2.Location = new System.Drawing.Point(3, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Result:";
            // 
            // checksUI1
            // 
            this.checksUI1.AllowsYesNoToAll = true;
            this.checksUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checksUI1.Location = new System.Drawing.Point(3, 25);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(743, 382);
            this.checksUI1.TabIndex = 0;
            // 
            // btnCreateSuite
            // 
            this.btnCreateSuite.Location = new System.Drawing.Point(452, 19);
            this.btnCreateSuite.Name = "btnCreateSuite";
            this.btnCreateSuite.Size = new System.Drawing.Size(107, 23);
            this.btnCreateSuite.TabIndex = 2;
            this.btnCreateSuite.Text = "Create";
            this.btnCreateSuite.UseVisualStyleBackColor = true;
            this.btnCreateSuite.Click += new System.EventHandler(this.btnCreateSuite_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Server:";
            // 
            // tbSuiteServer
            // 
            this.tbSuiteServer.Location = new System.Drawing.Point(59, 20);
            this.tbSuiteServer.Name = "tbSuiteServer";
            this.tbSuiteServer.Size = new System.Drawing.Size(143, 20);
            this.tbSuiteServer.TabIndex = 0;
            this.tbSuiteServer.Text = "localhost\\sqlexpress";
            // 
            // tbDatabasePrefix
            // 
            this.tbDatabasePrefix.Location = new System.Drawing.Point(304, 21);
            this.tbDatabasePrefix.Name = "tbDatabasePrefix";
            this.tbDatabasePrefix.Size = new System.Drawing.Size(143, 20);
            this.tbDatabasePrefix.TabIndex = 1;
            this.tbDatabasePrefix.Text = "RDMP_";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(213, 24);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(85, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Database Prefix:";
            // 
            // btnCreateNew
            // 
            this.btnCreateNew.Location = new System.Drawing.Point(3, 36);
            this.btnCreateNew.Name = "btnCreateNew";
            this.btnCreateNew.Size = new System.Drawing.Size(222, 23);
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
            this.pChooseOption.Location = new System.Drawing.Point(283, 301);
            this.pChooseOption.Name = "pChooseOption";
            this.pChooseOption.Size = new System.Drawing.Size(650, 100);
            this.pChooseOption.TabIndex = 14;
            // 
            // btnUseExisting
            // 
            this.btnUseExisting.Location = new System.Drawing.Point(231, 36);
            this.btnUseExisting.Name = "btnUseExisting";
            this.btnUseExisting.Size = new System.Drawing.Size(404, 23);
            this.btnUseExisting.TabIndex = 13;
            this.btnUseExisting.Text = "Our organisation already has RDMP Platform Databases that I want to connect to";
            this.btnUseExisting.UseVisualStyleBackColor = true;
            this.btnUseExisting.Click += new System.EventHandler(this.btnUseExisting_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 8);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(180, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Which best describes your situation?";
            // 
            // gbCreateNew
            // 
            this.gbCreateNew.Controls.Add(this.tbSeed);
            this.gbCreateNew.Controls.Add(this.tbRowCount);
            this.gbCreateNew.Controls.Add(this.label11);
            this.gbCreateNew.Controls.Add(this.label10);
            this.gbCreateNew.Controls.Add(this.tbPeopleCount);
            this.gbCreateNew.Controls.Add(this.label12);
            this.gbCreateNew.Controls.Add(this.cbCreateExampleDatasets);
            this.gbCreateNew.Controls.Add(this.label9);
            this.gbCreateNew.Controls.Add(this.tbPassword);
            this.gbCreateNew.Controls.Add(this.label4);
            this.gbCreateNew.Controls.Add(this.tbUsername);
            this.gbCreateNew.Controls.Add(this.label3);
            this.gbCreateNew.Controls.Add(this.tbSuiteServer);
            this.gbCreateNew.Controls.Add(this.btnBack1);
            this.gbCreateNew.Controls.Add(this.tbDatabasePrefix);
            this.gbCreateNew.Controls.Add(this.label5);
            this.gbCreateNew.Controls.Add(this.btnCreateSuite);
            this.gbCreateNew.Controls.Add(this.label6);
            this.gbCreateNew.Location = new System.Drawing.Point(479, 407);
            this.gbCreateNew.Name = "gbCreateNew";
            this.gbCreateNew.Size = new System.Drawing.Size(761, 131);
            this.gbCreateNew.TabIndex = 15;
            this.gbCreateNew.TabStop = false;
            this.gbCreateNew.Text = "Create New Platform Databases";
            this.gbCreateNew.Visible = false;
            // 
            // tbSeed
            // 
            this.tbSeed.Location = new System.Drawing.Point(592, 46);
            this.tbSeed.Name = "tbSeed";
            this.tbSeed.Size = new System.Drawing.Size(163, 20);
            this.tbSeed.TabIndex = 11;
            this.tbSeed.Text = "500";
            this.tbSeed.TextChanged += new System.EventHandler(this.Tb_TextChanged);
            // 
            // tbRowCount
            // 
            this.tbRowCount.Location = new System.Drawing.Point(592, 88);
            this.tbRowCount.Name = "tbRowCount";
            this.tbRowCount.Size = new System.Drawing.Size(163, 20);
            this.tbRowCount.TabIndex = 11;
            this.tbRowCount.TextChanged += new System.EventHandler(this.Tb_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(523, 91);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(63, 13);
            this.label11.TabIndex = 10;
            this.label11.Text = "Row Count:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(551, 50);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 13);
            this.label10.TabIndex = 10;
            this.label10.Text = "Seed:";
            // 
            // tbPeopleCount
            // 
            this.tbPeopleCount.Location = new System.Drawing.Point(592, 67);
            this.tbPeopleCount.Name = "tbPeopleCount";
            this.tbPeopleCount.Size = new System.Drawing.Size(163, 20);
            this.tbPeopleCount.TabIndex = 11;
            this.tbPeopleCount.TextChanged += new System.EventHandler(this.Tb_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(518, 70);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(74, 13);
            this.label12.TabIndex = 10;
            this.label12.Text = "Person Count:";
            // 
            // cbCreateExampleDatasets
            // 
            this.cbCreateExampleDatasets.AutoSize = true;
            this.cbCreateExampleDatasets.Location = new System.Drawing.Point(592, 23);
            this.cbCreateExampleDatasets.Name = "cbCreateExampleDatasets";
            this.cbCreateExampleDatasets.Size = new System.Drawing.Size(111, 17);
            this.cbCreateExampleDatasets.TabIndex = 9;
            this.cbCreateExampleDatasets.Text = "Example Datasets";
            this.cbCreateExampleDatasets.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 70);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(500, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "(* Username and Password are Optional.  If omitted then Integrated Security will " +
    "be used - recommended)";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(285, 47);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.Size = new System.Drawing.Size(143, 20);
            this.tbPassword.TabIndex = 4;
            this.tbPassword.UseSystemPasswordChar = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(221, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "*Password:";
            // 
            // tbUsername
            // 
            this.tbUsername.Location = new System.Drawing.Point(76, 47);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(143, 20);
            this.tbUsername.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "*Username:";
            // 
            // btnBack1
            // 
            this.btnBack1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBack1.Location = new System.Drawing.Point(6, 99);
            this.btnBack1.Name = "btnBack1";
            this.btnBack1.Size = new System.Drawing.Size(75, 23);
            this.btnBack1.TabIndex = 5;
            this.btnBack1.Text = "<< Back";
            this.btnBack1.UseVisualStyleBackColor = true;
            this.btnBack1.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // pResults
            // 
            this.pResults.Controls.Add(this.checksUI1);
            this.pResults.Controls.Add(this.label2);
            this.pResults.Location = new System.Drawing.Point(12, 193);
            this.pResults.Name = "pResults";
            this.pResults.Size = new System.Drawing.Size(749, 410);
            this.pResults.TabIndex = 16;
            // 
            // ChoosePlatformDatabasesUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1266, 615);
            this.Controls.Add(this.gbCreateNew);
            this.Controls.Add(this.pChooseOption);
            this.Controls.Add(this.gbUseExisting);
            this.Controls.Add(this.pResults);
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
    }
}