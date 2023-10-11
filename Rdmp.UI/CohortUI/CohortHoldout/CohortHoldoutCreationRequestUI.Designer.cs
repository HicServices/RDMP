using Rdmp.UI.ChecksUI;
using Rdmp.UI.SimpleControls;
using System.Collections.Generic;

namespace Rdmp.UI.CohortUI.CohortHoldout
{
    partial class CohortHoldoutCreationRequestUI
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
            gbNewCohort = new System.Windows.Forms.GroupBox();
            label7 = new System.Windows.Forms.Label();
            tbName = new System.Windows.Forms.TextBox();
            gbChooseCohortType = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            comboBox1 = new System.Windows.Forms.ComboBox();
            numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            groupBox3 = new System.Windows.Forms.GroupBox();
            label9 = new System.Windows.Forms.Label();
            tbDescription = new System.Windows.Forms.TextBox();
            btnClearProject = new System.Windows.Forms.Button();
            btnOk = new System.Windows.Forms.Button();
            ragSmiley1 = new RAGSmiley();
            gbDescription = new System.Windows.Forms.GroupBox();
            taskDescriptionLabel1 = new SimpleDialogs.TaskDescriptionLabel();
            panel1 = new System.Windows.Forms.Panel();
            panel2 = new System.Windows.Forms.Panel();
            groupBox1 = new System.Windows.Forms.GroupBox();
            scintilla1 = new ScintillaNET.Scintilla();
            gbNewCohort.SuspendLayout();
            gbChooseCohortType.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            groupBox3.SuspendLayout();
            gbDescription.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // gbNewCohort
            // 
            gbNewCohort.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbNewCohort.Controls.Add(label7);
            gbNewCohort.Controls.Add(tbName);
            gbNewCohort.Location = new System.Drawing.Point(13, 24);
            gbNewCohort.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbNewCohort.Name = "gbNewCohort";
            gbNewCohort.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbNewCohort.Size = new System.Drawing.Size(1005, 54);
            gbNewCohort.TabIndex = 0;
            gbNewCohort.TabStop = false;
            gbNewCohort.Text = "Holdout Cohort Name";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(7, 25);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(42, 15);
            label7.TabIndex = 0;
            label7.Text = "Name:";
            // 
            // tbName
            // 
            tbName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbName.Location = new System.Drawing.Point(58, 22);
            tbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(939, 23);
            tbName.TabIndex = 1;
            tbName.TextChanged += tbName_TextChanged;
            // 
            // gbChooseCohortType
            // 
            gbChooseCohortType.Controls.Add(label1);
            gbChooseCohortType.Controls.Add(comboBox1);
            gbChooseCohortType.Controls.Add(numericUpDown1);
            gbChooseCohortType.Dock = System.Windows.Forms.DockStyle.Top;
            gbChooseCohortType.Location = new System.Drawing.Point(0, 0);
            gbChooseCohortType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbChooseCohortType.Name = "gbChooseCohortType";
            gbChooseCohortType.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbChooseCohortType.Size = new System.Drawing.Size(1048, 217);
            gbChooseCohortType.TabIndex = 9;
            gbChooseCohortType.TabStop = false;
            gbChooseCohortType.Text = "1. Define Holdout settings";
            gbChooseCohortType.Enter += gbChooseCohortType_Enter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(171, 20);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(110, 15);
            label1.TabIndex = 5;
            label1.Text = "of people in Cohort";
            label1.Click += label1_Click;
            // 
            // comboBox1
            // 
            comboBox1.AllowDrop = true;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "%", "#" });
            comboBox1.Location = new System.Drawing.Point(122, 17);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new System.Drawing.Size(43, 23);
            comboBox1.TabIndex = 4;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new System.Drawing.Point(20, 18);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new System.Drawing.Size(87, 23);
            numericUpDown1.TabIndex = 3;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(gbNewCohort);
            groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            groupBox3.Location = new System.Drawing.Point(0, 217);
            groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Size = new System.Drawing.Size(1048, 95);
            groupBox3.TabIndex = 10;
            groupBox3.TabStop = false;
            groupBox3.Text = "3. Configure Cohort (doesn't exist yet, next screen will actually create it)";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(7, 24);
            label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(64, 15);
            label9.TabIndex = 2;
            label9.Text = "Comment:";
            // 
            // tbDescription
            // 
            tbDescription.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbDescription.Location = new System.Drawing.Point(71, 24);
            tbDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbDescription.Multiline = true;
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new System.Drawing.Size(969, 67);
            tbDescription.TabIndex = 3;
            // 
            // btnClearProject
            // 
            btnClearProject.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnClearProject.Location = new System.Drawing.Point(528, 7);
            btnClearProject.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnClearProject.Name = "btnClearProject";
            btnClearProject.Size = new System.Drawing.Size(141, 27);
            btnClearProject.TabIndex = 14;
            btnClearProject.Text = "Cancel";
            btnClearProject.UseVisualStyleBackColor = true;
            btnClearProject.Click += btnCancel_Click;
            // 
            // btnOk
            // 
            btnOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnOk.Location = new System.Drawing.Point(372, 7);
            btnOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(149, 27);
            btnOk.TabIndex = 13;
            btnOk.Text = "Ok";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // ragSmiley1
            // 
            ragSmiley1.AlwaysShowHandCursor = false;
            ragSmiley1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            ragSmiley1.Location = new System.Drawing.Point(335, 4);
            ragSmiley1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            ragSmiley1.Name = "ragSmiley1";
            ragSmiley1.Size = new System.Drawing.Size(30, 30);
            ragSmiley1.TabIndex = 12;
            // 
            // gbDescription
            // 
            gbDescription.Controls.Add(label9);
            gbDescription.Controls.Add(tbDescription);
            gbDescription.Dock = System.Windows.Forms.DockStyle.Top;
            gbDescription.Location = new System.Drawing.Point(0, 312);
            gbDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbDescription.Name = "gbDescription";
            gbDescription.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbDescription.Size = new System.Drawing.Size(1048, 99);
            gbDescription.TabIndex = 11;
            gbDescription.TabStop = false;
            gbDescription.Text = "4. Enter Description Of Holdout";
            // 
            // taskDescriptionLabel1
            // 
            taskDescriptionLabel1.AutoSize = true;
            taskDescriptionLabel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            taskDescriptionLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            taskDescriptionLabel1.Location = new System.Drawing.Point(0, 0);
            taskDescriptionLabel1.Name = "taskDescriptionLabel1";
            taskDescriptionLabel1.Size = new System.Drawing.Size(1048, 42);
            taskDescriptionLabel1.TabIndex = 19;
            // 
            // panel1
            // 
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(gbDescription);
            panel1.Controls.Add(groupBox1);
            panel1.Controls.Add(groupBox3);
            panel1.Controls.Add(gbChooseCohortType);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 42);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1048, 509);
            panel1.TabIndex = 20;
            // 
            // panel2
            // 
            panel2.Controls.Add(btnOk);
            panel2.Controls.Add(ragSmiley1);
            panel2.Controls.Add(btnClearProject);
            panel2.Dock = System.Windows.Forms.DockStyle.Top;
            panel2.Location = new System.Drawing.Point(0, 411);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(1048, 55);
            panel2.TabIndex = 20;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(scintilla1);
            groupBox1.Location = new System.Drawing.Point(13, 66);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(997, 139);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "2. Define Holdout Criteria";
            // 
            // scintilla1
            // 
            scintilla1.AutoCMaxHeight = 9;
            scintilla1.BiDirectionality = ScintillaNET.BiDirectionalDisplayType.Disabled;
            scintilla1.CaretLineBackColor = System.Drawing.Color.Black;
            scintilla1.LexerName = null;
            scintilla1.Location = new System.Drawing.Point(19, 22);
            scintilla1.Name = "scintilla1";
            scintilla1.ScrollWidth = 49;
            scintilla1.Size = new System.Drawing.Size(958, 100);
            scintilla1.TabIndents = true;
            scintilla1.TabIndex = 0;
            scintilla1.Text = "scintilla1";
            scintilla1.UseRightToLeftReadingLayout = false;
            scintilla1.WrapMode = ScintillaNET.WrapMode.None;
            // 
            // CohortHoldoutCreationRequestUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1048, 551);
            Controls.Add(panel1);
            Controls.Add(taskDescriptionLabel1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CohortHoldoutCreationRequestUI";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Create Cohort";
            Load += CohortHoldoutCreationRequestUI_Load;
            gbNewCohort.ResumeLayout(false);
            gbNewCohort.PerformLayout();
            gbChooseCohortType.ResumeLayout(false);
            gbChooseCohortType.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            groupBox3.ResumeLayout(false);
            gbDescription.ResumeLayout(false);
            gbDescription.PerformLayout();
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        //private System.Windows.Forms.Label label3;
        //private System.Windows.Forms.Label lblExternalCohortTable;
        private System.Windows.Forms.GroupBox gbNewCohort;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbName;
        //private System.Windows.Forms.Button btnNewProject;
        private System.Windows.Forms.GroupBox gbChooseCohortType;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbDescription;
        //private System.Windows.Forms.Label lblProject;
        //private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnClearProject;
        private System.Windows.Forms.Button btnOk;
        private RAGSmiley ragSmiley1;
        private System.Windows.Forms.GroupBox gbDescription;
        //private System.Windows.Forms.PictureBox pbProject;
        //private System.Windows.Forms.PictureBox pbCohortSource;
        //private System.Windows.Forms.Button btnExisting;
        //private System.Windows.Forms.Label lblErrorNoProjectNumber;
        //private System.Windows.Forms.TextBox tbSetProjectNumber;
        //private System.Windows.Forms.Button btnClear;
        private SimpleDialogs.TaskDescriptionLabel taskDescriptionLabel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox1;
        //private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private ScintillaNET.Scintilla scintilla1;
    }
}