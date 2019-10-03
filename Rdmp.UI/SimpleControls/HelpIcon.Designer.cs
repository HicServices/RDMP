namespace Rdmp.UI.SimpleControls
{
    partial class HelpIcon
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Rdmp.UI.SimpleControls.HelpIcon));
            this.SuspendLayout();
            // 
            // HelpIcon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.DoubleBuffered = true;
            this.MaximumSize = new System.Drawing.Size(19, 19);
            this.MinimumSize = new System.Drawing.Size(19, 19);
            this.Name = "HelpIcon";
            this.Size = new System.Drawing.Size(19, 19);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HelpIcon_MouseClick);
            this.ResumeLayout(false);

        }

        #endregion

    }
}
