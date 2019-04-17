namespace CatalogueManager.SimpleDialogs.Reports
{
    partial class DocumentationReportFormsAndControlsUI
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
            this.btnGenerateReport = new System.Windows.Forms.Button();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.cbGrabArbitraryOjbectsToPopulateUIs = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnGenerateReport
            // 
            this.btnGenerateReport.Location = new System.Drawing.Point(12, 12);
            this.btnGenerateReport.Name = "btnGenerateReport";
            this.btnGenerateReport.Size = new System.Drawing.Size(110, 23);
            this.btnGenerateReport.TabIndex = 1;
            this.btnGenerateReport.Text = "Generate Report";
            this.btnGenerateReport.UseVisualStyleBackColor = true;
            this.btnGenerateReport.Click += new System.EventHandler(this.btnGenerateReport_Click);
            // 
            // checksUI1
            // 
            this.checksUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checksUI1.Location = new System.Drawing.Point(15, 41);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(1371, 755);
            this.checksUI1.TabIndex = 2;
            // 
            // cbGrabArbitraryOjbectsToPopulateUIs
            // 
            this.cbGrabArbitraryOjbectsToPopulateUIs.AutoSize = true;
            this.cbGrabArbitraryOjbectsToPopulateUIs.Location = new System.Drawing.Point(128, 16);
            this.cbGrabArbitraryOjbectsToPopulateUIs.Name = "cbGrabArbitraryOjbectsToPopulateUIs";
            this.cbGrabArbitraryOjbectsToPopulateUIs.Size = new System.Drawing.Size(241, 17);
            this.cbGrabArbitraryOjbectsToPopulateUIs.TabIndex = 3;
            this.cbGrabArbitraryOjbectsToPopulateUIs.Text = "Attempt to populate UIs with your RMDP data";
            this.cbGrabArbitraryOjbectsToPopulateUIs.UseVisualStyleBackColor = true;
            this.cbGrabArbitraryOjbectsToPopulateUIs.CheckedChanged += new System.EventHandler(this.cbGrabArbitraryOjbectsToPopulateUIs_CheckedChanged);
            // 
            // DocumentationReportFormsAndControlsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1398, 808);
            this.Controls.Add(this.cbGrabArbitraryOjbectsToPopulateUIs);
            this.Controls.Add(this.checksUI1);
            this.Controls.Add(this.btnGenerateReport);
            this.Name = "DocumentationReportFormsAndControlsUI";
            this.Text = "Documentation Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGenerateReport;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.CheckBox cbGrabArbitraryOjbectsToPopulateUIs;
    }
}