namespace CatalogueManager.CommandExecution.AtomicCommands.UIFactory
{
    partial class AtomicCommandLinkLabel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AtomicCommandLinkLabel));
            this.pbCommandIcon = new System.Windows.Forms.PictureBox();
            this.lblName = new System.Windows.Forms.Label();
            this.helpIcon1 = new ReusableUIComponents.HelpIcon();
            ((System.ComponentModel.ISupportInitialize)(this.pbCommandIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // pbCommandIcon
            // 
            this.pbCommandIcon.Location = new System.Drawing.Point(0, 0);
            this.pbCommandIcon.Name = "pbCommandIcon";
            this.pbCommandIcon.Size = new System.Drawing.Size(19, 19);
            this.pbCommandIcon.TabIndex = 0;
            this.pbCommandIcon.TabStop = false;
            // 
            // lblName
            // 
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.lblName.Location = new System.Drawing.Point(25, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(345, 19);
            this.lblName.TabIndex = 1;
            this.lblName.Text = "label1";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblName.Click += new System.EventHandler(this.label1_Click);
            this.lblName.MouseEnter += new System.EventHandler(this.label1_MouseEnter);
            this.lblName.MouseLeave += new System.EventHandler(this.label1_MouseLeave);
            // 
            // helpIcon1
            // 
            this.helpIcon1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.helpIcon1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("helpIcon1.BackgroundImage")));
            this.helpIcon1.Location = new System.Drawing.Point(378, 0);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.TabIndex = 2;
            // 
            // AtomicCommandLinkLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.helpIcon1);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.pbCommandIcon);
            this.Name = "AtomicCommandLinkLabel";
            this.Size = new System.Drawing.Size(400, 22);
            ((System.ComponentModel.ISupportInitialize)(this.pbCommandIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbCommandIcon;
        private System.Windows.Forms.Label lblName;
        private ReusableUIComponents.HelpIcon helpIcon1;
    }
}
