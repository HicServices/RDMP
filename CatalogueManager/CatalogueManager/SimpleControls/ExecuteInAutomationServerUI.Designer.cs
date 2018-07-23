namespace CatalogueManager.SimpleControls
{
    partial class ExecuteInAutomationServerUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExecuteInAutomationServerUI));
            this.btnCopyCommandToClipboard = new System.Windows.Forms.Button();
            this.btnExecuteDetatched = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnCopyCommandToClipboard
            // 
            this.btnCopyCommandToClipboard.Image = ((System.Drawing.Image)(resources.GetObject("btnCopyCommandToClipboard.Image")));
            this.btnCopyCommandToClipboard.Location = new System.Drawing.Point(99, 3);
            this.btnCopyCommandToClipboard.Name = "btnCopyCommandToClipboard";
            this.btnCopyCommandToClipboard.Size = new System.Drawing.Size(51, 29);
            this.btnCopyCommandToClipboard.TabIndex = 62;
            this.btnCopyCommandToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyCommandToClipboard.Click += new System.EventHandler(this.btnCopyCommandToClipboard_Click);
            // 
            // btnExecuteDetatched
            // 
            this.btnExecuteDetatched.Image = ((System.Drawing.Image)(resources.GetObject("btnExecuteDetatched.Image")));
            this.btnExecuteDetatched.Location = new System.Drawing.Point(7, 2);
            this.btnExecuteDetatched.Name = "btnExecuteDetatched";
            this.btnExecuteDetatched.Size = new System.Drawing.Size(86, 29);
            this.btnExecuteDetatched.TabIndex = 63;
            this.btnExecuteDetatched.UseVisualStyleBackColor = true;
            this.btnExecuteDetatched.Click += new System.EventHandler(this.btnExecuteDetatched_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(110, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 13);
            this.label7.TabIndex = 60;
            this.label7.Text = "Copy";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(1, 31);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(99, 13);
            this.label6.TabIndex = 61;
            this.label6.Text = "Execute Detatched";
            // 
            // ExecuteInAutomationServerUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCopyCommandToClipboard);
            this.Controls.Add(this.btnExecuteDetatched);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Name = "ExecuteInAutomationServerUI";
            this.Size = new System.Drawing.Size(155, 52);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCopyCommandToClipboard;
        private System.Windows.Forms.Button btnExecuteDetatched;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
    }
}
