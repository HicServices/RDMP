using BrightIdeasSoftware;

namespace DataExportManager.Wizard
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
            this.olvDatasets = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
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
            this.ragCic = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.btnClearCohort = new System.Windows.Forms.Button();
            this.ddCicPipeline = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cbxCohort = new ReusableUIComponents.SuggestComboBox();
            this.gbFile = new System.Windows.Forms.GroupBox();
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
            this.ragExecute = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.ragProjectNumber = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.label11 = new System.Windows.Forms.Label();
            this.tbCohortName = new System.Windows.Forms.TextBox();
            this.cbDefineCohort = new System.Windows.Forms.CheckBox();
            this.gbCohortAndDatasets = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.olvDatasets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortFile)).BeginInit();
            this.gbCic.SuspendLayout();
            this.gbFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortSources)).BeginInit();
            this.gbCohortAndDatasets.SuspendLayout();
            this.SuspendLayout();
            // 
            // olvDatasets
            // 
            this.olvDatasets.AllColumns.Add(this.olvColumn1);
            this.olvDatasets.CellEditUseWholeCell = false;
            this.olvDatasets.CheckBoxes = true;
            this.olvDatasets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1});
            this.olvDatasets.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvDatasets.Location = new System.Drawing.Point(114, 182);
            this.olvDatasets.Name = "olvDatasets";
            this.olvDatasets.Size = new System.Drawing.Size(400, 273);
            this.olvDatasets.TabIndex = 10;
            this.olvDatasets.UseCompatibleStateImageBehavior = false;
            this.olvDatasets.View = System.Windows.Forms.View.Details;
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "ToString";
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.Groupable = false;
            this.olvColumn1.Text = "DataSets";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Project Name:";
            // 
            // tbProjectName
            // 
            this.tbProjectName.Location = new System.Drawing.Point(117, 9);
            this.tbProjectName.Name = "tbProjectName";
            this.tbProjectName.Size = new System.Drawing.Size(444, 20);
            this.tbProjectName.TabIndex = 0;
            // 
            // tbProjectNumber
            // 
            this.tbProjectNumber.Location = new System.Drawing.Point(117, 35);
            this.tbProjectNumber.Name = "tbProjectNumber";
            this.tbProjectNumber.Size = new System.Drawing.Size(146, 20);
            this.tbProjectNumber.TabIndex = 1;
            this.tbProjectNumber.TextChanged += new System.EventHandler(this.tbProjectNumber_TextChanged);
            // 
            // tbExtractionDirectory
            // 
            this.tbExtractionDirectory.Location = new System.Drawing.Point(117, 61);
            this.tbExtractionDirectory.Name = "tbExtractionDirectory";
            this.tbExtractionDirectory.Size = new System.Drawing.Size(400, 20);
            this.tbExtractionDirectory.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Project Number:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Extraction Directory:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(524, 57);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // pbCohort
            // 
            this.pbCohort.Location = new System.Drawing.Point(62, 28);
            this.pbCohort.Name = "pbCohort";
            this.pbCohort.Size = new System.Drawing.Size(20, 20);
            this.pbCohort.TabIndex = 5;
            this.pbCohort.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Cohort:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(531, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "OR";
            // 
            // pbCohortFile
            // 
            this.pbCohortFile.Location = new System.Drawing.Point(6, 21);
            this.pbCohortFile.Name = "pbCohortFile";
            this.pbCohortFile.Size = new System.Drawing.Size(20, 20);
            this.pbCohortFile.TabIndex = 8;
            this.pbCohortFile.TabStop = false;
            // 
            // lblCohortFile
            // 
            this.lblCohortFile.AutoSize = true;
            this.lblCohortFile.Location = new System.Drawing.Point(27, 24);
            this.lblCohortFile.Name = "lblCohortFile";
            this.lblCohortFile.Size = new System.Drawing.Size(66, 13);
            this.lblCohortFile.TabIndex = 0;
            this.lblCohortFile.Text = "Cohort File...";
            // 
            // btnSelectClearCohortFile
            // 
            this.btnSelectClearCohortFile.Location = new System.Drawing.Point(99, 19);
            this.btnSelectClearCohortFile.Name = "btnSelectClearCohortFile";
            this.btnSelectClearCohortFile.Size = new System.Drawing.Size(75, 23);
            this.btnSelectClearCohortFile.TabIndex = 1;
            this.btnSelectClearCohortFile.Text = "Browse...";
            this.btnSelectClearCohortFile.UseVisualStyleBackColor = true;
            this.btnSelectClearCohortFile.Click += new System.EventHandler(this.btnSelectClearCohortFile_Click);
            // 
            // gbCic
            // 
            this.gbCic.Controls.Add(this.ragCic);
            this.gbCic.Controls.Add(this.btnClearCohort);
            this.gbCic.Controls.Add(this.ddCicPipeline);
            this.gbCic.Controls.Add(this.label7);
            this.gbCic.Controls.Add(this.cbxCohort);
            this.gbCic.Controls.Add(this.label4);
            this.gbCic.Controls.Add(this.pbCohort);
            this.gbCic.Location = new System.Drawing.Point(6, 19);
            this.gbCic.Name = "gbCic";
            this.gbCic.Size = new System.Drawing.Size(519, 100);
            this.gbCic.TabIndex = 4;
            this.gbCic.TabStop = false;
            this.gbCic.Text = "RDMP Cohort Builder Query";
            // 
            // ragCic
            // 
            this.ragCic.AlwaysShowHandCursor = false;
            this.ragCic.BackColor = System.Drawing.Color.Transparent;
            this.ragCic.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragCic.Location = new System.Drawing.Point(456, 60);
            this.ragCic.Name = "ragCic";
            this.ragCic.Size = new System.Drawing.Size(25, 25);
            this.ragCic.TabIndex = 17;
            // 
            // btnClearCohort
            // 
            this.btnClearCohort.Location = new System.Drawing.Point(456, 23);
            this.btnClearCohort.Name = "btnClearCohort";
            this.btnClearCohort.Size = new System.Drawing.Size(57, 23);
            this.btnClearCohort.TabIndex = 1;
            this.btnClearCohort.Text = "Clear";
            this.btnClearCohort.UseVisualStyleBackColor = true;
            this.btnClearCohort.Click += new System.EventHandler(this.btnClearCohort_Click);
            // 
            // ddCicPipeline
            // 
            this.ddCicPipeline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCicPipeline.FormattingEnabled = true;
            this.ddCicPipeline.Location = new System.Drawing.Point(60, 64);
            this.ddCicPipeline.Name = "ddCicPipeline";
            this.ddCicPipeline.Size = new System.Drawing.Size(390, 21);
            this.ddCicPipeline.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 67);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Pipeline:";
            // 
            // cbxCohort
            // 
            this.cbxCohort.FilterRule = null;
            this.cbxCohort.FormattingEnabled = true;
            this.cbxCohort.Location = new System.Drawing.Point(88, 24);
            this.cbxCohort.Name = "cbxCohort";
            this.cbxCohort.PropertySelector = null;
            this.cbxCohort.Size = new System.Drawing.Size(362, 21);
            this.cbxCohort.SuggestBoxHeight = 96;
            this.cbxCohort.SuggestListOrderRule = null;
            this.cbxCohort.TabIndex = 0;
            this.cbxCohort.SelectedIndexChanged += new System.EventHandler(this.cbxCohort_SelectedIndexChanged);
            // 
            // gbFile
            // 
            this.gbFile.Controls.Add(this.ddFilePipeline);
            this.gbFile.Controls.Add(this.label8);
            this.gbFile.Controls.Add(this.btnSelectClearCohortFile);
            this.gbFile.Controls.Add(this.lblCohortFile);
            this.gbFile.Controls.Add(this.pbCohortFile);
            this.gbFile.Location = new System.Drawing.Point(560, 19);
            this.gbFile.Name = "gbFile";
            this.gbFile.Size = new System.Drawing.Size(456, 100);
            this.gbFile.TabIndex = 5;
            this.gbFile.TabStop = false;
            this.gbFile.Text = "Cohort From File";
            // 
            // ddFilePipeline
            // 
            this.ddFilePipeline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddFilePipeline.FormattingEnabled = true;
            this.ddFilePipeline.Location = new System.Drawing.Point(61, 64);
            this.ddFilePipeline.Name = "ddFilePipeline";
            this.ddFilePipeline.Size = new System.Drawing.Size(389, 21);
            this.ddFilePipeline.TabIndex = 2;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 67);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Pipeline:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(56, 182);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(52, 13);
            this.label9.TabIndex = 11;
            this.label9.Text = "Datasets:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(11, 464);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(97, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "Extraction Pipeline:";
            // 
            // ddExtractionPipeline
            // 
            this.ddExtractionPipeline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExtractionPipeline.FormattingEnabled = true;
            this.ddExtractionPipeline.Location = new System.Drawing.Point(114, 461);
            this.ddExtractionPipeline.Name = "ddExtractionPipeline";
            this.ddExtractionPipeline.Size = new System.Drawing.Size(390, 21);
            this.ddExtractionPipeline.TabIndex = 11;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(17, 603);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
            this.btnExecute.TabIndex = 12;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // ddCohortSources
            // 
            this.ddCohortSources.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCohortSources.FormattingEnabled = true;
            this.ddCohortSources.Location = new System.Drawing.Point(141, 155);
            this.ddCohortSources.Name = "ddCohortSources";
            this.ddCohortSources.Size = new System.Drawing.Size(390, 21);
            this.ddCohortSources.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 158);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(99, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Identifier Allocation:";
            // 
            // pbCohortSources
            // 
            this.pbCohortSources.Location = new System.Drawing.Point(115, 156);
            this.pbCohortSources.Name = "pbCohortSources";
            this.pbCohortSources.Size = new System.Drawing.Size(20, 20);
            this.pbCohortSources.TabIndex = 13;
            this.pbCohortSources.TabStop = false;
            // 
            // btnCreateNewCohortSource
            // 
            this.btnCreateNewCohortSource.Location = new System.Drawing.Point(540, 153);
            this.btnCreateNewCohortSource.Name = "btnCreateNewCohortSource";
            this.btnCreateNewCohortSource.Size = new System.Drawing.Size(75, 23);
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
            this.ragExecute.Location = new System.Drawing.Point(98, 603);
            this.ragExecute.Name = "ragExecute";
            this.ragExecute.Size = new System.Drawing.Size(25, 25);
            this.ragExecute.TabIndex = 13;
            // 
            // ragProjectNumber
            // 
            this.ragProjectNumber.AlwaysShowHandCursor = false;
            this.ragProjectNumber.BackColor = System.Drawing.Color.Transparent;
            this.ragProjectNumber.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragProjectNumber.Location = new System.Drawing.Point(269, 30);
            this.ragProjectNumber.Name = "ragProjectNumber";
            this.ragProjectNumber.Size = new System.Drawing.Size(25, 25);
            this.ragProjectNumber.TabIndex = 16;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(36, 132);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(72, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "Cohort Name:";
            // 
            // tbCohortName
            // 
            this.tbCohortName.Location = new System.Drawing.Point(113, 129);
            this.tbCohortName.Name = "tbCohortName";
            this.tbCohortName.Size = new System.Drawing.Size(400, 20);
            this.tbCohortName.TabIndex = 7;
            // 
            // cbDefineCohort
            // 
            this.cbDefineCohort.AutoSize = true;
            this.cbDefineCohort.Location = new System.Drawing.Point(17, 87);
            this.cbDefineCohort.Name = "cbDefineCohort";
            this.cbDefineCohort.Size = new System.Drawing.Size(182, 17);
            this.cbDefineCohort.TabIndex = 17;
            this.cbDefineCohort.Text = "Define Cohort and Datasets Now";
            this.cbDefineCohort.UseVisualStyleBackColor = true;
            this.cbDefineCohort.CheckedChanged += new System.EventHandler(this.cbDefineCohort_CheckedChanged);
            // 
            // gbCohortAndDatasets
            // 
            this.gbCohortAndDatasets.Controls.Add(this.gbCic);
            this.gbCohortAndDatasets.Controls.Add(this.label5);
            this.gbCohortAndDatasets.Controls.Add(this.tbCohortName);
            this.gbCohortAndDatasets.Controls.Add(this.gbFile);
            this.gbCohortAndDatasets.Controls.Add(this.label11);
            this.gbCohortAndDatasets.Controls.Add(this.olvDatasets);
            this.gbCohortAndDatasets.Controls.Add(this.label9);
            this.gbCohortAndDatasets.Controls.Add(this.btnCreateNewCohortSource);
            this.gbCohortAndDatasets.Controls.Add(this.label10);
            this.gbCohortAndDatasets.Controls.Add(this.ddCohortSources);
            this.gbCohortAndDatasets.Controls.Add(this.ddExtractionPipeline);
            this.gbCohortAndDatasets.Controls.Add(this.label6);
            this.gbCohortAndDatasets.Controls.Add(this.pbCohortSources);
            this.gbCohortAndDatasets.Enabled = false;
            this.gbCohortAndDatasets.Location = new System.Drawing.Point(17, 106);
            this.gbCohortAndDatasets.Name = "gbCohortAndDatasets";
            this.gbCohortAndDatasets.Size = new System.Drawing.Size(1023, 491);
            this.gbCohortAndDatasets.TabIndex = 18;
            this.gbCohortAndDatasets.TabStop = false;
            this.gbCohortAndDatasets.Text = "Cohort and Datasets";
            // 
            // CreateNewDataExtractionProjectUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1054, 630);
            this.Controls.Add(this.gbCohortAndDatasets);
            this.Controls.Add(this.cbDefineCohort);
            this.Controls.Add(this.ragProjectNumber);
            this.Controls.Add(this.ragExecute);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbExtractionDirectory);
            this.Controls.Add(this.tbProjectNumber);
            this.Controls.Add(this.tbProjectName);
            this.Name = "CreateNewDataExtractionProjectUI";
            this.Text = "CreateNewDataExtractionProjectUI";
            this.Load += new System.EventHandler(this.CreateNewDataExtractionProjectUI_Load);
            ((System.ComponentModel.ISupportInitialize)(this.olvDatasets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortFile)).EndInit();
            this.gbCic.ResumeLayout(false);
            this.gbCic.PerformLayout();
            this.gbFile.ResumeLayout(false);
            this.gbFile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortSources)).EndInit();
            this.gbCohortAndDatasets.ResumeLayout(false);
            this.gbCohortAndDatasets.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ObjectListView olvDatasets;
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
        private ReusableUIComponents.SuggestComboBox cbxCohort;
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
        private ReusableUIComponents.ChecksUI.RAGSmiley ragExecute;
        private OLVColumn olvColumn1;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragProjectNumber;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tbCohortName;
        private System.Windows.Forms.Button btnClearCohort;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragCic;
        private System.Windows.Forms.CheckBox cbDefineCohort;
        private System.Windows.Forms.GroupBox gbCohortAndDatasets;
    }
}