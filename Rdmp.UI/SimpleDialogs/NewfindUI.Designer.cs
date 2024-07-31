namespace Rdmp.UI.SimpleDialogs
{
    partial class NewfindUI
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
            panel1 = new System.Windows.Forms.Panel();
            tbFind = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            tbReplace = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(label2);
            panel1.Controls.Add(tbReplace);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(tbFind);
            panel1.Location = new System.Drawing.Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(776, 100);
            panel1.TabIndex = 0;
            // 
            // tbFind
            // 
            tbFind.Location = new System.Drawing.Point(88, 47);
            tbFind.Name = "tbFind";
            tbFind.Size = new System.Drawing.Size(387, 23);
            tbFind.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(49, 50);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(33, 15);
            label1.TabIndex = 1;
            label1.Text = "Find:";
            // 
            // tbReplace
            // 
            tbReplace.Location = new System.Drawing.Point(88, 76);
            tbReplace.Name = "tbReplace";
            tbReplace.Size = new System.Drawing.Size(387, 23);
            tbReplace.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 80);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(79, 15);
            label2.TabIndex = 3;
            label2.Text = "Replace With:";
            // 
            // NewfindUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(panel1);
            Name = "NewfindUI";
            Text = "NewfindUI";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbReplace;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFind;
    }
}