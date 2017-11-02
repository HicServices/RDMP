using CatalogueManager.AggregationUIs;

namespace CohortManager.SubComponents
{
    partial class CohortIdentificationConfigurationUI
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
            this.label1 = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.tbName = new System.Windows.Forms.TextBox();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.ticket = new CatalogueManager.LocationsMenu.Ticketing.TicketingControl();
            this.queryCachingServerSelector = new CatalogueManager.AggregationUIs.QueryCachingServerSelector();
            this.CohortCompilerUI1 = new CohortManager.SubComponents.CohortCompilerUI();
            this.btnCollapseOrExpand = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(61, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 13);
            this.label1.TabIndex = 51;
            this.label1.Text = "ID";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(85, 6);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 54;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(44, 35);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(35, 13);
            this.lblName.TabIndex = 52;
            this.lblName.Text = "Name";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(19, 61);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(60, 13);
            this.lblDescription.TabIndex = 53;
            this.lblDescription.Text = "Description";
            // 
            // tbDescription
            // 
            this.tbDescription.Location = new System.Drawing.Point(85, 58);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(1142, 80);
            this.tbDescription.TabIndex = 56;
            this.tbDescription.TextChanged += new System.EventHandler(this.tbDescription_TextChanged);
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(85, 32);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(822, 20);
            this.tbName.TabIndex = 57;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(85, 142);
            this.objectSaverButton1.Margin = new System.Windows.Forms.Padding(0);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(57, 26);
            this.objectSaverButton1.TabIndex = 58;
            // 
            // ticket
            // 
            this.ticket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ticket.Location = new System.Drawing.Point(913, 0);
            this.ticket.Name = "ticket";
            this.ticket.Size = new System.Drawing.Size(314, 62);
            this.ticket.TabIndex = 55;
            this.ticket.TicketText = "";
            this.ticket.TicketTextChanged += new System.EventHandler(this.ticket_TicketTextChanged);
            // 
            // queryCachingServerSelector
            // 
            this.queryCachingServerSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.queryCachingServerSelector.AutoSize = true;
            this.queryCachingServerSelector.Location = new System.Drawing.Point(557, 601);
            this.queryCachingServerSelector.Name = "queryCachingServerSelector";
            this.queryCachingServerSelector.SelecteExternalDatabaseServer = null;
            this.queryCachingServerSelector.Size = new System.Drawing.Size(683, 93);
            this.queryCachingServerSelector.TabIndex = 1;
            // 
            // CohortCompilerUI1
            // 
            this.CohortCompilerUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CohortCompilerUI1.CoreIconProvider = null;
            this.CohortCompilerUI1.Location = new System.Drawing.Point(3, 180);
            this.CohortCompilerUI1.Name = "CohortCompilerUI1";
            this.CohortCompilerUI1.Size = new System.Drawing.Size(1237, 511);
            this.CohortCompilerUI1.TabIndex = 0;
            // 
            // btnCollapseOrExpand
            // 
            this.btnCollapseOrExpand.Location = new System.Drawing.Point(39, 4);
            this.btnCollapseOrExpand.Name = "btnCollapseOrExpand";
            this.btnCollapseOrExpand.Size = new System.Drawing.Size(16, 22);
            this.btnCollapseOrExpand.TabIndex = 59;
            this.btnCollapseOrExpand.Text = "-";
            this.btnCollapseOrExpand.UseVisualStyleBackColor = true;
            this.btnCollapseOrExpand.Click += new System.EventHandler(this.btnCollapseOrExpand_Click);
            // 
            // CohortIdentificationConfigurationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCollapseOrExpand);
            this.Controls.Add(this.tbDescription);
            this.Controls.Add(this.objectSaverButton1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.ticket);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.queryCachingServerSelector);
            this.Controls.Add(this.CohortCompilerUI1);
            this.Name = "CohortIdentificationConfigurationUI";
            this.Size = new System.Drawing.Size(1243, 694);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CohortCompilerUI CohortCompilerUI1;
        private QueryCachingServerSelector queryCachingServerSelector;
        private CatalogueManager.SimpleControls.ObjectSaverButton objectSaverButton1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label lblName;
        private CatalogueManager.LocationsMenu.Ticketing.TicketingControl ticket;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Button btnCollapseOrExpand;
    }
}
