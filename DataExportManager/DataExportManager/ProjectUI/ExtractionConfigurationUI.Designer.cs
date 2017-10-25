using CatalogueManager.LocationsMenu.Ticketing;

namespace DataExportManager.ProjectUI
{
    partial class ExtractionConfigurationUI
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
            this.label4 = new System.Windows.Forms.Label();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.tbCreated = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnConfigureGlobalParameters = new System.Windows.Forms.Button();
            this.tcRelease = new CatalogueManager.LocationsMenu.Ticketing.TicketingControl();
            this.tcRequest = new CatalogueManager.LocationsMenu.Ticketing.TicketingControl();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.pChooseExtractionPipeline = new System.Windows.Forms.Panel();
            this.gbCohortRefreshing = new System.Windows.Forms.GroupBox();
            this.pChooseCohortRefreshPipeline = new System.Windows.Forms.Panel();
            this.btnClearCic = new System.Windows.Forms.Button();
            this.pbCic = new System.Windows.Forms.PictureBox();
            this.cbxCohortIdentificationConfiguration = new ReusableUIComponents.SuggestComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.label2 = new System.Windows.Forms.Label();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.gbCohortRefreshing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCic)).BeginInit();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(137, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Username:";
            // 
            // tbUsername
            // 
            this.tbUsername.Location = new System.Drawing.Point(201, 6);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.ReadOnly = true;
            this.tbUsername.Size = new System.Drawing.Size(158, 20);
            this.tbUsername.TabIndex = 7;
            // 
            // tbCreated
            // 
            this.tbCreated.Location = new System.Drawing.Point(412, 5);
            this.tbCreated.Name = "tbCreated";
            this.tbCreated.ReadOnly = true;
            this.tbCreated.Size = new System.Drawing.Size(158, 20);
            this.tbCreated.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(364, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Created:";
            // 
            // btnConfigureGlobalParameters
            // 
            this.btnConfigureGlobalParameters.Location = new System.Drawing.Point(578, 4);
            this.btnConfigureGlobalParameters.Name = "btnConfigureGlobalParameters";
            this.btnConfigureGlobalParameters.Size = new System.Drawing.Size(177, 23);
            this.btnConfigureGlobalParameters.TabIndex = 45;
            this.btnConfigureGlobalParameters.Text = "Configure Global Parameters";
            this.btnConfigureGlobalParameters.UseVisualStyleBackColor = true;
            this.btnConfigureGlobalParameters.Click += new System.EventHandler(this.btnConfigureGlobalParameters_Click);
            // 
            // tcRelease
            // 
            this.tcRelease.AutoSize = true;
            this.tcRelease.Location = new System.Drawing.Point(317, 42);
            this.tcRelease.Name = "tcRelease";
            this.tcRelease.Size = new System.Drawing.Size(300, 52);
            this.tcRelease.TabIndex = 46;
            this.tcRelease.TicketText = "";
            // 
            // tcRequest
            // 
            this.tcRequest.AutoSize = true;
            this.tcRequest.Location = new System.Drawing.Point(11, 42);
            this.tcRequest.Name = "tcRequest";
            this.tcRequest.Size = new System.Drawing.Size(300, 52);
            this.tcRequest.TabIndex = 46;
            this.tcRequest.TicketText = "";
            this.tcRequest.Load += new System.EventHandler(this.tcRequest_Load);
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(35, 6);
            this.tbID.Name = "tbID";
            this.tbID.ReadOnly = true;
            this.tbID.Size = new System.Drawing.Size(96, 20);
            this.tbID.TabIndex = 48;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(21, 13);
            this.label6.TabIndex = 47;
            this.label6.Text = "ID:";
            // 
            // pChooseExtractionPipeline
            // 
            this.pChooseExtractionPipeline.Location = new System.Drawing.Point(3, 257);
            this.pChooseExtractionPipeline.Name = "pChooseExtractionPipeline";
            this.pChooseExtractionPipeline.Size = new System.Drawing.Size(606, 201);
            this.pChooseExtractionPipeline.TabIndex = 49;
            // 
            // gbCohortRefreshing
            // 
            this.gbCohortRefreshing.Controls.Add(this.pChooseCohortRefreshPipeline);
            this.gbCohortRefreshing.Controls.Add(this.btnClearCic);
            this.gbCohortRefreshing.Controls.Add(this.pbCic);
            this.gbCohortRefreshing.Controls.Add(this.cbxCohortIdentificationConfiguration);
            this.gbCohortRefreshing.Controls.Add(this.label1);
            this.gbCohortRefreshing.Location = new System.Drawing.Point(3, 464);
            this.gbCohortRefreshing.Name = "gbCohortRefreshing";
            this.gbCohortRefreshing.Size = new System.Drawing.Size(714, 346);
            this.gbCohortRefreshing.TabIndex = 50;
            this.gbCohortRefreshing.TabStop = false;
            this.gbCohortRefreshing.Text = "Cohort Refreshing";
            // 
            // pChooseCohortRefreshPipeline
            // 
            this.pChooseCohortRefreshPipeline.Location = new System.Drawing.Point(17, 55);
            this.pChooseCohortRefreshPipeline.Name = "pChooseCohortRefreshPipeline";
            this.pChooseCohortRefreshPipeline.Size = new System.Drawing.Size(606, 201);
            this.pChooseCohortRefreshPipeline.TabIndex = 50;
            // 
            // btnClearCic
            // 
            this.btnClearCic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearCic.Location = new System.Drawing.Point(629, 15);
            this.btnClearCic.Name = "btnClearCic";
            this.btnClearCic.Size = new System.Drawing.Size(41, 23);
            this.btnClearCic.TabIndex = 4;
            this.btnClearCic.Text = "clear";
            this.btnClearCic.UseVisualStyleBackColor = true;
            this.btnClearCic.Click += new System.EventHandler(this.btnClearCic_Click);
            // 
            // pbCic
            // 
            this.pbCic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbCic.Location = new System.Drawing.Point(676, 17);
            this.pbCic.Name = "pbCic";
            this.pbCic.Size = new System.Drawing.Size(21, 21);
            this.pbCic.TabIndex = 3;
            this.pbCic.TabStop = false;
            // 
            // cbxCohortIdentificationConfiguration
            // 
            this.cbxCohortIdentificationConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxCohortIdentificationConfiguration.FilterRule = null;
            this.cbxCohortIdentificationConfiguration.FormattingEnabled = true;
            this.cbxCohortIdentificationConfiguration.Location = new System.Drawing.Point(227, 17);
            this.cbxCohortIdentificationConfiguration.Name = "cbxCohortIdentificationConfiguration";
            this.cbxCohortIdentificationConfiguration.PropertySelector = null;
            this.cbxCohortIdentificationConfiguration.Size = new System.Drawing.Size(396, 21);
            this.cbxCohortIdentificationConfiguration.SuggestBoxHeight = 96;
            this.cbxCohortIdentificationConfiguration.SuggestListOrderRule = null;
            this.cbxCohortIdentificationConfiguration.TabIndex = 1;
            this.cbxCohortIdentificationConfiguration.SelectedIndexChanged += new System.EventHandler(this.cbxCohortIdentificationConfiguration_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Cohort Identification Configuration (Query):";
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(11, 225);
            this.objectSaverButton1.Margin = new System.Windows.Forms.Padding(0);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(75, 26);
            this.objectSaverButton1.TabIndex = 51;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 52;
            this.label2.Text = "Description:";
            // 
            // tbDescription
            // 
            this.tbDescription.Location = new System.Drawing.Point(11, 117);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(744, 105);
            this.tbDescription.TabIndex = 53;
            this.tbDescription.TextChanged += new System.EventHandler(this.tbDescription_TextChanged);
            // 
            // ExtractionConfigurationUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.tbDescription);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.objectSaverButton1);
            this.Controls.Add(this.gbCohortRefreshing);
            this.Controls.Add(this.pChooseExtractionPipeline);
            this.Controls.Add(this.tbID);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tcRelease);
            this.Controls.Add(this.tcRequest);
            this.Controls.Add(this.btnConfigureGlobalParameters);
            this.Controls.Add(this.tbCreated);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbUsername);
            this.Controls.Add(this.label4);
            this.Name = "ExtractionConfigurationUI";
            this.Size = new System.Drawing.Size(1162, 834);
            this.Load += new System.EventHandler(this.ExtractionConfigurationUI_Load);
            this.gbCohortRefreshing.ResumeLayout(false);
            this.gbCohortRefreshing.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.TextBox tbCreated;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnConfigureGlobalParameters;
        private TicketingControl tcRequest;
        private TicketingControl tcRelease;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel pChooseExtractionPipeline;
        private System.Windows.Forms.GroupBox gbCohortRefreshing;
        private System.Windows.Forms.PictureBox pbCic;
        private ReusableUIComponents.SuggestComboBox cbxCohortIdentificationConfiguration;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClearCic;
        private System.Windows.Forms.Panel pChooseCohortRefreshPipeline;
        private CatalogueManager.SimpleControls.ObjectSaverButton objectSaverButton1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbDescription;
    }
}
