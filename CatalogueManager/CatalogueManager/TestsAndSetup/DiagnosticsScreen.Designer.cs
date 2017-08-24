using ReusableUIComponents;

namespace CatalogueManager.TestsAndSetup
{
    partial class DiagnosticsScreen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiagnosticsScreen));
            this.gbChecks = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnCatalogueCheck = new System.Windows.Forms.Button();
            this.btnCatalogueTableNames = new System.Windows.Forms.Button();
            this.btnListBadAssemblies = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnCatalogueFields = new System.Windows.Forms.Button();
            this.btnCohortDatabase = new System.Windows.Forms.Button();
            this.btnDataExportManagerFields = new System.Windows.Forms.Button();
            this.btnCheckANOConfigurations = new System.Windows.Forms.Button();
            this.btnCreateTestDataset = new System.Windows.Forms.Button();
            this.gbSetupTestDataset = new System.Windows.Forms.GroupBox();
            this.lblRawServer = new System.Windows.Forms.Label();
            this.gbAddDataExportFunctionality = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cbxProjectExtractionDirectory = new System.Windows.Forms.ComboBox();
            this.btnAddExportFunctionality = new System.Windows.Forms.Button();
            this.gbAnonymisation = new System.Windows.Forms.GroupBox();
            this.lblIdentifierDumpDatabaseState = new System.Windows.Forms.Label();
            this.lblANODatabaseState = new System.Windows.Forms.Label();
            this.btnCheckIdentifierDump = new System.Windows.Forms.Button();
            this.btnCheckANOStore = new System.Windows.Forms.Button();
            this.btnCreateNewIdentifierDump = new System.Windows.Forms.Button();
            this.btnCreateNewANOStore = new System.Windows.Forms.Button();
            this.tbDumpConnectionString = new ReusableUIComponents.ConnectionStringTextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbANOConnectionString = new ReusableUIComponents.ConnectionStringTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.cbIncludeAnonymisation = new System.Windows.Forms.CheckBox();
            this.lblDatasetFolder = new System.Windows.Forms.Label();
            this.cbxDataSetFolder = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbTestDatasetServer = new ReusableUIComponents.ConnectionStringTextBox();
            this.btnImportHospitalAdmissions = new System.Windows.Forms.Button();
            this.gbLogging = new System.Windows.Forms.GroupBox();
            this.btnRefreshDataLoadTasks = new System.Windows.Forms.Button();
            this.btnCreateNewLoggingDatabase = new System.Windows.Forms.Button();
            this.tbLoggingConnectionString = new ReusableUIComponents.ConnectionStringTextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.lblLoggingDatabaseState = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lbLoggingTask = new System.Windows.Forms.Label();
            this.ddLoggingTask = new System.Windows.Forms.ComboBox();
            this.btnViewOriginalException = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tpProgress = new System.Windows.Forms.TabPage();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.label8 = new System.Windows.Forms.Label();
            this.gbChecks.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.gbSetupTestDataset.SuspendLayout();
            this.gbAddDataExportFunctionality.SuspendLayout();
            this.gbAnonymisation.SuspendLayout();
            this.gbLogging.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tpProgress.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbChecks
            // 
            this.gbChecks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbChecks.Controls.Add(this.groupBox4);
            this.gbChecks.Controls.Add(this.btnListBadAssemblies);
            this.gbChecks.Controls.Add(this.groupBox2);
            this.gbChecks.Controls.Add(this.btnCheckANOConfigurations);
            this.gbChecks.Location = new System.Drawing.Point(12, 12);
            this.gbChecks.Name = "gbChecks";
            this.gbChecks.Size = new System.Drawing.Size(1130, 104);
            this.gbChecks.TabIndex = 0;
            this.gbChecks.TabStop = false;
            this.gbChecks.Text = "Checks:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnCatalogueCheck);
            this.groupBox4.Controls.Add(this.btnCatalogueTableNames);
            this.groupBox4.Location = new System.Drawing.Point(578, 17);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(377, 77);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Entity Checks";
            // 
            // btnCatalogueCheck
            // 
            this.btnCatalogueCheck.Location = new System.Drawing.Point(6, 19);
            this.btnCatalogueCheck.Name = "btnCatalogueCheck";
            this.btnCatalogueCheck.Size = new System.Drawing.Size(114, 23);
            this.btnCatalogueCheck.TabIndex = 5;
            this.btnCatalogueCheck.Text = "Catalogue.Check()";
            this.btnCatalogueCheck.UseVisualStyleBackColor = true;
            this.btnCatalogueCheck.Click += new System.EventHandler(this.btnCatalogueCheck_Click);
            // 
            // btnCatalogueTableNames
            // 
            this.btnCatalogueTableNames.Location = new System.Drawing.Point(126, 19);
            this.btnCatalogueTableNames.Name = "btnCatalogueTableNames";
            this.btnCatalogueTableNames.Size = new System.Drawing.Size(212, 23);
            this.btnCatalogueTableNames.TabIndex = 4;
            this.btnCatalogueTableNames.Text = "DodgyNamedTableAndColumnsChecker";
            this.btnCatalogueTableNames.UseVisualStyleBackColor = true;
            this.btnCatalogueTableNames.Click += new System.EventHandler(this.btnCatalogueTableNames_Click);
            // 
            // btnListBadAssemblies
            // 
            this.btnListBadAssemblies.Location = new System.Drawing.Point(196, 71);
            this.btnListBadAssemblies.Name = "btnListBadAssemblies";
            this.btnListBadAssemblies.Size = new System.Drawing.Size(179, 23);
            this.btnListBadAssemblies.TabIndex = 3;
            this.btnListBadAssemblies.Text = "Evaluate MEF Exports (dlls)";
            this.btnListBadAssemblies.UseVisualStyleBackColor = true;
            this.btnListBadAssemblies.Click += new System.EventHandler(this.btnListBadAssemblies_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnCatalogueFields);
            this.groupBox2.Controls.Add(this.btnCohortDatabase);
            this.groupBox2.Controls.Add(this.btnDataExportManagerFields);
            this.groupBox2.Location = new System.Drawing.Point(6, 19);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(566, 49);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Check For Missing Fields";
            // 
            // btnCatalogueFields
            // 
            this.btnCatalogueFields.Location = new System.Drawing.Point(5, 19);
            this.btnCatalogueFields.Name = "btnCatalogueFields";
            this.btnCatalogueFields.Size = new System.Drawing.Size(180, 23);
            this.btnCatalogueFields.TabIndex = 0;
            this.btnCatalogueFields.Text = "Catalogue Database";
            this.btnCatalogueFields.UseVisualStyleBackColor = true;
            this.btnCatalogueFields.Click += new System.EventHandler(this.btnCatalogueFields_Click);
            // 
            // btnCohortDatabase
            // 
            this.btnCohortDatabase.Location = new System.Drawing.Point(376, 19);
            this.btnCohortDatabase.Name = "btnCohortDatabase";
            this.btnCohortDatabase.Size = new System.Drawing.Size(180, 23);
            this.btnCohortDatabase.TabIndex = 2;
            this.btnCohortDatabase.Text = "Cohort Database";
            this.btnCohortDatabase.UseVisualStyleBackColor = true;
            this.btnCohortDatabase.Click += new System.EventHandler(this.btnCohortDatabase_Click);
            // 
            // btnDataExportManagerFields
            // 
            this.btnDataExportManagerFields.Location = new System.Drawing.Point(190, 19);
            this.btnDataExportManagerFields.Name = "btnDataExportManagerFields";
            this.btnDataExportManagerFields.Size = new System.Drawing.Size(180, 23);
            this.btnDataExportManagerFields.TabIndex = 1;
            this.btnDataExportManagerFields.Text = "Data Export Manager Database";
            this.btnDataExportManagerFields.UseVisualStyleBackColor = true;
            this.btnDataExportManagerFields.Click += new System.EventHandler(this.btnDataExportManagerFields_Click);
            // 
            // btnCheckANOConfigurations
            // 
            this.btnCheckANOConfigurations.Location = new System.Drawing.Point(11, 71);
            this.btnCheckANOConfigurations.Name = "btnCheckANOConfigurations";
            this.btnCheckANOConfigurations.Size = new System.Drawing.Size(180, 23);
            this.btnCheckANOConfigurations.TabIndex = 2;
            this.btnCheckANOConfigurations.Text = "Anonymisation Configurations";
            this.btnCheckANOConfigurations.UseVisualStyleBackColor = true;
            this.btnCheckANOConfigurations.Click += new System.EventHandler(this.btnCheckANOConfigurations_Click);
            // 
            // btnCreateTestDataset
            // 
            this.btnCreateTestDataset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCreateTestDataset.Enabled = false;
            this.btnCreateTestDataset.Location = new System.Drawing.Point(5, 397);
            this.btnCreateTestDataset.Name = "btnCreateTestDataset";
            this.btnCreateTestDataset.Size = new System.Drawing.Size(180, 23);
            this.btnCreateTestDataset.TabIndex = 8;
            this.btnCreateTestDataset.Text = "Create Test Dataset";
            this.btnCreateTestDataset.UseVisualStyleBackColor = true;
            this.btnCreateTestDataset.Click += new System.EventHandler(this.btnCreateTestDataset_Click);
            // 
            // gbSetupTestDataset
            // 
            this.gbSetupTestDataset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSetupTestDataset.Controls.Add(this.lblRawServer);
            this.gbSetupTestDataset.Controls.Add(this.gbAddDataExportFunctionality);
            this.gbSetupTestDataset.Controls.Add(this.gbAnonymisation);
            this.gbSetupTestDataset.Controls.Add(this.cbIncludeAnonymisation);
            this.gbSetupTestDataset.Controls.Add(this.lblDatasetFolder);
            this.gbSetupTestDataset.Controls.Add(this.cbxDataSetFolder);
            this.gbSetupTestDataset.Controls.Add(this.label2);
            this.gbSetupTestDataset.Controls.Add(this.label1);
            this.gbSetupTestDataset.Controls.Add(this.tbTestDatasetServer);
            this.gbSetupTestDataset.Controls.Add(this.btnImportHospitalAdmissions);
            this.gbSetupTestDataset.Controls.Add(this.btnCreateTestDataset);
            this.gbSetupTestDataset.Controls.Add(this.gbLogging);
            this.gbSetupTestDataset.Location = new System.Drawing.Point(6, 6);
            this.gbSetupTestDataset.Name = "gbSetupTestDataset";
            this.gbSetupTestDataset.Size = new System.Drawing.Size(1749, 477);
            this.gbSetupTestDataset.TabIndex = 1;
            this.gbSetupTestDataset.TabStop = false;
            this.gbSetupTestDataset.Text = "Setup Test:";
            // 
            // lblRawServer
            // 
            this.lblRawServer.AutoSize = true;
            this.lblRawServer.Location = new System.Drawing.Point(755, 22);
            this.lblRawServer.Name = "lblRawServer";
            this.lblRawServer.Size = new System.Drawing.Size(10, 13);
            this.lblRawServer.TabIndex = 2;
            this.lblRawServer.Text = "-";
            // 
            // gbAddDataExportFunctionality
            // 
            this.gbAddDataExportFunctionality.Controls.Add(this.label7);
            this.gbAddDataExportFunctionality.Controls.Add(this.cbxProjectExtractionDirectory);
            this.gbAddDataExportFunctionality.Controls.Add(this.btnAddExportFunctionality);
            this.gbAddDataExportFunctionality.Enabled = false;
            this.gbAddDataExportFunctionality.Location = new System.Drawing.Point(191, 393);
            this.gbAddDataExportFunctionality.Name = "gbAddDataExportFunctionality";
            this.gbAddDataExportFunctionality.Size = new System.Drawing.Size(903, 79);
            this.gbAddDataExportFunctionality.TabIndex = 10;
            this.gbAddDataExportFunctionality.TabStop = false;
            this.gbAddDataExportFunctionality.Text = "Add Data Export Functionality";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(102, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Extraction Directory:";
            // 
            // cbxProjectExtractionDirectory
            // 
            this.cbxProjectExtractionDirectory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbxProjectExtractionDirectory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.cbxProjectExtractionDirectory.FormattingEnabled = true;
            this.cbxProjectExtractionDirectory.Location = new System.Drawing.Point(119, 19);
            this.cbxProjectExtractionDirectory.Name = "cbxProjectExtractionDirectory";
            this.cbxProjectExtractionDirectory.Size = new System.Drawing.Size(778, 21);
            this.cbxProjectExtractionDirectory.TabIndex = 1;
            // 
            // btnAddExportFunctionality
            // 
            this.btnAddExportFunctionality.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddExportFunctionality.Location = new System.Drawing.Point(119, 46);
            this.btnAddExportFunctionality.Name = "btnAddExportFunctionality";
            this.btnAddExportFunctionality.Size = new System.Drawing.Size(180, 23);
            this.btnAddExportFunctionality.TabIndex = 2;
            this.btnAddExportFunctionality.Text = "Add Data Export to TestDataset";
            this.btnAddExportFunctionality.UseVisualStyleBackColor = true;
            this.btnAddExportFunctionality.Click += new System.EventHandler(this.btnAddExportFunctionality_Click);
            // 
            // gbAnonymisation
            // 
            this.gbAnonymisation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbAnonymisation.Controls.Add(this.lblIdentifierDumpDatabaseState);
            this.gbAnonymisation.Controls.Add(this.lblANODatabaseState);
            this.gbAnonymisation.Controls.Add(this.btnCheckIdentifierDump);
            this.gbAnonymisation.Controls.Add(this.btnCheckANOStore);
            this.gbAnonymisation.Controls.Add(this.btnCreateNewIdentifierDump);
            this.gbAnonymisation.Controls.Add(this.btnCreateNewANOStore);
            this.gbAnonymisation.Controls.Add(this.tbDumpConnectionString);
            this.gbAnonymisation.Controls.Add(this.label4);
            this.gbAnonymisation.Controls.Add(this.tbANOConnectionString);
            this.gbAnonymisation.Controls.Add(this.label3);
            this.gbAnonymisation.Controls.Add(this.label6);
            this.gbAnonymisation.Controls.Add(this.label15);
            this.gbAnonymisation.Enabled = false;
            this.gbAnonymisation.Location = new System.Drawing.Point(6, 244);
            this.gbAnonymisation.Name = "gbAnonymisation";
            this.gbAnonymisation.Size = new System.Drawing.Size(1079, 143);
            this.gbAnonymisation.TabIndex = 7;
            this.gbAnonymisation.TabStop = false;
            // 
            // lblIdentifierDumpDatabaseState
            // 
            this.lblIdentifierDumpDatabaseState.AutoSize = true;
            this.lblIdentifierDumpDatabaseState.Location = new System.Drawing.Point(940, 97);
            this.lblIdentifierDumpDatabaseState.Name = "lblIdentifierDumpDatabaseState";
            this.lblIdentifierDumpDatabaseState.Size = new System.Drawing.Size(10, 13);
            this.lblIdentifierDumpDatabaseState.TabIndex = 10;
            this.lblIdentifierDumpDatabaseState.Text = "-";
            // 
            // lblANODatabaseState
            // 
            this.lblANODatabaseState.AutoSize = true;
            this.lblANODatabaseState.Location = new System.Drawing.Point(940, 46);
            this.lblANODatabaseState.Name = "lblANODatabaseState";
            this.lblANODatabaseState.Size = new System.Drawing.Size(10, 13);
            this.lblANODatabaseState.TabIndex = 10;
            this.lblANODatabaseState.Text = "-";
            // 
            // btnCheckIdentifierDump
            // 
            this.btnCheckIdentifierDump.Location = new System.Drawing.Point(180, 97);
            this.btnCheckIdentifierDump.Name = "btnCheckIdentifierDump";
            this.btnCheckIdentifierDump.Size = new System.Drawing.Size(64, 23);
            this.btnCheckIdentifierDump.TabIndex = 8;
            this.btnCheckIdentifierDump.Text = "Check";
            this.btnCheckIdentifierDump.UseVisualStyleBackColor = true;
            this.btnCheckIdentifierDump.Click += new System.EventHandler(this.btnCheckIdentifierDump_Click);
            // 
            // btnCheckANOStore
            // 
            this.btnCheckANOStore.Location = new System.Drawing.Point(180, 44);
            this.btnCheckANOStore.Name = "btnCheckANOStore";
            this.btnCheckANOStore.Size = new System.Drawing.Size(64, 23);
            this.btnCheckANOStore.TabIndex = 3;
            this.btnCheckANOStore.Text = "Check";
            this.btnCheckANOStore.UseVisualStyleBackColor = true;
            this.btnCheckANOStore.Click += new System.EventHandler(this.btnCheckANOStore_Click);
            // 
            // btnCreateNewIdentifierDump
            // 
            this.btnCreateNewIdentifierDump.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewIdentifierDump.Location = new System.Drawing.Point(972, 71);
            this.btnCreateNewIdentifierDump.Name = "btnCreateNewIdentifierDump";
            this.btnCreateNewIdentifierDump.Size = new System.Drawing.Size(101, 23);
            this.btnCreateNewIdentifierDump.TabIndex = 7;
            this.btnCreateNewIdentifierDump.Text = "Create New...";
            this.btnCreateNewIdentifierDump.UseVisualStyleBackColor = true;
            this.btnCreateNewIdentifierDump.Click += new System.EventHandler(this.btnCreateNewIdentifierDump_Click);
            // 
            // btnCreateNewANOStore
            // 
            this.btnCreateNewANOStore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewANOStore.Location = new System.Drawing.Point(972, 20);
            this.btnCreateNewANOStore.Name = "btnCreateNewANOStore";
            this.btnCreateNewANOStore.Size = new System.Drawing.Size(101, 23);
            this.btnCreateNewANOStore.TabIndex = 2;
            this.btnCreateNewANOStore.Text = "Create New...";
            this.btnCreateNewANOStore.UseVisualStyleBackColor = true;
            this.btnCreateNewANOStore.Click += new System.EventHandler(this.btnCreateNewANOStore_Click);
            // 
            // tbDumpConnectionString
            // 
            this.tbDumpConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDumpConnectionString.DatabaseType = ReusableLibraryCode.DatabaseType.MicrosoftSQLServer;
            this.tbDumpConnectionString.Location = new System.Drawing.Point(180, 73);
            this.tbDumpConnectionString.Name = "tbDumpConnectionString";
            this.tbDumpConnectionString.Size = new System.Drawing.Size(786, 20);
            this.tbDumpConnectionString.TabIndex = 6;
            this.tbDumpConnectionString.Leave += new System.EventHandler(this.tbDumpConnectionString_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(168, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Identifier Dump Connection String:";
            // 
            // tbANOConnectionString
            // 
            this.tbANOConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbANOConnectionString.DatabaseType = ReusableLibraryCode.DatabaseType.MicrosoftSQLServer;
            this.tbANOConnectionString.Location = new System.Drawing.Point(180, 22);
            this.tbANOConnectionString.Name = "tbANOConnectionString";
            this.tbANOConnectionString.Size = new System.Drawing.Size(786, 20);
            this.tbANOConnectionString.TabIndex = 1;
            this.tbANOConnectionString.Leave += new System.EventHandler(this.tbANOConnectionString_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(163, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "ANO Identifier Connection String:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(278, 97);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(435, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "(Where superfluous identifiable data is saved before discarding/diluting it in yo" +
    "ur live tables)";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(278, 46);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(405, 13);
            this.label15.TabIndex = 4;
            this.label15.Text = "(Where identifiers are mapped to anonymous identifiers - on data load, not extrac" +
    "tion)";
            // 
            // cbIncludeAnonymisation
            // 
            this.cbIncludeAnonymisation.AutoSize = true;
            this.cbIncludeAnonymisation.Location = new System.Drawing.Point(5, 221);
            this.cbIncludeAnonymisation.Name = "cbIncludeAnonymisation";
            this.cbIncludeAnonymisation.Size = new System.Drawing.Size(132, 17);
            this.cbIncludeAnonymisation.TabIndex = 6;
            this.cbIncludeAnonymisation.Text = "Include Anonymisation";
            this.cbIncludeAnonymisation.UseVisualStyleBackColor = true;
            this.cbIncludeAnonymisation.CheckedChanged += new System.EventHandler(this.cbIncludeAnonymisation_CheckedChanged);
            // 
            // lblDatasetFolder
            // 
            this.lblDatasetFolder.AutoSize = true;
            this.lblDatasetFolder.Location = new System.Drawing.Point(739, 51);
            this.lblDatasetFolder.Name = "lblDatasetFolder";
            this.lblDatasetFolder.Size = new System.Drawing.Size(10, 13);
            this.lblDatasetFolder.TabIndex = 4;
            this.lblDatasetFolder.Text = "-";
            // 
            // cbxDataSetFolder
            // 
            this.cbxDataSetFolder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cbxDataSetFolder.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.cbxDataSetFolder.FormattingEnabled = true;
            this.cbxDataSetFolder.Location = new System.Drawing.Point(187, 48);
            this.cbxDataSetFolder.Name = "cbxDataSetFolder";
            this.cbxDataSetFolder.Size = new System.Drawing.Size(546, 21);
            this.cbxDataSetFolder.TabIndex = 3;
            this.cbxDataSetFolder.Leave += new System.EventHandler(this.cbxDataSetFolder_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Dataset Folder:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server:";
            // 
            // tbTestDatasetServer
            // 
            this.tbTestDatasetServer.DatabaseType = ReusableLibraryCode.DatabaseType.MicrosoftSQLServer;
            this.tbTestDatasetServer.Location = new System.Drawing.Point(187, 22);
            this.tbTestDatasetServer.Name = "tbTestDatasetServer";
            this.tbTestDatasetServer.Size = new System.Drawing.Size(562, 20);
            this.tbTestDatasetServer.TabIndex = 0;
            this.tbTestDatasetServer.Leave += new System.EventHandler(this.tbTestDatasetServer_Leave);
            // 
            // btnImportHospitalAdmissions
            // 
            this.btnImportHospitalAdmissions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnImportHospitalAdmissions.Enabled = false;
            this.btnImportHospitalAdmissions.Location = new System.Drawing.Point(5, 426);
            this.btnImportHospitalAdmissions.Name = "btnImportHospitalAdmissions";
            this.btnImportHospitalAdmissions.Size = new System.Drawing.Size(180, 23);
            this.btnImportHospitalAdmissions.TabIndex = 9;
            this.btnImportHospitalAdmissions.Text = "Import Hospital Admissions";
            this.btnImportHospitalAdmissions.UseVisualStyleBackColor = true;
            this.btnImportHospitalAdmissions.Click += new System.EventHandler(this.btnImportHospitalAdmissions_Click);
            // 
            // gbLogging
            // 
            this.gbLogging.Controls.Add(this.btnRefreshDataLoadTasks);
            this.gbLogging.Controls.Add(this.btnCreateNewLoggingDatabase);
            this.gbLogging.Controls.Add(this.tbLoggingConnectionString);
            this.gbLogging.Controls.Add(this.label16);
            this.gbLogging.Controls.Add(this.lblLoggingDatabaseState);
            this.gbLogging.Controls.Add(this.label12);
            this.gbLogging.Controls.Add(this.lbLoggingTask);
            this.gbLogging.Controls.Add(this.ddLoggingTask);
            this.gbLogging.Location = new System.Drawing.Point(0, 75);
            this.gbLogging.Name = "gbLogging";
            this.gbLogging.Size = new System.Drawing.Size(1094, 133);
            this.gbLogging.TabIndex = 5;
            this.gbLogging.TabStop = false;
            this.gbLogging.Text = "Logging";
            // 
            // btnRefreshDataLoadTasks
            // 
            this.btnRefreshDataLoadTasks.Location = new System.Drawing.Point(601, 93);
            this.btnRefreshDataLoadTasks.Name = "btnRefreshDataLoadTasks";
            this.btnRefreshDataLoadTasks.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshDataLoadTasks.TabIndex = 7;
            this.btnRefreshDataLoadTasks.Text = "Refresh";
            this.btnRefreshDataLoadTasks.UseVisualStyleBackColor = true;
            this.btnRefreshDataLoadTasks.Click += new System.EventHandler(this.btnRefreshDataLoadTasks_Click);
            // 
            // btnCreateNewLoggingDatabase
            // 
            this.btnCreateNewLoggingDatabase.Location = new System.Drawing.Point(53, 50);
            this.btnCreateNewLoggingDatabase.Name = "btnCreateNewLoggingDatabase";
            this.btnCreateNewLoggingDatabase.Size = new System.Drawing.Size(200, 23);
            this.btnCreateNewLoggingDatabase.TabIndex = 4;
            this.btnCreateNewLoggingDatabase.Text = "Create A New Logging Database...";
            this.btnCreateNewLoggingDatabase.UseVisualStyleBackColor = true;
            this.btnCreateNewLoggingDatabase.Click += new System.EventHandler(this.btnCreateNewLoggingDatabase_Click);
            // 
            // tbLoggingConnectionString
            // 
            this.tbLoggingConnectionString.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.tbLoggingConnectionString.DatabaseType = ReusableLibraryCode.DatabaseType.MicrosoftSQLServer;
            this.tbLoggingConnectionString.Location = new System.Drawing.Point(186, 24);
            this.tbLoggingConnectionString.Name = "tbLoggingConnectionString";
            this.tbLoggingConnectionString.Size = new System.Drawing.Size(563, 20);
            this.tbLoggingConnectionString.TabIndex = 0;
            this.tbLoggingConnectionString.Leave += new System.EventHandler(this.tbLoggingConnectionString_Leave);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(10, 27);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(135, 13);
            this.label16.TabIndex = 13;
            this.label16.Text = "Logging Connection String:";
            // 
            // lblLoggingDatabaseState
            // 
            this.lblLoggingDatabaseState.AutoSize = true;
            this.lblLoggingDatabaseState.Location = new System.Drawing.Point(755, 27);
            this.lblLoggingDatabaseState.Name = "lblLoggingDatabaseState";
            this.lblLoggingDatabaseState.Size = new System.Drawing.Size(10, 13);
            this.lblLoggingDatabaseState.TabIndex = 2;
            this.lblLoggingDatabaseState.Text = "-";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(26, 55);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(24, 13);
            this.label12.TabIndex = 3;
            this.label12.Text = "Or:";
            // 
            // lbLoggingTask
            // 
            this.lbLoggingTask.AutoSize = true;
            this.lbLoggingTask.Location = new System.Drawing.Point(12, 98);
            this.lbLoggingTask.Name = "lbLoggingTask";
            this.lbLoggingTask.Size = new System.Drawing.Size(75, 13);
            this.lbLoggingTask.TabIndex = 5;
            this.lbLoggingTask.Text = "Logging Task:";
            // 
            // ddLoggingTask
            // 
            this.ddLoggingTask.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddLoggingTask.FormattingEnabled = true;
            this.ddLoggingTask.Location = new System.Drawing.Point(186, 95);
            this.ddLoggingTask.Name = "ddLoggingTask";
            this.ddLoggingTask.Size = new System.Drawing.Size(409, 21);
            this.ddLoggingTask.Sorted = true;
            this.ddLoggingTask.TabIndex = 6;
            this.ddLoggingTask.SelectedIndexChanged += new System.EventHandler(this.ddLoggingTask_SelectedIndexChanged);
            // 
            // btnViewOriginalException
            // 
            this.btnViewOriginalException.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnViewOriginalException.Enabled = false;
            this.btnViewOriginalException.Location = new System.Drawing.Point(981, 647);
            this.btnViewOriginalException.Name = "btnViewOriginalException";
            this.btnViewOriginalException.Size = new System.Drawing.Size(144, 23);
            this.btnViewOriginalException.TabIndex = 2;
            this.btnViewOriginalException.Text = "View Original Exception";
            this.btnViewOriginalException.UseVisualStyleBackColor = true;
            this.btnViewOriginalException.Click += new System.EventHandler(this.btnViewOriginalException_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tpProgress);
            this.tabControl1.Location = new System.Drawing.Point(12, 122);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1119, 519);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.gbSetupTestDataset);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1111, 493);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Databases Setup Test";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tpProgress
            // 
            this.tpProgress.Controls.Add(this.checksUI1);
            this.tpProgress.Location = new System.Drawing.Point(4, 22);
            this.tpProgress.Name = "tpProgress";
            this.tpProgress.Padding = new System.Windows.Forms.Padding(3);
            this.tpProgress.Size = new System.Drawing.Size(1111, 493);
            this.tpProgress.TabIndex = 1;
            this.tpProgress.Text = "Progress";
            this.tpProgress.UseVisualStyleBackColor = true;
            // 
            // checksUI1
            // 
            this.checksUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checksUI1.Location = new System.Drawing.Point(3, 3);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(1105, 487);
            this.checksUI1.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(15, 644);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(683, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "\"Please report any unauthorized database interactions to your direct superior. Re" +
    "member: A smooth operation is everyone\'s direct responsibility.\"";
            // 
            // DiagnosticsScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1137, 675);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.gbChecks);
            this.Controls.Add(this.btnViewOriginalException);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DiagnosticsScreen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Diagnostics Screen";
            this.Load += new System.EventHandler(this.DiagnosticsScreen_Load);
            this.gbChecks.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.gbSetupTestDataset.ResumeLayout(false);
            this.gbSetupTestDataset.PerformLayout();
            this.gbAddDataExportFunctionality.ResumeLayout(false);
            this.gbAddDataExportFunctionality.PerformLayout();
            this.gbAnonymisation.ResumeLayout(false);
            this.gbAnonymisation.PerformLayout();
            this.gbLogging.ResumeLayout(false);
            this.gbLogging.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tpProgress.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbChecks;
        private System.Windows.Forms.Button btnCreateTestDataset;
        private System.Windows.Forms.Button btnCheckANOConfigurations;
        private System.Windows.Forms.GroupBox gbSetupTestDataset;
        private System.Windows.Forms.Label label1;
        private ConnectionStringTextBox tbTestDatasetServer;
        private System.Windows.Forms.ComboBox cbxDataSetFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnDataExportManagerFields;
        private System.Windows.Forms.Button btnCatalogueFields;
        private System.Windows.Forms.ComboBox ddLoggingTask;
        private System.Windows.Forms.Label lbLoggingTask;
        private System.Windows.Forms.Label lblLoggingDatabaseState;
        private System.Windows.Forms.Button btnViewOriginalException;
        private System.Windows.Forms.CheckBox cbIncludeAnonymisation;
        private System.Windows.Forms.GroupBox gbAnonymisation;
        private System.Windows.Forms.Label lblDatasetFolder;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnCohortDatabase;
        private System.Windows.Forms.Button btnAddExportFunctionality;
        private System.Windows.Forms.GroupBox gbAddDataExportFunctionality;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cbxProjectExtractionDirectory;
        private System.Windows.Forms.Button btnListBadAssemblies;
        private System.Windows.Forms.Button btnImportHospitalAdmissions;
        private System.Windows.Forms.Button btnCreateNewLoggingDatabase;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label15;
        private ConnectionStringTextBox tbLoggingConnectionString;
        private System.Windows.Forms.Label label16;
        private ConnectionStringTextBox tbANOConnectionString;
        private System.Windows.Forms.Label label3;
        private ConnectionStringTextBox tbDumpConnectionString;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox gbLogging;
        private System.Windows.Forms.Label lblRawServer;
        private System.Windows.Forms.Button btnRefreshDataLoadTasks;
        private System.Windows.Forms.Button btnCatalogueTableNames;
        private System.Windows.Forms.Button btnCreateNewIdentifierDump;
        private System.Windows.Forms.Button btnCreateNewANOStore;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnCheckIdentifierDump;
        private System.Windows.Forms.Button btnCheckANOStore;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnCatalogueCheck;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tpProgress;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.Label lblIdentifierDumpDatabaseState;
        private System.Windows.Forms.Label lblANODatabaseState;
        private System.Windows.Forms.Label label8;
    }
}