using ReusableUIComponents;

namespace CatalogueManager.MainFormUITabs.SubComponents
{
    partial class ImportSQLTable
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
            this.btnImport = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ddContext = new System.Windows.Forms.ComboBox();
            this.lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue = new System.Windows.Forms.Label();
            this.serverDatabaseTableSelector1 = new ReusableUIComponents.ServerDatabaseTableSelector();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnImport
            // 
            this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnImport.Enabled = false;
            this.btnImport.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnImport.Location = new System.Drawing.Point(328, 201);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(131, 30);
            this.btnImport.TabIndex = 158;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.ddContext);
            this.groupBox1.Controls.Add(this.lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue);
            this.groupBox1.Controls.Add(this.serverDatabaseTableSelector1);
            this.groupBox1.Controls.Add(this.btnImport);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(654, 245);
            this.groupBox1.TabIndex = 159;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Import SQL Table";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 213);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 13);
            this.label1.TabIndex = 162;
            this.label1.Text = "Save Credentials For Use In:";
            // 
            // ddContext
            // 
            this.ddContext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ddContext.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddContext.Enabled = false;
            this.ddContext.FormattingEnabled = true;
            this.ddContext.Location = new System.Drawing.Point(155, 210);
            this.ddContext.Name = "ddContext";
            this.ddContext.Size = new System.Drawing.Size(167, 21);
            this.ddContext.TabIndex = 161;
            this.ddContext.SelectedIndexChanged += new System.EventHandler(this.ddContext_SelectedIndexChanged);
            // 
            // lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue
            // 
            this.lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue.ForeColor = System.Drawing.Color.Red;
            this.lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue.Location = new System.Drawing.Point(6, 230);
            this.lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue.Name = "lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue";
            this.lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue.Size = new System.Drawing.Size(627, 19);
            this.lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue.TabIndex = 160;
            this.lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue.Text = "Encrypted username and password will be stored in the Catalogue database.";
            this.lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue.Visible = false;
            // 
            // serverDatabaseTableSelector1
            // 
            this.serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = false;
            this.serverDatabaseTableSelector1.AutoSize = true;
            this.serverDatabaseTableSelector1.Database = "";
            this.serverDatabaseTableSelector1.Dock = System.Windows.Forms.DockStyle.Top;
            this.serverDatabaseTableSelector1.Location = new System.Drawing.Point(3, 16);
            this.serverDatabaseTableSelector1.Name = "serverDatabaseTableSelector1";
            this.serverDatabaseTableSelector1.Password = "";
            this.serverDatabaseTableSelector1.Server = "";
            this.serverDatabaseTableSelector1.Size = new System.Drawing.Size(648, 146);
            this.serverDatabaseTableSelector1.TabIndex = 159;
            this.serverDatabaseTableSelector1.Table = "";
            this.serverDatabaseTableSelector1.Username = "";
            this.serverDatabaseTableSelector1.IntegratedSecurityUseChanged += new ReusableUIComponents.IntegratedSecurityUseChangedHandler(this.serverDatabaseTableSelector1_IntegratedSecurityUseChanged);
            // 
            // ImportSQLTable
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(675, 270);
            this.Controls.Add(this.groupBox1);
            this.Name = "ImportSQLTable";
            this.Text = "Import SQL Table";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.GroupBox groupBox1;
        private ServerDatabaseTableSelector serverDatabaseTableSelector1;
        private System.Windows.Forms.Label lblWarningAboutToSaveUsernameAndPasswordIntoCatalogue;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddContext;

    }
}
