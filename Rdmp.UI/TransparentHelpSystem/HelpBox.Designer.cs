namespace Rdmp.UI.TransparentHelpSystem
{
    partial class HelpBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Rdmp.UI.TransparentHelpSystem.HelpBox));
            this.btnCloseHelp = new System.Windows.Forms.Button();
            this.btnOption1 = new System.Windows.Forms.Button();
            this.lblHelp = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCloseHelp
            // 
            this.btnCloseHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCloseHelp.Image = ((System.Drawing.Image)(resources.GetObject("btnCloseHelp.Image")));
            this.btnCloseHelp.Location = new System.Drawing.Point(378, 1);
            this.btnCloseHelp.Name = "btnCloseHelp";
            this.btnCloseHelp.Size = new System.Drawing.Size(23, 23);
            this.btnCloseHelp.TabIndex = 17;
            this.btnCloseHelp.UseVisualStyleBackColor = true;
            this.btnCloseHelp.Click += new System.EventHandler(this.btnCloseHelp_Click);
            // 
            // btnOption1
            // 
            this.btnOption1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOption1.Location = new System.Drawing.Point(3, 78);
            this.btnOption1.Name = "btnOption1";
            this.btnOption1.Size = new System.Drawing.Size(208, 19);
            this.btnOption1.TabIndex = 16;
            this.btnOption1.Text = "Option that might exist";
            this.btnOption1.UseVisualStyleBackColor = true;
            // 
            // lblHelp
            // 
            this.lblHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHelp.BackColor = System.Drawing.Color.Cornsilk;
            this.lblHelp.Location = new System.Drawing.Point(0, 0);
            this.lblHelp.Name = "lblHelp";
            this.lblHelp.Size = new System.Drawing.Size(372, 76);
            this.lblHelp.TabIndex = 15;
            this.lblHelp.Text = "Some Help text that is useful to the user";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Cornsilk;
            this.panel1.Controls.Add(this.lblHelp);
            this.panel1.Controls.Add(this.btnCloseHelp);
            this.panel1.Controls.Add(this.btnOption1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(1, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(397, 97);
            this.panel1.TabIndex = 18;
            // 
            // HelpBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.panel1);
            this.Name = "HelpBox";
            this.Padding = new System.Windows.Forms.Padding(1, 1, 2, 2);
            this.Size = new System.Drawing.Size(400, 100);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCloseHelp;
        private System.Windows.Forms.Button btnOption1;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Panel panel1;
    }
}
