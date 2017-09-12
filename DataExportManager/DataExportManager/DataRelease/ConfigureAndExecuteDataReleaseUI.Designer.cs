namespace DataExportManager.DataRelease
{
    partial class ConfigureAndExecuteDataReleaseUI
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
            this.btnExecutePipeline = new System.Windows.Forms.Button();
            this.pPipeline = new System.Windows.Forms.Panel();
            this.rdmpObjectsRibbonUI1 = new CatalogueManager.ObjectVisualisation.RDMPObjectsRibbonUI();
            this.progressUI1 = new ReusableUIComponents.Progress.ProgressUI();
            this.SuspendLayout();
            // 
            // btnExecutePipeline
            // 
            this.btnExecutePipeline.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExecutePipeline.Location = new System.Drawing.Point(3, 667);
            this.btnExecutePipeline.Name = "btnExecutePipeline";
            this.btnExecutePipeline.Size = new System.Drawing.Size(97, 23);
            this.btnExecutePipeline.TabIndex = 9;
            this.btnExecutePipeline.Text = "Execute Pipeline";
            this.btnExecutePipeline.UseVisualStyleBackColor = true;
            this.btnExecutePipeline.Click += new System.EventHandler(this.btnExecutePipeline_Click);
            // 
            // pPipeline
            // 
            this.pPipeline.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pPipeline.Location = new System.Drawing.Point(3, 526);
            this.pPipeline.Name = "pPipeline";
            this.pPipeline.Size = new System.Drawing.Size(663, 135);
            this.pPipeline.TabIndex = 8;
            // 
            // rdmpObjectsRibbonUI1
            // 
            this.rdmpObjectsRibbonUI1.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdmpObjectsRibbonUI1.Location = new System.Drawing.Point(0, 0);
            this.rdmpObjectsRibbonUI1.Margin = new System.Windows.Forms.Padding(0);
            this.rdmpObjectsRibbonUI1.Name = "rdmpObjectsRibbonUI1";
            this.rdmpObjectsRibbonUI1.Size = new System.Drawing.Size(1047, 22);
            this.rdmpObjectsRibbonUI1.TabIndex = 10;
            // 
            // progressUI1
            // 
            this.progressUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressUI1.Location = new System.Drawing.Point(0, 25);
            this.progressUI1.Name = "progressUI1";
            this.progressUI1.Size = new System.Drawing.Size(1044, 495);
            this.progressUI1.TabIndex = 11;
            // 
            // ConfigureAndExecuteDataReleaseUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressUI1);
            this.Controls.Add(this.rdmpObjectsRibbonUI1);
            this.Controls.Add(this.btnExecutePipeline);
            this.Controls.Add(this.pPipeline);
            this.Name = "ConfigureAndExecuteDataReleaseUI";
            this.Size = new System.Drawing.Size(1047, 693);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnExecutePipeline;
        private System.Windows.Forms.Panel pPipeline;
        private CatalogueManager.ObjectVisualisation.RDMPObjectsRibbonUI rdmpObjectsRibbonUI1;
        private ReusableUIComponents.Progress.ProgressUI progressUI1;
    }
}
