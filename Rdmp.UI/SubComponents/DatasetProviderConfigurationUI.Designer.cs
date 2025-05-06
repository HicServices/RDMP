namespace Rdmp.UI.SubComponents
{
    partial class DatasetProviderConfigurationUI
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
            label1 = new System.Windows.Forms.Label();
            tbName = new System.Windows.Forms.TextBox();
            btnSave = new System.Windows.Forms.Button();
            tbType = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            tbUrl = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            tbOrgId = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            cbAccessCredentials = new System.Windows.Forms.ComboBox();
            label5 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(102, 75);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(42, 15);
            label1.TabIndex = 0;
            label1.Text = "Name:";
            label1.Click += label1_Click;
            // 
            // tbName
            // 
            tbName.Location = new System.Drawing.Point(146, 72);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(252, 23);
            tbName.TabIndex = 1;
            // 
            // btnSave
            // 
            btnSave.Location = new System.Drawing.Point(323, 279);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(75, 23);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // tbType
            // 
            tbType.Enabled = false;
            tbType.Location = new System.Drawing.Point(146, 115);
            tbType.Name = "tbType";
            tbType.Size = new System.Drawing.Size(252, 23);
            tbType.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(102, 118);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(34, 15);
            label2.TabIndex = 3;
            label2.Text = "Type:";
            // 
            // tbUrl
            // 
            tbUrl.Location = new System.Drawing.Point(146, 154);
            tbUrl.Name = "tbUrl";
            tbUrl.Size = new System.Drawing.Size(252, 23);
            tbUrl.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(102, 157);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(31, 15);
            label3.TabIndex = 5;
            label3.Text = "URL:";
            label3.Click += label3_Click;
            // 
            // tbOrgId
            // 
            tbOrgId.Location = new System.Drawing.Point(146, 194);
            tbOrgId.Name = "tbOrgId";
            tbOrgId.Size = new System.Drawing.Size(252, 23);
            tbOrgId.TabIndex = 8;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(47, 197);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(92, 15);
            label4.TabIndex = 7;
            label4.Text = "Organisation ID:";
            // 
            // cbAccessCredentials
            // 
            cbAccessCredentials.FormattingEnabled = true;
            cbAccessCredentials.Location = new System.Drawing.Point(146, 236);
            cbAccessCredentials.Name = "cbAccessCredentials";
            cbAccessCredentials.Size = new System.Drawing.Size(252, 23);
            cbAccessCredentials.TabIndex = 9;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(1, 239);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(135, 15);
            label5.TabIndex = 10;
            label5.Text = "Data Access Credentials:";
            // 
            // DatasetProviderConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(label5);
            Controls.Add(cbAccessCredentials);
            Controls.Add(tbOrgId);
            Controls.Add(label4);
            Controls.Add(tbUrl);
            Controls.Add(label3);
            Controls.Add(tbType);
            Controls.Add(label2);
            Controls.Add(btnSave);
            Controls.Add(tbName);
            Controls.Add(label1);
            Name = "DatasetProviderConfigurationUI";
            Size = new System.Drawing.Size(1003, 530);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox tbType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbUrl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbOrgId;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbAccessCredentials;
        private System.Windows.Forms.Label label5;
    }
}