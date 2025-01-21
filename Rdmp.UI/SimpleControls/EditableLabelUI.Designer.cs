namespace Rdmp.UI.SimpleControls
{
    partial class EditableLabelUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditableLabelUI));
            pictureBox1 = new System.Windows.Forms.PictureBox();
            lblEditable = new System.Windows.Forms.Label();
            tbEditable = new System.Windows.Forms.TextBox();
            lblTitle = new System.Windows.Forms.Label();
            pbIcon = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbIcon).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            pictureBox1.Image = (System.Drawing.Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new System.Drawing.Point(235, 30);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(19, 19);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // lblEditable
            // 
            lblEditable.AutoSize = true;
            lblEditable.Location = new System.Drawing.Point(31, 32);
            lblEditable.Name = "lblEditable";
            lblEditable.Size = new System.Drawing.Size(38, 15);
            lblEditable.TabIndex = 1;
            lblEditable.Text = "label1";
            // 
            // tbEditable
            // 
            tbEditable.Location = new System.Drawing.Point(31, 28);
            tbEditable.Name = "tbEditable";
            tbEditable.Size = new System.Drawing.Size(201, 23);
            tbEditable.TabIndex = 2;
            tbEditable.LostFocus += tbEditable_Lostfocus;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F);
            lblTitle.Location = new System.Drawing.Point(25, 3);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(0, 21);
            lblTitle.TabIndex = 3;
            // 
            // pbIcon
            // 
            pbIcon.Location = new System.Drawing.Point(0, 3);
            pbIcon.Name = "pbIcon";
            pbIcon.Size = new System.Drawing.Size(22, 22);
            pbIcon.TabIndex = 4;
            pbIcon.TabStop = false;
            // 
            // EditableLabelUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(pbIcon);
            Controls.Add(lblTitle);
            Controls.Add(tbEditable);
            Controls.Add(lblEditable);
            Controls.Add(pictureBox1);
            Name = "EditableLabelUI";
            Size = new System.Drawing.Size(278, 59);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbIcon).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblEditable;
        private System.Windows.Forms.TextBox tbEditable;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.PictureBox pbIcon;
    }
}
