namespace Rdmp.UI.SimpleDialogs
{
    partial class PerformanceCounterUI
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
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnToggleCommandAuditing = new System.Windows.Forms.Button();
            this.btnViewPerformanceResults = new System.Windows.Forms.Button();
            this.lblCommandsAudited = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnToggleCommandAuditing
            // 
            this.btnToggleCommandAuditing.Location = new System.Drawing.Point(12, 12);
            this.btnToggleCommandAuditing.Name = "btnToggleCommandAuditing";
            this.btnToggleCommandAuditing.Size = new System.Drawing.Size(148, 23);
            this.btnToggleCommandAuditing.TabIndex = 1;
            this.btnToggleCommandAuditing.Text = "Start Command Auditing";
            this.btnToggleCommandAuditing.UseVisualStyleBackColor = true;
            this.btnToggleCommandAuditing.Click += new System.EventHandler(this.btnToggleCommandAuditing_Click);
            // 
            // btnViewPerformanceResults
            // 
            this.btnViewPerformanceResults.Enabled = false;
            this.btnViewPerformanceResults.Location = new System.Drawing.Point(166, 12);
            this.btnViewPerformanceResults.Name = "btnViewPerformanceResults";
            this.btnViewPerformanceResults.Size = new System.Drawing.Size(148, 23);
            this.btnViewPerformanceResults.TabIndex = 1;
            this.btnViewPerformanceResults.Text = "View Results";
            this.btnViewPerformanceResults.UseVisualStyleBackColor = true;
            this.btnViewPerformanceResults.Click += new System.EventHandler(this.btnViewPerformanceResults_Click);
            // 
            // lblCommandsAudited
            // 
            this.lblCommandsAudited.AutoSize = true;
            this.lblCommandsAudited.Location = new System.Drawing.Point(320, 17);
            this.lblCommandsAudited.Name = "lblCommandsAudited";
            this.lblCommandsAudited.Size = new System.Drawing.Size(101, 13);
            this.lblCommandsAudited.TabIndex = 2;
            this.lblCommandsAudited.Text = "Commands Audited:";
            // 
            // PerformanceCounterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 41);
            this.Controls.Add(this.lblCommandsAudited);
            this.Controls.Add(this.btnViewPerformanceResults);
            this.Controls.Add(this.btnToggleCommandAuditing);
            this.Name = "PerformanceCounterUI";
            this.Text = "Performance";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CatalogueLibraryPerformanceCounterUI_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnToggleCommandAuditing;
        private System.Windows.Forms.Button btnViewPerformanceResults;
        private System.Windows.Forms.Label lblCommandsAudited;
    }
}