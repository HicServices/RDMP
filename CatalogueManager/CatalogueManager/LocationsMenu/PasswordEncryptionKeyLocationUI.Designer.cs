namespace CatalogueManager.LocationsMenu
{
    partial class PasswordEncryptionKeyLocationUI
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
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.lblLocationInvalid = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.tbCertificate = new System.Windows.Forms.TextBox();
            this.btnShowDirectory = new System.Windows.Forms.Button();
            this.btnCreateKeyFile = new System.Windows.Forms.Button();
            this.btnDeleteKeyLocation = new System.Windows.Forms.Button();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Controls.Add(this.lblLocationInvalid);
            this.groupBox6.Controls.Add(this.label13);
            this.groupBox6.Controls.Add(this.tbCertificate);
            this.groupBox6.Controls.Add(this.btnShowDirectory);
            this.groupBox6.Controls.Add(this.btnDeleteKeyLocation);
            this.groupBox6.Controls.Add(this.btnCreateKeyFile);
            this.groupBox6.Location = new System.Drawing.Point(3, 3);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(533, 132);
            this.groupBox6.TabIndex = 17;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Password Encryption Key (Ensure location has appropriate Windows access controls)" +
    "";
            // 
            // lblLocationInvalid
            // 
            this.lblLocationInvalid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLocationInvalid.Location = new System.Drawing.Point(6, 87);
            this.lblLocationInvalid.Name = "lblLocationInvalid";
            this.lblLocationInvalid.Size = new System.Drawing.Size(521, 42);
            this.lblLocationInvalid.TabIndex = 3;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 48);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(91, 13);
            this.label13.TabIndex = 2;
            this.label13.Text = "Change Location:";
            // 
            // tbCertificate
            // 
            this.tbCertificate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCertificate.Location = new System.Drawing.Point(7, 64);
            this.tbCertificate.Name = "tbCertificate";
            this.tbCertificate.Size = new System.Drawing.Size(470, 20);
            this.tbCertificate.TabIndex = 1;
            this.tbCertificate.TextChanged += new System.EventHandler(this.tbCertificate_TextChanged);
            // 
            // btnShowDirectory
            // 
            this.btnShowDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowDirectory.Location = new System.Drawing.Point(483, 61);
            this.btnShowDirectory.Name = "btnShowDirectory";
            this.btnShowDirectory.Size = new System.Drawing.Size(44, 23);
            this.btnShowDirectory.TabIndex = 0;
            this.btnShowDirectory.Text = "Show Location";
            this.btnShowDirectory.UseVisualStyleBackColor = true;
            this.btnShowDirectory.Click += new System.EventHandler(this.btnShowDirectory_Click);
            // 
            // btnCreateKeyFile
            // 
            this.btnCreateKeyFile.Location = new System.Drawing.Point(7, 20);
            this.btnCreateKeyFile.Name = "btnCreateKeyFile";
            this.btnCreateKeyFile.Size = new System.Drawing.Size(211, 23);
            this.btnCreateKeyFile.TabIndex = 0;
            this.btnCreateKeyFile.Text = "Create Key File";
            this.btnCreateKeyFile.UseVisualStyleBackColor = true;
            this.btnCreateKeyFile.Click += new System.EventHandler(this.btnCreateKeyFile_Click);
            // 
            // btnDeleteKeyLocation
            // 
            this.btnDeleteKeyLocation.Location = new System.Drawing.Point(224, 19);
            this.btnDeleteKeyLocation.Name = "btnDeleteKeyLocation";
            this.btnDeleteKeyLocation.Size = new System.Drawing.Size(253, 23);
            this.btnDeleteKeyLocation.TabIndex = 0;
            this.btnDeleteKeyLocation.Text = "Clear Key Location (ERASES ALL PASSWORDS)";
            this.btnDeleteKeyLocation.UseVisualStyleBackColor = true;
            this.btnDeleteKeyLocation.Click += new System.EventHandler(this.btnDeleteKeyLocation_Click);
            // 
            // PasswordEncryptionKeyLocationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox6);
            this.Name = "PasswordEncryptionKeyLocationUI";
            this.Size = new System.Drawing.Size(539, 138);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label lblLocationInvalid;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbCertificate;
        private System.Windows.Forms.Button btnShowDirectory;
        private System.Windows.Forms.Button btnCreateKeyFile;
        private System.Windows.Forms.Button btnDeleteKeyLocation;
    }
}
