namespace Rdmp.UI.ChecksUI
{
    partial class PopupChecksUI
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
            this.checksUI1 = new Rdmp.UI.ChecksUI.ChecksUI();
            this.SuspendLayout();
            // 
            // checksUI1
            // 
            this.checksUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checksUI1.Location = new System.Drawing.Point(0, 0);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(624, 614);
            this.checksUI1.TabIndex = 0;
            // 
            // PopupChecksUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 614);
            this.Controls.Add(this.checksUI1);
            this.Name = "PopupChecksUI";
            this.Text = "Checks";
            this.ResumeLayout(false);

        }

        #endregion

        public Rdmp.UI.ChecksUI.ChecksUI checksUI1;
    }
}