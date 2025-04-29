using Org.BouncyCastle.Asn1.Crmf;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs.Datasets.HDR
{
    partial class ImportExistingHDRDatasetUI
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
            btnCreate = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            tbUrl = new System.Windows.Forms.TextBox();
            lblError = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            cbProviders = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // btnCreate
            // 
            btnCreate.Location = new System.Drawing.Point(550, 108);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new System.Drawing.Size(75, 23);
            btnCreate.TabIndex = 14;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(459, 108);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 15;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // tbUrl
            // 
            tbUrl.Location = new System.Drawing.Point(74, 56);
            tbUrl.Name = "tbUrl";
            tbUrl.Size = new System.Drawing.Size(551, 23);
            tbUrl.TabIndex = 17;
            // 
            // lblError
            // 
            lblError.AutoSize = true;
            lblError.ForeColor = System.Drawing.Color.Red;
            lblError.Location = new System.Drawing.Point(23, 95);
            lblError.Name = "lblError";
            lblError.Size = new System.Drawing.Size(0, 15);
            lblError.TabIndex = 18;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(14, 20);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(54, 15);
            label2.TabIndex = 19;
            label2.Text = "Provider:";
            // 
            // cbProviders
            // 
            cbProviders.FormattingEnabled = true;
            cbProviders.Location = new System.Drawing.Point(74, 17);
            cbProviders.Name = "cbProviders";
            cbProviders.Size = new System.Drawing.Size(345, 23);
            cbProviders.TabIndex = 20;
            cbProviders.SelectedIndexChanged += cbProviders_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(37, 59);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(37, 15);
            label1.TabIndex = 16;
            label1.Text = "UUID:";
            // 
            // ImportExistingPureDatasetUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(667, 158);
            Controls.Add(cbProviders);
            Controls.Add(label2);
            Controls.Add(lblError);
            Controls.Add(tbUrl);
            Controls.Add(label1);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Name = "ImportExistingPureDatasetUI";
            Text = "Import Existing HDR Dataset";
            Load += ImportExistingPureDatasetUI_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox tbUrl;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbProviders;
        private System.Windows.Forms.Label label1;
    }
}