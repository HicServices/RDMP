using BrightIdeasSoftware;

namespace CatalogueManager.Issues
{
    partial class SelectIssueSystemUser
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
            this.btnOk = new System.Windows.Forms.Button();
            this.btnNewUser = new System.Windows.Forms.Button();
            this.lbUsers = new BrightIdeasSoftware.ObjectListView();
            this.olvCol_ID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCol_Name = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCol_EmailAddress = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.lbUsers)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-2, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Users:";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOk.Location = new System.Drawing.Point(457, 712);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(427, 23);
            this.btnOk.TabIndex = 8;
            this.btnOk.Text = "Select User";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnNewUser
            // 
            this.btnNewUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNewUser.Location = new System.Drawing.Point(1, 712);
            this.btnNewUser.Name = "btnNewUser";
            this.btnNewUser.Size = new System.Drawing.Size(450, 23);
            this.btnNewUser.TabIndex = 10;
            this.btnNewUser.Text = "New User";
            this.btnNewUser.UseVisualStyleBackColor = true;
            this.btnNewUser.Click += new System.EventHandler(this.btnNewUser_Click);
            // 
            // lbUsers
            // 
            this.lbUsers.AllColumns.Add(this.olvCol_ID);
            this.lbUsers.AllColumns.Add(this.olvCol_Name);
            this.lbUsers.AllColumns.Add(this.olvCol_EmailAddress);
            this.lbUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbUsers.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.lbUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvCol_ID,
            this.olvCol_Name,
            this.olvCol_EmailAddress});
            this.lbUsers.FullRowSelect = true;
            this.lbUsers.Location = new System.Drawing.Point(1, 25);
            this.lbUsers.Name = "lbUsers";
            this.lbUsers.Size = new System.Drawing.Size(883, 680);
            this.lbUsers.TabIndex = 0;
            this.lbUsers.UseCompatibleStateImageBehavior = false;
            this.lbUsers.View = System.Windows.Forms.View.Details;
            this.lbUsers.CellEditFinishing += new BrightIdeasSoftware.CellEditEventHandler(this.lbUsers_CellEditFinishing);
            this.lbUsers.ItemActivate += new System.EventHandler(this.lbUsers_ItemActivate);
            this.lbUsers.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbUsers_KeyUp);
            // 
            // olvCol_ID
            // 
            this.olvCol_ID.AspectName = "ID";
            this.olvCol_ID.Groupable = false;
            this.olvCol_ID.IsEditable = false;
            this.olvCol_ID.Text = "ID";
            // 
            // olvCol_Name
            // 
            this.olvCol_Name.AspectName = "Name";
            this.olvCol_Name.FillsFreeSpace = true;
            this.olvCol_Name.Groupable = false;
            this.olvCol_Name.Text = "Name";
            // 
            // olvCol_EmailAddress
            // 
            this.olvCol_EmailAddress.AspectName = "EmailAddress";
            this.olvCol_EmailAddress.FillsFreeSpace = true;
            this.olvCol_EmailAddress.Groupable = false;
            this.olvCol_EmailAddress.Text = "EmailAddress";
            this.olvCol_EmailAddress.Width = 123;
            // 
            // SelectIssueSystemUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(896, 744);
            this.Controls.Add(this.btnNewUser);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbUsers);
            this.KeyPreview = true;
            this.Name = "SelectIssueSystemUser";
            this.Text = "SelectIssueSystemUser";
            ((System.ComponentModel.ISupportInitialize)(this.lbUsers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnNewUser;
        private ObjectListView lbUsers;
        private OLVColumn olvCol_ID;
        private OLVColumn olvCol_Name;
        private OLVColumn olvCol_EmailAddress;
    }
}