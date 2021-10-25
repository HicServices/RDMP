using BrightIdeasSoftware;
using Rdmp.UI.LocationsMenu.Ticketing;

namespace Rdmp.UI.SimpleDialogs.Governance
{
    partial class GovernancePeriodUI
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
            this.label3 = new System.Windows.Forms.Label();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.rbNeverExpires = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.rbExpiresOn = new System.Windows.Forms.RadioButton();
            this.tbName = new System.Windows.Forms.TextBox();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.olvCatalogues = new BrightIdeasSoftware.ObjectListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnAddCatalogue = new System.Windows.Forms.Button();
            this.btnImportCatalogues = new System.Windows.Forms.Button();
            this.lblExpired = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ticketingControl1 = new TicketingControlUI();
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogues)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 233);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Start Date:";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Location = new System.Drawing.Point(78, 227);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(159, 20);
            this.dtpStartDate.TabIndex = 4;
            this.dtpStartDate.ValueChanged += new System.EventHandler(this.dtpStartDate_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 255);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "End Date:";
            // 
            // rbNeverExpires
            // 
            this.rbNeverExpires.AutoSize = true;
            this.rbNeverExpires.Location = new System.Drawing.Point(78, 255);
            this.rbNeverExpires.Name = "rbNeverExpires";
            this.rbNeverExpires.Size = new System.Drawing.Size(91, 17);
            this.rbNeverExpires.TabIndex = 6;
            this.rbNeverExpires.TabStop = true;
            this.rbNeverExpires.Text = "Never Expires";
            this.rbNeverExpires.UseVisualStyleBackColor = true;
            this.rbNeverExpires.CheckedChanged += new System.EventHandler(this.rbNeverExpires_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Description:";
            // 
            // rbExpiresOn
            // 
            this.rbExpiresOn.AutoSize = true;
            this.rbExpiresOn.Location = new System.Drawing.Point(78, 278);
            this.rbExpiresOn.Name = "rbExpiresOn";
            this.rbExpiresOn.Size = new System.Drawing.Size(79, 17);
            this.rbExpiresOn.TabIndex = 6;
            this.rbExpiresOn.TabStop = true;
            this.rbExpiresOn.Text = "Expires On:";
            this.rbExpiresOn.UseVisualStyleBackColor = true;
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(78, 29);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(444, 20);
            this.tbName.TabIndex = 1;
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.Location = new System.Drawing.Point(163, 275);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(159, 20);
            this.dtpEndDate.TabIndex = 7;
            this.dtpEndDate.ValueChanged += new System.EventHandler(this.dtpEndDate_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // olvCatalogues
            // 
            this.olvCatalogues.AllColumns.Add(this.olvName);
            this.olvCatalogues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvCatalogues.CellEditUseWholeCell = false;
            this.olvCatalogues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.olvCatalogues.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvCatalogues.Location = new System.Drawing.Point(390, 183);
            this.olvCatalogues.Name = "olvCatalogues";
            this.olvCatalogues.RowHeight = 19;
            this.olvCatalogues.ShowGroups = false;
            this.olvCatalogues.Size = new System.Drawing.Size(656, 242);
            this.olvCatalogues.TabIndex = 9;
            this.olvCatalogues.UseCompatibleStateImageBehavior = false;
            this.olvCatalogues.View = System.Windows.Forms.View.Details;
            this.olvCatalogues.ItemActivate += new System.EventHandler(this.olvCatalogues_ItemActivate);
            this.olvCatalogues.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbCatalogues_KeyUp);
            // 
            // olvName
            // 
            this.olvName.AspectName = "Name";
            this.olvName.Text = "Name";
            this.olvName.MinimumWidth = 100;
            // 
            // tbDescription
            // 
            this.tbDescription.AcceptsReturn = true;
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(78, 55);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(997, 106);
            this.tbDescription.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(387, 167);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(234, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Period relates to the Governance of Catalogues:";
            // 
            // btnAddCatalogue
            // 
            this.btnAddCatalogue.Location = new System.Drawing.Point(387, 455);
            this.btnAddCatalogue.Name = "btnAddCatalogue";
            this.btnAddCatalogue.Size = new System.Drawing.Size(98, 23);
            this.btnAddCatalogue.TabIndex = 11;
            this.btnAddCatalogue.Text = "Add Catalogue(s)";
            this.btnAddCatalogue.UseVisualStyleBackColor = true;
            this.btnAddCatalogue.Click += new System.EventHandler(this.btnAddCatalogue_Click);
            // 
            // btnImportCatalogues
            // 
            this.btnImportCatalogues.Location = new System.Drawing.Point(491, 455);
            this.btnImportCatalogues.Name = "btnImportCatalogues";
            this.btnImportCatalogues.Size = new System.Drawing.Size(282, 23);
            this.btnImportCatalogues.TabIndex = 11;
            this.btnImportCatalogues.Text = "Import Catalogue List From Another Governance Period";
            this.btnImportCatalogues.UseVisualStyleBackColor = true;
            this.btnImportCatalogues.Click += new System.EventHandler(this.btnImportCatalogues_Click);
            // 
            // lblExpired
            // 
            this.lblExpired.BackColor = System.Drawing.Color.MistyRose;
            this.lblExpired.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblExpired.Location = new System.Drawing.Point(0, 0);
            this.lblExpired.Name = "lblExpired";
            this.lblExpired.Size = new System.Drawing.Size(1078, 13);
            this.lblExpired.TabIndex = 20;
            this.lblExpired.Text = "Period Expired";
            this.lblExpired.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.tbFilter);
            this.panel1.Controls.Add(this.tbID);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.tbName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnImportCatalogues);
            this.panel1.Controls.Add(this.btnAddCatalogue);
            this.panel1.Controls.Add(this.tbDescription);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.olvCatalogues);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.ticketingControl1);
            this.panel1.Controls.Add(this.dtpStartDate);
            this.panel1.Controls.Add(this.rbExpiresOn);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.rbNeverExpires);
            this.panel1.Controls.Add(this.dtpEndDate);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1078, 481);
            this.panel1.TabIndex = 21;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(392, 432);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(430, 429);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(616, 20);
            this.tbFilter.TabIndex = 21;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(78, 3);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(121, 20);
            this.tbID.TabIndex = 20;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(45, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(21, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "ID:";
            // 
            // ticketingControl1
            // 
            this.ticketingControl1.Location = new System.Drawing.Point(78, 167);
            this.ticketingControl1.Name = "ticketingControl1";
            this.ticketingControl1.Size = new System.Drawing.Size(303, 54);
            this.ticketingControl1.TabIndex = 3;
            this.ticketingControl1.TicketText = "";
            // 
            // GovernancePeriodUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblExpired);
            this.Name = "GovernancePeriodUI";
            this.Size = new System.Drawing.Size(1078, 494);
            ((System.ComponentModel.ISupportInitialize)(this.olvCatalogues)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnImportCatalogues;
        private System.Windows.Forms.Button btnAddCatalogue;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbDescription;
        private ObjectListView olvCatalogues;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.RadioButton rbExpiresOn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rbNeverExpires;
        private TicketingControlUI ticketingControl1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.Label label3;
        
        private System.Windows.Forms.Label lblExpired;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label6;
        private OLVColumn olvName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbFilter;
    }
}
