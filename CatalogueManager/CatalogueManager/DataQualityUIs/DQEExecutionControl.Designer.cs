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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.checkAndExecuteUI1 = new CatalogueManager.SimpleControls.CheckAndExecuteUI();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1376, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
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
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SimpleControls.CheckAndExecuteUI checkAndExecuteUI1;
        private System.Windows.Forms.ToolStrip toolStrip1;


    }
}
