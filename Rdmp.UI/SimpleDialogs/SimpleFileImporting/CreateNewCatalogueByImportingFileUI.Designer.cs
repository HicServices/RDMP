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
            this.ragSmileyFile = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.btnClearFile = new System.Windows.Forms.Button();
            this.lblFile = new System.Windows.Forms.Label();
            this.pbFile = new System.Windows.Forms.PictureBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.gbPickDatabase = new System.Windows.Forms.GroupBox();
            this.btnConfirmDatabase = new System.Windows.Forms.Button();
            this.serverDatabaseTableSelector1 = new ReusableUIComponents.ServerDatabaseTableSelector();
            this.gbPickPipeline = new System.Windows.Forms.GroupBox();
            this.ddPipeline = new System.Windows.Forms.ComboBox();
            this.gbExecute = new System.Windows.Forms.GroupBox();
            this.ragSmileyExecute = new ReusableUIComponents.ChecksUI.RAGSmiley();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.pbHelp = new System.Windows.Forms.PictureBox();
            this.cbAutoClose = new System.Windows.Forms.CheckBox();
            this.gbPickFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile)).BeginInit();
            this.gbPickDatabase.SuspendLayout();
            this.gbPickPipeline.SuspendLayout();
            this.gbExecute.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbHelp)).BeginInit();
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
            this.gbPickFile.Controls.Add(this.btnBrowse);
            this.gbPickFile.Location = new System.Drawing.Point(12, 12);
            this.gbPickFile.Name = "gbPickFile";
            this.gbPickFile.Size = new System.Drawing.Size(632, 54);
            this.gbPickFile.TabIndex = 0;
            this.gbPickFile.TabStop = false;
            this.gbPickFile.Text = "1. Pick File";
            // 
            // ragSmileyFile
            // 
            this.ragSmileyFile.AlwaysShowHandCursor = false;
            this.ragSmileyFile.BackColor = System.Drawing.Color.Transparent;
            this.ragSmileyFile.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmileyFile.Location = new System.Drawing.Point(293, 14);
            this.ragSmileyFile.Name = "ragSmileyFile";
            this.ragSmileyFile.Size = new System.Drawing.Size(25, 25);
            this.ragSmileyFile.TabIndex = 4;
            // 
            // btnClearFile
            // 
            this.btnClearFile.Location = new System.Drawing.Point(324, 14);
            this.btnClearFile.Name = "btnClearFile";
            this.btnClearFile.Size = new System.Drawing.Size(75, 23);
            this.btnClearFile.TabIndex = 3;
            this.btnClearFile.Text = "Clear";
            this.btnClearFile.UseVisualStyleBackColor = true;
            this.btnClearFile.Click += new System.EventHandler(this.btnClearFile_Click);
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(254, 19);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(33, 13);
            this.lblFile.TabIndex = 2;
            this.lblFile.Text = "lblFile";
            // 
            // pbFile
            // 
            this.pbFile.Location = new System.Drawing.Point(6, 19);
            this.pbFile.Name = "pbFile";
            this.pbFile.Size = new System.Drawing.Size(20, 20);
            this.pbFile.TabIndex = 1;
            this.pbFile.TabStop = false;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(6, 19);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
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
            this.gbPickDatabase.Location = new System.Drawing.Point(12, 72);
            this.gbPickDatabase.Name = "gbPickDatabase";
            this.gbPickDatabase.Size = new System.Drawing.Size(632, 168);
            this.gbPickDatabase.TabIndex = 1;
            this.gbPickDatabase.TabStop = false;
            this.gbPickDatabase.Text = "2. Pick Destination Database for the Imported Data";
            // 
            // btnConfirmDatabase
            // 
            this.btnConfirmDatabase.Location = new System.Drawing.Point(6, 143);
            this.btnConfirmDatabase.Name = "btnConfirmDatabase";
            this.btnConfirmDatabase.Size = new System.Drawing.Size(105, 23);
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
            this.serverDatabaseTableSelector1.Location = new System.Drawing.Point(6, 19);
            this.serverDatabaseTableSelector1.Name = "serverDatabaseTableSelector1";
            this.serverDatabaseTableSelector1.Password = "";
            this.serverDatabaseTableSelector1.Server = "";
            this.serverDatabaseTableSelector1.Size = new System.Drawing.Size(581, 146);
            this.serverDatabaseTableSelector1.TabIndex = 0;
            this.serverDatabaseTableSelector1.Table = "";
            this.serverDatabaseTableSelector1.Username = "";
            // 
            // gbPickPipeline
            // 
            this.gbPickPipeline.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbPickPipeline.Controls.Add(this.ddPipeline);
            this.gbPickPipeline.Location = new System.Drawing.Point(12, 246);
            this.gbPickPipeline.Name = "gbPickPipeline";
            this.gbPickPipeline.Size = new System.Drawing.Size(604, 52);
            this.gbPickPipeline.TabIndex = 2;
            this.gbPickPipeline.TabStop = false;
            this.gbPickPipeline.Text = "3. Pick Pipeline";
            // 
            // ddPipeline
            // 
            this.ddPipeline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddPipeline.FormattingEnabled = true;
            this.ddPipeline.Location = new System.Drawing.Point(21, 19);
            this.ddPipeline.Name = "ddPipeline";
            this.ddPipeline.Size = new System.Drawing.Size(575, 21);
            this.ddPipeline.TabIndex = 0;
            this.ddPipeline.SelectedIndexChanged += new System.EventHandler(this.ddPipeline_SelectedIndexChanged);
            // 
            // gbExecute
            // 
            this.gbExecute.Controls.Add(this.ragSmileyExecute);
            this.gbExecute.Controls.Add(this.btnExecute);
            this.gbExecute.Controls.Add(this.btnPreview);
            this.gbExecute.Location = new System.Drawing.Point(12, 304);
            this.gbExecute.Name = "gbExecute";
            this.gbExecute.Size = new System.Drawing.Size(604, 51);
            this.gbExecute.TabIndex = 4;
            this.gbExecute.TabStop = false;
            this.gbExecute.Text = "4. Import Data";
            // 
            // ragSmileyExecute
            // 
            this.ragSmileyExecute.AlwaysShowHandCursor = false;
            this.ragSmileyExecute.BackColor = System.Drawing.Color.Transparent;
            this.ragSmileyExecute.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmileyExecute.Location = new System.Drawing.Point(183, 17);
            this.ragSmileyExecute.Name = "ragSmileyExecute";
            this.ragSmileyExecute.Size = new System.Drawing.Size(25, 25);
            this.ragSmileyExecute.TabIndex = 5;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(102, 19);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
            this.btnExecute.TabIndex = 3;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(21, 19);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(75, 23);
            this.btnPreview.TabIndex = 3;
            this.btnPreview.Text = "Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnAdvanced
            // 
            this.btnAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdvanced.Location = new System.Drawing.Point(622, 253);
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.Size = new System.Drawing.Size(75, 23);
            this.btnAdvanced.TabIndex = 4;
            this.btnAdvanced.Text = "Advanced";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            // 
            // pbHelp
            // 
            this.pbHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbHelp.Location = new System.Drawing.Point(680, 12);
            this.pbHelp.Name = "pbHelp";
            this.pbHelp.Size = new System.Drawing.Size(25, 25);
            this.pbHelp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbHelp.TabIndex = 6;
            this.pbHelp.TabStop = false;
            this.pbHelp.Click += new System.EventHandler(this.pbHelp_Click);
            // 
            // cbAutoClose
            // 
            this.cbAutoClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbAutoClose.AutoSize = true;
            this.cbAutoClose.Checked = true;
            this.cbAutoClose.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAutoClose.Location = new System.Drawing.Point(12, 363);
            this.cbAutoClose.Name = "cbAutoClose";
            this.cbAutoClose.Size = new System.Drawing.Size(131, 17);
            this.cbAutoClose.TabIndex = 7;
            this.cbAutoClose.Text = "Close When Complete";
            this.cbAutoClose.UseVisualStyleBackColor = true;
            // 
            // CreateNewCatalogueByImportingFileUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(727, 383);
            this.Controls.Add(this.gbPickPipeline);
            this.Controls.Add(this.gbExecute);
            this.Controls.Add(this.cbAutoClose);
            this.Controls.Add(this.pbHelp);
            this.Controls.Add(this.btnAdvanced);
            this.Controls.Add(this.gbPickFile);
            this.Controls.Add(this.gbPickDatabase);
            this.Name = "CreateNewCatalogueByImportingFileUI";
            this.Text = "Create Catalogue By Importing A File";
            this.gbPickFile.ResumeLayout(false);
            this.gbPickFile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFile)).EndInit();
            this.gbPickDatabase.ResumeLayout(false);
            this.gbPickDatabase.PerformLayout();
            this.gbPickPipeline.ResumeLayout(false);
            this.gbExecute.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbHelp)).EndInit();
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
        private ReusableUIComponents.ServerDatabaseTableSelector serverDatabaseTableSelector1;
        private System.Windows.Forms.Button btnConfirmDatabase;
        private System.Windows.Forms.GroupBox gbPickPipeline;
        private System.Windows.Forms.ComboBox ddPipeline;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmileyFile;
        private System.Windows.Forms.Button btnAdvanced;
        private System.Windows.Forms.GroupBox gbExecute;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnExecute;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmileyExecute;
        private System.Windows.Forms.PictureBox pbHelp;
        private System.Windows.Forms.CheckBox cbAutoClose;
    }
}