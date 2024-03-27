using Rdmp.UI.SimpleControls;
using Rdmp.UI.ChecksUI;
using System;

namespace Rdmp.UI.SimpleControls
{
    partial class ServerDatabaseTableSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerDatabaseTableSelector));
            lblTable = new System.Windows.Forms.Label();
            cbxTable = new System.Windows.Forms.ComboBox();
            label43 = new System.Windows.Forms.Label();
            cbxDatabase = new System.Windows.Forms.ComboBox();
            label50 = new System.Windows.Forms.Label();
            cbxServer = new System.Windows.Forms.ComboBox();
            lblOr = new System.Windows.Forms.Label();
            lblTableValuedFunction = new System.Windows.Forms.Label();
            cbxTableValueFunctions = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            tbUsername = new System.Windows.Forms.TextBox();
            tbPassword = new System.Windows.Forms.TextBox();
            llLoading = new System.Windows.Forms.LinkLabel();
            pbLoading = new System.Windows.Forms.PictureBox();
            btnRefreshDatabases = new System.Windows.Forms.Button();
            btnRefreshTables = new System.Windows.Forms.Button();
            databaseTypeUI1 = new DatabaseTypeUI();
            btnPickCredentials = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            tbTimeout = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)pbLoading).BeginInit();
            SuspendLayout();
            // 
            // lblTable
            // 
            lblTable.AutoSize = true;
            lblTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            lblTable.Location = new System.Drawing.Point(81, 167);
            lblTable.Name = "lblTable";
            lblTable.Size = new System.Drawing.Size(37, 13);
            lblTable.TabIndex = 7;
            lblTable.Text = "Table:";
            // 
            // cbxTable
            // 
            cbxTable.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cbxTable.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            cbxTable.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            cbxTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            cbxTable.FormattingEnabled = true;
            cbxTable.Location = new System.Drawing.Point(125, 164);
            cbxTable.Name = "cbxTable";
            cbxTable.Size = new System.Drawing.Size(667, 21);
            cbxTable.TabIndex = 6;
            cbxTable.SelectedIndexChanged += cbxTable_SelectedIndexChanged;
            // 
            // label43
            // 
            label43.AutoSize = true;
            label43.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            label43.Location = new System.Drawing.Point(62, 86);
            label43.Name = "label43";
            label43.Size = new System.Drawing.Size(56, 13);
            label43.TabIndex = 160;
            label43.Text = "Database:";
            // 
            // cbxDatabase
            // 
            cbxDatabase.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cbxDatabase.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            cbxDatabase.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            cbxDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            cbxDatabase.FormattingEnabled = true;
            cbxDatabase.Location = new System.Drawing.Point(125, 83);
            cbxDatabase.Name = "cbxDatabase";
            cbxDatabase.Size = new System.Drawing.Size(667, 21);
            cbxDatabase.Sorted = true;
            cbxDatabase.TabIndex = 4;
            cbxDatabase.SelectedIndexChanged += cbxDatabase_SelectedIndexChanged;
            cbxDatabase.TextChanged += cbxDatabase_TextChanged;
            // 
            // label50
            // 
            label50.AutoSize = true;
            label50.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            label50.Location = new System.Drawing.Point(77, 59);
            label50.Name = "label50";
            label50.Size = new System.Drawing.Size(41, 13);
            label50.TabIndex = 159;
            label50.Text = "Server:";
            // 
            // cbxServer
            // 
            cbxServer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cbxServer.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            cbxServer.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            cbxServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            cbxServer.FormattingEnabled = true;
            cbxServer.Location = new System.Drawing.Point(125, 56);
            cbxServer.Name = "cbxServer";
            cbxServer.Size = new System.Drawing.Size(589, 21);
            cbxServer.TabIndex = 2;
            cbxServer.SelectedIndexChanged += cbxServer_SelectedIndexChanged;
            cbxServer.Leave += cbxServer_Leave;
            // 
            // lblOr
            // 
            lblOr.AutoSize = true;
            lblOr.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            lblOr.Location = new System.Drawing.Point(214, 187);
            lblOr.Name = "lblOr";
            lblOr.Size = new System.Drawing.Size(23, 13);
            lblOr.TabIndex = 164;
            lblOr.Text = "OR";
            // 
            // lblTableValuedFunction
            // 
            lblTableValuedFunction.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            lblTableValuedFunction.Location = new System.Drawing.Point(2, 203);
            lblTableValuedFunction.Name = "lblTableValuedFunction";
            lblTableValuedFunction.Size = new System.Drawing.Size(123, 40);
            lblTableValuedFunction.TabIndex = 10;
            lblTableValuedFunction.Text = "Table Valued Function:";
            // 
            // cbxTableValueFunctions
            // 
            cbxTableValueFunctions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cbxTableValueFunctions.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            cbxTableValueFunctions.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            cbxTableValueFunctions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            cbxTableValueFunctions.FormattingEnabled = true;
            cbxTableValueFunctions.Location = new System.Drawing.Point(125, 202);
            cbxTableValueFunctions.Name = "cbxTableValueFunctions";
            cbxTableValueFunctions.Size = new System.Drawing.Size(667, 21);
            cbxTableValueFunctions.TabIndex = 8;
            cbxTableValueFunctions.SelectedIndexChanged += cbxTableValueFunctions_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            label1.Location = new System.Drawing.Point(29, 4);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(89, 13);
            label1.TabIndex = 161;
            label1.Text = "UserID (optional):";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            label2.Location = new System.Drawing.Point(16, 31);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(102, 13);
            label2.TabIndex = 161;
            label2.Text = "Password (optional):";
            // 
            // tbUsername
            // 
            tbUsername.Location = new System.Drawing.Point(125, 0);
            tbUsername.Name = "tbUsername";
            tbUsername.Size = new System.Drawing.Size(329, 23);
            tbUsername.TabIndex = 0;
            tbUsername.TextChanged += tbUsername_TextChanged;
            // 
            // tbPassword
            // 
            tbPassword.Location = new System.Drawing.Point(125, 27);
            tbPassword.Name = "tbPassword";
            tbPassword.PasswordChar = '*';
            tbPassword.Size = new System.Drawing.Size(329, 23);
            tbPassword.TabIndex = 1;
            tbPassword.TextChanged += tbPassword_TextChanged;
            // 
            // llLoading
            // 
            llLoading.AutoSize = true;
            llLoading.Location = new System.Drawing.Point(153, 136);
            llLoading.Name = "llLoading";
            llLoading.Size = new System.Drawing.Size(108, 15);
            llLoading.TabIndex = 6;
            llLoading.TabStop = true;
            llLoading.Text = "Cancel Connection";
            llLoading.Visible = false;
            llLoading.LinkClicked += llLoading_LinkClicked;
            // 
            // pbLoading
            // 
            pbLoading.Image = (System.Drawing.Image)resources.GetObject("pbLoading.Image");
            pbLoading.Location = new System.Drawing.Point(125, 136);
            pbLoading.Name = "pbLoading";
            pbLoading.Size = new System.Drawing.Size(22, 22);
            pbLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pbLoading.TabIndex = 167;
            pbLoading.TabStop = false;
            pbLoading.Visible = false;
            // 
            // btnRefreshDatabases
            // 
            btnRefreshDatabases.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnRefreshDatabases.Image = (System.Drawing.Image)resources.GetObject("btnRefreshDatabases.Image");
            btnRefreshDatabases.Location = new System.Drawing.Point(798, 82);
            btnRefreshDatabases.Name = "btnRefreshDatabases";
            btnRefreshDatabases.Size = new System.Drawing.Size(22, 24);
            btnRefreshDatabases.TabIndex = 5;
            btnRefreshDatabases.UseVisualStyleBackColor = true;
            btnRefreshDatabases.Click += btnRefreshDatabases_Click;
            // 
            // btnRefreshTables
            // 
            btnRefreshTables.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnRefreshTables.Image = (System.Drawing.Image)resources.GetObject("btnRefreshTables.Image");
            btnRefreshTables.Location = new System.Drawing.Point(798, 162);
            btnRefreshTables.Name = "btnRefreshTables";
            btnRefreshTables.Size = new System.Drawing.Size(22, 24);
            btnRefreshTables.TabIndex = 7;
            btnRefreshTables.UseVisualStyleBackColor = true;
            btnRefreshTables.Click += btnRefreshTables_Click;
            // 
            // databaseTypeUI1
            // 
            databaseTypeUI1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            databaseTypeUI1.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            databaseTypeUI1.Location = new System.Drawing.Point(721, 52);
            databaseTypeUI1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            databaseTypeUI1.Name = "databaseTypeUI1";
            databaseTypeUI1.Size = new System.Drawing.Size(177, 28);
            databaseTypeUI1.TabIndex = 168;
            databaseTypeUI1.DatabaseTypeChanged += databaseTypeUI1_DatabaseTypeChanged;
            // 
            // btnPickCredentials
            // 
            btnPickCredentials.Location = new System.Drawing.Point(460, 27);
            btnPickCredentials.Name = "btnPickCredentials";
            btnPickCredentials.Size = new System.Drawing.Size(23, 23);
            btnPickCredentials.TabIndex = 169;
            btnPickCredentials.UseVisualStyleBackColor = true;
            btnPickCredentials.Click += btnPickCredentials_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            label3.Location = new System.Drawing.Point(24, 116);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(94, 13);
            label3.TabIndex = 170;
            label3.Text = "Timeout (optional):";
            label3.Click += label3_Click;
            // 
            // tbTimeout
            // 
            tbTimeout.Location = new System.Drawing.Point(125, 112);
            tbTimeout.Name = "tbTimeout";
            tbTimeout.Size = new System.Drawing.Size(329, 23);
            tbTimeout.TabIndex = 171;
            // 
            // ServerDatabaseTableSelector
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            AutoSize = true;
            Controls.Add(tbTimeout);
            Controls.Add(label3);
            Controls.Add(btnPickCredentials);
            Controls.Add(databaseTypeUI1);
            Controls.Add(btnRefreshTables);
            Controls.Add(btnRefreshDatabases);
            Controls.Add(pbLoading);
            Controls.Add(llLoading);
            Controls.Add(tbPassword);
            Controls.Add(tbUsername);
            Controls.Add(lblOr);
            Controls.Add(lblTableValuedFunction);
            Controls.Add(cbxTableValueFunctions);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lblTable);
            Controls.Add(cbxTable);
            Controls.Add(label43);
            Controls.Add(cbxDatabase);
            Controls.Add(label50);
            Controls.Add(cbxServer);
            Name = "ServerDatabaseTableSelector";
            Size = new System.Drawing.Size(901, 243);
            ((System.ComponentModel.ISupportInitialize)pbLoading).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTable;
        private System.Windows.Forms.ComboBox cbxTable;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.ComboBox cbxDatabase;
        private System.Windows.Forms.Label label50;
        public System.Windows.Forms.ComboBox cbxServer;
        private System.Windows.Forms.Label lblOr;
        private System.Windows.Forms.Label lblTableValuedFunction;
        private System.Windows.Forms.ComboBox cbxTableValueFunctions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.LinkLabel llLoading;
        private System.Windows.Forms.PictureBox pbLoading;
        private System.Windows.Forms.Button btnRefreshDatabases;
        private System.Windows.Forms.Button btnRefreshTables;
        private DatabaseTypeUI databaseTypeUI1;
        private System.Windows.Forms.Button btnPickCredentials;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbTimeout;
    }
}
