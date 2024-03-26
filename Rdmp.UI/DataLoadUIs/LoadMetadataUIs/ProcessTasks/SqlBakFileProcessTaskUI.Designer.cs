namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.ProcessTasks
{
    partial class SqlBakFileProcessTaskUI
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
            btnBrowse = new System.Windows.Forms.Button();
            lblID = new System.Windows.Forms.Label();
            tbID = new System.Windows.Forms.TextBox();
            panel1 = new System.Windows.Forms.Panel();
            tbName = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            tbPath = new System.Windows.Forms.TextBox();
            loadStageIconUI1 = new LoadStageIconUI();
            tbPrimaryFile = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            tbLogFile = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new System.Drawing.Point(471, 36);
            btnBrowse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new System.Drawing.Size(88, 27);
            btnBrowse.TabIndex = 5;
            btnBrowse.Text = "Browse...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // lblID
            // 
            lblID.AutoSize = true;
            lblID.Location = new System.Drawing.Point(566, 42);
            lblID.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblID.Name = "lblID";
            lblID.Size = new System.Drawing.Size(21, 15);
            lblID.TabIndex = 6;
            lblID.Text = "ID:";
            // 
            // tbID
            // 
            tbID.Location = new System.Drawing.Point(597, 38);
            tbID.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbID.Name = "tbID";
            tbID.ReadOnly = true;
            tbID.Size = new System.Drawing.Size(116, 23);
            tbID.TabIndex = 7;
            // 
            // panel1
            // 
            panel1.Controls.Add(tbLogFile);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(tbPrimaryFile);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(tbName);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(tbPath);
            panel1.Controls.Add(loadStageIconUI1);
            panel1.Controls.Add(tbID);
            panel1.Controls.Add(btnBrowse);
            panel1.Controls.Add(lblID);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1043, 811);
            panel1.TabIndex = 9;
            // 
            // tbName
            // 
            tbName.Location = new System.Drawing.Point(58, 8);
            tbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(405, 23);
            tbName.TabIndex = 11;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(7, 10);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(42, 15);
            label2.TabIndex = 10;
            label2.Text = "Name:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(14, 42);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(34, 15);
            label1.TabIndex = 10;
            label1.Text = "Path:";
            // 
            // tbPath
            // 
            tbPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            tbPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            tbPath.Location = new System.Drawing.Point(58, 38);
            tbPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbPath.Name = "tbPath";
            tbPath.Size = new System.Drawing.Size(405, 23);
            tbPath.TabIndex = 9;
            // 
            // loadStageIconUI1
            // 
            loadStageIconUI1.Location = new System.Drawing.Point(721, 39);
            loadStageIconUI1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            loadStageIconUI1.Name = "loadStageIconUI1";
            loadStageIconUI1.Size = new System.Drawing.Size(271, 22);
            loadStageIconUI1.TabIndex = 8;
            // 
            // tbPrimaryFile
            // 
            tbPrimaryFile.Location = new System.Drawing.Point(223, 79);
            tbPrimaryFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbPrimaryFile.Name = "tbPrimaryFile";
            tbPrimaryFile.Size = new System.Drawing.Size(405, 23);
            tbPrimaryFile.TabIndex = 13;
            tbPrimaryFile.TextChanged += tbPrimary_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(7, 79);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(208, 15);
            label3.TabIndex = 12;
            label3.Text = "Primary File Physical Name (optional):";
            label3.Click += label3_Click;
            // 
            // tbLogFile
            // 
            tbLogFile.Location = new System.Drawing.Point(223, 108);
            tbLogFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbLogFile.Name = "tbLogFile";
            tbLogFile.Size = new System.Drawing.Size(405, 23);
            tbLogFile.TabIndex = 15;
            tbLogFile.TextChanged += tbLog_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(28, 108);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(187, 15);
            label4.TabIndex = 14;
            label4.Text = "Log File Physical Name (optional):";
            // 
            // SqlBakFileProcessTaskUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(panel1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "SqlBakFileProcessTaskUI";
            Size = new System.Drawing.Size(1043, 811);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.TextBox tbID;
        private LoadStageIconUI loadStageIconUI1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbPath;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbLogFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbPrimaryFile;
        private System.Windows.Forms.Label label3;
    }
}
