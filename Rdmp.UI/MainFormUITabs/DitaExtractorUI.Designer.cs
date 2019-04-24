namespace CatalogueManager.MainFormUITabs
{
    partial class DitaExtractorUI
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
            this.btnExtract = new System.Windows.Forms.Button();
            this.tbExtractionDirectory = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblCataloguesProcessed = new System.Windows.Forms.Label();
            this.btnShowDirectory = new System.Windows.Forms.Button();
            this.btnCheck = new System.Windows.Forms.Button();
            this.progressBarsUI1 = new ReusableUIComponents.Progress.ProgressBarsUI();
            this.SuspendLayout();
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(218, 43);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(103, 23);
            this.btnExtract.TabIndex = 0;
            this.btnExtract.Text = "Create DITA Files";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // tbExtractionDirectory
            // 
            this.tbExtractionDirectory.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.tbExtractionDirectory.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.tbExtractionDirectory.Location = new System.Drawing.Point(108, 17);
            this.tbExtractionDirectory.Name = "tbExtractionDirectory";
            this.tbExtractionDirectory.Size = new System.Drawing.Size(354, 20);
            this.tbExtractionDirectory.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Extraction Directory:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(468, 15);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(79, 23);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblCataloguesProcessed
            // 
            this.lblCataloguesProcessed.AutoSize = true;
            this.lblCataloguesProcessed.Location = new System.Drawing.Point(13, 90);
            this.lblCataloguesProcessed.Name = "lblCataloguesProcessed";
            this.lblCataloguesProcessed.Size = new System.Drawing.Size(0, 13);
            this.lblCataloguesProcessed.TabIndex = 5;
            // 
            // btnShowDirectory
            // 
            this.btnShowDirectory.Location = new System.Drawing.Point(327, 43);
            this.btnShowDirectory.Name = "btnShowDirectory";
            this.btnShowDirectory.Size = new System.Drawing.Size(103, 23);
            this.btnShowDirectory.TabIndex = 0;
            this.btnShowDirectory.Text = "Show Directory";
            this.btnShowDirectory.UseVisualStyleBackColor = true;
            this.btnShowDirectory.Click += new System.EventHandler(this.btnShowDirectory_Click);
            // 
            // btnCheck
            // 
            this.btnCheck.Location = new System.Drawing.Point(108, 43);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(103, 23);
            this.btnCheck.TabIndex = 7;
            this.btnCheck.Text = "Check";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // progressBarsUI1
            // 
            this.progressBarsUI1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarsUI1.Location = new System.Drawing.Point(3, 72);
            this.progressBarsUI1.Name = "progressBarsUI1";
            this.progressBarsUI1.Size = new System.Drawing.Size(754, 100);
            this.progressBarsUI1.TabIndex = 8;
            // 
            // DitaExtractorUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressBarsUI1);
            this.Controls.Add(this.btnCheck);
            this.Controls.Add(this.lblCataloguesProcessed);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbExtractionDirectory);
            this.Controls.Add(this.btnShowDirectory);
            this.Controls.Add(this.btnExtract);
            this.Name = "DitaExtractorUI";
            this.Size = new System.Drawing.Size(760, 178);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnExtract;
        private System.Windows.Forms.TextBox tbExtractionDirectory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblCataloguesProcessed;
        private System.Windows.Forms.Button btnShowDirectory;
        private System.Windows.Forms.Button btnCheck;
        private ReusableUIComponents.Progress.ProgressBarsUI progressBarsUI1;
    }
}
