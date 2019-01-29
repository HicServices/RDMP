using CatalogueManager.LocationsMenu.Ticketing;

namespace CatalogueManager.SimpleDialogs
{
    partial class SupportingDocumentUI
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
            this.label2 = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbUrl = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnOpen = new System.Windows.Forms.Button();
            this.cbExtractable = new System.Windows.Forms.CheckBox();
            this.cbIsGlobal = new System.Windows.Forms.CheckBox();
            this.gbSelectedEntity = new System.Windows.Forms.GroupBox();
            
            this.ticketingControl1 = new CatalogueManager.LocationsMenu.Ticketing.TicketingControl();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.gbSelectedEntity.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "ID:";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(83, 27);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 3;
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(83, 69);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(920, 20);
            this.tbName.TabIndex = 5;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Name:";
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(83, 95);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(920, 358);
            this.tbDescription.TabIndex = 7;
            this.tbDescription.TextChanged += new System.EventHandler(this.tbDescription_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Description:";
            // 
            // tbUrl
            // 
            this.tbUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbUrl.Location = new System.Drawing.Point(83, 459);
            this.tbUrl.Name = "tbUrl";
            this.tbUrl.Size = new System.Drawing.Size(920, 20);
            this.tbUrl.TabIndex = 9;
            this.tbUrl.TextChanged += new System.EventHandler(this.tbUrl_TextChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 459);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "URL:";
            // 
            // btnOpen
            // 
            this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOpen.Location = new System.Drawing.Point(83, 485);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(113, 23);
            this.btnOpen.TabIndex = 12;
            this.btnOpen.Text = "Open File";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // cbExtractable
            // 
            this.cbExtractable.AutoSize = true;
            this.cbExtractable.Location = new System.Drawing.Point(190, 27);
            this.cbExtractable.Name = "cbExtractable";
            this.cbExtractable.Size = new System.Drawing.Size(79, 17);
            this.cbExtractable.TabIndex = 14;
            this.cbExtractable.Text = "Extractable";
            this.cbExtractable.UseVisualStyleBackColor = true;
            this.cbExtractable.CheckedChanged += new System.EventHandler(this.cbExtractable_CheckedChanged);
            // 
            // cbIsGlobal
            // 
            this.cbIsGlobal.AutoSize = true;
            this.cbIsGlobal.Location = new System.Drawing.Point(275, 26);
            this.cbIsGlobal.Name = "cbIsGlobal";
            this.cbIsGlobal.Size = new System.Drawing.Size(64, 17);
            this.cbIsGlobal.TabIndex = 14;
            this.cbIsGlobal.Text = "IsGlobal";
            this.cbIsGlobal.UseVisualStyleBackColor = true;
            this.cbIsGlobal.CheckedChanged += new System.EventHandler(this.cbIsGlobal_CheckedChanged);
            // 
            // gbSelectedEntity
            // 
            this.gbSelectedEntity.Controls.Add(this.ticketingControl1);
            this.gbSelectedEntity.Controls.Add(this.label2);
            this.gbSelectedEntity.Controls.Add(this.tbID);
            this.gbSelectedEntity.Controls.Add(this.label3);
            this.gbSelectedEntity.Controls.Add(this.tbName);
            this.gbSelectedEntity.Controls.Add(this.label4);
            this.gbSelectedEntity.Controls.Add(this.cbIsGlobal);
            this.gbSelectedEntity.Controls.Add(this.tbDescription);
            this.gbSelectedEntity.Controls.Add(this.cbExtractable);
            this.gbSelectedEntity.Controls.Add(this.label5);
            this.gbSelectedEntity.Controls.Add(this.btnBrowse);
            this.gbSelectedEntity.Controls.Add(this.btnOpen);
            this.gbSelectedEntity.Controls.Add(this.tbUrl);
            this.gbSelectedEntity.Location = new System.Drawing.Point(3, 3);
            this.gbSelectedEntity.Name = "gbSelectedEntity";
            this.gbSelectedEntity.Size = new System.Drawing.Size(1009, 514);
            this.gbSelectedEntity.TabIndex = 37;
            this.gbSelectedEntity.TabStop = false;
            this.gbSelectedEntity.Text = "Supporting Document";
            // 
            // ticketingControl1
            // 
            this.ticketingControl1.Location = new System.Drawing.Point(433, 12);
            this.ticketingControl1.Name = "ticketingControl1";
            this.ticketingControl1.Size = new System.Drawing.Size(303, 54);
            this.ticketingControl1.TabIndex = 37;
            this.ticketingControl1.TicketText = "";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(890, 485);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(113, 23);
            this.btnBrowse.TabIndex = 12;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // SupportingDocumentUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbSelectedEntity);
            this.Name = "SupportingDocumentUI";
            this.Size = new System.Drawing.Size(1015, 520);
            this.gbSelectedEntity.ResumeLayout(false);
            this.gbSelectedEntity.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbUrl;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.CheckBox cbExtractable;
        private System.Windows.Forms.CheckBox cbIsGlobal;
        private System.Windows.Forms.GroupBox gbSelectedEntity;
        private TicketingControl ticketingControl1;
        private System.Windows.Forms.Button btnBrowse;
        
    }
}