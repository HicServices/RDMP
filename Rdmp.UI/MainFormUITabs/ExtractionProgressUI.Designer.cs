
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.tbID = new System.Windows.Forms.TextBox();
            this.lblID = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tbProgress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tbStartDate = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.tbEndDate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.tbDaysPerBatch = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.ddColumn = new System.Windows.Forms.ComboBox();
            this.btnPickColumn = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.ragSmiley1 = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tbID);
            this.panel1.Controls.Add(this.lblID);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(688, 24);
            this.panel1.TabIndex = 0;
            // 
            // tbID
            // 
            this.tbID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbID.Enabled = false;
            this.tbID.Location = new System.Drawing.Point(100, 0);
            this.tbID.Name = "tbID";
            this.tbID.Size = new System.Drawing.Size(588, 23);
            this.tbID.TabIndex = 1;
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblID.Location = new System.Drawing.Point(0, 0);
            this.lblID.MinimumSize = new System.Drawing.Size(100, 0);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(100, 15);
            this.lblID.TabIndex = 0;
            this.lblID.Text = "ID:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tbProgress);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(688, 24);
            this.panel2.TabIndex = 0;
            // 
            // tbProgress
            // 
            this.tbProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbProgress.Location = new System.Drawing.Point(100, 0);
            this.tbProgress.Name = "tbProgress";
            this.tbProgress.Size = new System.Drawing.Size(588, 23);
            this.tbProgress.TabIndex = 2;
            this.tbProgress.TextChanged += new System.EventHandler(this.tbDate_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.MinimumSize = new System.Drawing.Size(100, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Progress:";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.tbStartDate);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 48);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(688, 24);
            this.panel3.TabIndex = 0;
            // 
            // tbStartDate
            // 
            this.tbStartDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbStartDate.Location = new System.Drawing.Point(100, 0);
            this.tbStartDate.Name = "tbStartDate";
            this.tbStartDate.Size = new System.Drawing.Size(588, 23);
            this.tbStartDate.TabIndex = 3;
            this.tbStartDate.TextChanged += new System.EventHandler(this.tbDate_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.MinimumSize = new System.Drawing.Size(100, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Start Date:";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.tbEndDate);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 72);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(688, 24);
            this.panel4.TabIndex = 0;
            // 
            // tbEndDate
            // 
            this.tbEndDate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbEndDate.Location = new System.Drawing.Point(100, 0);
            this.tbEndDate.Name = "tbEndDate";
            this.tbEndDate.Size = new System.Drawing.Size(588, 23);
            this.tbEndDate.TabIndex = 4;
            this.tbEndDate.TextChanged += new System.EventHandler(this.tbDate_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.MinimumSize = new System.Drawing.Size(100, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "End Date:";
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.tbDaysPerBatch);
            this.panel5.Controls.Add(this.label4);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 96);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(688, 24);
            this.panel5.TabIndex = 0;
            // 
            // tbDaysPerBatch
            // 
            this.tbDaysPerBatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbDaysPerBatch.Location = new System.Drawing.Point(100, 0);
            this.tbDaysPerBatch.Name = "tbDaysPerBatch";
            this.tbDaysPerBatch.Size = new System.Drawing.Size(588, 23);
            this.tbDaysPerBatch.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Left;
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.MinimumSize = new System.Drawing.Size(100, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "Days Per Batch:";
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.ddColumn);
            this.panel6.Controls.Add(this.btnPickColumn);
            this.panel6.Controls.Add(this.label5);
            this.panel6.Controls.Add(this.ragSmiley1);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 120);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(688, 24);
            this.panel6.TabIndex = 0;
            // 
            // ddColumn
            // 
            this.ddColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ddColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddColumn.FormattingEnabled = true;
            this.ddColumn.Location = new System.Drawing.Point(100, 0);
            this.ddColumn.Name = "ddColumn";
            this.ddColumn.Size = new System.Drawing.Size(529, 23);
            this.ddColumn.TabIndex = 6;
            this.ddColumn.SelectionChangeCommitted += new System.EventHandler(this.ddColumn_SelectionChangeCommitted);
            // 
            // btnPickColumn
            // 
            this.btnPickColumn.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnPickColumn.Location = new System.Drawing.Point(629, 0);
            this.btnPickColumn.Name = "btnPickColumn";
            this.btnPickColumn.Size = new System.Drawing.Size(30, 24);
            this.btnPickColumn.TabIndex = 7;
            this.btnPickColumn.Text = "...";
            this.btnPickColumn.UseVisualStyleBackColor = true;
            this.btnPickColumn.Click += new System.EventHandler(this.btnPickColumn_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Left;
            this.label5.Location = new System.Drawing.Point(0, 0);
            this.label5.MinimumSize = new System.Drawing.Size(100, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(100, 15);
            this.label5.TabIndex = 0;
            this.label5.Text = "Column:";
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Dock = System.Windows.Forms.DockStyle.Right;
            this.ragSmiley1.Location = new System.Drawing.Point(659, 0);
            this.ragSmiley1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(29, 24);
            this.ragSmiley1.TabIndex = 1;
            // 
            // ExtractionProgressUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "ExtractionProgressUI";
            this.Size = new System.Drawing.Size(688, 436);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox tbProgress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox tbStartDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TextBox tbEndDate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.TextBox tbDaysPerBatch;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox ddColumn;
        private System.Windows.Forms.Button btnPickColumn;
        private ChecksUI.RAGSmiley ragSmiley1;
    }
}
