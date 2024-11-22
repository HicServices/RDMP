namespace Rdmp.UI.SimpleDialogs.Datasets
{
    sealed partial class CreateNewDatasetUI
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
            tbName = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            tbDOI = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            tbSource = new System.Windows.Forms.TextBox();
            btnCancel = new System.Windows.Forms.Button();
            btnCreate = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // tbName
            // 
            tbName.Location = new System.Drawing.Point(64, 75);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(301, 23);
            tbName.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(66, 52);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(113, 15);
            label1.TabIndex = 1;
            label1.Text = "1. Name the Dataset";
            // 
            // tbDOI
            // 
            tbDOI.Location = new System.Drawing.Point(64, 142);
            tbDOI.Name = "tbDOI";
            tbDOI.Size = new System.Drawing.Size(301, 23);
            tbDOI.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(64, 124);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(106, 15);
            label2.TabIndex = 3;
            label2.Text = "2. What is the DOI?";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(66, 185);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(200, 15);
            label3.TabIndex = 4;
            label3.Text = "3. What is the Source of this Dataset?";
            // 
            // tbSource
            // 
            tbSource.Location = new System.Drawing.Point(64, 203);
            tbSource.Name = "tbSource";
            tbSource.Size = new System.Drawing.Size(301, 23);
            tbSource.TabIndex = 5;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(191, 270);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnCreate
            // 
            btnCreate.Location = new System.Drawing.Point(290, 270);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new System.Drawing.Size(75, 23);
            btnCreate.TabIndex = 7;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;
            // 
            // CreateNewDatasetUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(425, 346);
            Controls.Add(btnCreate);
            Controls.Add(btnCancel);
            Controls.Add(tbSource);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(tbDOI);
            Controls.Add(label1);
            Controls.Add(tbName);
            Name = "CreateNewDatasetUI";
            Text = "Create a new Dataset";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbDOI;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbSource;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCreate;
    }
}