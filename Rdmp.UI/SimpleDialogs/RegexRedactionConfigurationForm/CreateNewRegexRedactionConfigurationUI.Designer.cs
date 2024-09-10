namespace Rdmp.UI.SimpleDialogs.RegexRedactionConfigurationForm
{
    partial class CreateNewRegexRedactionConfigurationUI
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
            tbName = new System.Windows.Forms.TextBox();
            tbRegexPattern = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            tbRedactionString = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            tbDescription = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            btnCreate = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            lblError = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(26, 30);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(47, 15);
            label1.TabIndex = 0;
            label1.Text = "Name *";
            // 
            // tbName
            // 
            tbName.Location = new System.Drawing.Point(32, 47);
            tbName.Name = "tbName";
            tbName.Size = new System.Drawing.Size(516, 23);
            tbName.TabIndex = 1;
            // 
            // tbRegexPattern
            // 
            tbRegexPattern.Location = new System.Drawing.Point(32, 114);
            tbRegexPattern.Name = "tbRegexPattern";
            tbRegexPattern.Size = new System.Drawing.Size(516, 23);
            tbRegexPattern.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(26, 97);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(88, 15);
            label2.TabIndex = 2;
            label2.Text = "Regex Pattern *";
            // 
            // tbRedactionString
            // 
            tbRedactionString.Location = new System.Drawing.Point(32, 176);
            tbRedactionString.Name = "tbRedactionString";
            tbRedactionString.Size = new System.Drawing.Size(516, 23);
            tbRedactionString.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(26, 159);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(118, 15);
            label3.TabIndex = 4;
            label3.Text = " Replacement String*";
            // 
            // tbDescription
            // 
            tbDescription.Location = new System.Drawing.Point(32, 237);
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new System.Drawing.Size(516, 23);
            tbDescription.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(26, 220);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(67, 15);
            label4.TabIndex = 6;
            label4.Text = "Description";
            // 
            // btnCreate
            // 
            btnCreate.Location = new System.Drawing.Point(476, 288);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new System.Drawing.Size(75, 23);
            btnCreate.TabIndex = 8;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(395, 288);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblError
            // 
            lblError.AutoSize = true;
            lblError.Location = new System.Drawing.Point(35, 288);
            lblError.Name = "lblError";
            lblError.Size = new System.Drawing.Size(39, 15);
            lblError.TabIndex = 10;
            lblError.Text = "Name";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(492, 266);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(62, 15);
            label5.TabIndex = 11;
            label5.Text = "* Required";
            // 
            // CreateNewRegexRedactionConfigurationUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(label5);
            Controls.Add(lblError);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(tbDescription);
            Controls.Add(label4);
            Controls.Add(tbRedactionString);
            Controls.Add(label3);
            Controls.Add(tbRegexPattern);
            Controls.Add(label2);
            Controls.Add(tbName);
            Controls.Add(label1);
            Name = "CreateNewRegexRedactionConfigurationUI";
            Text = "Create New Regex Redaction Configuration";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbRegexPattern;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbRedactionString;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbDescription;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Label label5;
    }
}