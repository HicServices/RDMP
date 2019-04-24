namespace CatalogueManager.DataQualityUIs
{
    partial class DQEExecutionControlUI
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
            this.SuspendLayout();
            // 
            // checkAndExecuteUI1
            // 
            this.checkAndExecuteUI1.AllowsYesNoToAll = true;
            this.checkAndExecuteUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkAndExecuteUI1.Location = new System.Drawing.Point(0, 0);
            this.checkAndExecuteUI1.Name = "checkAndExecuteUI1";
            this.checkAndExecuteUI1.Size = new System.Drawing.Size(1376, 716);
            this.checkAndExecuteUI1.TabIndex = 0;
            // 
            // DQEExecutionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkAndExecuteUI1);
            this.Name = "DQEExecutionControl";
            this.Size = new System.Drawing.Size(1376, 716);
            this.ResumeLayout(false);

        }

        #endregion

        private SimpleControls.CheckAndExecuteUI checkAndExecuteUI1;


    }
}
