using System.Data;
using System.Security.AccessControl;
using System.Windows.Forms;

namespace RDMPObjectVisualisation.Pipelines
{
    partial class ConfigurePipelineUI<T>
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
            this.label5 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnRetryPreview = new System.Windows.Forms.Button();
            this.lblPreviewStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Pipeline Name:";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(93, 10);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(396, 20);
            this.tbName.TabIndex = 0;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(477, 926);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(295, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Save and Close";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(24, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(63, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Description:";
            // 
            // tbDescription
            // 
            this.tbDescription.Location = new System.Drawing.Point(93, 32);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.Size = new System.Drawing.Size(1158, 67);
            this.tbDescription.TabIndex = 1;
            this.tbDescription.TextChanged += new System.EventHandler(this.tbDescription_TextChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(12, 105);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1239, 815);
            this.panel1.TabIndex = 11;
            // 
            // btnRetryPreview
            // 
            this.btnRetryPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRetryPreview.Location = new System.Drawing.Point(1153, 3);
            this.btnRetryPreview.Name = "btnRetryPreview";
            this.btnRetryPreview.Size = new System.Drawing.Size(98, 23);
            this.btnRetryPreview.TabIndex = 12;
            this.btnRetryPreview.Text = "Retry Preview";
            this.btnRetryPreview.UseVisualStyleBackColor = true;
            this.btnRetryPreview.Click += new System.EventHandler(this.btnRetryPreview_Click);
            // 
            // lblPreviewStatus
            // 
            this.lblPreviewStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPreviewStatus.AutoSize = true;
            this.lblPreviewStatus.ForeColor = System.Drawing.Color.Red;
            this.lblPreviewStatus.Location = new System.Drawing.Point(997, 8);
            this.lblPreviewStatus.Name = "lblPreviewStatus";
            this.lblPreviewStatus.Size = new System.Drawing.Size(131, 13);
            this.lblPreviewStatus.TabIndex = 13;
            this.lblPreviewStatus.Text = "Preview Generation Failed";
            // 
            // ConfigurePipelineUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1263, 951);
            this.Controls.Add(this.lblPreviewStatus);
            this.Controls.Add(this.btnRetryPreview);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tbDescription);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Name = "ConfigurePipelineUI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ConfigurePipelineUI";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbDescription;
        private Panel panel1;
        private Button btnRetryPreview;
        private Label lblPreviewStatus;
    }
}