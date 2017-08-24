using BrightIdeasSoftware;

namespace DataExportManager.CohortUI.ImportCustomData
{
    partial class CohortCreationRequestUI
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
            this.ddExistingCohort = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblExternalCohortTable = new System.Windows.Forms.Label();
            this.gbRevisedCohort = new System.Windows.Forms.GroupBox();
            this.cbShowEvenWhenProjectNumberDoesntMatch = new System.Windows.Forms.CheckBox();
            this.lblNewVersionNumber = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.rbNewCohort = new System.Windows.Forms.RadioButton();
            this.rbRevisedCohort = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.gbNewCohort = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbFilterProject = new System.Windows.Forms.TextBox();
            this.btnNewProject = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listBox1 = new BrightIdeasSoftware.ObjectListView();
            this.olvColumn1 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvColumn2 = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.lblProject = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.gbDescription = new System.Windows.Forms.GroupBox();
            this.pbProject = new System.Windows.Forms.PictureBox();
            this.pbCohortSource = new System.Windows.Forms.PictureBox();
            this.gbRevisedCohort.SuspendLayout();
            this.gbNewCohort.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listBox1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.gbDescription.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbProject)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(292, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "This form lets you describe the cohort you are trying to import";
            // 
            // ddExistingCohort
            // 
            this.ddExistingCohort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddExistingCohort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExistingCohort.FormattingEnabled = true;
            this.ddExistingCohort.Location = new System.Drawing.Point(147, 16);
            this.ddExistingCohort.Name = "ddExistingCohort";
            this.ddExistingCohort.Size = new System.Drawing.Size(661, 21);
            this.ddExistingCohort.Sorted = true;
            this.ddExistingCohort.TabIndex = 1;
            this.ddExistingCohort.SelectedIndexChanged += new System.EventHandler(this.ddExistingCohort_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "OR Existing:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "ExternalCohortTable:";
            // 
            // lblExternalCohortTable
            // 
            this.lblExternalCohortTable.AutoSize = true;
            this.lblExternalCohortTable.Location = new System.Drawing.Point(156, 28);
            this.lblExternalCohortTable.Name = "lblExternalCohortTable";
            this.lblExternalCohortTable.Size = new System.Drawing.Size(27, 13);
            this.lblExternalCohortTable.TabIndex = 2;
            this.lblExternalCohortTable.Text = "blah";
            // 
            // gbRevisedCohort
            // 
            this.gbRevisedCohort.Controls.Add(this.cbShowEvenWhenProjectNumberDoesntMatch);
            this.gbRevisedCohort.Controls.Add(this.lblNewVersionNumber);
            this.gbRevisedCohort.Controls.Add(this.label6);
            this.gbRevisedCohort.Controls.Add(this.label5);
            this.gbRevisedCohort.Controls.Add(this.ddExistingCohort);
            this.gbRevisedCohort.Enabled = false;
            this.gbRevisedCohort.Location = new System.Drawing.Point(11, 74);
            this.gbRevisedCohort.Name = "gbRevisedCohort";
            this.gbRevisedCohort.Size = new System.Drawing.Size(814, 68);
            this.gbRevisedCohort.TabIndex = 1;
            this.gbRevisedCohort.TabStop = false;
            this.gbRevisedCohort.Text = "Revised Cohort";
            // 
            // cbShowEvenWhenProjectNumberDoesntMatch
            // 
            this.cbShowEvenWhenProjectNumberDoesntMatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbShowEvenWhenProjectNumberDoesntMatch.AutoSize = true;
            this.cbShowEvenWhenProjectNumberDoesntMatch.Location = new System.Drawing.Point(487, 41);
            this.cbShowEvenWhenProjectNumberDoesntMatch.Name = "cbShowEvenWhenProjectNumberDoesntMatch";
            this.cbShowEvenWhenProjectNumberDoesntMatch.Size = new System.Drawing.Size(317, 17);
            this.cbShowEvenWhenProjectNumberDoesntMatch.TabIndex = 4;
            this.cbShowEvenWhenProjectNumberDoesntMatch.Text = "Show All Cohorts (even when project number does not match)";
            this.cbShowEvenWhenProjectNumberDoesntMatch.UseVisualStyleBackColor = true;
            this.cbShowEvenWhenProjectNumberDoesntMatch.CheckedChanged += new System.EventHandler(this.cbShowEvenWhenProjectNumberDoesntMatch_CheckedChanged);
            // 
            // lblNewVersionNumber
            // 
            this.lblNewVersionNumber.AutoSize = true;
            this.lblNewVersionNumber.Location = new System.Drawing.Point(151, 40);
            this.lblNewVersionNumber.Name = "lblNewVersionNumber";
            this.lblNewVersionNumber.Size = new System.Drawing.Size(16, 13);
            this.lblNewVersionNumber.TabIndex = 3;
            this.lblNewVersionNumber.Text = "-1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 41);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(139, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "New version number will be:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(61, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Existing Cohort:";
            // 
            // rbNewCohort
            // 
            this.rbNewCohort.AutoSize = true;
            this.rbNewCohort.Location = new System.Drawing.Point(69, 17);
            this.rbNewCohort.Name = "rbNewCohort";
            this.rbNewCohort.Size = new System.Drawing.Size(84, 17);
            this.rbNewCohort.TabIndex = 1;
            this.rbNewCohort.TabStop = true;
            this.rbNewCohort.Text = "New Cohort ";
            this.rbNewCohort.UseVisualStyleBackColor = true;
            this.rbNewCohort.CheckedChanged += new System.EventHandler(this.rbNewCohort_CheckedChanged);
            // 
            // rbRevisedCohort
            // 
            this.rbRevisedCohort.AutoSize = true;
            this.rbRevisedCohort.Location = new System.Drawing.Point(159, 17);
            this.rbRevisedCohort.Name = "rbRevisedCohort";
            this.rbRevisedCohort.Size = new System.Drawing.Size(188, 17);
            this.rbRevisedCohort.TabIndex = 2;
            this.rbRevisedCohort.TabStop = true;
            this.rbRevisedCohort.Text = "Revised version of existing Cohort ";
            this.rbRevisedCohort.UseVisualStyleBackColor = true;
            this.rbRevisedCohort.CheckedChanged += new System.EventHandler(this.rbRevisedCohort_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "This is a:";
            // 
            // gbNewCohort
            // 
            this.gbNewCohort.Controls.Add(this.label7);
            this.gbNewCohort.Controls.Add(this.tbName);
            this.gbNewCohort.Enabled = false;
            this.gbNewCohort.Location = new System.Drawing.Point(11, 21);
            this.gbNewCohort.Name = "gbNewCohort";
            this.gbNewCohort.Size = new System.Drawing.Size(814, 47);
            this.gbNewCohort.TabIndex = 0;
            this.gbNewCohort.TabStop = false;
            this.gbNewCohort.Text = "New Cohort";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Name:";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(50, 19);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(758, 20);
            this.tbName.TabIndex = 1;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(51, 183);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(32, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "Filter:";
            // 
            // tbFilterProject
            // 
            this.tbFilterProject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbFilterProject.Location = new System.Drawing.Point(89, 180);
            this.tbFilterProject.Name = "tbFilterProject";
            this.tbFilterProject.Size = new System.Drawing.Size(278, 20);
            this.tbFilterProject.TabIndex = 1;
            this.tbFilterProject.TextChanged += new System.EventHandler(this.tbFilterProject_TextChanged);
            // 
            // btnNewProject
            // 
            this.btnNewProject.Location = new System.Drawing.Point(89, 19);
            this.btnNewProject.Name = "btnNewProject";
            this.btnNewProject.Size = new System.Drawing.Size(102, 23);
            this.btnNewProject.TabIndex = 2;
            this.btnNewProject.Text = "New Project...";
            this.btnNewProject.UseVisualStyleBackColor = true;
            this.btnNewProject.Click += new System.EventHandler(this.btnNewProject_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.listBox1);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.btnNewProject);
            this.groupBox1.Controls.Add(this.tbFilterProject);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(27, 81);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(851, 203);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "1. Choose  / Create Project";
            // 
            // listBox1
            // 
            this.listBox1.AllColumns.Add(this.olvColumn1);
            this.listBox1.AllColumns.Add(this.olvColumn2);
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.CellEditUseWholeCell = false;
            this.listBox1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumn1,
            this.olvColumn2});
            this.listBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.listBox1.FullRowSelect = true;
            this.listBox1.HideSelection = false;
            this.listBox1.Location = new System.Drawing.Point(89, 47);
            this.listBox1.Name = "listBox1";
            this.listBox1.ShowGroups = false;
            this.listBox1.Size = new System.Drawing.Size(756, 127);
            this.listBox1.TabIndex = 0;
            this.listBox1.UseCompatibleStateImageBehavior = false;
            this.listBox1.UseFiltering = true;
            this.listBox1.View = System.Windows.Forms.View.Details;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // olvColumn1
            // 
            this.olvColumn1.AspectName = "ToString";
            this.olvColumn1.FillsFreeSpace = true;
            this.olvColumn1.Text = "Name";
            // 
            // olvColumn2
            // 
            this.olvColumn2.AspectName = "CountCohorts";
            this.olvColumn2.Text = "Cohorts In Use";
            this.olvColumn2.Width = 120;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.rbRevisedCohort);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.rbNewCohort);
            this.groupBox2.Location = new System.Drawing.Point(27, 290);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(851, 42);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "2. Choose Cohort Type";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.gbNewCohort);
            this.groupBox3.Controls.Add(this.gbRevisedCohort);
            this.groupBox3.Location = new System.Drawing.Point(27, 338);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(851, 154);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "3. Configure Cohort (doesn\'t exist yet, next screen will actually create it)";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 21);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(63, 13);
            this.label9.TabIndex = 2;
            this.label9.Text = "Description:";
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(75, 21);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(770, 59);
            this.tbDescription.TabIndex = 3;
            // 
            // lblProject
            // 
            this.lblProject.AutoSize = true;
            this.lblProject.Location = new System.Drawing.Point(156, 59);
            this.lblProject.Name = "lblProject";
            this.lblProject.Size = new System.Drawing.Size(27, 13);
            this.lblProject.TabIndex = 4;
            this.lblProject.Text = "blah";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(75, 59);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(43, 13);
            this.label11.TabIndex = 3;
            this.label11.Text = "Project:";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Location = new System.Drawing.Point(417, 593);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(121, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOk.Location = new System.Drawing.Point(283, 593);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(128, 23);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(251, 590);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(26, 26);
            this.ragSmiley1.TabIndex = 4;
            // 
            // gbDescription
            // 
            this.gbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDescription.Controls.Add(this.label9);
            this.gbDescription.Controls.Add(this.tbDescription);
            this.gbDescription.Location = new System.Drawing.Point(27, 498);
            this.gbDescription.Name = "gbDescription";
            this.gbDescription.Size = new System.Drawing.Size(851, 86);
            this.gbDescription.TabIndex = 16;
            this.gbDescription.TabStop = false;
            this.gbDescription.Text = "4. Enter Description Of Cohort";
            // 
            // pbProject
            // 
            this.pbProject.Location = new System.Drawing.Point(124, 52);
            this.pbProject.Name = "pbProject";
            this.pbProject.Size = new System.Drawing.Size(26, 26);
            this.pbProject.TabIndex = 17;
            this.pbProject.TabStop = false;
            // 
            // pbCohortSource
            // 
            this.pbCohortSource.Location = new System.Drawing.Point(124, 25);
            this.pbCohortSource.Name = "pbCohortSource";
            this.pbCohortSource.Size = new System.Drawing.Size(26, 26);
            this.pbCohortSource.TabIndex = 18;
            this.pbCohortSource.TabStop = false;
            // 
            // CohortCreationRequestUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(890, 628);
            this.Controls.Add(this.pbCohortSource);
            this.Controls.Add(this.pbProject);
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.gbDescription);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblProject);
            this.Controls.Add(this.lblExternalCohortTable);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Name = "CohortCreationRequestUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CohortCreationRequestUI";
            this.Load += new System.EventHandler(this.CohortCreationRequestUI_Load);
            this.gbRevisedCohort.ResumeLayout(false);
            this.gbRevisedCohort.PerformLayout();
            this.gbNewCohort.ResumeLayout(false);
            this.gbNewCohort.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.listBox1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.gbDescription.ResumeLayout(false);
            this.gbDescription.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbProject)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbCohortSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddExistingCohort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblExternalCohortTable;
        private System.Windows.Forms.GroupBox gbRevisedCohort;
        private System.Windows.Forms.RadioButton rbNewCohort;
        private System.Windows.Forms.RadioButton rbRevisedCohort;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblNewVersionNumber;
        private System.Windows.Forms.GroupBox gbNewCohort;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.CheckBox cbShowEvenWhenProjectNumberDoesntMatch;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbFilterProject;
        private System.Windows.Forms.Button btnNewProject;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private ObjectListView listBox1;
        private OLVColumn olvColumn1;
        private OLVColumn olvColumn2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label lblProject;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private System.Windows.Forms.GroupBox gbDescription;
        private System.Windows.Forms.PictureBox pbProject;
        private System.Windows.Forms.PictureBox pbCohortSource;
    }
}