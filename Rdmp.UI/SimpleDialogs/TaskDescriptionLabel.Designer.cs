namespace Rdmp.UI.SimpleDialogs
{
    partial class TaskDescriptionLabel
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
            this.lblTaskDescription = new System.Windows.Forms.Label();
            this.lblEntryLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblTaskDescription
            // 
            this.lblTaskDescription.AutoSize = true;
            this.lblTaskDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTaskDescription.Location = new System.Drawing.Point(0, 0);
            this.lblTaskDescription.Name = "lblTaskDescription";
            this.lblTaskDescription.Size = new System.Drawing.Size(89, 15);
            this.lblTaskDescription.TabIndex = 0;
            this.lblTaskDescription.Text = "TaskDescription";
            // 
            // lblEntryLabel
            // 
            this.lblEntryLabel.AutoSize = true;
            this.lblEntryLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblEntryLabel.Location = new System.Drawing.Point(0, 15);
            this.lblEntryLabel.Name = "lblEntryLabel";
            this.lblEntryLabel.Size = new System.Drawing.Size(62, 15);
            this.lblEntryLabel.TabIndex = 1;
            this.lblEntryLabel.Text = "EntryLabel";
            // 
            // TaskDescriptionLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblEntryLabel);
            this.Controls.Add(this.lblTaskDescription);
            this.Name = "TaskDescriptionLabel";
            this.Size = new System.Drawing.Size(258, 40);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTaskDescription;
        private System.Windows.Forms.Label lblEntryLabel;
    }
}
