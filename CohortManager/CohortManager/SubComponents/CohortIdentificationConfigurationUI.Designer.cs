using CatalogueManager.AggregationUIs;
using CatalogueManager.Collections;
using CatalogueManager.LocationsMenu.Ticketing;
using CatalogueManager.MainFormUITabs.SubComponents;
using CatalogueManager.SimpleControls;

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
            this.btnConfigureGlobalParameters = new System.Windows.Forms.Button();
            this.ticket = new CatalogueManager.LocationsMenu.Ticketing.TicketingControl();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSave = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.SuspendLayout();
            // 
            // btnConfigureGlobalParameters
            // 
            this.btnConfigureGlobalParameters.Location = new System.Drawing.Point(145, 56);
            this.btnConfigureGlobalParameters.Name = "btnConfigureGlobalParameters";
            this.btnConfigureGlobalParameters.Size = new System.Drawing.Size(177, 23);
            this.btnConfigureGlobalParameters.TabIndex = 46;
            this.btnConfigureGlobalParameters.Text = "Configure Global Parameters";
            this.btnConfigureGlobalParameters.UseVisualStyleBackColor = true;
            this.btnConfigureGlobalParameters.Click += new System.EventHandler(this.btnConfigureGlobalParameters_Click);
            // 
            // ticket
            // 
            this.ticket.Location = new System.Drawing.Point(328, 56);
            this.ticket.Name = "ticket";
            this.ticket.Size = new System.Drawing.Size(314, 62);
            this.ticket.TabIndex = 1;
            this.ticket.TicketText = "";
            this.ticket.TicketTextChanged += new System.EventHandler(this.ticket_TicketTextChanged);
            // 
            // tbDescription
            // 
            this.tbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDescription.Location = new System.Drawing.Point(77, 124);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(672, 702);
            this.tbDescription.TabIndex = 1;
            this.tbDescription.TextChanged += new System.EventHandler(this.tbDescription_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 124);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Description";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(64, 30);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(685, 20);
            this.tbName.TabIndex = 1;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name";
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(64, 4);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(100, 20);
            this.tbID.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "ID";
            // 
            // btnSave
            // 
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(64, 53);
            this.btnSave.Margin = new System.Windows.Forms.Padding(0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 26);
            this.btnSave.TabIndex = 50;
            // 
            // CohortIdentificationConfigurationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ticket);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbDescription);
            this.Controls.Add(this.btnConfigureGlobalParameters);
            this.Controls.Add(this.tbName);
            this.Name = "CohortIdentificationConfigurationUI";
            this.Size = new System.Drawing.Size(752, 829);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label1;
        private TicketingControl ticket;
        private System.Windows.Forms.Button btnConfigureGlobalParameters;
        private ObjectSaverButton btnSave;
    }
}
