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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DQEExecutionControl));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnValidationRules = new System.Windows.Forms.ToolStripButton();
            this.btnViewResults = new System.Windows.Forms.ToolStripButton();
            this.checkAndExecuteUI1 = new CatalogueManager.SimpleControls.CheckAndExecuteUI();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnValidationRules,
            this.btnViewResults});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1376, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnValidationRules
            // 
            this.btnValidationRules.Image = ((System.Drawing.Image)(resources.GetObject("btnValidationRules.Image")));
            this.btnValidationRules.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnValidationRules.Name = "btnValidationRules";
            this.btnValidationRules.Size = new System.Drawing.Size(145, 22);
            this.btnValidationRules.Text = "Configure Validation...";
            this.btnValidationRules.Click += new System.EventHandler(this.btnValidationRules_Click);
            // 
            // btnViewResults
            // 
            this.btnViewResults.Image = ((System.Drawing.Image)(resources.GetObject("btnViewResults.Image")));
            this.btnViewResults.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnViewResults.Name = "btnViewResults";
            this.btnViewResults.Size = new System.Drawing.Size(73, 22);
            this.btnViewResults.Text = "Results...";
            this.btnViewResults.Click += new System.EventHandler(this.btnViewResults_Click);
            // 
            // checkAndExecuteUI1
            // 
            this.checkAndExecuteUI1.AllowsYesNoToAll = true;
            this.checkAndExecuteUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkAndExecuteUI1.Location = new System.Drawing.Point(0, 25);
            this.checkAndExecuteUI1.Name = "checkAndExecuteUI1";
            this.checkAndExecuteUI1.Size = new System.Drawing.Size(1376, 691);
            this.checkAndExecuteUI1.TabIndex = 0;
            // 
            // DQEExecutionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkAndExecuteUI1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "DQEExecutionControl";
            this.Size = new System.Drawing.Size(1376, 716);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SimpleControls.CheckAndExecuteUI checkAndExecuteUI1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnValidationRules;
        private System.Windows.Forms.ToolStripButton btnViewResults;


    }
}
