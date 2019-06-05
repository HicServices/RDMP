namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.ProcessTasks
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
            this.loadStageIconUI1 = new LoadStageIconUI();
            this.tbID = new System.Windows.Forms.TextBox();
            this.lblID = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.pbFile = new System.Windows.Forms.PictureBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.tbExeCommand = new System.Windows.Forms.TextBox();
            this.btnRunExe = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // loadStageIconUI1
            // 
            this.loadStageIconUI1.Location = new System.Drawing.Point(360, 7);
            this.loadStageIconUI1.Name = "loadStageIconUI1";
            this.loadStageIconUI1.Size = new System.Drawing.Size(232, 19);
            this.loadStageIconUI1.TabIndex = 15;
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(254, 6);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 14;
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Location = new System.Drawing.Point(227, 9);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(21, 13);
            this.lblID.TabIndex = 13;
            this.lblID.Text = "ID:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(146, 4);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 12;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // pbFile
            // 
            this.pbFile.Location = new System.Drawing.Point(5, 6);
            this.pbFile.Name = "pbFile";
            this.pbFile.Size = new System.Drawing.Size(19, 19);
            this.pbFile.TabIndex = 11;
            this.pbFile.TabStop = false;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(30, 9);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(39, 13);
            this.lblPath.TabIndex = 10;
            this.lblPath.Text = "lblPath";
            // 
            // tbExeCommand
            // 
            this.tbExeCommand.BackColor = System.Drawing.Color.Black;
            this.tbExeCommand.ForeColor = System.Drawing.Color.White;
            this.tbExeCommand.Location = new System.Drawing.Point(5, 30);
            this.tbExeCommand.Multiline = true;
            this.tbExeCommand.Name = "tbExeCommand";
            this.tbExeCommand.ReadOnly = true;
            this.tbExeCommand.Size = new System.Drawing.Size(842, 140);
            this.tbExeCommand.TabIndex = 16;
            // 
            // btnRunExe
            // 
            this.btnRunExe.Location = new System.Drawing.Point(5, 176);
            this.btnRunExe.Name = "btnRunExe";
            this.btnRunExe.Size = new System.Drawing.Size(75, 23);
            this.btnRunExe.TabIndex = 17;
            this.btnRunExe.Text = "Run Exe";
            this.btnRunExe.UseVisualStyleBackColor = true;
            this.btnRunExe.Click += new System.EventHandler(this.btnRunExe_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pbFile);
            this.panel1.Controls.Add(this.btnRunExe);
            this.panel1.Controls.Add(this.lblPath);
            this.panel1.Controls.Add(this.tbExeCommand);
            this.panel1.Controls.Add(this.btnBrowse);
            this.panel1.Controls.Add(this.loadStageIconUI1);
            this.panel1.Controls.Add(this.lblID);
            this.panel1.Controls.Add(this.tbID);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(856, 207);
            this.panel1.TabIndex = 19;
            // 
            // checksUI1
            // 
            this.checksUI1.AllowsYesNoToAll = true;
            this.checksUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checksUI1.Location = new System.Drawing.Point(0, 207);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(856, 375);
            this.checksUI1.TabIndex = 18;
            // 
            // ExeProcessTaskUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checksUI1);
            this.Controls.Add(this.panel1);
            this.Name = "ExeProcessTaskUI";
            this.Size = new System.Drawing.Size(856, 582);
            ((System.ComponentModel.ISupportInitialize)(this.pbFile)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private LoadStageIconUI loadStageIconUI1;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.PictureBox pbFile;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.TextBox tbExeCommand;
        private System.Windows.Forms.Button btnRunExe;
        private System.Windows.Forms.Panel panel1;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
    }
}
