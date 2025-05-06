namespace Rdmp.UI.SimpleDialogs.Datasets
{
    partial class CreateNewDatasetProviderConfigurationUI
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
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            tbName = new System.Windows.Forms.TextBox();
            tbUrl = new System.Windows.Forms.TextBox();
            tbOrganisationId = new System.Windows.Forms.TextBox();
            cbCredentials = new System.Windows.Forms.ComboBox();
            btnSave = new System.Windows.Forms.Button();
            cbImportCatalogues = new System.Windows.Forms.CheckBox();
            cbIncludeInternal = new System.Windows.Forms.CheckBox();
            cbImportProjectSpecific = new System.Windows.Forms.CheckBox();
            cbImportDeprecated = new System.Windows.Forms.CheckBox();
            aiImportAll = new Rdmp.UI.SimpleControls.AdditionalInfomationUI();
            aiInternal = new Rdmp.UI.SimpleControls.AdditionalInfomationUI();
            aiProjectSpecific = new Rdmp.UI.SimpleControls.AdditionalInfomationUI();
            aiDeprecated = new Rdmp.UI.SimpleControls.AdditionalInfomationUI();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(57, 76);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(104, 15);
            label1.TabIndex = 0;
            label1.Text = "URL/ID:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(119, 37);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(42, 15);
            label2.TabIndex = 1;
            label2.Text = "Name:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(26, 116);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(135, 15);
            label3.TabIndex = 2;
            label3.Text = "Data Access Credentials:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(69, 152);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(92, 15);
            label4.TabIndex = 3;
            label4.Text = "Organisation ID:";
            // 
            // tbName
            // 
            tbName.Location = new System.Drawing.Point(169, 34);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(370, 23);
            tbName.TabIndex = 4;
            tbName.TextChanged += ValidateForm;
            // 
            // tbUrl
            // 
            tbUrl.Location = new System.Drawing.Point(169, 73);
            tbUrl.Name = "tbUrl";
            tbUrl.Size = new System.Drawing.Size(370, 23);
            tbUrl.TabIndex = 5;
            tbUrl.TextChanged += ValidateForm;
            // 
            // tbOrganisationId
            // 
            tbOrganisationId.Location = new System.Drawing.Point(169, 149);
            tbOrganisationId.Name = "tbOrganisationId";
            tbOrganisationId.Size = new System.Drawing.Size(370, 23);
            tbOrganisationId.TabIndex = 6;
            tbOrganisationId.TextChanged += ValidateForm;
            // 
            // cbCredentials
            // 
            cbCredentials.FormattingEnabled = true;
            cbCredentials.Location = new System.Drawing.Point(169, 108);
            cbCredentials.Name = "cbCredentials";
            cbCredentials.Size = new System.Drawing.Size(208, 23);
            cbCredentials.TabIndex = 7;
            cbCredentials.SelectedIndexChanged += ValidateForm;
            // 
            // btnSave
            // 
            btnSave.Enabled = false;
            btnSave.Location = new System.Drawing.Point(464, 318);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(75, 23);
            btnSave.TabIndex = 8;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += Save;
            // 
            // cbImportCatalogues
            // 
            cbImportCatalogues.AutoSize = true;
            cbImportCatalogues.Location = new System.Drawing.Point(337, 188);
            cbImportCatalogues.Name = "cbImportCatalogues";
            cbImportCatalogues.Size = new System.Drawing.Size(202, 19);
            cbImportCatalogues.TabIndex = 9;
            cbImportCatalogues.Text = "Import All Extractable Catalogues";
            cbImportCatalogues.UseVisualStyleBackColor = true;
            // 
            // cbIncludeInternal
            // 
            cbIncludeInternal.AutoSize = true;
            cbIncludeInternal.Location = new System.Drawing.Point(337, 213);
            cbIncludeInternal.Name = "cbIncludeInternal";
            cbIncludeInternal.Size = new System.Drawing.Size(184, 19);
            cbIncludeInternal.TabIndex = 10;
            cbIncludeInternal.Text = "Import All Internal Catalogues";
            cbIncludeInternal.UseVisualStyleBackColor = true;
            // 
            // cbImportProjectSpecific
            // 
            cbImportProjectSpecific.AutoSize = true;
            cbImportProjectSpecific.Location = new System.Drawing.Point(337, 238);
            cbImportProjectSpecific.Name = "cbImportProjectSpecific";
            cbImportProjectSpecific.Size = new System.Drawing.Size(225, 19);
            cbImportProjectSpecific.TabIndex = 11;
            cbImportProjectSpecific.Text = "Import All Project Sepcific Catalogues";
            cbImportProjectSpecific.UseVisualStyleBackColor = true;
            // 
            // cbImportDeprecated
            // 
            cbImportDeprecated.AutoSize = true;
            cbImportDeprecated.Location = new System.Drawing.Point(337, 263);
            cbImportDeprecated.Name = "cbImportDeprecated";
            cbImportDeprecated.Size = new System.Drawing.Size(204, 19);
            cbImportDeprecated.TabIndex = 12;
            cbImportDeprecated.Text = "Import All Deprecated Catalogues";
            cbImportDeprecated.UseVisualStyleBackColor = true;
            // 
            // aiImportAll
            // 
            aiImportAll.Location = new System.Drawing.Point(542, 188);
            aiImportAll.Name = "aiImportAll";
            aiImportAll.Size = new System.Drawing.Size(20, 20);
            aiImportAll.TabIndex = 13;
            // 
            // aiInternal
            // 
            aiInternal.Location = new System.Drawing.Point(521, 213);
            aiInternal.Name = "aiInternal";
            aiInternal.Size = new System.Drawing.Size(20, 20);
            aiInternal.TabIndex = 14;
            // 
            // aiProjectSpecific
            // 
            aiProjectSpecific.Location = new System.Drawing.Point(559, 237);
            aiProjectSpecific.Name = "aiProjectSpecific";
            aiProjectSpecific.Size = new System.Drawing.Size(20, 20);
            aiProjectSpecific.TabIndex = 15;
            // 
            // aiDeprecated
            // 
            aiDeprecated.Location = new System.Drawing.Point(542, 263);
            aiDeprecated.Name = "aiDeprecated";
            aiDeprecated.Size = new System.Drawing.Size(20, 20);
            aiDeprecated.TabIndex = 16;
            // 
            // CreateNewJiraConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(aiDeprecated);
            Controls.Add(aiProjectSpecific);
            Controls.Add(aiInternal);
            Controls.Add(aiImportAll);
            Controls.Add(cbImportDeprecated);
            Controls.Add(cbImportProjectSpecific);
            Controls.Add(cbIncludeInternal);
            Controls.Add(cbImportCatalogues);
            Controls.Add(btnSave);
            Controls.Add(cbCredentials);
            Controls.Add(tbOrganisationId);
            Controls.Add(tbUrl);
            Controls.Add(tbName);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "CreateNewJiraConfigurationUI";
            Text = "Create New Dataset Provider Configuration";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbUrl;
        private System.Windows.Forms.TextBox tbOrganisationId;
        private System.Windows.Forms.ComboBox cbCredentials;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox cbImportCatalogues;
        private System.Windows.Forms.CheckBox cbIncludeInternal;
        private System.Windows.Forms.CheckBox cbImportProjectSpecific;
        private System.Windows.Forms.CheckBox cbImportDeprecated;
        private SimpleControls.AdditionalInfomationUI aiImportAll;
        private SimpleControls.AdditionalInfomationUI aiInternal;
        private SimpleControls.AdditionalInfomationUI aiProjectSpecific;
        private SimpleControls.AdditionalInfomationUI aiDeprecated;
    }
}