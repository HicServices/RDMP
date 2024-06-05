namespace Rdmp.UI.AggregationUIs.Advanced
{
    partial class AggregateGraphDateSelector
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
            btnRefresh = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            tbStartDate = new System.Windows.Forms.TextBox();
            tbEndDate = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new System.Drawing.Point(63, 110);
            btnRefresh.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(88, 27);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "Refresh Data";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(187, 110);
            btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(88, 27);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // tbStartDate
            // 
            tbStartDate.Location = new System.Drawing.Point(28, 57);
            tbStartDate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbStartDate.Name = "tbStartDate";
            tbStartDate.Size = new System.Drawing.Size(123, 23);
            tbStartDate.TabIndex = 5;
            tbStartDate.TextChanged += tbStartDate_TextChanged;
            // 
            // tbEndDate
            // 
            tbEndDate.Location = new System.Drawing.Point(186, 57);
            tbEndDate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbEndDate.Name = "tbEndDate";
            tbEndDate.Size = new System.Drawing.Size(123, 23);
            tbEndDate.TabIndex = 6;
            tbEndDate.TextChanged += tbEndDate_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(58, 28);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(58, 15);
            label1.TabIndex = 7;
            label1.Text = "Start Date";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(221, 28);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(54, 15);
            label2.TabIndex = 8;
            label2.Text = "End Date";
            label2.Click += label2_Click;
            // 
            // AggregateGraphDateSelector
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(360, 174);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(tbEndDate);
            Controls.Add(tbStartDate);
            Controls.Add(btnCancel);
            Controls.Add(btnRefresh);
            Name = "AggregateGraphDateSelector";
            Text = "Select a start and end date to refresh";
            Load += AggregateGraphDateSelector_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox tbStartDate;
        private System.Windows.Forms.TextBox tbEndDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}