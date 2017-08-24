namespace DataExportManager.SimpleDialogs
{
    partial class BlacklistOptions
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlacklistOptions));
            this.lblProblem = new System.Windows.Forms.Label();
            this.btnIgnoreError = new System.Windows.Forms.Button();
            this.btnBlacklistSource = new System.Windows.Forms.Button();
            this.btnBlacklistAllSources = new System.Windows.Forms.Button();
            this.btnUnsetDataExportRepositoryLocation = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblProblem
            // 
            this.lblProblem.Location = new System.Drawing.Point(0, 0);
            this.lblProblem.Name = "lblProblem";
            this.lblProblem.Size = new System.Drawing.Size(650, 64);
            this.lblProblem.TabIndex = 0;
            this.lblProblem.Text = "There was a problem...";
            // 
            // btnIgnoreError
            // 
            this.btnIgnoreError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnIgnoreError.Location = new System.Drawing.Point(132, 121);
            this.btnIgnoreError.Name = "btnIgnoreError";
            this.btnIgnoreError.Size = new System.Drawing.Size(75, 23);
            this.btnIgnoreError.TabIndex = 1;
            this.btnIgnoreError.Text = "Ignore Error";
            this.btnIgnoreError.UseVisualStyleBackColor = true;
            this.btnIgnoreError.Click += new System.EventHandler(this.btnIgnoreError_Click);
            // 
            // btnBlacklistSource
            // 
            this.btnBlacklistSource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBlacklistSource.Location = new System.Drawing.Point(257, 121);
            this.btnBlacklistSource.Name = "btnBlacklistSource";
            this.btnBlacklistSource.Size = new System.Drawing.Size(99, 23);
            this.btnBlacklistSource.TabIndex = 2;
            this.btnBlacklistSource.Text = "Blacklist Source";
            this.btnBlacklistSource.UseVisualStyleBackColor = true;
            this.btnBlacklistSource.Click += new System.EventHandler(this.btnBlacklistSource_Click);
            // 
            // btnBlacklistAllSources
            // 
            this.btnBlacklistAllSources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBlacklistAllSources.Location = new System.Drawing.Point(388, 121);
            this.btnBlacklistAllSources.Name = "btnBlacklistAllSources";
            this.btnBlacklistAllSources.Size = new System.Drawing.Size(116, 23);
            this.btnBlacklistAllSources.TabIndex = 3;
            this.btnBlacklistAllSources.Text = "Blacklist All Sources";
            this.btnBlacklistAllSources.UseVisualStyleBackColor = true;
            this.btnBlacklistAllSources.Click += new System.EventHandler(this.btnBlacklistAllSources_Click);
            // 
            // btnUnsetDataExportRepositoryLocation
            // 
            this.btnUnsetDataExportRepositoryLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUnsetDataExportRepositoryLocation.Location = new System.Drawing.Point(534, 121);
            this.btnUnsetDataExportRepositoryLocation.Name = "btnUnsetDataExportRepositoryLocation";
            this.btnUnsetDataExportRepositoryLocation.Size = new System.Drawing.Size(116, 23);
            this.btnUnsetDataExportRepositoryLocation.TabIndex = 3;
            this.btnUnsetDataExportRepositoryLocation.Text = "Disable Data Export";
            this.btnUnsetDataExportRepositoryLocation.UseVisualStyleBackColor = true;
            this.btnUnsetDataExportRepositoryLocation.Click += new System.EventHandler(this.btnUnsetDataExportRepositoryLocation_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(132, 80);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(518, 35);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(228, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Please Choose how to respond to this problem:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Severity of Response:";
            // 
            // BlacklistOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(661, 156);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnUnsetDataExportRepositoryLocation);
            this.Controls.Add(this.btnBlacklistAllSources);
            this.Controls.Add(this.btnBlacklistSource);
            this.Controls.Add(this.btnIgnoreError);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblProblem);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BlacklistOptions";
            this.Text = "BlacklistOptions";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblProblem;
        private System.Windows.Forms.Button btnIgnoreError;
        private System.Windows.Forms.Button btnBlacklistSource;
        private System.Windows.Forms.Button btnBlacklistAllSources;
        private System.Windows.Forms.Button btnUnsetDataExportRepositoryLocation;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}