using CatalogueManager.LocationsMenu.Ticketing;

namespace CatalogueManager.SimpleDialogs
{
    partial class SupportingSQLTableUI
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
            this.cbExtractable = new System.Windows.Forms.CheckBox();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pSQL = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.cbGlobal = new System.Windows.Forms.CheckBox();
            this.ddExternalServers = new System.Windows.Forms.ComboBox();
            this.gbSelectedEntity = new System.Windows.Forms.GroupBox();
            
            this.tcTicket = new CatalogueManager.LocationsMenu.Ticketing.TicketingControl();
            this.btnAdd = new System.Windows.Forms.Button();
            this.gbSelectedEntity.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbExtractable
            // 
            this.cbExtractable.AutoSize = true;
            this.cbExtractable.Location = new System.Drawing.Point(199, 25);
            this.cbExtractable.Name = "cbExtractable";
            this.cbExtractable.Size = new System.Drawing.Size(79, 17);
            this.cbExtractable.TabIndex = 25;
            this.cbExtractable.Text = "Extractable";
            this.cbExtractable.UseVisualStyleBackColor = true;
            this.cbExtractable.CheckedChanged += new System.EventHandler(this.cbExtractable_CheckedChanged);
            // 
            // tbDescription
            // 
            this.tbDescription.AcceptsReturn = true;
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(92, 93);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbDescription.Size = new System.Drawing.Size(1156, 150);
            this.tbDescription.TabIndex = 22;
            this.tbDescription.TextChanged += new System.EventHandler(this.tbDescription_TextChanged);
            this.tbDescription.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbDescription_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Description:";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(92, 67);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(1156, 20);
            this.tbName.TabIndex = 20;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Name:";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(92, 25);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 18;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "ID:";
            // 
            // pSQL
            // 
            this.pSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pSQL.Location = new System.Drawing.Point(26, 305);
            this.pSQL.Name = "pSQL";
            this.pSQL.Size = new System.Drawing.Size(1222, 332);
            this.pSQL.TabIndex = 26;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 249);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(173, 13);
            this.label5.TabIndex = 27;
            this.label5.Text = "Connection String (Database Only):";
            // 
            // cbGlobal
            // 
            this.cbGlobal.AutoSize = true;
            this.cbGlobal.Location = new System.Drawing.Point(284, 24);
            this.cbGlobal.Name = "cbGlobal";
            this.cbGlobal.Size = new System.Drawing.Size(225, 17);
            this.cbGlobal.TabIndex = 29;
            this.cbGlobal.Text = "IsGlobal (Shared with all other Catalogues)";
            this.cbGlobal.UseVisualStyleBackColor = true;
            this.cbGlobal.CheckedChanged += new System.EventHandler(this.cbGlobal_CheckedChanged);
            // 
            // ddExternalServers
            // 
            this.ddExternalServers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddExternalServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExternalServers.FormattingEnabled = true;
            this.ddExternalServers.Location = new System.Drawing.Point(199, 246);
            this.ddExternalServers.Name = "ddExternalServers";
            this.ddExternalServers.Size = new System.Drawing.Size(990, 21);
            this.ddExternalServers.TabIndex = 34;
            this.ddExternalServers.SelectedIndexChanged += new System.EventHandler(this.ddExternalServers_SelectedIndexChanged);
            // 
            // gbSelectedEntity
            // 
            this.gbSelectedEntity.Controls.Add(this.btnAdd);
            this.gbSelectedEntity.Controls.Add(this.tcTicket);
            this.gbSelectedEntity.Controls.Add(this.label2);
            this.gbSelectedEntity.Controls.Add(this.tbID);
            this.gbSelectedEntity.Controls.Add(this.ddExternalServers);
            this.gbSelectedEntity.Controls.Add(this.label3);
            this.gbSelectedEntity.Controls.Add(this.tbName);
            this.gbSelectedEntity.Controls.Add(this.label4);
            this.gbSelectedEntity.Controls.Add(this.tbDescription);
            this.gbSelectedEntity.Controls.Add(this.cbExtractable);
            this.gbSelectedEntity.Controls.Add(this.cbGlobal);
            this.gbSelectedEntity.Controls.Add(this.pSQL);
            this.gbSelectedEntity.Controls.Add(this.label5);
            this.gbSelectedEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbSelectedEntity.Location = new System.Drawing.Point(0, 0);
            this.gbSelectedEntity.Name = "gbSelectedEntity";
            this.gbSelectedEntity.Size = new System.Drawing.Size(1254, 672);
            this.gbSelectedEntity.TabIndex = 36;
            this.gbSelectedEntity.TabStop = false;
            // 
            // tcTicket
            // 
            this.tcTicket.Location = new System.Drawing.Point(515, 12);
            this.tcTicket.Name = "tcTicket";
            this.tcTicket.Size = new System.Drawing.Size(303, 54);
            this.tcTicket.TabIndex = 37;
            this.tcTicket.TicketText = "";
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(1192, 244);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(59, 23);
            this.btnAdd.TabIndex = 39;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // SupportingSQLTableUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbSelectedEntity);
            this.Name = "SupportingSQLTableUI";
            this.Size = new System.Drawing.Size(1254, 672);
            this.gbSelectedEntity.ResumeLayout(false);
            this.gbSelectedEntity.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox cbExtractable;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel pSQL;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox cbGlobal;
        private System.Windows.Forms.ComboBox ddExternalServers;
        private System.Windows.Forms.GroupBox gbSelectedEntity;
        private TicketingControl tcTicket;
        
        private System.Windows.Forms.Button btnAdd;
    }
}