namespace Rdmp.UI.SubComponents
{
    partial class PureDatasetLinkUI
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
            tbLink = new System.Windows.Forms.TextBox();
            tbDescription = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // tbLink
            // 
            tbLink.Location = new System.Drawing.Point(3, 3);
            tbLink.Name = "tbLink";
            tbLink.Size = new System.Drawing.Size(100, 23);
            tbLink.TabIndex = 0;
            tbLink.TextChanged += tbLink_TextChanged_1;
            // 
            // tbDescription
            // 
            tbDescription.Location = new System.Drawing.Point(122, 3);
            tbDescription.Name = "tbDescription";
            tbDescription.Size = new System.Drawing.Size(100, 23);
            tbDescription.TabIndex = 1;
            tbDescription.TextChanged += tbDescription_TextChanged_1;
            // 
            // PureDatasetLinkUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tbDescription);
            Controls.Add(tbLink);
            Name = "PureDatasetLinkUI";
            Size = new System.Drawing.Size(254, 34);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox tbLink;
        private System.Windows.Forms.TextBox tbDescription;
    }
}
