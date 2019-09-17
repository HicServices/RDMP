namespace Rdmp.UI.Versioning
{
    partial class PatchingUI
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
            this.label4 = new System.Windows.Forms.Label();
            this.btnAttemptPatching = new System.Windows.Forms.Button();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.tbPatch = new System.Windows.Forms.TextBox();
            this.tbDatabase = new System.Windows.Forms.TextBox();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Patcher:";
            // 
            // btnAttemptPatching
            // 
            this.btnAttemptPatching.Location = new System.Drawing.Point(74, 55);
            this.btnAttemptPatching.Name = "btnAttemptPatching";
            this.btnAttemptPatching.Size = new System.Drawing.Size(96, 23);
            this.btnAttemptPatching.TabIndex = 5;
            this.btnAttemptPatching.Text = "Apply";
            this.btnAttemptPatching.UseVisualStyleBackColor = true;
            this.btnAttemptPatching.Click += new System.EventHandler(this.btnAttemptPatching_Click);
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(13, 32);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(56, 13);
            this.lblDatabase.TabIndex = 2;
            this.lblDatabase.Text = "Database:";
            // 
            // tbPatch
            // 
            this.tbPatch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPatch.Location = new System.Drawing.Point(74, 6);
            this.tbPatch.Name = "tbPatch";
            this.tbPatch.ReadOnly = true;
            this.tbPatch.Size = new System.Drawing.Size(1209, 20);
            this.tbPatch.TabIndex = 6;
            // 
            // tbDatabase
            // 
            this.tbDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDatabase.Location = new System.Drawing.Point(75, 29);
            this.tbDatabase.Name = "tbDatabase";
            this.tbDatabase.ReadOnly = true;
            this.tbDatabase.Size = new System.Drawing.Size(1210, 20);
            this.tbDatabase.TabIndex = 6;
            // 
            // checksUI1
            // 
            this.checksUI1.AllowsYesNoToAll = true;
            this.checksUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checksUI1.Location = new System.Drawing.Point(12, 84);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(1271, 526);
            this.checksUI1.TabIndex = 4;
            // 
            // PatchingUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1297, 622);
            this.Controls.Add(this.tbDatabase);
            this.Controls.Add(this.tbPatch);
            this.Controls.Add(this.btnAttemptPatching);
            this.Controls.Add(this.checksUI1);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.label4);
            this.Name = "PatchingUI";
            this.Text = "Patching";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label4;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.Button btnAttemptPatching;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.TextBox tbPatch;
        private System.Windows.Forms.TextBox tbDatabase;
    }
}