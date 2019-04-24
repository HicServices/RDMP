namespace CatalogueManager.SimpleDialogs.Reports
{
    partial class DatabaseSizeReportUI
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
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.btnStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checksUI1
            // 
            this.checksUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checksUI1.Location = new System.Drawing.Point(3, 42);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(914, 605);
            this.checksUI1.TabIndex = 0;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(13, 13);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start Report";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // DatabaseSizeReportUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 659);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.checksUI1);
            this.Name = "DatabaseSizeReportUI";
            this.Text = "Database Size Report";
            this.ResumeLayout(false);

        }

        #endregion

        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.Button btnStart;
    }
}