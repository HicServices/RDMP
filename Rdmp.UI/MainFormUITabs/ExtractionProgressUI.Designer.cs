
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
            this.tbID = new System.Windows.Forms.TextBox();
            this.lblID = new System.Windows.Forms.Label();
            this.tbProgress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbStartDate = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbEndDate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbDaysPerBatch = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ddColumn = new System.Windows.Forms.ComboBox();
            this.btnPickColumn = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.ragSmiley1 = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnFromDQE = new System.Windows.Forms.Button();
            this.lblEvaluationDate = new System.Windows.Forms.Label();
            this.btnResetProgress = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbID
            // 
            this.tbID.Enabled = false;
            this.tbID.Location = new System.Drawing.Point(109, 3);
            this.tbID.Name = "tbID";
            this.tbID.Size = new System.Drawing.Size(91, 23);
            this.tbID.TabIndex = 1;
            // 
            // lblID
            // 
            this.lblID.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblID.AutoSize = true;
            this.lblID.Location = new System.Drawing.Point(3, 7);
            this.lblID.MinimumSize = new System.Drawing.Size(100, 0);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(100, 15);
            this.lblID.TabIndex = 0;
            this.lblID.Text = "ID:";
            this.lblID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbProgress
            // 
            this.tbProgress.Location = new System.Drawing.Point(109, 149);
            this.tbProgress.Name = "tbProgress";
            this.tbProgress.ReadOnly = true;
            this.tbProgress.Size = new System.Drawing.Size(313, 23);
            this.tbProgress.TabIndex = 2;
            this.tbProgress.TextChanged += new System.EventHandler(this.tbDate_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 153);
            this.label1.MinimumSize = new System.Drawing.Size(100, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Progress:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbStartDate
            // 
            this.tbStartDate.Location = new System.Drawing.Point(109, 62);
            this.tbStartDate.Name = "tbStartDate";
            this.tbStartDate.Size = new System.Drawing.Size(313, 23);
            this.tbStartDate.TabIndex = 3;
            this.tbStartDate.TextChanged += new System.EventHandler(this.tbDate_TextChanged);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 66);
            this.label2.MinimumSize = new System.Drawing.Size(100, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Start Date:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbEndDate
            // 
            this.tbEndDate.Location = new System.Drawing.Point(109, 91);
            this.tbEndDate.Name = "tbEndDate";
            this.tbEndDate.Size = new System.Drawing.Size(313, 23);
            this.tbEndDate.TabIndex = 4;
            this.tbEndDate.TextChanged += new System.EventHandler(this.tbDate_TextChanged);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 95);
            this.label3.MinimumSize = new System.Drawing.Size(100, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "End Date:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbDaysPerBatch
            // 
            this.tbDaysPerBatch.Location = new System.Drawing.Point(109, 120);
            this.tbDaysPerBatch.Name = "tbDaysPerBatch";
            this.tbDaysPerBatch.Size = new System.Drawing.Size(91, 23);
            this.tbDaysPerBatch.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 124);
            this.label4.MinimumSize = new System.Drawing.Size(100, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "Days Per Batch:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ddColumn
            // 
            this.ddColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddColumn.FormattingEnabled = true;
            this.ddColumn.Location = new System.Drawing.Point(3, 4);
            this.ddColumn.Name = "ddColumn";
            this.ddColumn.Size = new System.Drawing.Size(250, 23);
            this.ddColumn.TabIndex = 6;
            this.ddColumn.SelectionChangeCommitted += new System.EventHandler(this.ddColumn_SelectionChangeCommitted);
            // 
            // btnPickColumn
            // 
            this.btnPickColumn.Location = new System.Drawing.Point(256, 3);
            this.btnPickColumn.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.btnPickColumn.Name = "btnPickColumn";
            this.btnPickColumn.Size = new System.Drawing.Size(30, 25);
            this.btnPickColumn.TabIndex = 7;
            this.btnPickColumn.Text = "...";
            this.btnPickColumn.UseVisualStyleBackColor = true;
            this.btnPickColumn.Click += new System.EventHandler(this.btnPickColumn_Click);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 36);
            this.label5.MinimumSize = new System.Drawing.Size(100, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 15);
            this.label5.TabIndex = 0;
            this.label5.Text = "Batch Column:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(287, 3);
            this.ragSmiley1.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(29, 24);
            this.ragSmiley1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.lblID, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbDaysPerBatch, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.tbEndDate, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.tbStartDate, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.tbID, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.tbProgress, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 21);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(426, 176);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.ragSmiley1);
            this.panel1.Controls.Add(this.btnPickColumn);
            this.panel1.Controls.Add(this.ddColumn);
            this.panel1.Location = new System.Drawing.Point(106, 29);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(320, 30);
            this.panel1.TabIndex = 9;
            // 
            // btnFromDQE
            // 
            this.btnFromDQE.Location = new System.Drawing.Point(435, 80);
            this.btnFromDQE.Name = "btnFromDQE";
            this.btnFromDQE.Size = new System.Drawing.Size(31, 28);
            this.btnFromDQE.TabIndex = 9;
            this.btnFromDQE.UseVisualStyleBackColor = true;
            this.btnFromDQE.Click += new System.EventHandler(this.btnFromDQE_Click);
            // 
            // lblEvaluationDate
            // 
            this.lblEvaluationDate.AutoSize = true;
            this.lblEvaluationDate.Location = new System.Drawing.Point(435, 112);
            this.lblEvaluationDate.Name = "lblEvaluationDate";
            this.lblEvaluationDate.Size = new System.Drawing.Size(38, 15);
            this.lblEvaluationDate.TabIndex = 10;
            this.lblEvaluationDate.Text = "label6";
            // 
            // btnResetProgress
            // 
            this.btnResetProgress.Location = new System.Drawing.Point(432, 170);
            this.btnResetProgress.Name = "btnResetProgress";
            this.btnResetProgress.Size = new System.Drawing.Size(44, 23);
            this.btnResetProgress.TabIndex = 11;
            this.btnResetProgress.Text = "Reset";
            this.btnResetProgress.UseVisualStyleBackColor = true;
            this.btnResetProgress.Click += new System.EventHandler(this.btnResetProgress_Click);
            // 
            // ExtractionProgressUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnResetProgress);
            this.Controls.Add(this.lblEvaluationDate);
            this.Controls.Add(this.btnFromDQE);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ExtractionProgressUI";
            this.Size = new System.Drawing.Size(479, 205);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}
