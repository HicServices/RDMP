namespace Rdmp.UI.LocationsMenu.Ticketing
{
    partial class TicketingSystemConfigurationUI
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
            label1 = new System.Windows.Forms.Label();
            tbID = new System.Windows.Forms.TextBox();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            gbTicketingSystem = new System.Windows.Forms.GroupBox();
            label5 = new System.Windows.Forms.Label();
            cbDisabled = new System.Windows.Forms.CheckBox();
            cbxType = new System.Windows.Forms.ComboBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            ddCredentials = new System.Windows.Forms.ComboBox();
            btnDeleteCredentials = new System.Windows.Forms.Button();
            btnEditCredentials = new System.Windows.Forms.Button();
            btnAddCredentials = new System.Windows.Forms.Button();
            btnCheck = new System.Windows.Forms.Button();
            btnSave = new System.Windows.Forms.Button();
            label4 = new System.Windows.Forms.Label();
            tbUrl = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            tbName = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            checksUI1 = new ChecksUI.ChecksUI();
            btnCreate = new System.Windows.Forms.Button();
            btnDelete = new System.Windows.Forms.Button();
            tbReleases = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            gbTicketingSystem.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(36, 25);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(18, 15);
            label1.TabIndex = 0;
            label1.Text = "ID";
            // 
            // tbID
            // 
            tbID.Location = new System.Drawing.Point(64, 22);
            tbID.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbID.Name = "tbID";
            tbID.ReadOnly = true;
            tbID.Size = new System.Drawing.Size(116, 23);
            tbID.TabIndex = 1;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer1.Location = new System.Drawing.Point(2, 42);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(gbTicketingSystem);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(checksUI1);
            splitContainer1.Size = new System.Drawing.Size(1029, 707);
            splitContainer1.SplitterDistance = 249;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 2;
            // 
            // gbTicketingSystem
            // 
            gbTicketingSystem.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbTicketingSystem.Controls.Add(tbReleases);
            gbTicketingSystem.Controls.Add(label5);
            gbTicketingSystem.Controls.Add(cbDisabled);
            gbTicketingSystem.Controls.Add(cbxType);
            gbTicketingSystem.Controls.Add(groupBox1);
            gbTicketingSystem.Controls.Add(btnCheck);
            gbTicketingSystem.Controls.Add(btnSave);
            gbTicketingSystem.Controls.Add(label4);
            gbTicketingSystem.Controls.Add(tbUrl);
            gbTicketingSystem.Controls.Add(label3);
            gbTicketingSystem.Controls.Add(tbName);
            gbTicketingSystem.Controls.Add(label2);
            gbTicketingSystem.Controls.Add(tbID);
            gbTicketingSystem.Controls.Add(label1);
            gbTicketingSystem.Enabled = false;
            gbTicketingSystem.Location = new System.Drawing.Point(4, 3);
            gbTicketingSystem.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbTicketingSystem.Name = "gbTicketingSystem";
            gbTicketingSystem.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbTicketingSystem.Size = new System.Drawing.Size(1025, 243);
            gbTicketingSystem.TabIndex = 0;
            gbTicketingSystem.TabStop = false;
            gbTicketingSystem.Text = "TicketingSystem";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(0, 196);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(62, 30);
            label5.TabIndex = 19;
            label5.Text = "Releasable\r\nStatuses";
            label5.Click += label5_Click;
            // 
            // cbDisabled
            // 
            cbDisabled.AutoSize = true;
            cbDisabled.Location = new System.Drawing.Point(64, 174);
            cbDisabled.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbDisabled.Name = "cbDisabled";
            cbDisabled.Size = new System.Drawing.Size(64, 19);
            cbDisabled.TabIndex = 17;
            cbDisabled.Text = "Disable";
            cbDisabled.UseVisualStyleBackColor = true;
            cbDisabled.CheckedChanged += cbDisabled_CheckedChanged;
            // 
            // cbxType
            // 
            cbxType.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cbxType.FormattingEnabled = true;
            cbxType.Location = new System.Drawing.Point(64, 110);
            cbxType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbxType.Name = "cbxType";
            cbxType.Size = new System.Drawing.Size(956, 23);
            cbxType.TabIndex = 3;
            cbxType.TextChanged += tb_TextChanged;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            groupBox1.Controls.Add(ddCredentials);
            groupBox1.Controls.Add(btnDeleteCredentials);
            groupBox1.Controls.Add(btnEditCredentials);
            groupBox1.Controls.Add(btnAddCredentials);
            groupBox1.Location = new System.Drawing.Point(514, 141);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(503, 93);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Credentials";
            // 
            // ddCredentials
            // 
            ddCredentials.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddCredentials.FormattingEnabled = true;
            ddCredentials.Location = new System.Drawing.Point(20, 22);
            ddCredentials.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ddCredentials.Name = "ddCredentials";
            ddCredentials.Size = new System.Drawing.Size(465, 23);
            ddCredentials.Sorted = true;
            ddCredentials.TabIndex = 18;
            ddCredentials.SelectedIndexChanged += ddCredentials_SelectedIndexChanged;
            // 
            // btnDeleteCredentials
            // 
            btnDeleteCredentials.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnDeleteCredentials.Location = new System.Drawing.Point(310, 53);
            btnDeleteCredentials.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnDeleteCredentials.Name = "btnDeleteCredentials";
            btnDeleteCredentials.Size = new System.Drawing.Size(138, 27);
            btnDeleteCredentials.TabIndex = 17;
            btnDeleteCredentials.Text = "Delete Credentials";
            btnDeleteCredentials.UseVisualStyleBackColor = true;
            btnDeleteCredentials.Click += btnDeleteCredentials_Click;
            // 
            // btnEditCredentials
            // 
            btnEditCredentials.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnEditCredentials.Location = new System.Drawing.Point(164, 53);
            btnEditCredentials.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnEditCredentials.Name = "btnEditCredentials";
            btnEditCredentials.Size = new System.Drawing.Size(138, 27);
            btnEditCredentials.TabIndex = 17;
            btnEditCredentials.Text = "Edit Credentials";
            btnEditCredentials.UseVisualStyleBackColor = true;
            btnEditCredentials.Click += btnEditCredentials_Click;
            // 
            // btnAddCredentials
            // 
            btnAddCredentials.Anchor = System.Windows.Forms.AnchorStyles.Top;
            btnAddCredentials.Location = new System.Drawing.Point(20, 53);
            btnAddCredentials.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnAddCredentials.Name = "btnAddCredentials";
            btnAddCredentials.Size = new System.Drawing.Size(138, 27);
            btnAddCredentials.TabIndex = 16;
            btnAddCredentials.Text = "Add New Credentials";
            btnAddCredentials.UseVisualStyleBackColor = true;
            btnAddCredentials.Click += btnAddCredentials_Click;
            // 
            // btnCheck
            // 
            btnCheck.Location = new System.Drawing.Point(210, 141);
            btnCheck.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnCheck.Name = "btnCheck";
            btnCheck.Size = new System.Drawing.Size(138, 27);
            btnCheck.TabIndex = 16;
            btnCheck.Text = "Check";
            btnCheck.UseVisualStyleBackColor = true;
            btnCheck.Click += btnCheck_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new System.Drawing.Point(64, 141);
            btnSave.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(138, 27);
            btnSave.TabIndex = 16;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(23, 113);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(31, 15);
            label4.TabIndex = 0;
            label4.Text = "Type";
            // 
            // tbUrl
            // 
            tbUrl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbUrl.Location = new System.Drawing.Point(64, 80);
            tbUrl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbUrl.Name = "tbUrl";
            tbUrl.Size = new System.Drawing.Size(956, 23);
            tbUrl.TabIndex = 1;
            tbUrl.TextChanged += tb_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(32, 83);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(22, 15);
            label3.TabIndex = 0;
            label3.Text = "Url";
            // 
            // tbName
            // 
            tbName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbName.Location = new System.Drawing.Point(64, 52);
            tbName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(956, 23);
            tbName.TabIndex = 1;
            tbName.TextChanged += tb_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(15, 55);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(39, 15);
            label2.TabIndex = 0;
            label2.Text = "Name";
            // 
            // checksUI1
            // 
            checksUI1.AllowsYesNoToAll = true;
            checksUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            checksUI1.Location = new System.Drawing.Point(0, 0);
            checksUI1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            checksUI1.Name = "checksUI1";
            checksUI1.Size = new System.Drawing.Size(1029, 453);
            checksUI1.TabIndex = 0;
            // 
            // btnCreate
            // 
            btnCreate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnCreate.Location = new System.Drawing.Point(743, 8);
            btnCreate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new System.Drawing.Size(138, 27);
            btnCreate.TabIndex = 17;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnDelete.Location = new System.Drawing.Point(888, 8);
            btnDelete.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(138, 27);
            btnDelete.TabIndex = 17;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // tbReleases
            // 
            tbReleases.Location = new System.Drawing.Point(64, 198);
            tbReleases.Name = "tbReleases";
            tbReleases.Size = new System.Drawing.Size(421, 23);
            tbReleases.TabIndex = 20;
            tbReleases.TextChanged += tReleases_TextChanged;
            // 
            // TicketingSystemConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(btnDelete);
            Controls.Add(btnCreate);
            Controls.Add(splitContainer1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "TicketingSystemConfigurationUI";
            Size = new System.Drawing.Size(1034, 751);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            gbTicketingSystem.ResumeLayout(false);
            gbTicketingSystem.PerformLayout();
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox gbTicketingSystem;
        private ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox ddCredentials;
        private System.Windows.Forms.Button btnEditCredentials;
        private System.Windows.Forms.Button btnAddCredentials;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbUrl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbxType;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnDeleteCredentials;
        private System.Windows.Forms.CheckBox cbDisabled;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbReleases;
    }
}