using BrightIdeasSoftware;
using Rdmp.UI.ChecksUI;

namespace Rdmp.UI.Wizard
{
    partial class CreateNewDataExtractionProjectUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateNewDataExtractionProjectUI));
            this.label1 = new System.Windows.Forms.Label();
            this.tbProjectName = new System.Windows.Forms.TextBox();
            this.tbProjectNumber = new System.Windows.Forms.TextBox();
            this.tbExtractionDirectory = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.pbCohort = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.pbCohortFile = new System.Windows.Forms.PictureBox();
            this.lblCohortFile = new System.Windows.Forms.Label();
            this.btnSelectClearCohortFile = new System.Windows.Forms.Button();
            this.gbCic = new System.Windows.Forms.GroupBox();
            this.hlpCicPipe = new Rdmp.UI.SimpleControls.HelpIcon();
            this.ragCic = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.btnClearCohort = new System.Windows.Forms.Button();
            this.ddCicPipeline = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cbxCohort = new Rdmp.UI.SuggestComboBox();
            this.gbFile = new System.Windows.Forms.GroupBox();
            this.hlpFlatFilePipe = new Rdmp.UI.SimpleControls.HelpIcon();
            this.ddFilePipeline = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.ddExtractionPipeline = new System.Windows.Forms.ComboBox();
            this.btnExecute = new System.Windows.Forms.Button();
            this.ddCohortSources = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.pbCohortSources = new System.Windows.Forms.PictureBox();
            this.btnCreateNewCohortSource = new System.Windows.Forms.Button();
            this.ragExecute = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.ragProjectNumber = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.label11 = new System.Windows.Forms.Label();
            this.tbCohortName = new System.Windows.Forms.TextBox();
            this.cbDefineCohort = new System.Windows.Forms.CheckBox();
            this.gbCohortAndDatasets = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.hlpExtractionPipeline = new Rdmp.UI.SimpleControls.HelpIcon();
            this.hlpIdentifierAllocation = new Rdmp.UI.SimpleControls.HelpIcon();
            this.hlpDatasets = new Rdmp.UI.SimpleControls.HelpIcon();
            this.btnClearDatasets = new System.Windows.Forms.Button();
            this.lblDatasets = new System.Windows.Forms.Label();
            this.btnPackage = new System.Windows.Forms.Button();
            this.btnPick = new System.Windows.Forms.Button();
            this.cbxDatasets = new System.Windows.Forms.ComboBox();
            this.hlpDefineCohortAndDatasets = new Rdmp.UI.SimpleControls.HelpIcon();
            this.btnCancel = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortFile)).BeginInit();
            this.gbCic.SuspendLayout();
            this.gbFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortSources)).BeginInit();
            this.gbCohortAndDatasets.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Project Name:";
            // 
            // tbProjectName
            // 
            this.tbProjectName.Location = new System.Drawing.Point(125, 3);
            this.tbProjectName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbProjectName.Name = "tbProjectName";
            this.tbProjectName.Size = new System.Drawing.Size(400, 23);
            this.tbProjectName.TabIndex = 0;
            // 
            // tbProjectNumber
            // 
            this.tbProjectNumber.Location = new System.Drawing.Point(125, 33);
            this.tbProjectNumber.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbProjectNumber.Name = "tbProjectNumber";
            this.tbProjectNumber.Size = new System.Drawing.Size(170, 23);
            this.tbProjectNumber.TabIndex = 1;
            this.tbProjectNumber.TextChanged += new System.EventHandler(this.tbProjectNumber_TextChanged);
            // 
            // tbExtractionDirectory
            // 
            this.tbExtractionDirectory.Location = new System.Drawing.Point(125, 63);
            this.tbExtractionDirectory.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbExtractionDirectory.Name = "tbExtractionDirectory";
            this.tbExtractionDirectory.Size = new System.Drawing.Size(310, 23);
            this.tbExtractionDirectory.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 36);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Project Number:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 67);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "Extraction Directory:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(437, 62);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(88, 25);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // pbCohort
            // 
            this.pbCohort.Location = new System.Drawing.Point(72, 28);
            this.pbCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbCohort.Name = "pbCohort";
            this.pbCohort.Size = new System.Drawing.Size(23, 23);
            this.pbCohort.TabIndex = 5;
            this.pbCohort.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 32);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 15);
            this.label4.TabIndex = 3;
            this.label4.Text = "Cohort:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(568, 55);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 15);
            this.label5.TabIndex = 3;
            this.label5.Text = "OR";
            // 
            // pbCohortFile
            // 
            this.pbCohortFile.Location = new System.Drawing.Point(8, 28);
            this.pbCohortFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbCohortFile.Name = "pbCohortFile";
            this.pbCohortFile.Size = new System.Drawing.Size(23, 23);
            this.pbCohortFile.TabIndex = 8;
            this.pbCohortFile.TabStop = false;
            // 
            // lblCohortFile
            // 
            this.lblCohortFile.AutoSize = true;
            this.lblCohortFile.Location = new System.Drawing.Point(39, 33);
            this.lblCohortFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCohortFile.Name = "lblCohortFile";
            this.lblCohortFile.Size = new System.Drawing.Size(74, 15);
            this.lblCohortFile.TabIndex = 0;
            this.lblCohortFile.Text = "Cohort File...";
            // 
            // btnSelectClearCohortFile
            // 
            this.btnSelectClearCohortFile.Location = new System.Drawing.Point(137, 28);
            this.btnSelectClearCohortFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSelectClearCohortFile.Name = "btnSelectClearCohortFile";
            this.btnSelectClearCohortFile.Size = new System.Drawing.Size(88, 25);
            this.btnSelectClearCohortFile.TabIndex = 1;
            this.btnSelectClearCohortFile.Text = "Browse...";
            this.btnSelectClearCohortFile.UseVisualStyleBackColor = true;
            this.btnSelectClearCohortFile.Click += new System.EventHandler(this.btnSelectClearCohortFile_Click);
            // 
            // gbCic
            // 
            this.gbCic.Controls.Add(this.hlpCicPipe);
            this.gbCic.Controls.Add(this.ragCic);
            this.gbCic.Controls.Add(this.btnClearCohort);
            this.gbCic.Controls.Add(this.ddCicPipeline);
            this.gbCic.Controls.Add(this.label7);
            this.gbCic.Controls.Add(this.cbxCohort);
            this.gbCic.Controls.Add(this.label4);
            this.gbCic.Controls.Add(this.pbCohort);
            this.gbCic.Location = new System.Drawing.Point(8, 22);
            this.gbCic.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbCic.Name = "gbCic";
            this.gbCic.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbCic.Size = new System.Drawing.Size(552, 106);
            this.gbCic.TabIndex = 4;
            this.gbCic.TabStop = false;
            this.gbCic.Text = "RDMP Cohort Builder Query";
            // 
            // hlpCicPipe
            // 
            this.hlpCicPipe.BackColor = System.Drawing.Color.Transparent;
            this.hlpCicPipe.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hlpCicPipe.BackgroundImage")));
            this.hlpCicPipe.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.hlpCicPipe.Location = new System.Drawing.Point(513, 68);
            this.hlpCicPipe.Margin = new System.Windows.Forms.Padding(0);
            this.hlpCicPipe.MaximumSize = new System.Drawing.Size(22, 22);
            this.hlpCicPipe.MinimumSize = new System.Drawing.Size(22, 22);
            this.hlpCicPipe.Name = "hlpCicPipe";
            this.hlpCicPipe.Size = new System.Drawing.Size(22, 22);
            this.hlpCicPipe.SuppressClick = false;
            this.hlpCicPipe.TabIndex = 21;
            // 
            // ragCic
            // 
            this.ragCic.AlwaysShowHandCursor = false;
            this.ragCic.BackColor = System.Drawing.Color.Transparent;
            this.ragCic.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragCic.Location = new System.Drawing.Point(481, 65);
            this.ragCic.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ragCic.Name = "ragCic";
            this.ragCic.Size = new System.Drawing.Size(29, 29);
            this.ragCic.TabIndex = 17;
            // 
            // btnClearCohort
            // 
            this.btnClearCohort.Location = new System.Drawing.Point(418, 27);
            this.btnClearCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClearCohort.Name = "btnClearCohort";
            this.btnClearCohort.Size = new System.Drawing.Size(88, 25);
            this.btnClearCohort.TabIndex = 1;
            this.btnClearCohort.Text = "Clear";
            this.btnClearCohort.UseVisualStyleBackColor = true;
            this.btnClearCohort.Click += new System.EventHandler(this.btnClearCohort_Click);
            // 
            // ddCicPipeline
            // 
            this.ddCicPipeline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCicPipeline.FormattingEnabled = true;
            this.ddCicPipeline.Location = new System.Drawing.Point(72, 68);
            this.ddCicPipeline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddCicPipeline.Name = "ddCicPipeline";
            this.ddCicPipeline.Size = new System.Drawing.Size(400, 23);
            this.ddCicPipeline.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 72);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(52, 15);
            this.label7.TabIndex = 11;
            this.label7.Text = "Pipeline:";
            // 
            // cbxCohort
            // 
            this.cbxCohort.FilterRule = null;
            this.cbxCohort.FormattingEnabled = true;
            this.cbxCohort.Location = new System.Drawing.Point(103, 28);
            this.cbxCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbxCohort.Name = "cbxCohort";
            this.cbxCohort.PropertySelector = null;
            this.cbxCohort.Size = new System.Drawing.Size(310, 23);
            this.cbxCohort.SuggestBoxHeight = 110;
            this.cbxCohort.SuggestListOrderRule = null;
            this.cbxCohort.TabIndex = 0;
            this.cbxCohort.SelectionChangeCommitted += new System.EventHandler(this.cbxCohort_SelectionChangeCommitted);
            // 
            // gbFile
            // 
            this.gbFile.Controls.Add(this.hlpFlatFilePipe);
            this.gbFile.Controls.Add(this.ddFilePipeline);
            this.gbFile.Controls.Add(this.label8);
            this.gbFile.Controls.Add(this.btnSelectClearCohortFile);
            this.gbFile.Controls.Add(this.lblCohortFile);
            this.gbFile.Controls.Add(this.pbCohortFile);
            this.gbFile.Location = new System.Drawing.Point(601, 22);
            this.gbFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbFile.Name = "gbFile";
            this.gbFile.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbFile.Size = new System.Drawing.Size(532, 106);
            this.gbFile.TabIndex = 5;
            this.gbFile.TabStop = false;
            this.gbFile.Text = "Cohort From File";
            // 
            // hlpFlatFilePipe
            // 
            this.hlpFlatFilePipe.BackColor = System.Drawing.Color.Transparent;
            this.hlpFlatFilePipe.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hlpFlatFilePipe.BackgroundImage")));
            this.hlpFlatFilePipe.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.hlpFlatFilePipe.Location = new System.Drawing.Point(472, 68);
            this.hlpFlatFilePipe.Margin = new System.Windows.Forms.Padding(0);
            this.hlpFlatFilePipe.MaximumSize = new System.Drawing.Size(22, 22);
            this.hlpFlatFilePipe.MinimumSize = new System.Drawing.Size(22, 22);
            this.hlpFlatFilePipe.Name = "hlpFlatFilePipe";
            this.hlpFlatFilePipe.Size = new System.Drawing.Size(22, 22);
            this.hlpFlatFilePipe.SuppressClick = false;
            this.hlpFlatFilePipe.TabIndex = 22;
            // 
            // ddFilePipeline
            // 
            this.ddFilePipeline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddFilePipeline.FormattingEnabled = true;
            this.ddFilePipeline.Location = new System.Drawing.Point(68, 68);
            this.ddFilePipeline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddFilePipeline.Name = "ddFilePipeline";
            this.ddFilePipeline.Size = new System.Drawing.Size(400, 23);
            this.ddFilePipeline.TabIndex = 2;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 72);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(52, 15);
            this.label8.TabIndex = 11;
            this.label8.Text = "Pipeline:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(64, 94);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(62, 15);
            this.label9.TabIndex = 11;
            this.label9.Text = "Dataset(s):";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(18, 124);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(108, 15);
            this.label10.TabIndex = 11;
            this.label10.Text = "Extraction Pipeline:";
            // 
            // ddExtractionPipeline
            // 
            this.ddExtractionPipeline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExtractionPipeline.FormattingEnabled = true;
            this.ddExtractionPipeline.Location = new System.Drawing.Point(134, 121);
            this.ddExtractionPipeline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddExtractionPipeline.Name = "ddExtractionPipeline";
            this.ddExtractionPipeline.Size = new System.Drawing.Size(432, 23);
            this.ddExtractionPipeline.TabIndex = 11;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(100, 4);
            this.btnExecute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(88, 27);
            this.btnExecute.TabIndex = 12;
            this.btnExecute.Text = "Create";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // ddCohortSources
            // 
            this.ddCohortSources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCohortSources.FormattingEnabled = true;
            this.ddCohortSources.Location = new System.Drawing.Point(166, 59);
            this.ddCohortSources.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddCohortSources.Name = "ddCohortSources";
            this.ddCohortSources.Size = new System.Drawing.Size(310, 23);
            this.ddCohortSources.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 63);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(114, 15);
            this.label6.TabIndex = 12;
            this.label6.Text = "Identifier Allocation:";
            // 
            // pbCohortSources
            // 
            this.pbCohortSources.Location = new System.Drawing.Point(136, 60);
            this.pbCohortSources.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbCohortSources.Name = "pbCohortSources";
            this.pbCohortSources.Size = new System.Drawing.Size(23, 23);
            this.pbCohortSources.TabIndex = 13;
            this.pbCohortSources.TabStop = false;
            // 
            // btnCreateNewCohortSource
            // 
            this.btnCreateNewCohortSource.Location = new System.Drawing.Point(478, 58);
            this.btnCreateNewCohortSource.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCreateNewCohortSource.Name = "btnCreateNewCohortSource";
            this.btnCreateNewCohortSource.Size = new System.Drawing.Size(88, 25);
            this.btnCreateNewCohortSource.TabIndex = 9;
            this.btnCreateNewCohortSource.Text = "Create...";
            this.btnCreateNewCohortSource.UseVisualStyleBackColor = true;
            this.btnCreateNewCohortSource.Click += new System.EventHandler(this.btnCreateNewCohortSource_Click);
            // 
            // ragExecute
            // 
            this.ragExecute.AlwaysShowHandCursor = false;
            this.ragExecute.BackColor = System.Drawing.Color.Transparent;
            this.ragExecute.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragExecute.Location = new System.Drawing.Point(194, 4);
            this.ragExecute.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ragExecute.Name = "ragExecute";
            this.ragExecute.Size = new System.Drawing.Size(29, 29);
            this.ragExecute.TabIndex = 13;
            // 
            // ragProjectNumber
            // 
            this.ragProjectNumber.AlwaysShowHandCursor = false;
            this.ragProjectNumber.BackColor = System.Drawing.Color.Transparent;
            this.ragProjectNumber.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragProjectNumber.Location = new System.Drawing.Point(301, 30);
            this.ragProjectNumber.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ragProjectNumber.Name = "ragProjectNumber";
            this.ragProjectNumber.Size = new System.Drawing.Size(29, 29);
            this.ragProjectNumber.TabIndex = 16;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(44, 32);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(82, 15);
            this.label11.TabIndex = 6;
            this.label11.Text = "Cohort Name:";
            // 
            // tbCohortName
            // 
            this.tbCohortName.Location = new System.Drawing.Point(134, 29);
            this.tbCohortName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbCohortName.Name = "tbCohortName";
            this.tbCohortName.Size = new System.Drawing.Size(432, 23);
            this.tbCohortName.TabIndex = 7;
            // 
            // cbDefineCohort
            // 
            this.cbDefineCohort.AutoSize = true;
            this.cbDefineCohort.Location = new System.Drawing.Point(7, 96);
            this.cbDefineCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbDefineCohort.Name = "cbDefineCohort";
            this.cbDefineCohort.Size = new System.Drawing.Size(198, 19);
            this.cbDefineCohort.TabIndex = 17;
            this.cbDefineCohort.Text = "Define Cohort and Datasets Now";
            this.cbDefineCohort.UseVisualStyleBackColor = true;
            this.cbDefineCohort.CheckedChanged += new System.EventHandler(this.cbDefineCohort_CheckedChanged);
            // 
            // gbCohortAndDatasets
            // 
            this.gbCohortAndDatasets.Controls.Add(this.groupBox1);
            this.gbCohortAndDatasets.Controls.Add(this.gbCic);
            this.gbCohortAndDatasets.Controls.Add(this.label5);
            this.gbCohortAndDatasets.Controls.Add(this.gbFile);
            this.gbCohortAndDatasets.Location = new System.Drawing.Point(4, 129);
            this.gbCohortAndDatasets.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbCohortAndDatasets.Name = "gbCohortAndDatasets";
            this.gbCohortAndDatasets.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbCohortAndDatasets.Size = new System.Drawing.Size(1146, 302);
            this.gbCohortAndDatasets.TabIndex = 18;
            this.gbCohortAndDatasets.TabStop = false;
            this.gbCohortAndDatasets.Text = "Load Cohort from...";
            this.gbCohortAndDatasets.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.hlpExtractionPipeline);
            this.groupBox1.Controls.Add(this.pbCohortSources);
            this.groupBox1.Controls.Add(this.hlpIdentifierAllocation);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.hlpDatasets);
            this.groupBox1.Controls.Add(this.ddExtractionPipeline);
            this.groupBox1.Controls.Add(this.btnClearDatasets);
            this.groupBox1.Controls.Add(this.ddCohortSources);
            this.groupBox1.Controls.Add(this.lblDatasets);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.btnPackage);
            this.groupBox1.Controls.Add(this.btnCreateNewCohortSource);
            this.groupBox1.Controls.Add(this.btnPick);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.cbxDatasets);
            this.groupBox1.Controls.Add(this.tbCohortName);
            this.groupBox1.Location = new System.Drawing.Point(7, 134);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(608, 160);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "New Cohort Properties";
            // 
            // hlpExtractionPipeline
            // 
            this.hlpExtractionPipeline.BackColor = System.Drawing.Color.Transparent;
            this.hlpExtractionPipeline.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hlpExtractionPipeline.BackgroundImage")));
            this.hlpExtractionPipeline.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.hlpExtractionPipeline.Location = new System.Drawing.Point(570, 121);
            this.hlpExtractionPipeline.Margin = new System.Windows.Forms.Padding(0);
            this.hlpExtractionPipeline.MaximumSize = new System.Drawing.Size(22, 22);
            this.hlpExtractionPipeline.MinimumSize = new System.Drawing.Size(22, 22);
            this.hlpExtractionPipeline.Name = "hlpExtractionPipeline";
            this.hlpExtractionPipeline.Size = new System.Drawing.Size(22, 22);
            this.hlpExtractionPipeline.SuppressClick = false;
            this.hlpExtractionPipeline.TabIndex = 20;
            // 
            // hlpIdentifierAllocation
            // 
            this.hlpIdentifierAllocation.BackColor = System.Drawing.Color.Transparent;
            this.hlpIdentifierAllocation.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hlpIdentifierAllocation.BackgroundImage")));
            this.hlpIdentifierAllocation.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.hlpIdentifierAllocation.Location = new System.Drawing.Point(570, 60);
            this.hlpIdentifierAllocation.Margin = new System.Windows.Forms.Padding(0);
            this.hlpIdentifierAllocation.MaximumSize = new System.Drawing.Size(22, 22);
            this.hlpIdentifierAllocation.MinimumSize = new System.Drawing.Size(22, 22);
            this.hlpIdentifierAllocation.Name = "hlpIdentifierAllocation";
            this.hlpIdentifierAllocation.Size = new System.Drawing.Size(22, 22);
            this.hlpIdentifierAllocation.SuppressClick = false;
            this.hlpIdentifierAllocation.TabIndex = 20;
            // 
            // hlpDatasets
            // 
            this.hlpDatasets.BackColor = System.Drawing.Color.Transparent;
            this.hlpDatasets.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hlpDatasets.BackgroundImage")));
            this.hlpDatasets.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.hlpDatasets.Location = new System.Drawing.Point(570, 91);
            this.hlpDatasets.Margin = new System.Windows.Forms.Padding(0);
            this.hlpDatasets.MaximumSize = new System.Drawing.Size(22, 22);
            this.hlpDatasets.MinimumSize = new System.Drawing.Size(22, 22);
            this.hlpDatasets.Name = "hlpDatasets";
            this.hlpDatasets.Size = new System.Drawing.Size(22, 22);
            this.hlpDatasets.SuppressClick = false;
            this.hlpDatasets.TabIndex = 20;
            // 
            // btnClearDatasets
            // 
            this.btnClearDatasets.Location = new System.Drawing.Point(478, 90);
            this.btnClearDatasets.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClearDatasets.Name = "btnClearDatasets";
            this.btnClearDatasets.Size = new System.Drawing.Size(88, 25);
            this.btnClearDatasets.TabIndex = 19;
            this.btnClearDatasets.Text = "Clear";
            this.btnClearDatasets.UseVisualStyleBackColor = true;
            this.btnClearDatasets.Click += new System.EventHandler(this.btnClearDatasets_Click);
            // 
            // lblDatasets
            // 
            this.lblDatasets.AutoSize = true;
            this.lblDatasets.Location = new System.Drawing.Point(136, 94);
            this.lblDatasets.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDatasets.Name = "lblDatasets";
            this.lblDatasets.Size = new System.Drawing.Size(59, 15);
            this.lblDatasets.TabIndex = 18;
            this.lblDatasets.Text = "x datasets";
            this.lblDatasets.Visible = false;
            // 
            // btnPackage
            // 
            this.btnPackage.Location = new System.Drawing.Point(440, 90);
            this.btnPackage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPackage.Name = "btnPackage";
            this.btnPackage.Size = new System.Drawing.Size(37, 25);
            this.btnPackage.TabIndex = 17;
            this.btnPackage.UseVisualStyleBackColor = true;
            this.btnPackage.Click += new System.EventHandler(this.btnPackage_Click);
            // 
            // btnPick
            // 
            this.btnPick.Location = new System.Drawing.Point(402, 90);
            this.btnPick.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPick.Name = "btnPick";
            this.btnPick.Size = new System.Drawing.Size(37, 25);
            this.btnPick.TabIndex = 16;
            this.btnPick.Text = "...";
            this.btnPick.UseVisualStyleBackColor = true;
            this.btnPick.Click += new System.EventHandler(this.btnPick_Click);
            // 
            // cbxDatasets
            // 
            this.cbxDatasets.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.cbxDatasets.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbxDatasets.FormattingEnabled = true;
            this.cbxDatasets.Location = new System.Drawing.Point(134, 91);
            this.cbxDatasets.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbxDatasets.Name = "cbxDatasets";
            this.cbxDatasets.Size = new System.Drawing.Size(266, 23);
            this.cbxDatasets.TabIndex = 15;
            this.cbxDatasets.SelectedIndexChanged += new System.EventHandler(this.cbxDatasets_SelectedIndexChanged);
            // 
            // hlpDefineCohortAndDatasets
            // 
            this.hlpDefineCohortAndDatasets.BackColor = System.Drawing.Color.Transparent;
            this.hlpDefineCohortAndDatasets.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("hlpDefineCohortAndDatasets.BackgroundImage")));
            this.hlpDefineCohortAndDatasets.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.hlpDefineCohortAndDatasets.Location = new System.Drawing.Point(200, 93);
            this.hlpDefineCohortAndDatasets.Margin = new System.Windows.Forms.Padding(0);
            this.hlpDefineCohortAndDatasets.MaximumSize = new System.Drawing.Size(22, 22);
            this.hlpDefineCohortAndDatasets.MinimumSize = new System.Drawing.Size(22, 22);
            this.hlpDefineCohortAndDatasets.Name = "hlpDefineCohortAndDatasets";
            this.hlpDefineCohortAndDatasets.Size = new System.Drawing.Size(22, 22);
            this.hlpDefineCohortAndDatasets.SuppressClick = false;
            this.hlpDefineCohortAndDatasets.TabIndex = 21;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(4, 4);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 27);
            this.btnCancel.TabIndex = 22;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.panel1);
            this.flowLayoutPanel1.Controls.Add(this.gbCohortAndDatasets);
            this.flowLayoutPanel1.Controls.Add(this.panel2);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1154, 476);
            this.flowLayoutPanel1.TabIndex = 23;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.tbProjectName);
            this.panel1.Controls.Add(this.tbProjectNumber);
            this.panel1.Controls.Add(this.hlpDefineCohortAndDatasets);
            this.panel1.Controls.Add(this.tbExtractionDirectory);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cbDefineCohort);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.ragProjectNumber);
            this.panel1.Controls.Add(this.btnBrowse);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(530, 120);
            this.panel1.TabIndex = 24;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnExecute);
            this.panel2.Controls.Add(this.ragExecute);
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Location = new System.Drawing.Point(3, 437);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(530, 36);
            this.panel2.TabIndex = 24;
            // 
            // CreateNewDataExtractionProjectUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1179, 503);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CreateNewDataExtractionProjectUI";
            this.Text = "New Project";
            this.Load += new System.EventHandler(this.CreateNewDataExtractionProjectUI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbCohort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortFile)).EndInit();
            this.gbCic.ResumeLayout(false);
            this.gbCic.PerformLayout();
            this.gbFile.ResumeLayout(false);
            this.gbFile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortSources)).EndInit();
            this.gbCohortAndDatasets.ResumeLayout(false);
            this.gbCohortAndDatasets.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbProjectName;
        private System.Windows.Forms.TextBox tbProjectNumber;
        private System.Windows.Forms.TextBox tbExtractionDirectory;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.PictureBox pbCohort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox pbCohortFile;
        private System.Windows.Forms.Label lblCohortFile;
        private System.Windows.Forms.Button btnSelectClearCohortFile;
        private System.Windows.Forms.GroupBox gbCic;
        private System.Windows.Forms.GroupBox gbFile;
        private SuggestComboBox cbxCohort;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox ddCicPipeline;
        private System.Windows.Forms.ComboBox ddFilePipeline;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox ddExtractionPipeline;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.ComboBox ddCohortSources;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox pbCohortSources;
        private System.Windows.Forms.Button btnCreateNewCohortSource;
        private RAGSmiley ragExecute;
        private RAGSmiley ragProjectNumber;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tbCohortName;
        private System.Windows.Forms.Button btnClearCohort;
        private RAGSmiley ragCic;
        private System.Windows.Forms.CheckBox cbDefineCohort;
        private System.Windows.Forms.GroupBox gbCohortAndDatasets;
        private System.Windows.Forms.Button btnPackage;
        private System.Windows.Forms.Button btnPick;
        private System.Windows.Forms.ComboBox cbxDatasets;
        private System.Windows.Forms.Label lblDatasets;
        private System.Windows.Forms.Button btnClearDatasets;
        private SimpleControls.HelpIcon hlpExtractionPipeline;
        private SimpleControls.HelpIcon hlpIdentifierAllocation;
        private SimpleControls.HelpIcon hlpDatasets;
        private SimpleControls.HelpIcon hlpDefineCohortAndDatasets;
        private SimpleControls.HelpIcon hlpCicPipe;
        private SimpleControls.HelpIcon hlpFlatFilePipe;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}