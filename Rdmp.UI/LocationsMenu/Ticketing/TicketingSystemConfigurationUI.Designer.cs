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
            this.label1 = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.gbTicketingSystem = new System.Windows.Forms.GroupBox();
            this.cbxType = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ddCredentials = new System.Windows.Forms.ComboBox();
            this.btnDeleteCredentials = new System.Windows.Forms.Button();
            this.btnEditCredentials = new System.Windows.Forms.Button();
            this.btnAddCredentials = new System.Windows.Forms.Button();
            this.btnCheck = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tbUrl = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checksUI1 = new ChecksUI.ChecksUI();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.gbTicketingSystem.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "ID";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(55, 19);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(2, 36);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.gbTicketingSystem);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.checksUI1);
            this.splitContainer1.Size = new System.Drawing.Size(882, 613);
            this.splitContainer1.SplitterDistance = 216;
            this.splitContainer1.TabIndex = 2;
            // 
            // gbTicketingSystem
            // 
            this.gbTicketingSystem.Controls.Add(this.cbxType);
            this.gbTicketingSystem.Controls.Add(this.groupBox1);
            this.gbTicketingSystem.Controls.Add(this.btnCheck);
            this.gbTicketingSystem.Controls.Add(this.btnSave);
            this.gbTicketingSystem.Controls.Add(this.label4);
            this.gbTicketingSystem.Controls.Add(this.tbUrl);
            this.gbTicketingSystem.Controls.Add(this.label3);
            this.gbTicketingSystem.Controls.Add(this.tbName);
            this.gbTicketingSystem.Controls.Add(this.label2);
            this.gbTicketingSystem.Controls.Add(this.tbID);
            this.gbTicketingSystem.Controls.Add(this.label1);
            this.gbTicketingSystem.Enabled = false;
            this.gbTicketingSystem.Location = new System.Drawing.Point(3, 3);
            this.gbTicketingSystem.Name = "gbTicketingSystem";
            this.gbTicketingSystem.Size = new System.Drawing.Size(869, 211);
            this.gbTicketingSystem.TabIndex = 0;
            this.gbTicketingSystem.TabStop = false;
            this.gbTicketingSystem.Text = "TicketingSystem";
            // 
            // cbxType
            // 
            this.cbxType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxType.FormattingEnabled = true;
            this.cbxType.Location = new System.Drawing.Point(55, 95);
            this.cbxType.Name = "cbxType";
            this.cbxType.Size = new System.Drawing.Size(808, 21);
            this.cbxType.TabIndex = 3;
            this.cbxType.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ddCredentials);
            this.groupBox1.Controls.Add(this.btnDeleteCredentials);
            this.groupBox1.Controls.Add(this.btnEditCredentials);
            this.groupBox1.Controls.Add(this.btnAddCredentials);
            this.groupBox1.Location = new System.Drawing.Point(441, 122);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(422, 81);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Credentials";
            // 
            // ddCredentials
            // 
            this.ddCredentials.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCredentials.FormattingEnabled = true;
            this.ddCredentials.Location = new System.Drawing.Point(17, 19);
            this.ddCredentials.Name = "ddCredentials";
            this.ddCredentials.Size = new System.Drawing.Size(399, 21);
            this.ddCredentials.Sorted = true;
            this.ddCredentials.TabIndex = 18;
            this.ddCredentials.SelectedIndexChanged += new System.EventHandler(this.ddCredentials_SelectedIndexChanged);
            // 
            // btnDeleteCredentials
            // 
            this.btnDeleteCredentials.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnDeleteCredentials.Location = new System.Drawing.Point(266, 46);
            this.btnDeleteCredentials.Name = "btnDeleteCredentials";
            this.btnDeleteCredentials.Size = new System.Drawing.Size(118, 23);
            this.btnDeleteCredentials.TabIndex = 17;
            this.btnDeleteCredentials.Text = "Delete Credentials";
            this.btnDeleteCredentials.UseVisualStyleBackColor = true;
            this.btnDeleteCredentials.Click += new System.EventHandler(this.btnDeleteCredentials_Click);
            // 
            // btnEditCredentials
            // 
            this.btnEditCredentials.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnEditCredentials.Location = new System.Drawing.Point(142, 46);
            this.btnEditCredentials.Name = "btnEditCredentials";
            this.btnEditCredentials.Size = new System.Drawing.Size(118, 23);
            this.btnEditCredentials.TabIndex = 17;
            this.btnEditCredentials.Text = "Edit Credentials";
            this.btnEditCredentials.UseVisualStyleBackColor = true;
            this.btnEditCredentials.Click += new System.EventHandler(this.btnEditCredentials_Click);
            // 
            // btnAddCredentials
            // 
            this.btnAddCredentials.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnAddCredentials.Location = new System.Drawing.Point(18, 46);
            this.btnAddCredentials.Name = "btnAddCredentials";
            this.btnAddCredentials.Size = new System.Drawing.Size(118, 23);
            this.btnAddCredentials.TabIndex = 16;
            this.btnAddCredentials.Text = "Add New Credentials";
            this.btnAddCredentials.UseVisualStyleBackColor = true;
            this.btnAddCredentials.Click += new System.EventHandler(this.btnAddCredentials_Click);
            // 
            // btnCheck
            // 
            this.btnCheck.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnCheck.Location = new System.Drawing.Point(179, 122);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(118, 23);
            this.btnCheck.TabIndex = 16;
            this.btnCheck.Text = "Check";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnSave.Location = new System.Drawing.Point(55, 122);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(118, 23);
            this.btnSave.TabIndex = 16;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Type";
            // 
            // tbUrl
            // 
            this.tbUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbUrl.Location = new System.Drawing.Point(55, 68);
            this.tbUrl.Name = "tbUrl";
            this.tbUrl.Size = new System.Drawing.Size(808, 20);
            this.tbUrl.TabIndex = 1;
            this.tbUrl.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Url";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(55, 45);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(808, 20);
            this.tbName.TabIndex = 1;
            this.tbName.TextChanged += new System.EventHandler(this.tb_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name";
            // 
            // checksUI1
            // 
            this.checksUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checksUI1.Location = new System.Drawing.Point(0, 0);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(882, 393);
            this.checksUI1.TabIndex = 0;
            // 
            // btnCreate
            // 
            this.btnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreate.Location = new System.Drawing.Point(637, 7);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(118, 23);
            this.btnCreate.TabIndex = 17;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Location = new System.Drawing.Point(761, 7);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(118, 23);
            this.btnDelete.TabIndex = 17;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // TicketingSystemConfigurationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(886, 651);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.splitContainer1);
            this.Name = "TicketingSystemConfigurationUI";
            this.Text = "TicketingUI";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.gbTicketingSystem.ResumeLayout(false);
            this.gbTicketingSystem.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

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
    }
}