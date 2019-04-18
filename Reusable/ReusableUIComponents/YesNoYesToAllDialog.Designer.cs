namespace ReusableUIComponents
{
    partial class YesNoYesToAllDialog
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
            this.btnYes = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnYesToAll = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnNoToAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnYes
            // 
            this.btnYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnYes.Location = new System.Drawing.Point(51, 236);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(100, 23);
            this.btnYes.TabIndex = 0;
            this.btnYes.Text = "Yes";
            this.btnYes.UseVisualStyleBackColor = true;
            this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
            // 
            // btnNo
            // 
            this.btnNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNo.Location = new System.Drawing.Point(157, 236);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(100, 23);
            this.btnNo.TabIndex = 0;
            this.btnNo.Text = "No";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // btnYesToAll
            // 
            this.btnYesToAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnYesToAll.Location = new System.Drawing.Point(263, 236);
            this.btnYesToAll.Name = "btnYesToAll";
            this.btnYesToAll.Size = new System.Drawing.Size(100, 23);
            this.btnYesToAll.TabIndex = 0;
            this.btnYesToAll.Text = "Yes To All";
            this.btnYesToAll.UseVisualStyleBackColor = true;
            this.btnYesToAll.Click += new System.EventHandler(this.btnYesToAll_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(3, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(522, 231);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
            // 
            // btnNoToAll
            // 
            this.btnNoToAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNoToAll.Location = new System.Drawing.Point(369, 236);
            this.btnNoToAll.Name = "btnNoToAll";
            this.btnNoToAll.Size = new System.Drawing.Size(100, 23);
            this.btnNoToAll.TabIndex = 2;
            this.btnNoToAll.Text = "No To All";
            this.btnNoToAll.UseVisualStyleBackColor = true;
            this.btnNoToAll.Click += new System.EventHandler(this.btnNoToAll_Click);
            // 
            // YesNoYesToAllDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(537, 271);
            this.Controls.Add(this.btnNoToAll);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnYesToAll);
            this.Controls.Add(this.btnNo);
            this.Controls.Add(this.btnYes);
            this.Name = "YesNoYesToAllDialog";
            this.Text = "Yes/No";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Button btnYesToAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnNoToAll;
    }
}