namespace Rdmp.UI.SimpleDialogs.Datasets
{
    partial class CreateNewHDRConfigurationUI
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
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(136, 76);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(25, 15);
            label1.TabIndex = 0;
            label1.Text = "Url:";
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
            btnSave.Location = new System.Drawing.Point(464, 192);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(75, 23);
            btnSave.TabIndex = 8;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += Save;
            // 
            // CreateNewPureConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(btnSave);
            Controls.Add(cbCredentials);
            Controls.Add(tbOrganisationId);
            Controls.Add(tbUrl);
            Controls.Add(tbName);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "CreateNewPureConfigurationUI";
            Text = "Create Pure Configuration";
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
    }
}