namespace CatalogueManager.SimpleDialogs.Reports
{
    partial class WordAccessRightsByUserUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WordAccessRightsByUserUI));
            this.label1 = new System.Windows.Forms.Label();
            this.btnDisplayPrerequisites = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.serverDatabaseTableSelector1 = new ReusableUIComponents.ServerDatabaseTableSelector();
            this.btnGeneratePerUser = new System.Windows.Forms.Button();
            this.btnGeneratePerDatabase = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbCurrentUsersOnly = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(813, 38);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // btnDisplayPrerequisites
            // 
            this.btnDisplayPrerequisites.Location = new System.Drawing.Point(12, 50);
            this.btnDisplayPrerequisites.Name = "btnDisplayPrerequisites";
            this.btnDisplayPrerequisites.Size = new System.Drawing.Size(209, 22);
            this.btnDisplayPrerequisites.TabIndex = 0;
            this.btnDisplayPrerequisites.Text = "Display Prerequisites SQL...";
            this.btnDisplayPrerequisites.UseVisualStyleBackColor = true;
            this.btnDisplayPrerequisites.Click += new System.EventHandler(this.btnDisplayPrerequisites_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 139);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(352, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "3. Once you have run this stored procedure you can generate the report";
            // 
            // serverDatabaseTableSelector1
            // 
            this.serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = false;
            this.serverDatabaseTableSelector1.AutoSize = true;
            this.serverDatabaseTableSelector1.Database = "";
            this.serverDatabaseTableSelector1.Location = new System.Drawing.Point(12, 155);
            this.serverDatabaseTableSelector1.Name = "serverDatabaseTableSelector1";
            this.serverDatabaseTableSelector1.Password = "";
            this.serverDatabaseTableSelector1.Server = "";
            this.serverDatabaseTableSelector1.Size = new System.Drawing.Size(457, 143);
            this.serverDatabaseTableSelector1.TabIndex = 1;
            this.serverDatabaseTableSelector1.Table = "";
            this.serverDatabaseTableSelector1.Username = "";
            // 
            // btnGeneratePerUser
            // 
            this.btnGeneratePerUser.Location = new System.Drawing.Point(102, 301);
            this.btnGeneratePerUser.Name = "btnGeneratePerUser";
            this.btnGeneratePerUser.Size = new System.Drawing.Size(145, 23);
            this.btnGeneratePerUser.TabIndex = 2;
            this.btnGeneratePerUser.Text = "Generate Per User";
            this.btnGeneratePerUser.UseVisualStyleBackColor = true;
            this.btnGeneratePerUser.Click += new System.EventHandler(this.btnGeneratePerUser_Click);
            // 
            // btnGeneratePerDatabase
            // 
            this.btnGeneratePerDatabase.Location = new System.Drawing.Point(303, 301);
            this.btnGeneratePerDatabase.Name = "btnGeneratePerDatabase";
            this.btnGeneratePerDatabase.Size = new System.Drawing.Size(145, 23);
            this.btnGeneratePerDatabase.TabIndex = 3;
            this.btnGeneratePerDatabase.Text = "Generate Per Database";
            this.btnGeneratePerDatabase.UseVisualStyleBackColor = true;
            this.btnGeneratePerDatabase.Click += new System.EventHandler(this.btnGeneratePerDatabase_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.Location = new System.Drawing.Point(12, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(813, 21);
            this.label3.TabIndex = 0;
            this.label3.Text = "2. You will then need to run (ideally through a daily scheduled agent job) the st" +
    "ored procedure:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(38, 109);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(410, 20);
            this.textBox1.TabIndex = 4;
            this.textBox1.Text = "exec Audit.dbo.UpdatePrivilegesAudit";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 271);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(779, 18);
            this.label4.TabIndex = 5;
            this.label4.Text = "(Default database name is \'Audit\' but if you have edited the prerequisites SQL to" +
    " name it something else then type that name in here)";
            // 
            // cbCurrentUsersOnly
            // 
            this.cbCurrentUsersOnly.AutoSize = true;
            this.cbCurrentUsersOnly.Location = new System.Drawing.Point(114, 330);
            this.cbCurrentUsersOnly.Name = "cbCurrentUsersOnly";
            this.cbCurrentUsersOnly.Size = new System.Drawing.Size(114, 17);
            this.cbCurrentUsersOnly.TabIndex = 6;
            this.cbCurrentUsersOnly.Text = "Current Users Only";
            this.cbCurrentUsersOnly.UseVisualStyleBackColor = true;
            // 
            // ConfigureAccessRightsReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(829, 549);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbCurrentUsersOnly);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnGeneratePerDatabase);
            this.Controls.Add(this.btnGeneratePerUser);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.serverDatabaseTableSelector1);
            this.Controls.Add(this.btnDisplayPrerequisites);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Name = "WordAccessRightsByUserUI";
            this.Text = "Database Access Report";
            this.Load += new System.EventHandler(this.ConfigureAccessRightsReport_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDisplayPrerequisites;
        private System.Windows.Forms.Label label2;
        private ReusableUIComponents.ServerDatabaseTableSelector serverDatabaseTableSelector1;
        private System.Windows.Forms.Button btnGeneratePerUser;
        private System.Windows.Forms.Button btnGeneratePerDatabase;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbCurrentUsersOnly;
    }
}