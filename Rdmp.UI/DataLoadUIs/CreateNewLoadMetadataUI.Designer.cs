using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.DataLoadUIs
{
    partial class CreateNewLoadMetadataUI
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
            this.btnCreate = new System.Windows.Forms.Button();
            this.tbLoadMetadataNameToCreate = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chooseLoggingTaskUI1 = new ChooseLoggingTaskUI();
            this.SuspendLayout();
            // 
            // btnCreate
            // 
            this.btnCreate.Enabled = false;
            this.btnCreate.Location = new System.Drawing.Point(39, 175);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(75, 23);
            this.btnCreate.TabIndex = 3;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // tbLoadMetadataNameToCreate
            // 
            this.tbLoadMetadataNameToCreate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLoadMetadataNameToCreate.Location = new System.Drawing.Point(63, 8);
            this.tbLoadMetadataNameToCreate.Name = "tbLoadMetadataNameToCreate";
            this.tbLoadMetadataNameToCreate.Size = new System.Drawing.Size(1003, 20);
            this.tbLoadMetadataNameToCreate.TabIndex = 2;
            this.tbLoadMetadataNameToCreate.TextChanged += new System.EventHandler(this.tbLoadMetadataNameToCreate_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Name:";
            // 
            // chooseLoggingTaskUI1
            // 
            this.chooseLoggingTaskUI1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chooseLoggingTaskUI1.Catalogue = null;
            this.chooseLoggingTaskUI1.Location = new System.Drawing.Point(22, 34);
            this.chooseLoggingTaskUI1.Name = "chooseLoggingTaskUI1";
            this.chooseLoggingTaskUI1.Size = new System.Drawing.Size(1044, 144);
            this.chooseLoggingTaskUI1.TabIndex = 0;
            // 
            // CreateNewLoadMetadataUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1078, 202);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.tbLoadMetadataNameToCreate);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chooseLoggingTaskUI1);
            this.Name = "CreateNewLoadMetadataUI";
            this.Text = "Create New Load";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SimpleDialogs.ChooseLoggingTaskUI chooseLoggingTaskUI1;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.TextBox tbLoadMetadataNameToCreate;
        private System.Windows.Forms.Label label3;
    }
}