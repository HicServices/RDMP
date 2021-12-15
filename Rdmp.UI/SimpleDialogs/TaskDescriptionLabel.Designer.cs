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
            this.tbTaskDescription = new System.Windows.Forms.TextBox();
            this.tbEntryLabel = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbTaskDescription
            // 
            this.tbTaskDescription.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(236)))), ((int)(((byte)(242)))));
            this.tbTaskDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbTaskDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbTaskDescription.Enabled = false;
            this.tbTaskDescription.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.tbTaskDescription.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(108)))), ((int)(((byte)(128)))));
            this.tbTaskDescription.Location = new System.Drawing.Point(0, 0);
            this.tbTaskDescription.Multiline = true;
            this.tbTaskDescription.Name = "tbTaskDescription";
            this.tbTaskDescription.Size = new System.Drawing.Size(200, 50);
            this.tbTaskDescription.TabIndex = 2;
            this.tbTaskDescription.Text = "tbTaskDescription";
            this.tbTaskDescription.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.tbTaskDescription.Resize += new System.EventHandler(this.textBox1_Resize);
            // 
            // tbEntryLabel
            // 
            this.tbEntryLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(248)))), ((int)(((byte)(228)))));
            this.tbEntryLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbEntryLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbEntryLabel.Enabled = false;
            this.tbEntryLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.tbEntryLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(134)))), ((int)(((byte)(105)))), ((int)(((byte)(53)))));
            this.tbEntryLabel.Location = new System.Drawing.Point(0, 50);
            this.tbEntryLabel.Multiline = true;
            this.tbEntryLabel.Name = "tbEntryLabel";
            this.tbEntryLabel.Size = new System.Drawing.Size(200, 50);
            this.tbEntryLabel.TabIndex = 3;
            this.tbEntryLabel.Text = "tbEntryLabel";
            this.tbEntryLabel.Resize += new System.EventHandler(this.tbEntryLabel_Resize);
            // 
            // TaskDescriptionLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tbEntryLabel);
            this.Controls.Add(this.tbTaskDescription);
            this.MinimumSize = new System.Drawing.Size(200, 0);
            this.Name = "TaskDescriptionLabel";
            this.Size = new System.Drawing.Size(200, 100);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox tbTaskDescription;
        private System.Windows.Forms.TextBox tbEntryLabel;
    }
}
