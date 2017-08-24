using BrightIdeasSoftware;

namespace DataExportManager.ProjectUI.DataUsers
{
    partial class DataUserManagement
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
            this.label2 = new System.Windows.Forms.Label();
            this.lbUsersRegisteredOnProject = new System.Windows.Forms.ListBox();
            this.btnAddNewUser = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lbKnownUsers = new BrightIdeasSoftware.ObjectListView();
            this.olvCol_ID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCol_Forename = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCol_Surname = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCol_Email = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.lbKnownUsers)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Known Data Users:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(688, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Registered On Project";
            // 
            // lbUsersRegisteredOnProject
            // 
            this.lbUsersRegisteredOnProject.AllowDrop = true;
            this.lbUsersRegisteredOnProject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbUsersRegisteredOnProject.FormattingEnabled = true;
            this.lbUsersRegisteredOnProject.Location = new System.Drawing.Point(691, 46);
            this.lbUsersRegisteredOnProject.Name = "lbUsersRegisteredOnProject";
            this.lbUsersRegisteredOnProject.Size = new System.Drawing.Size(289, 615);
            this.lbUsersRegisteredOnProject.TabIndex = 3;
            this.lbUsersRegisteredOnProject.SelectedIndexChanged += new System.EventHandler(this.lbUsersRegisteredOnProject_SelectedIndexChanged);
            // 
            // btnAddNewUser
            // 
            this.btnAddNewUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddNewUser.Location = new System.Drawing.Point(435, 638);
            this.btnAddNewUser.Name = "btnAddNewUser";
            this.btnAddNewUser.Size = new System.Drawing.Size(75, 23);
            this.btnAddNewUser.TabIndex = 13;
            this.btnAddNewUser.Text = "Add New";
            this.btnAddNewUser.UseVisualStyleBackColor = true;
            this.btnAddNewUser.Click += new System.EventHandler(this.btnAddNewUser_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(516, 46);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(169, 23);
            this.btnAdd.TabIndex = 15;
            this.btnAdd.Text = ">";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemove.Location = new System.Drawing.Point(516, 75);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(169, 23);
            this.btnRemove.TabIndex = 15;
            this.btnRemove.Text = "<";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // lbKnownUsers
            // 
            this.lbKnownUsers.AllColumns.Add(this.olvCol_ID);
            this.lbKnownUsers.AllColumns.Add(this.olvCol_Forename);
            this.lbKnownUsers.AllColumns.Add(this.olvCol_Surname);
            this.lbKnownUsers.AllColumns.Add(this.olvCol_Email);
            this.lbKnownUsers.AllowDrop = true;
            this.lbKnownUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbKnownUsers.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.lbKnownUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvCol_ID,
            this.olvCol_Forename,
            this.olvCol_Surname,
            this.olvCol_Email});
            this.lbKnownUsers.FullRowSelect = true;
            this.lbKnownUsers.Location = new System.Drawing.Point(12, 46);
            this.lbKnownUsers.Name = "lbKnownUsers";
            this.lbKnownUsers.Size = new System.Drawing.Size(498, 586);
            this.lbKnownUsers.TabIndex = 0;
            this.lbKnownUsers.UseCompatibleStateImageBehavior = false;
            this.lbKnownUsers.View = System.Windows.Forms.View.Details;
            this.lbKnownUsers.CellEditFinishing += new BrightIdeasSoftware.CellEditEventHandler(this.lbKnownUsers_CellEditFinishing);
            this.lbKnownUsers.SelectedIndexChanged += new System.EventHandler(this.lbKnownUsers_SelectedIndexChanged);
            this.lbKnownUsers.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbKnownUsers_KeyUp);
            // 
            // olvCol_ID
            // 
            this.olvCol_ID.AspectName = "ID";
            this.olvCol_ID.Groupable = false;
            this.olvCol_ID.IsEditable = false;
            this.olvCol_ID.Text = "ID";
            // 
            // olvCol_Forename
            // 
            this.olvCol_Forename.AspectName = "Forename";
            this.olvCol_Forename.FillsFreeSpace = true;
            this.olvCol_Forename.Groupable = false;
            this.olvCol_Forename.Text = "Forename";
            // 
            // olvCol_Surname
            // 
            this.olvCol_Surname.AspectName = "Surname";
            this.olvCol_Surname.FillsFreeSpace = true;
            this.olvCol_Surname.Groupable = false;
            this.olvCol_Surname.Text = "Surname";
            // 
            // olvCol_Email
            // 
            this.olvCol_Email.AspectName = "Email";
            this.olvCol_Email.FillsFreeSpace = true;
            this.olvCol_Email.Groupable = false;
            this.olvCol_Email.Text = "Email";
            // 
            // DataUserManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 673);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnAddNewUser);
            this.Controls.Add(this.lbUsersRegisteredOnProject);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbKnownUsers);
            this.KeyPreview = true;
            this.Name = "DataUserManagement";
            this.Text = "DataUserManagement";
            ((System.ComponentModel.ISupportInitialize)(this.lbKnownUsers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox lbUsersRegisteredOnProject;
        private System.Windows.Forms.Button btnAddNewUser;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private ObjectListView lbKnownUsers;
        private OLVColumn olvCol_ID;
        private OLVColumn olvCol_Forename;
        private OLVColumn olvCol_Surname;
        private OLVColumn olvCol_Email;
    }
}