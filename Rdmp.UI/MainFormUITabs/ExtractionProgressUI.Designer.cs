
namespace Rdmp.UI.MainFormUITabs
{
    partial class ExtractionProgressUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtractionProgressUI));
            tbID = new System.Windows.Forms.TextBox();
            lblID = new System.Windows.Forms.Label();
            tbProgress = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            tbStartDate = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            tbEndDate = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            tbDaysPerBatch = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            ddColumn = new System.Windows.Forms.ComboBox();
            btnPickColumn = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            ragSmiley1 = new Rdmp.UI.ChecksUI.RAGSmiley();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            panel1 = new System.Windows.Forms.Panel();
            label6 = new System.Windows.Forms.Label();
            ddRetry = new System.Windows.Forms.ComboBox();
            label7 = new System.Windows.Forms.Label();
            cbIsDeltaExtraction = new System.Windows.Forms.CheckBox();
            btnFromDQE = new System.Windows.Forms.Button();
            lblEvaluationDate = new System.Windows.Forms.Label();
            btnResetProgress = new System.Windows.Forms.Button();
            deltaExtractionHelpIcon = new Rdmp.UI.SimpleControls.HelpIcon();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tbID
            // 
            tbID.Enabled = false;
            tbID.Location = new System.Drawing.Point(113, 3);
            tbID.Name = "tbID";
            tbID.Size = new System.Drawing.Size(91, 23);
            tbID.TabIndex = 1;
            // 
            // lblID
            // 
            lblID.Anchor = System.Windows.Forms.AnchorStyles.None;
            lblID.AutoSize = true;
            lblID.Location = new System.Drawing.Point(5, 7);
            lblID.MinimumSize = new System.Drawing.Size(100, 0);
            lblID.Name = "lblID";
            lblID.Size = new System.Drawing.Size(100, 15);
            lblID.TabIndex = 0;
            lblID.Text = "ID:";
            lblID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbProgress
            // 
            tbProgress.Location = new System.Drawing.Point(113, 149);
            tbProgress.Name = "tbProgress";
            tbProgress.ReadOnly = true;
            tbProgress.Size = new System.Drawing.Size(313, 23);
            tbProgress.TabIndex = 2;
            tbProgress.TextChanged += tbDate_TextChanged;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(5, 153);
            label1.MinimumSize = new System.Drawing.Size(100, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(100, 15);
            label1.TabIndex = 0;
            label1.Text = "Progress:";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbStartDate
            // 
            tbStartDate.Location = new System.Drawing.Point(113, 62);
            tbStartDate.Name = "tbStartDate";
            tbStartDate.Size = new System.Drawing.Size(313, 23);
            tbStartDate.TabIndex = 3;
            tbStartDate.TextChanged += tbDate_TextChanged;
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(5, 66);
            label2.MinimumSize = new System.Drawing.Size(100, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(100, 15);
            label2.TabIndex = 0;
            label2.Text = "Start Date:";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbEndDate
            // 
            tbEndDate.Location = new System.Drawing.Point(113, 91);
            tbEndDate.Name = "tbEndDate";
            tbEndDate.Size = new System.Drawing.Size(313, 23);
            tbEndDate.TabIndex = 4;
            tbEndDate.TextChanged += tbDate_TextChanged;
            // 
            // label3
            // 
            label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(5, 95);
            label3.MinimumSize = new System.Drawing.Size(100, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(100, 15);
            label3.TabIndex = 0;
            label3.Text = "End Date:";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbDaysPerBatch
            // 
            tbDaysPerBatch.Location = new System.Drawing.Point(113, 120);
            tbDaysPerBatch.Name = "tbDaysPerBatch";
            tbDaysPerBatch.Size = new System.Drawing.Size(91, 23);
            tbDaysPerBatch.TabIndex = 5;
            // 
            // label4
            // 
            label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(5, 124);
            label4.MinimumSize = new System.Drawing.Size(100, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(100, 15);
            label4.TabIndex = 0;
            label4.Text = "Days Per Batch:";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ddColumn
            // 
            ddColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddColumn.FormattingEnabled = true;
            ddColumn.Location = new System.Drawing.Point(3, 4);
            ddColumn.Name = "ddColumn";
            ddColumn.Size = new System.Drawing.Size(250, 23);
            ddColumn.TabIndex = 6;
            ddColumn.SelectionChangeCommitted += ddColumn_SelectionChangeCommitted;
            // 
            // btnPickColumn
            // 
            btnPickColumn.Location = new System.Drawing.Point(256, 3);
            btnPickColumn.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            btnPickColumn.Name = "btnPickColumn";
            btnPickColumn.Size = new System.Drawing.Size(30, 25);
            btnPickColumn.TabIndex = 7;
            btnPickColumn.Text = "...";
            btnPickColumn.UseVisualStyleBackColor = true;
            btnPickColumn.Click += btnPickColumn_Click;
            // 
            // label5
            // 
            label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(5, 36);
            label5.MinimumSize = new System.Drawing.Size(100, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(100, 15);
            label5.TabIndex = 0;
            label5.Text = "Batch Column:";
            label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ragSmiley1
            // 
            ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            ragSmiley1.Location = new System.Drawing.Point(287, 3);
            ragSmiley1.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            ragSmiley1.Name = "ragSmiley1";
            ragSmiley1.Size = new System.Drawing.Size(29, 24);
            ragSmiley1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel1.Controls.Add(lblID, 0, 0);
            tableLayoutPanel1.Controls.Add(panel1, 1, 2);
            tableLayoutPanel1.Controls.Add(tbDaysPerBatch, 1, 5);
            tableLayoutPanel1.Controls.Add(tbEndDate, 1, 4);
            tableLayoutPanel1.Controls.Add(tbStartDate, 1, 3);
            tableLayoutPanel1.Controls.Add(tbID, 1, 0);
            tableLayoutPanel1.Controls.Add(label3, 0, 4);
            tableLayoutPanel1.Controls.Add(label4, 0, 5);
            tableLayoutPanel1.Controls.Add(label1, 0, 7);
            tableLayoutPanel1.Controls.Add(tbProgress, 1, 7);
            tableLayoutPanel1.Controls.Add(label5, 0, 2);
            tableLayoutPanel1.Controls.Add(label2, 0, 3);
            tableLayoutPanel1.Controls.Add(label6, 0, 8);
            tableLayoutPanel1.Controls.Add(ddRetry, 1, 8);
            tableLayoutPanel1.Controls.Add(label7, 0, 9);
            tableLayoutPanel1.Controls.Add(cbIsDeltaExtraction, 1, 9);
            tableLayoutPanel1.Location = new System.Drawing.Point(3, 21);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 10;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            tableLayoutPanel1.Size = new System.Drawing.Size(426, 236);
            tableLayoutPanel1.TabIndex = 8;
            // 
            // panel1
            // 
            panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panel1.Controls.Add(ragSmiley1);
            panel1.Controls.Add(btnPickColumn);
            panel1.Controls.Add(ddColumn);
            panel1.Location = new System.Drawing.Point(110, 29);
            panel1.Margin = new System.Windows.Forms.Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(320, 30);
            panel1.TabIndex = 9;
            // 
            // label6
            // 
            label6.Anchor = System.Windows.Forms.AnchorStyles.None;
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(5, 183);
            label6.MinimumSize = new System.Drawing.Size(100, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(100, 15);
            label6.TabIndex = 10;
            label6.Text = "Retry:";
            label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ddRetry
            // 
            ddRetry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddRetry.FormattingEnabled = true;
            ddRetry.Location = new System.Drawing.Point(113, 178);
            ddRetry.Name = "ddRetry";
            ddRetry.Size = new System.Drawing.Size(250, 23);
            ddRetry.TabIndex = 11;
            ddRetry.SelectedIndexChanged += ddRetry_SelectedIndexChanged;
            // 
            // label7
            // 
            label7.Anchor = System.Windows.Forms.AnchorStyles.None;
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(3, 214);
            label7.MinimumSize = new System.Drawing.Size(100, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(104, 15);
            label7.TabIndex = 12;
            label7.Text = "Is Delta Extraction:";
            label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbIsDeltaExtraction
            // 
            cbIsDeltaExtraction.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbIsDeltaExtraction.AutoSize = true;
            cbIsDeltaExtraction.Location = new System.Drawing.Point(113, 214);
            cbIsDeltaExtraction.Name = "cbIsDeltaExtraction";
            cbIsDeltaExtraction.Size = new System.Drawing.Size(15, 14);
            cbIsDeltaExtraction.TabIndex = 13;
            cbIsDeltaExtraction.UseVisualStyleBackColor = true;
            cbIsDeltaExtraction.CheckedChanged += cbIsDeltaExtraction_CheckedChanged;
            // 
            // btnFromDQE
            // 
            btnFromDQE.Location = new System.Drawing.Point(435, 80);
            btnFromDQE.Name = "btnFromDQE";
            btnFromDQE.Size = new System.Drawing.Size(31, 28);
            btnFromDQE.TabIndex = 9;
            btnFromDQE.UseVisualStyleBackColor = true;
            btnFromDQE.Click += btnFromDQE_Click;
            // 
            // lblEvaluationDate
            // 
            lblEvaluationDate.AutoSize = true;
            lblEvaluationDate.Location = new System.Drawing.Point(435, 112);
            lblEvaluationDate.Name = "lblEvaluationDate";
            lblEvaluationDate.Size = new System.Drawing.Size(38, 15);
            lblEvaluationDate.TabIndex = 10;
            lblEvaluationDate.Text = "label6";
            // 
            // btnResetProgress
            // 
            btnResetProgress.Location = new System.Drawing.Point(432, 170);
            btnResetProgress.Name = "btnResetProgress";
            btnResetProgress.Size = new System.Drawing.Size(44, 23);
            btnResetProgress.TabIndex = 11;
            btnResetProgress.Text = "Reset";
            btnResetProgress.UseVisualStyleBackColor = true;
            btnResetProgress.Click += btnResetProgress_Click;
            // 
            // deltaExtractionHelpIcon
            // 
            deltaExtractionHelpIcon.BackColor = System.Drawing.Color.Transparent;
            deltaExtractionHelpIcon.BackgroundImage = (System.Drawing.Image)resources.GetObject("deltaExtractionHelpIcon.BackgroundImage");
            deltaExtractionHelpIcon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            deltaExtractionHelpIcon.Location = new System.Drawing.Point(435, 235);
            deltaExtractionHelpIcon.Margin = new System.Windows.Forms.Padding(0);
            deltaExtractionHelpIcon.MinimumSize = new System.Drawing.Size(22, 22);
            deltaExtractionHelpIcon.Name = "deltaExtractionHelpIcon";
            deltaExtractionHelpIcon.Size = new System.Drawing.Size(22, 22);
            deltaExtractionHelpIcon.TabIndex = 12;
            // 
            // ExtractionProgressUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(deltaExtractionHelpIcon);
            Controls.Add(btnResetProgress);
            Controls.Add(lblEvaluationDate);
            Controls.Add(btnFromDQE);
            Controls.Add(tableLayoutPanel1);
            Name = "ExtractionProgressUI";
            Size = new System.Drawing.Size(479, 300);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.TextBox tbProgress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbStartDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbEndDate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbDaysPerBatch;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox ddColumn;
        private System.Windows.Forms.Button btnPickColumn;
        private ChecksUI.RAGSmiley ragSmiley1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnFromDQE;
        private System.Windows.Forms.Label lblEvaluationDate;
        private System.Windows.Forms.Button btnResetProgress;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox ddRetry;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox cbIsDeltaExtraction;
        private SimpleControls.HelpIcon deltaExtractionHelpIcon;
    }
}
