using CatalogueManager.Refreshing;

namespace DataExportManager.ProjectUI
{
    partial class ConfigureDatasetUI
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbSelectedColumns = new System.Windows.Forms.ListBox();
            this.btnExcludeAll = new System.Windows.Forms.Button();
            this.btnExclude = new System.Windows.Forms.Button();
            this.btnInclude = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnSelectCore = new System.Windows.Forms.Button();
            this.cbShowDeprecatedColumns = new System.Windows.Forms.CheckBox();
            this.cbShowSpecialApproval = new System.Windows.Forms.CheckBox();
            this.cbShowCohortColumns = new System.Windows.Forms.CheckBox();
            this.cbShowSupplemental = new System.Windows.Forms.CheckBox();
            this.lbAvailableColumns = new System.Windows.Forms.ListBox();
            this.btnViewSQL = new System.Windows.Forms.Button();
            this.lblExtractionInformationDeletedColor = new System.Windows.Forms.Label();
            this.lblExtractionInformationDeleted = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Available Columns:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Selected Columns:";
            // 
            // lbSelectedColumns
            // 
            this.lbSelectedColumns.AllowDrop = true;
            this.lbSelectedColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbSelectedColumns.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbSelectedColumns.FormattingEnabled = true;
            this.lbSelectedColumns.HorizontalScrollbar = true;
            this.lbSelectedColumns.Location = new System.Drawing.Point(6, 16);
            this.lbSelectedColumns.Name = "lbSelectedColumns";
            this.lbSelectedColumns.Size = new System.Drawing.Size(317, 667);
            this.lbSelectedColumns.TabIndex = 2;
            this.lbSelectedColumns.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbSelectedColumns_DrawItem);
            this.lbSelectedColumns.DragDrop += new System.Windows.Forms.DragEventHandler(this.lbSelectedColumns_DragDrop);
            this.lbSelectedColumns.DragOver += new System.Windows.Forms.DragEventHandler(this.lbSelectedColumns_DragOver);
            this.lbSelectedColumns.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbSelectedColumns_KeyUp);
            this.lbSelectedColumns.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lbSelectedColumns_MouseDown);
            // 
            // btnExcludeAll
            // 
            this.btnExcludeAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExcludeAll.Location = new System.Drawing.Point(337, 95);
            this.btnExcludeAll.Name = "btnExcludeAll";
            this.btnExcludeAll.Size = new System.Drawing.Size(75, 23);
            this.btnExcludeAll.TabIndex = 44;
            this.btnExcludeAll.Text = "<<";
            this.btnExcludeAll.UseVisualStyleBackColor = true;
            this.btnExcludeAll.Click += new System.EventHandler(this.btnExcludeAll_Click);
            // 
            // btnExclude
            // 
            this.btnExclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExclude.Location = new System.Drawing.Point(337, 66);
            this.btnExclude.Name = "btnExclude";
            this.btnExclude.Size = new System.Drawing.Size(75, 23);
            this.btnExclude.TabIndex = 42;
            this.btnExclude.Text = "<";
            this.btnExclude.UseVisualStyleBackColor = true;
            this.btnExclude.Click += new System.EventHandler(this.btnExclude_Click);
            // 
            // btnInclude
            // 
            this.btnInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInclude.Location = new System.Drawing.Point(337, 37);
            this.btnInclude.Name = "btnInclude";
            this.btnInclude.Size = new System.Drawing.Size(75, 23);
            this.btnInclude.TabIndex = 41;
            this.btnInclude.Text = ">";
            this.btnInclude.UseVisualStyleBackColor = true;
            this.btnInclude.Click += new System.EventHandler(this.btnInclude_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(182, 688);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 13);
            this.label3.TabIndex = 47;
            this.label3.Text = "(Ctrl+V to paste in headers)";
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label13);
            this.splitContainer1.Panel1.Controls.Add(this.label12);
            this.splitContainer1.Panel1.Controls.Add(this.label8);
            this.splitContainer1.Panel1.Controls.Add(this.label9);
            this.splitContainer1.Panel1.Controls.Add(this.label11);
            this.splitContainer1.Panel1.Controls.Add(this.label10);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            this.splitContainer1.Panel1.Controls.Add(this.label7);
            this.splitContainer1.Panel1.Controls.Add(this.btnSelectCore);
            this.splitContainer1.Panel1.Controls.Add(this.cbShowDeprecatedColumns);
            this.splitContainer1.Panel1.Controls.Add(this.cbShowSpecialApproval);
            this.splitContainer1.Panel1.Controls.Add(this.cbShowCohortColumns);
            this.splitContainer1.Panel1.Controls.Add(this.cbShowSupplemental);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.lbAvailableColumns);
            this.splitContainer1.Panel1.Controls.Add(this.btnInclude);
            this.splitContainer1.Panel1.Controls.Add(this.btnExclude);
            this.splitContainer1.Panel1.Controls.Add(this.btnExcludeAll);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnViewSQL);
            this.splitContainer1.Panel2.Controls.Add(this.lblExtractionInformationDeletedColor);
            this.splitContainer1.Panel2.Controls.Add(this.lblExtractionInformationDeleted);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.lbSelectedColumns);
            this.splitContainer1.Size = new System.Drawing.Size(755, 738);
            this.splitContainer1.SplitterDistance = 421;
            this.splitContainer1.TabIndex = 52;
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(37, 707);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(106, 13);
            this.label13.TabIndex = 153;
            this.label13.Text = "Deprecated Columns";
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label12.BackColor = System.Drawing.Color.Red;
            this.label12.ForeColor = System.Drawing.Color.Red;
            this.label12.Location = new System.Drawing.Point(21, 707);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(10, 10);
            this.label12.TabIndex = 152;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(37, 721);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 13);
            this.label8.TabIndex = 153;
            this.label8.Text = "Cohort Columns";
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label9.BackColor = System.Drawing.Color.Blue;
            this.label9.ForeColor = System.Drawing.Color.Blue;
            this.label9.Location = new System.Drawing.Point(21, 721);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(10, 10);
            this.label9.TabIndex = 152;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(38, 692);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(111, 13);
            this.label11.TabIndex = 151;
            this.label11.Text = "Special Approval Only";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label10.BackColor = System.Drawing.Color.Tan;
            this.label10.ForeColor = System.Drawing.Color.Blue;
            this.label10.Location = new System.Drawing.Point(21, 693);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(10, 10);
            this.label10.TabIndex = 150;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 679);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(121, 13);
            this.label4.TabIndex = 151;
            this.label4.Text = "Supplemental Extraction";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.BackColor = System.Drawing.Color.Orange;
            this.label5.ForeColor = System.Drawing.Color.Blue;
            this.label5.Location = new System.Drawing.Point(21, 679);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 10);
            this.label5.TabIndex = 150;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(37, 665);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 149;
            this.label6.Text = "Core Extraction";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.BackColor = System.Drawing.Color.Green;
            this.label7.ForeColor = System.Drawing.Color.Blue;
            this.label7.Location = new System.Drawing.Point(21, 665);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(10, 10);
            this.label7.TabIndex = 148;
            // 
            // btnSelectCore
            // 
            this.btnSelectCore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectCore.Location = new System.Drawing.Point(337, 210);
            this.btnSelectCore.Name = "btnSelectCore";
            this.btnSelectCore.Size = new System.Drawing.Size(75, 26);
            this.btnSelectCore.TabIndex = 46;
            this.btnSelectCore.Text = "Select Core";
            this.btnSelectCore.UseVisualStyleBackColor = true;
            this.btnSelectCore.Click += new System.EventHandler(this.btnSelectCore_Click);
            // 
            // cbShowDeprecatedColumns
            // 
            this.cbShowDeprecatedColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbShowDeprecatedColumns.AutoSize = true;
            this.cbShowDeprecatedColumns.Location = new System.Drawing.Point(19, 648);
            this.cbShowDeprecatedColumns.Name = "cbShowDeprecatedColumns";
            this.cbShowDeprecatedColumns.Size = new System.Drawing.Size(142, 17);
            this.cbShowDeprecatedColumns.TabIndex = 45;
            this.cbShowDeprecatedColumns.Text = "Show Deprecated Fields";
            this.cbShowDeprecatedColumns.UseVisualStyleBackColor = true;
            this.cbShowDeprecatedColumns.CheckedChanged += new System.EventHandler(this.cbAnyShowFieldsRadioButton_CheckedChanged);
            // 
            // cbShowSpecialApproval
            // 
            this.cbShowSpecialApproval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbShowSpecialApproval.AutoSize = true;
            this.cbShowSpecialApproval.Location = new System.Drawing.Point(19, 632);
            this.cbShowSpecialApproval.Name = "cbShowSpecialApproval";
            this.cbShowSpecialApproval.Size = new System.Drawing.Size(194, 17);
            this.cbShowSpecialApproval.TabIndex = 45;
            this.cbShowSpecialApproval.Text = "Show \'Special Approval Only\' Fields";
            this.cbShowSpecialApproval.UseVisualStyleBackColor = true;
            this.cbShowSpecialApproval.CheckedChanged += new System.EventHandler(this.cbAnyShowFieldsRadioButton_CheckedChanged);
            // 
            // cbShowCohortColumns
            // 
            this.cbShowCohortColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbShowCohortColumns.AutoSize = true;
            this.cbShowCohortColumns.Location = new System.Drawing.Point(19, 596);
            this.cbShowCohortColumns.Name = "cbShowCohortColumns";
            this.cbShowCohortColumns.Size = new System.Drawing.Size(168, 17);
            this.cbShowCohortColumns.TabIndex = 45;
            this.cbShowCohortColumns.Text = "Show Custom Cohort Columns";
            this.cbShowCohortColumns.UseVisualStyleBackColor = true;
            this.cbShowCohortColumns.CheckedChanged += new System.EventHandler(this.cbAnyShowFieldsRadioButton_CheckedChanged);
            // 
            // cbShowSupplemental
            // 
            this.cbShowSupplemental.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbShowSupplemental.AutoSize = true;
            this.cbShowSupplemental.Checked = true;
            this.cbShowSupplemental.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbShowSupplemental.Location = new System.Drawing.Point(19, 614);
            this.cbShowSupplemental.Name = "cbShowSupplemental";
            this.cbShowSupplemental.Size = new System.Drawing.Size(150, 17);
            this.cbShowSupplemental.TabIndex = 45;
            this.cbShowSupplemental.Text = "Show Supplemental Fields";
            this.cbShowSupplemental.UseVisualStyleBackColor = true;
            this.cbShowSupplemental.CheckedChanged += new System.EventHandler(this.cbAnyShowFieldsRadioButton_CheckedChanged);
            // 
            // lbAvailableColumns
            // 
            this.lbAvailableColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbAvailableColumns.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbAvailableColumns.FormattingEnabled = true;
            this.lbAvailableColumns.HorizontalScrollbar = true;
            this.lbAvailableColumns.Location = new System.Drawing.Point(16, 32);
            this.lbAvailableColumns.Name = "lbAvailableColumns";
            this.lbAvailableColumns.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbAvailableColumns.Size = new System.Drawing.Size(315, 550);
            this.lbAvailableColumns.TabIndex = 0;
            this.lbAvailableColumns.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbAvailableColumns_DrawItem);
            // 
            // btnViewSQL
            // 
            this.btnViewSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnViewSQL.Location = new System.Drawing.Point(8, 707);
            this.btnViewSQL.Name = "btnViewSQL";
            this.btnViewSQL.Size = new System.Drawing.Size(92, 23);
            this.btnViewSQL.TabIndex = 154;
            this.btnViewSQL.Text = "View SQL...";
            this.btnViewSQL.UseVisualStyleBackColor = true;
            this.btnViewSQL.Click += new System.EventHandler(this.btnViewSQL_Click);
            // 
            // lblExtractionInformationDeletedColor
            // 
            this.lblExtractionInformationDeletedColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblExtractionInformationDeletedColor.BackColor = System.Drawing.Color.Red;
            this.lblExtractionInformationDeletedColor.ForeColor = System.Drawing.Color.Red;
            this.lblExtractionInformationDeletedColor.Location = new System.Drawing.Point(8, 691);
            this.lblExtractionInformationDeletedColor.Name = "lblExtractionInformationDeletedColor";
            this.lblExtractionInformationDeletedColor.Size = new System.Drawing.Size(10, 10);
            this.lblExtractionInformationDeletedColor.TabIndex = 152;
            // 
            // lblExtractionInformationDeleted
            // 
            this.lblExtractionInformationDeleted.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblExtractionInformationDeleted.AutoSize = true;
            this.lblExtractionInformationDeleted.Location = new System.Drawing.Point(24, 690);
            this.lblExtractionInformationDeleted.Name = "lblExtractionInformationDeleted";
            this.lblExtractionInformationDeleted.Size = new System.Drawing.Size(122, 13);
            this.lblExtractionInformationDeleted.TabIndex = 153;
            this.lblExtractionInformationDeleted.Text = "Catalogue Entry Deleted";
            // 
            // ConfigureDatasetUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ConfigureDatasetUI";
            this.Size = new System.Drawing.Size(755, 738);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lbSelectedColumns;
        private System.Windows.Forms.Button btnExcludeAll;
        private System.Windows.Forms.Button btnExclude;
        private System.Windows.Forms.Button btnInclude;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox cbShowSupplemental;
        private System.Windows.Forms.ListBox lbAvailableColumns;
        private System.Windows.Forms.Button btnSelectCore;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox cbShowSpecialApproval;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox cbShowCohortColumns;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox cbShowDeprecatedColumns;
        private System.Windows.Forms.Label lblExtractionInformationDeleted;
        private System.Windows.Forms.Label lblExtractionInformationDeletedColor;
        private System.Windows.Forms.Button btnViewSQL;
    }
}
