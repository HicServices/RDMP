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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PasswordEncryptionKeyLocationUI));
            this.lblLocationInvalid = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.tbCertificate = new System.Windows.Forms.TextBox();
            this.btnShowDirectory = new System.Windows.Forms.Button();
            this.btnDeleteKeyLocation = new System.Windows.Forms.Button();
            this.btnCreateKeyFile = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblLocationInvalid
            // 
            this.lblLocationInvalid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLocationInvalid.Location = new System.Drawing.Point(13, 222);
            this.lblLocationInvalid.Name = "lblLocationInvalid";
            this.lblLocationInvalid.Size = new System.Drawing.Size(1019, 41);
            this.lblLocationInvalid.TabIndex = 3;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(11, 182);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(91, 13);
            this.label13.TabIndex = 2;
            this.label13.Text = "Change Location:";
            // 
            // tbCertificate
            // 
            this.tbCertificate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCertificate.Location = new System.Drawing.Point(12, 198);
            this.tbCertificate.Name = "tbCertificate";
            this.tbCertificate.Size = new System.Drawing.Size(741, 20);
            this.tbCertificate.TabIndex = 1;
            this.tbCertificate.TextChanged += new System.EventHandler(this.tbCertificate_TextChanged);
            // 
            // btnShowDirectory
            // 
            this.btnShowDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowDirectory.Location = new System.Drawing.Point(759, 196);
            this.btnShowDirectory.Name = "btnShowDirectory";
            this.btnShowDirectory.Size = new System.Drawing.Size(44, 23);
            this.btnShowDirectory.TabIndex = 0;
            this.btnShowDirectory.Text = "Show Location";
            this.btnShowDirectory.UseVisualStyleBackColor = true;
            this.btnShowDirectory.Click += new System.EventHandler(this.btnShowDirectory_Click);
            // 
            // btnDeleteKeyLocation
            // 
            this.btnDeleteKeyLocation.Location = new System.Drawing.Point(229, 152);
            this.btnDeleteKeyLocation.Name = "btnDeleteKeyLocation";
            this.btnDeleteKeyLocation.Size = new System.Drawing.Size(253, 23);
            this.btnDeleteKeyLocation.TabIndex = 0;
            this.btnDeleteKeyLocation.Text = "Clear Key Location (ERASES ALL PASSWORDS)";
            this.btnDeleteKeyLocation.UseVisualStyleBackColor = true;
            this.btnDeleteKeyLocation.Click += new System.EventHandler(this.btnDeleteKeyLocation_Click);
            // 
            // btnCreateKeyFile
            // 
            this.btnCreateKeyFile.Location = new System.Drawing.Point(12, 152);
            this.btnCreateKeyFile.Name = "btnCreateKeyFile";
            this.btnCreateKeyFile.Size = new System.Drawing.Size(211, 23);
            this.btnCreateKeyFile.TabIndex = 0;
            this.btnCreateKeyFile.Text = "Create Key File";
            this.btnCreateKeyFile.UseVisualStyleBackColor = true;
            this.btnCreateKeyFile.Click += new System.EventHandler(this.btnCreateKeyFile_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(13, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1021, 70);
            this.label1.TabIndex = 18;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(9, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 16);
            this.label2.TabIndex = 19;
            this.label2.Text = "WARNING:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(863, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Changing the Encryption Key will result in all currently configured passwords bei" +
    "ng irretrievable.  You will need to open and reset them individually once the ne" +
    "w key has been created.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(9, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(139, 16);
            this.label4.TabIndex = 19;
            this.label4.Text = "Creating a Key File";
            // 
            // PasswordEncryptionKeyLocationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblLocationInvalid);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbCertificate);
            this.Controls.Add(this.btnShowDirectory);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnDeleteKeyLocation);
            this.Controls.Add(this.btnCreateKeyFile);
            this.Name = "PasswordEncryptionKeyLocationUI";
            this.Size = new System.Drawing.Size(1054, 549);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblLocationInvalid;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbCertificate;
        private System.Windows.Forms.Button btnShowDirectory;
        private System.Windows.Forms.Button btnCreateKeyFile;
        private System.Windows.Forms.Button btnDeleteKeyLocation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}
