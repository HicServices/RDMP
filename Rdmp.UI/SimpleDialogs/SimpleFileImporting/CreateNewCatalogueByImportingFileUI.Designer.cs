using Rdmp.UI.SimpleControls;
using Rdmp.UI.ChecksUI;

namespace Rdmp.UI.SimpleDialogs.SimpleFileImporting
{
    partial class CreateNewCatalogueByImportingFileUI
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
            this.gbPickFile = new System.Windows.Forms.GroupBox();
            this.ragSmileyFile = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.btnClearFile = new System.Windows.Forms.Button();
            this.lblFile = new System.Windows.Forms.Label();
            this.pbFile = new System.Windows.Forms.PictureBox();
            this.pbHelp = new System.Windows.Forms.PictureBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.gbPickDatabase = new System.Windows.Forms.GroupBox();
            this.btnConfirmDatabase = new System.Windows.Forms.Button();
            this.serverDatabaseTableSelector1 = new Rdmp.UI.SimpleControls.ServerDatabaseTableSelector();
            this.gbPickPipeline = new System.Windows.Forms.GroupBox();
            this.cbOther = new System.Windows.Forms.CheckBox();
            this.ddPipeline = new System.Windows.Forms.ComboBox();
            this.gbExecute = new System.Windows.Forms.GroupBox();
            this.ragSmileyExecute = new Rdmp.UI.ChecksUI.RAGSmiley();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.cbAutoClose = new System.Windows.Forms.CheckBox();
            this.gbTableName = new System.Windows.Forms.GroupBox();
            this.tbTableName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gbPickFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbHelp)).BeginInit();
            this.gbPickDatabase.SuspendLayout();
            this.gbPickPipeline.SuspendLayout();
            this.gbExecute.SuspendLayout();
            this.gbTableName.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbPickFile
            // 
            this.gbPickFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbPickFile.Controls.Add(this.ragSmileyFile);
            this.gbPickFile.Controls.Add(this.btnClearFile);
            this.gbPickFile.Controls.Add(this.lblFile);
            this.gbPickFile.Controls.Add(this.pbFile);
            this.gbPickFile.Controls.Add(this.pbHelp);
            this.gbPickFile.Controls.Add(this.btnBrowse);
            this.gbPickFile.Location = new System.Drawing.Point(14, 14);
            this.gbPickFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbPickFile.Name = "gbPickFile";
            this.gbPickFile.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbPickFile.Size = new System.Drawing.Size(706, 62);
            this.gbPickFile.TabIndex = 0;
            this.gbPickFile.TabStop = false;
            this.gbPickFile.Text = "1. Pick File";
            // 
            // ragSmileyFile
            // 
            this.ragSmileyFile.AlwaysShowHandCursor = false;
            this.ragSmileyFile.BackColor = System.Drawing.Color.Transparent;
            this.ragSmileyFile.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmileyFile.Location = new System.Drawing.Point(342, 16);
            this.ragSmileyFile.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ragSmileyFile.Name = "ragSmileyFile";
            this.ragSmileyFile.Size = new System.Drawing.Size(29, 29);
            this.ragSmileyFile.TabIndex = 4;
            // 
            // btnClearFile
            // 
            this.btnClearFile.Location = new System.Drawing.Point(378, 16);
            this.btnClearFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClearFile.Name = "btnClearFile";
            this.btnClearFile.Size = new System.Drawing.Size(88, 27);
            this.btnClearFile.TabIndex = 3;
            this.btnClearFile.Text = "Clear";
            this.btnClearFile.UseVisualStyleBackColor = true;
            this.btnClearFile.Click += new System.EventHandler(this.btnClearFile_Click);
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(296, 22);
            this.lblFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(38, 15);
            this.lblFile.TabIndex = 2;
            this.lblFile.Text = "lblFile";
            // 
            // pbFile
            // 
            this.pbFile.Location = new System.Drawing.Point(7, 22);
            this.pbFile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbFile.Name = "pbFile";
            this.pbFile.Size = new System.Drawing.Size(23, 23);
            this.pbFile.TabIndex = 1;
            this.pbFile.TabStop = false;
            // 
            // pbHelp
            // 
            this.pbHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbHelp.Location = new System.Drawing.Point(677, 0);
            this.pbHelp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbHelp.Name = "pbHelp";
            this.pbHelp.Size = new System.Drawing.Size(25, 25);
            this.pbHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbHelp.TabIndex = 6;
            this.pbHelp.TabStop = false;
            this.pbHelp.Click += new System.EventHandler(this.pbHelp_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(7, 22);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(88, 27);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // gbPickDatabase
            // 
            this.gbPickDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbPickDatabase.Controls.Add(this.btnConfirmDatabase);
            this.gbPickDatabase.Controls.Add(this.serverDatabaseTableSelector1);
            this.gbPickDatabase.Location = new System.Drawing.Point(14, 83);
            this.gbPickDatabase.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbPickDatabase.Name = "gbPickDatabase";
            this.gbPickDatabase.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbPickDatabase.Size = new System.Drawing.Size(706, 194);
            this.gbPickDatabase.TabIndex = 1;
            this.gbPickDatabase.TabStop = false;
            this.gbPickDatabase.Text = "2. Pick Destination Database for the Imported Data";
            // 
            // btnConfirmDatabase
            // 
            this.btnConfirmDatabase.Location = new System.Drawing.Point(7, 165);
            this.btnConfirmDatabase.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnConfirmDatabase.Name = "btnConfirmDatabase";
            this.btnConfirmDatabase.Size = new System.Drawing.Size(122, 27);
            this.btnConfirmDatabase.TabIndex = 1;
            this.btnConfirmDatabase.Text = "Confirm Database";
            this.btnConfirmDatabase.UseVisualStyleBackColor = true;
            this.btnConfirmDatabase.Click += new System.EventHandler(this.btnConfirmDatabase_Click);
            // 
            // serverDatabaseTableSelector1
            // 
            this.serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = false;
            this.serverDatabaseTableSelector1.AutoSize = true;
            this.serverDatabaseTableSelector1.Database = "";
            this.serverDatabaseTableSelector1.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            this.serverDatabaseTableSelector1.Location = new System.Drawing.Point(7, 22);
            this.serverDatabaseTableSelector1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.serverDatabaseTableSelector1.Name = "serverDatabaseTableSelector1";
            this.serverDatabaseTableSelector1.Password = "";
            this.serverDatabaseTableSelector1.Server = "";
            this.serverDatabaseTableSelector1.Size = new System.Drawing.Size(678, 168);
            this.serverDatabaseTableSelector1.TabIndex = 0;
            this.serverDatabaseTableSelector1.Username = "";
            // 
            // gbPickPipeline
            // 
            this.gbPickPipeline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbPickPipeline.Controls.Add(this.ddPipeline);
            this.gbPickPipeline.Controls.Add(this.cbOther);
            this.gbPickPipeline.Location = new System.Drawing.Point(14, 351);
            this.gbPickPipeline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbPickPipeline.Name = "gbPickPipeline";
            this.gbPickPipeline.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbPickPipeline.Size = new System.Drawing.Size(705, 48);
            this.gbPickPipeline.TabIndex = 3;
            this.gbPickPipeline.TabStop = false;
            this.gbPickPipeline.Text = "4. Pick Pipeline";
            // 
            // cbOther
            // 
            this.cbOther.AutoSize = true;
            this.cbOther.Dock = System.Windows.Forms.DockStyle.Right;
            this.cbOther.Location = new System.Drawing.Point(642, 19);
            this.cbOther.Name = "cbOther";
            this.cbOther.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.cbOther.Size = new System.Drawing.Size(59, 26);
            this.cbOther.TabIndex = 1;
            this.cbOther.Text = "Other";
            this.cbOther.UseVisualStyleBackColor = true;
            this.cbOther.CheckedChanged += new System.EventHandler(this.cbOther_CheckedChanged);
            // 
            // ddPipeline
            // 
            this.ddPipeline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ddPipeline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddPipeline.FormattingEnabled = true;
            this.ddPipeline.Location = new System.Drawing.Point(4, 19);
            this.ddPipeline.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ddPipeline.Name = "ddPipeline";
            this.ddPipeline.Size = new System.Drawing.Size(638, 23);
            this.ddPipeline.TabIndex = 0;
            this.ddPipeline.SelectedIndexChanged += new System.EventHandler(this.ddPipeline_SelectedIndexChanged);
            // 
            // gbExecute
            // 
            this.gbExecute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbExecute.Controls.Add(this.ragSmileyExecute);
            this.gbExecute.Controls.Add(this.btnExecute);
            this.gbExecute.Controls.Add(this.btnPreview);
            this.gbExecute.Location = new System.Drawing.Point(14, 418);
            this.gbExecute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbExecute.Name = "gbExecute";
            this.gbExecute.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbExecute.Size = new System.Drawing.Size(705, 59);
            this.gbExecute.TabIndex = 4;
            this.gbExecute.TabStop = false;
            this.gbExecute.Text = "5. Import Data";
            // 
            // ragSmileyExecute
            // 
            this.ragSmileyExecute.AlwaysShowHandCursor = false;
            this.ragSmileyExecute.BackColor = System.Drawing.Color.Transparent;
            this.ragSmileyExecute.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmileyExecute.Location = new System.Drawing.Point(214, 20);
            this.ragSmileyExecute.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.ragSmileyExecute.Name = "ragSmileyExecute";
            this.ragSmileyExecute.Size = new System.Drawing.Size(29, 29);
            this.ragSmileyExecute.TabIndex = 5;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(119, 22);
            this.btnExecute.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(88, 27);
            this.btnExecute.TabIndex = 3;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(24, 22);
            this.btnPreview.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(88, 27);
            this.btnPreview.TabIndex = 3;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // cbAutoClose
            // 
            this.cbAutoClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbAutoClose.AutoSize = true;
            this.cbAutoClose.Checked = true;
            this.cbAutoClose.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAutoClose.Location = new System.Drawing.Point(14, 483);
            this.cbAutoClose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbAutoClose.Name = "cbAutoClose";
            this.cbAutoClose.Size = new System.Drawing.Size(144, 19);
            this.cbAutoClose.TabIndex = 5;
            this.cbAutoClose.Text = "Close When Complete";
            this.cbAutoClose.UseVisualStyleBackColor = true;
            // 
            // gbTableName
            // 
            this.gbTableName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbTableName.Controls.Add(this.tbTableName);
            this.gbTableName.Controls.Add(this.label1);
            this.gbTableName.Location = new System.Drawing.Point(14, 282);
            this.gbTableName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbTableName.Name = "gbTableName";
            this.gbTableName.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbTableName.Size = new System.Drawing.Size(705, 62);
            this.gbTableName.TabIndex = 2;
            this.gbTableName.TabStop = false;
            this.gbTableName.Text = "3. Pick Catalogue name (created Table will have the same name)";
            // 
            // tbTableName
            // 
            this.tbTableName.Location = new System.Drawing.Point(58, 25);
            this.tbTableName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbTableName.Name = "tbTableName";
            this.tbTableName.Size = new System.Drawing.Size(636, 23);
            this.tbTableName.TabIndex = 1;
            this.tbTableName.TextChanged += new System.EventHandler(this.tbTableName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // CreateNewCatalogueByImportingFileUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(736, 505);
            this.Controls.Add(this.gbTableName);
            this.Controls.Add(this.gbPickPipeline);
            this.Controls.Add(this.gbExecute);
            this.Controls.Add(this.cbAutoClose);
            this.Controls.Add(this.gbPickFile);
            this.Controls.Add(this.gbPickDatabase);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CreateNewCatalogueByImportingFileUI";
            this.Text = "Create Catalogue By Importing A File";
            this.gbPickFile.ResumeLayout(false);
            this.gbPickFile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbHelp)).EndInit();
            this.gbPickDatabase.ResumeLayout(false);
            this.gbPickDatabase.PerformLayout();
            this.gbPickPipeline.ResumeLayout(false);
            this.gbPickPipeline.PerformLayout();
            this.gbExecute.ResumeLayout(false);
            this.gbTableName.ResumeLayout(false);
            this.gbTableName.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbPickFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.PictureBox pbFile;
        private System.Windows.Forms.Button btnClearFile;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.GroupBox gbPickDatabase;
        private ServerDatabaseTableSelector serverDatabaseTableSelector1;
        private System.Windows.Forms.Button btnConfirmDatabase;
        private System.Windows.Forms.GroupBox gbPickPipeline;
        private System.Windows.Forms.ComboBox ddPipeline;
        private RAGSmiley ragSmileyFile;
        private System.Windows.Forms.GroupBox gbExecute;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnExecute;
        private RAGSmiley ragSmileyExecute;
        private System.Windows.Forms.PictureBox pbHelp;
        private System.Windows.Forms.CheckBox cbAutoClose;
        private System.Windows.Forms.GroupBox gbTableName;
        private System.Windows.Forms.TextBox tbTableName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbOther;
    }
}