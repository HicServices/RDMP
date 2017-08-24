using ResearchDataManagementPlatform.WindowManagement.TopMenu;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform
{
    partial class RDMPMainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RDMPMainForm));
            this.rdmpMenuStrip1 = new ResearchDataManagementPlatform.WindowManagement.TopMenu.RDMPMenuStrip();
            this.dockPanel1 = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.SuspendLayout();
            // 
            // rdmpMenuStrip1
            // 
            this.rdmpMenuStrip1.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdmpMenuStrip1.Location = new System.Drawing.Point(0, 0);
            this.rdmpMenuStrip1.Name = "rdmpMenuStrip1";
            this.rdmpMenuStrip1.Size = new System.Drawing.Size(1300, 53);
            this.rdmpMenuStrip1.TabIndex = 3;
            // 
            // dockPanel1
            // 
            this.dockPanel1.AutoSize = true;
            this.dockPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel1.Location = new System.Drawing.Point(0, 53);
            this.dockPanel1.Name = "dockPanel1";
            this.dockPanel1.Size = new System.Drawing.Size(1300, 732);
            this.dockPanel1.TabIndex = 4;
            this.dockPanel1.Text = "label1";
            // 
            // RDMPMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1300, 785);
            this.Controls.Add(this.dockPanel1);
            this.Controls.Add(this.rdmpMenuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RDMPMainForm";
            this.Text = "RDMPMainForm";
            this.Load += new System.EventHandler(this.RDMPMainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private RDMPMenuStrip rdmpMenuStrip1;
        private DockPanel dockPanel1;
    }
}

