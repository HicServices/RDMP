namespace Rdmp.UI.SubComponents
{
    partial class LinkDatasetDialog
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
            groupBox2 = new System.Windows.Forms.GroupBox();
            tbID = new System.Windows.Forms.TextBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            cbPovider = new System.Windows.Forms.ComboBox();
            button1 = new System.Windows.Forms.Button();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(tbID);
            groupBox2.Location = new System.Drawing.Point(12, 82);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(200, 60);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Dataset ID";
            // 
            // tbID
            // 
            tbID.Location = new System.Drawing.Point(6, 20);
            tbID.Name = "tbID";
            tbID.Size = new System.Drawing.Size(188, 23);
            tbID.TabIndex = 0;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(cbPovider);
            groupBox3.Location = new System.Drawing.Point(12, 8);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(200, 60);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Dataset Provider";
            // 
            // cbPovider
            // 
            cbPovider.FormattingEnabled = true;
            cbPovider.Location = new System.Drawing.Point(6, 22);
            cbPovider.Name = "cbPovider";
            cbPovider.Size = new System.Drawing.Size(188, 23);
            cbPovider.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(216, 155);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(89, 23);
            button1.TabIndex = 3;
            button1.Text = "Link Dataset";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // LinkDatasetDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(button1);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Name = "LinkDatasetDialog";
            Text = "Link Existing Dataset";
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox cbPovider;
        private System.Windows.Forms.Button button1;
    }
}