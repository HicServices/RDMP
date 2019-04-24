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
            this.btnAdd = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tcTicket = new CatalogueManager.LocationsMenu.Ticketing.TicketingControlUI();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbExtractable
            // 
            this.cbExtractable.AutoSize = true;
            this.cbExtractable.Location = new System.Drawing.Point(183, 4);
            this.cbExtractable.Name = "cbExtractable";
            this.cbExtractable.Size = new System.Drawing.Size(79, 17);
            this.cbExtractable.TabIndex = 25;
            this.cbExtractable.Text = "Extractable";
            this.cbExtractable.UseVisualStyleBackColor = true;
            // 
            // tbDescription
            // 
            this.tbDescription.AcceptsReturn = true;
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(76, 53);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbDescription.Size = new System.Drawing.Size(771, 169);
            this.tbDescription.TabIndex = 22;
            this.tbDescription.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbDescription_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Description:";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(76, 27);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(490, 20);
            this.tbName.TabIndex = 20;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Name:";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(76, 4);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 18;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 8);
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
            this.pSQL.Location = new System.Drawing.Point(3, 252);
            this.pSQL.Name = "pSQL";
            this.pSQL.Size = new System.Drawing.Size(844, 168);
            this.pSQL.TabIndex = 26;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 228);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(173, 13);
            this.label5.TabIndex = 27;
            this.label5.Text = "Connection String (Database Only):";
            // 
            // cbGlobal
            // 
            this.cbGlobal.AutoSize = true;
            this.cbGlobal.Location = new System.Drawing.Point(305, 4);
            this.cbGlobal.Name = "cbGlobal";
            this.cbGlobal.Size = new System.Drawing.Size(64, 17);
            this.cbGlobal.TabIndex = 29;
            this.cbGlobal.Text = "IsGlobal";
            this.cbGlobal.UseVisualStyleBackColor = true;
            this.cbGlobal.CheckedChanged += new System.EventHandler(this.cbGlobal_CheckedChanged);
            // 
            // ddExternalServers
            // 
            this.ddExternalServers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddExternalServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExternalServers.FormattingEnabled = true;
            this.ddExternalServers.Location = new System.Drawing.Point(183, 225);
            this.ddExternalServers.Name = "ddExternalServers";
            this.ddExternalServers.Size = new System.Drawing.Size(605, 21);
            this.ddExternalServers.TabIndex = 34;
            this.ddExternalServers.SelectedIndexChanged += new System.EventHandler(this.ddExternalServers_SelectedIndexChanged);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(791, 223);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(59, 23);
            this.btnAdd.TabIndex = 39;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.pSQL);
            this.panel1.Controls.Add(this.btnAdd);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.tcTicket);
            this.panel1.Controls.Add(this.cbGlobal);
            this.panel1.Controls.Add(this.cbExtractable);
            this.panel1.Controls.Add(this.tbID);
            this.panel1.Controls.Add(this.tbDescription);
            this.panel1.Controls.Add(this.ddExternalServers);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.tbName);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(853, 423);
            this.panel1.TabIndex = 40;
            // 
            // tcTicket
            // 
            this.tcTicket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tcTicket.Location = new System.Drawing.Point(572, -3);
            this.tcTicket.Name = "tcTicket";
            this.tcTicket.Size = new System.Drawing.Size(275, 54);
            this.tcTicket.TabIndex = 37;
            this.tcTicket.TicketText = "";
            // 
            // SupportingSQLTableUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "SupportingSQLTableUI";
            this.Size = new System.Drawing.Size(853, 423);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private TicketingControlUI tcTicket;
        
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Panel panel1;
    }
}