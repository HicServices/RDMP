namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.ProcessTasks
{
    partial class ExeProcessTaskUI
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
            this.loadStageIconUI1 = new CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadStageIconUI();
            this.tbID = new System.Windows.Forms.TextBox();
            this.lblID = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.pbFile = new System.Windows.Forms.PictureBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.tbExeCommand = new System.Windows.Forms.TextBox();
            this.btnRunExe = new System.Windows.Forms.Button();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile)).BeginInit();
            this.SuspendLayout();
            // 
            // loadStageIconUI1
            // 
            this.loadStageIconUI1.Location = new System.Drawing.Point(361, 3);
            this.loadStageIconUI1.Name = "loadStageIconUI1";
            this.loadStageIconUI1.Size = new System.Drawing.Size(232, 19);
            this.loadStageIconUI1.TabIndex = 15;
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(255, 2);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 14;
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Location = new System.Drawing.Point(228, 5);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(21, 13);
            this.lblID.TabIndex = 13;
            this.lblID.Text = "ID:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(147, 0);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 12;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // pbFile
            // 
            this.pbFile.Location = new System.Drawing.Point(6, 2);
            this.pbFile.Name = "pbFile";
            this.pbFile.Size = new System.Drawing.Size(19, 19);
            this.pbFile.TabIndex = 11;
            this.pbFile.TabStop = false;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(31, 5);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(39, 13);
            this.lblPath.TabIndex = 10;
            this.lblPath.Text = "lblPath";
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(116, 0);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 9;
            // 
            // tbExeCommand
            // 
            this.tbExeCommand.BackColor = System.Drawing.Color.Black;
            this.tbExeCommand.ForeColor = System.Drawing.Color.White;
            this.tbExeCommand.Location = new System.Drawing.Point(0, 42);
            this.tbExeCommand.Multiline = true;
            this.tbExeCommand.Name = "tbExeCommand";
            this.tbExeCommand.ReadOnly = true;
            this.tbExeCommand.Size = new System.Drawing.Size(842, 140);
            this.tbExeCommand.TabIndex = 16;
            // 
            // btnRunExe
            // 
            this.btnRunExe.Location = new System.Drawing.Point(3, 188);
            this.btnRunExe.Name = "btnRunExe";
            this.btnRunExe.Size = new System.Drawing.Size(75, 23);
            this.btnRunExe.TabIndex = 17;
            this.btnRunExe.Text = "Run Exe";
            this.btnRunExe.UseVisualStyleBackColor = true;
            this.btnRunExe.Click += new System.EventHandler(this.btnRunExe_Click);
            // 
            // checksUI1
            // 
            this.checksUI1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.checksUI1.Location = new System.Drawing.Point(3, 217);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(839, 362);
            this.checksUI1.TabIndex = 18;
            // 
            // ExeProcessTaskUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checksUI1);
            this.Controls.Add(this.btnRunExe);
            this.Controls.Add(this.tbExeCommand);
            this.Controls.Add(this.loadStageIconUI1);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.pbFile);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.ragSmiley1);
            this.Name = "ExeProcessTaskUI";
            this.Size = new System.Drawing.Size(856, 582);
            ((System.ComponentModel.ISupportInitialize)(this.pbFile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LoadStageIconUI loadStageIconUI1;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.PictureBox pbFile;
        private System.Windows.Forms.Label lblPath;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private System.Windows.Forms.TextBox tbExeCommand;
        private System.Windows.Forms.Button btnRunExe;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
    }
}
