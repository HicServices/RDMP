namespace CatalogueManager.DataQualityUIs
{
    partial class DQEExecutionControl
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
            this.checkAndExecuteUI1 = new CatalogueManager.SimpleControls.CheckAndExecuteUI();
            this.rdmpObjectsRibbonUI1 = new CatalogueManager.ObjectVisualisation.RDMPObjectsRibbonUI();
            this.btnConfigureValidation = new System.Windows.Forms.Button();
            this.btnViewResults = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkAndExecuteUI1
            // 
            this.checkAndExecuteUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkAndExecuteUI1.Location = new System.Drawing.Point(0, 22);
            this.checkAndExecuteUI1.Name = "checkAndExecuteUI1";
            this.checkAndExecuteUI1.Size = new System.Drawing.Size(1376, 694);
            this.checkAndExecuteUI1.TabIndex = 0;
            // 
            // rdmpObjectsRibbonUI1
            // 
            this.rdmpObjectsRibbonUI1.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdmpObjectsRibbonUI1.Location = new System.Drawing.Point(0, 0);
            this.rdmpObjectsRibbonUI1.Margin = new System.Windows.Forms.Padding(0);
            this.rdmpObjectsRibbonUI1.Name = "rdmpObjectsRibbonUI1";
            this.rdmpObjectsRibbonUI1.Size = new System.Drawing.Size(1376, 22);
            this.rdmpObjectsRibbonUI1.TabIndex = 1;
            // 
            // btnConfigureValidation
            // 
            this.btnConfigureValidation.Location = new System.Drawing.Point(195, 33);
            this.btnConfigureValidation.Name = "btnConfigureValidation";
            this.btnConfigureValidation.Size = new System.Drawing.Size(149, 23);
            this.btnConfigureValidation.TabIndex = 2;
            this.btnConfigureValidation.Text = "Configure Validation Rules";
            this.btnConfigureValidation.UseVisualStyleBackColor = true;
            this.btnConfigureValidation.Click += new System.EventHandler(this.btnConfigureValidation_Click);
            // 
            // btnViewResults
            // 
            this.btnViewResults.Location = new System.Drawing.Point(195, 62);
            this.btnViewResults.Name = "btnViewResults";
            this.btnViewResults.Size = new System.Drawing.Size(149, 23);
            this.btnViewResults.TabIndex = 2;
            this.btnViewResults.Text = "View Results";
            this.btnViewResults.UseVisualStyleBackColor = true;
            this.btnViewResults.Click += new System.EventHandler(this.btnViewResults_Click);
            // 
            // DQEExecutionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnViewResults);
            this.Controls.Add(this.btnConfigureValidation);
            this.Controls.Add(this.checkAndExecuteUI1);
            this.Controls.Add(this.rdmpObjectsRibbonUI1);
            this.Name = "DQEExecutionControl";
            this.Size = new System.Drawing.Size(1376, 716);
            this.ResumeLayout(false);

        }

        #endregion

        private SimpleControls.CheckAndExecuteUI checkAndExecuteUI1;
        private ObjectVisualisation.RDMPObjectsRibbonUI rdmpObjectsRibbonUI1;
        private System.Windows.Forms.Button btnConfigureValidation;
        private System.Windows.Forms.Button btnViewResults;


    }
}
