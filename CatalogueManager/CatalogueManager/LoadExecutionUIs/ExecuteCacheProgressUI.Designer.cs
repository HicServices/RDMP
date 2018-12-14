namespace CatalogueManager.LoadExecutionUIs
{
    partial class ExecuteCacheProgressUI
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
            this.cbFailures = new System.Windows.Forms.CheckBox();
            this.checkAndExecuteUI1 = new CatalogueManager.SimpleControls.CheckAndExecuteUI();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbFailures
            // 
            this.cbFailures.AutoSize = true;
            this.cbFailures.Location = new System.Drawing.Point(194, 11);
            this.cbFailures.Name = "cbFailures";
            this.cbFailures.Size = new System.Drawing.Size(120, 17);
            this.cbFailures.TabIndex = 2;
            this.cbFailures.Text = "Retry Failures Mode";
            this.cbFailures.UseVisualStyleBackColor = true;
            // 
            // checkAndExecuteUI1
            // 
            this.checkAndExecuteUI1.AllowsYesNoToAll = true;
            this.checkAndExecuteUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkAndExecuteUI1.Location = new System.Drawing.Point(0, 0);
            this.checkAndExecuteUI1.Name = "checkAndExecuteUI1";
            this.checkAndExecuteUI1.Size = new System.Drawing.Size(900, 747);
            this.checkAndExecuteUI1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbFailures);
            this.panel1.Controls.Add(this.checkAndExecuteUI1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(900, 747);
            this.panel1.TabIndex = 3;
            // 
            // ExecuteCacheProgressUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "ExecuteCacheProgressUI";
            this.Size = new System.Drawing.Size(900, 747);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private SimpleControls.CheckAndExecuteUI checkAndExecuteUI1;
        private System.Windows.Forms.CheckBox cbFailures;
        private System.Windows.Forms.Panel panel1;

    }
}

