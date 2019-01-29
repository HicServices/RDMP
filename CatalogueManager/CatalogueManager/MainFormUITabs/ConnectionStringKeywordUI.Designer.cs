namespace CatalogueManager.MainFormUITabs
{
    partial class ConnectionStringKeywordUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionStringKeywordUI));
            this.label1 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.ddDatabaseType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbValue = new System.Windows.Forms.TextBox();
            this.pbDatabaseProvider = new System.Windows.Forms.PictureBox();
            this.ragSmiley = new ReusableUIComponents.ChecksUI.RAGSmiley();
            
            this.label4 = new System.Windows.Forms.Label();
            this.lblID = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.tbCommandToDelete = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbDatabaseProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Keyword Name:";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(89, 35);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(212, 20);
            this.tbName.TabIndex = 2;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // ddDatabaseType
            // 
            this.ddDatabaseType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDatabaseType.FormattingEnabled = true;
            this.ddDatabaseType.Location = new System.Drawing.Point(89, 61);
            this.ddDatabaseType.Name = "ddDatabaseType";
            this.ddDatabaseType.Size = new System.Drawing.Size(212, 21);
            this.ddDatabaseType.TabIndex = 3;
            this.ddDatabaseType.SelectedIndexChanged += new System.EventHandler(this.ddDatabaseType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Database Type:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(46, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Value:";
            // 
            // tbValue
            // 
            this.tbValue.Location = new System.Drawing.Point(89, 88);
            this.tbValue.Name = "tbValue";
            this.tbValue.Size = new System.Drawing.Size(212, 20);
            this.tbValue.TabIndex = 2;
            this.tbValue.TextChanged += new System.EventHandler(this.tbValue_TextChanged);
            // 
            // pbDatabaseProvider
            // 
            this.pbDatabaseProvider.Location = new System.Drawing.Point(307, 63);
            this.pbDatabaseProvider.Name = "pbDatabaseProvider";
            this.pbDatabaseProvider.Size = new System.Drawing.Size(19, 19);
            this.pbDatabaseProvider.TabIndex = 171;
            this.pbDatabaseProvider.TabStop = false;
            // 
            // ragSmiley
            // 
            this.ragSmiley.AlwaysShowHandCursor = false;
            this.ragSmiley.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley.Location = new System.Drawing.Point(343, 61);
            this.ragSmiley.Name = "ragSmiley";
            this.ragSmiley.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.ForeColor = System.Drawing.Color.Red;
            this.label4.Location = new System.Drawing.Point(3, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(780, 81);
            this.label4.TabIndex = 172;
            this.label4.Text = resources.GetString("label4.Text");
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Location = new System.Drawing.Point(62, 12);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(21, 13);
            this.lblID.TabIndex = 172;
            this.lblID.Text = "ID:";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(89, 9);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(114, 20);
            this.tbID.TabIndex = 2;
            this.tbID.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // tbCommandToDelete
            // 
            this.tbCommandToDelete.Location = new System.Drawing.Point(7, 195);
            this.tbCommandToDelete.Name = "tbCommandToDelete";
            this.tbCommandToDelete.ReadOnly = true;
            this.tbCommandToDelete.Size = new System.Drawing.Size(764, 20);
            this.tbCommandToDelete.TabIndex = 173;
            // 
            // ConnectionStringKeywordUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbCommandToDelete);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pbDatabaseProvider);
            
            this.Controls.Add(this.ddDatabaseType);
            this.Controls.Add(this.tbValue);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ragSmiley);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ConnectionStringKeywordUI";
            this.Size = new System.Drawing.Size(786, 502);
            ((System.ComponentModel.ISupportInitialize)(this.pbDatabaseProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.ComboBox ddDatabaseType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbValue;
        
        private System.Windows.Forms.PictureBox pbDatabaseProvider;
        private ReusableUIComponents.ChecksUI.RAGSmiley ragSmiley;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.TextBox tbCommandToDelete;
    }
}
