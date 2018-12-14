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
            this.checkAndExecuteUI1 = new CatalogueManager.SimpleControls.CheckAndExecuteUI();
            this.cbFailures = new System.Windows.Forms.CheckBox();
            this.btnViewFailures = new System.Windows.Forms.Button();
            this.SuspendLayout();
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
            // cbFailures
            // 
            this.cbFailures.AutoSize = true;
            this.cbFailures.Location = new System.Drawing.Point(195, 32);
            this.cbFailures.Name = "cbFailures";
            this.cbFailures.Size = new System.Drawing.Size(120, 17);
            this.cbFailures.TabIndex = 2;
            this.cbFailures.Text = "Retry Failures Mode";
            this.cbFailures.UseVisualStyleBackColor = true;
            // 
            // btnViewFailures
            // 
            this.btnViewFailures.Location = new System.Drawing.Point(321, 28);
            this.btnViewFailures.Name = "btnViewFailures";
            this.btnViewFailures.Size = new System.Drawing.Size(84, 23);
            this.btnViewFailures.TabIndex = 3;
            this.btnViewFailures.Text = "View Failures";
            this.btnViewFailures.UseVisualStyleBackColor = true;
            this.btnViewFailures.Click += new System.EventHandler(this.btnViewFailures_Click);
            // 
            // ExecuteCacheProgressUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnViewFailures);
            this.Controls.Add(this.cbFailures);
            this.Controls.Add(this.checkAndExecuteUI1);
            this.Name = "ExecuteCacheProgressUI";
            this.Size = new System.Drawing.Size(900, 747);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SimpleControls.CheckAndExecuteUI checkAndExecuteUI1;
        private System.Windows.Forms.CheckBox cbFailures;
        private System.Windows.Forms.Button btnViewFailures;

    }
}

