using Rdmp.UI.SimpleControls;
using Rdmp.UI.ChecksUI;

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
            this.lblTable = new System.Windows.Forms.Label();
            this.cbxTable = new System.Windows.Forms.ComboBox();
            this.label43 = new System.Windows.Forms.Label();
            this.cbxDatabase = new System.Windows.Forms.ComboBox();
            this.label50 = new System.Windows.Forms.Label();
            this.cbxServer = new System.Windows.Forms.ComboBox();
            this.lblOr = new System.Windows.Forms.Label();
            this.lblTableValuedFunction = new System.Windows.Forms.Label();
            this.cbxTableValueFunctions = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.llLoading = new System.Windows.Forms.LinkLabel();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.btnRefreshDatabases = new System.Windows.Forms.Button();
            this.btnRefreshTables = new System.Windows.Forms.Button();
            this.databaseTypeUI1 = new Rdmp.UI.SimpleControls.DatabaseTypeUI();
            this.btnPickCredentials = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTable
            // 
            this.lblTable.AutoSize = true;
            this.lblTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTable.Location = new System.Drawing.Point(81, 124);
            this.lblTable.Name = "lblTable";
            this.lblTable.Size = new System.Drawing.Size(37, 13);
            this.lblTable.TabIndex = 7;
            this.lblTable.Text = "Table:";
            // 
            // cbxTable
            // 
            this.cbxTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxTable.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.cbxTable.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbxTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbxTable.FormattingEnabled = true;
            this.cbxTable.Location = new System.Drawing.Point(125, 121);
            this.cbxTable.Name = "cbxTable";
            this.cbxTable.Size = new System.Drawing.Size(667, 21);
            this.cbxTable.TabIndex = 6;
            this.cbxTable.SelectedIndexChanged += new System.EventHandler(this.cbxTable_SelectedIndexChanged);
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label43.Location = new System.Drawing.Point(62, 86);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(56, 13);
            this.label43.TabIndex = 160;
            this.label43.Text = "Database:";
            // 
            // cbxDatabase
            // 
            this.cbxDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxDatabase.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbxDatabase.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbxDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbxDatabase.FormattingEnabled = true;
            this.cbxDatabase.Location = new System.Drawing.Point(125, 83);
            this.cbxDatabase.Name = "cbxDatabase";
            this.cbxDatabase.Size = new System.Drawing.Size(667, 21);
            this.cbxDatabase.Sorted = true;
            this.cbxDatabase.TabIndex = 4;
            this.cbxDatabase.SelectedIndexChanged += new System.EventHandler(this.cbxDatabase_SelectedIndexChanged);
            this.cbxDatabase.TextChanged += new System.EventHandler(this.cbxDatabase_TextChanged);
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label50.Location = new System.Drawing.Point(77, 59);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(41, 13);
            this.label50.TabIndex = 159;
            this.label50.Text = "Server:";
            // 
            // cbxServer
            // 
            this.cbxServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxServer.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbxServer.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbxServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbxServer.FormattingEnabled = true;
            this.cbxServer.Location = new System.Drawing.Point(125, 56);
            this.cbxServer.Name = "cbxServer";
            this.cbxServer.Size = new System.Drawing.Size(589, 21);
            this.cbxServer.TabIndex = 2;
            this.cbxServer.SelectedIndexChanged += new System.EventHandler(this.cbxServer_SelectedIndexChanged);
            this.cbxServer.Leave += new System.EventHandler(this.cbxServer_Leave);
            // 
            // lblOr
            // 
            this.lblOr.AutoSize = true;
            this.lblOr.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblOr.Location = new System.Drawing.Point(214, 144);
            this.lblOr.Name = "lblOr";
            this.lblOr.Size = new System.Drawing.Size(23, 13);
            this.lblOr.TabIndex = 164;
            this.lblOr.Text = "OR";
            // 
            // lblTableValuedFunction
            // 
            this.lblTableValuedFunction.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTableValuedFunction.Location = new System.Drawing.Point(2, 160);
            this.lblTableValuedFunction.Name = "lblTableValuedFunction";
            this.lblTableValuedFunction.Size = new System.Drawing.Size(123, 40);
            this.lblTableValuedFunction.TabIndex = 10;
            this.lblTableValuedFunction.Text = "Table Valued Function:";
            // 
            // cbxTableValueFunctions
            // 
            this.cbxTableValueFunctions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxTableValueFunctions.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.cbxTableValueFunctions.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbxTableValueFunctions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbxTableValueFunctions.FormattingEnabled = true;
            this.cbxTableValueFunctions.Location = new System.Drawing.Point(125, 159);
            this.cbxTableValueFunctions.Name = "cbxTableValueFunctions";
            this.cbxTableValueFunctions.Size = new System.Drawing.Size(667, 21);
            this.cbxTableValueFunctions.TabIndex = 8;
            this.cbxTableValueFunctions.SelectedIndexChanged += new System.EventHandler(this.cbxTableValueFunctions_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(29, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 161;
            this.label1.Text = "UserID (optional):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(16, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 13);
            this.label2.TabIndex = 161;
            this.label2.Text = "Password (optional):";
            // 
            // tbUsername
            // 
            this.tbUsername.Location = new System.Drawing.Point(125, 0);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(329, 23);
            this.tbUsername.TabIndex = 0;
            this.tbUsername.TextChanged += new System.EventHandler(this.tbUsername_TextChanged);
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(125, 27);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(329, 23);
            this.tbPassword.TabIndex = 1;
            // 
            // llLoading
            // 
            this.llLoading.AutoSize = true;
            this.llLoading.Location = new System.Drawing.Point(125, 105);
            this.llLoading.Name = "llLoading";
            this.llLoading.Size = new System.Drawing.Size(108, 15);
            this.llLoading.TabIndex = 6;
            this.llLoading.TabStop = true;
            this.llLoading.Text = "Cancel Connection";
            this.llLoading.Visible = false;
            this.llLoading.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llLoading_LinkClicked);
            // 
            // pbLoading
            // 
            this.pbLoading.Image = ((System.Drawing.Image)(resources.GetObject("pbLoading.Image")));
            this.pbLoading.Location = new System.Drawing.Point(97, 102);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(22, 22);
            this.pbLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbLoading.TabIndex = 167;
            this.pbLoading.TabStop = false;
            this.pbLoading.Visible = false;
            // 
            // btnRefreshDatabases
            // 
            this.btnRefreshDatabases.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshDatabases.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshDatabases.Image")));
            this.btnRefreshDatabases.Location = new System.Drawing.Point(798, 82);
            this.btnRefreshDatabases.Name = "btnRefreshDatabases";
            this.btnRefreshDatabases.Size = new System.Drawing.Size(22, 24);
            this.btnRefreshDatabases.TabIndex = 5;
            this.btnRefreshDatabases.UseVisualStyleBackColor = true;
            this.btnRefreshDatabases.Click += new System.EventHandler(this.btnRefreshDatabases_Click);
            // 
            // btnRefreshTables
            // 
            this.btnRefreshTables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshTables.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshTables.Image")));
            this.btnRefreshTables.Location = new System.Drawing.Point(798, 119);
            this.btnRefreshTables.Name = "btnRefreshTables";
            this.btnRefreshTables.Size = new System.Drawing.Size(22, 24);
            this.btnRefreshTables.TabIndex = 7;
            this.btnRefreshTables.UseVisualStyleBackColor = true;
            this.btnRefreshTables.Click += new System.EventHandler(this.btnRefreshTables_Click);
            // 
            // databaseTypeUI1
            // 
            this.databaseTypeUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.databaseTypeUI1.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            this.databaseTypeUI1.Location = new System.Drawing.Point(721, 52);
            this.databaseTypeUI1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.databaseTypeUI1.Name = "databaseTypeUI1";
            this.databaseTypeUI1.Size = new System.Drawing.Size(177, 28);
            this.databaseTypeUI1.TabIndex = 168;
            this.databaseTypeUI1.DatabaseTypeChanged += new System.EventHandler(this.databaseTypeUI1_DatabaseTypeChanged);
            // 
            // btnPickCredentials
            // 
            this.btnPickCredentials.Location = new System.Drawing.Point(460, 27);
            this.btnPickCredentials.Name = "btnPickCredentials";
            this.btnPickCredentials.Size = new System.Drawing.Size(23, 23);
            this.btnPickCredentials.TabIndex = 169;
            this.btnPickCredentials.UseVisualStyleBackColor = true;
            this.btnPickCredentials.Click += new System.EventHandler(this.btnPickCredentials_Click);
            // 
            // ServerDatabaseTableSelector
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.Controls.Add(this.btnPickCredentials);
            this.Controls.Add(this.databaseTypeUI1);
            this.Controls.Add(this.btnRefreshTables);
            this.Controls.Add(this.btnRefreshDatabases);
            this.Controls.Add(this.pbLoading);
            this.Controls.Add(this.llLoading);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.tbUsername);
            this.Controls.Add(this.lblOr);
            this.Controls.Add(this.lblTableValuedFunction);
            this.Controls.Add(this.cbxTableValueFunctions);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblTable);
            this.Controls.Add(this.cbxTable);
            this.Controls.Add(this.label43);
            this.Controls.Add(this.cbxDatabase);
            this.Controls.Add(this.label50);
            this.Controls.Add(this.cbxServer);
            this.Name = "ServerDatabaseTableSelector";
            this.Size = new System.Drawing.Size(901, 204);
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}
