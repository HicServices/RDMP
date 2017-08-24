using System;

namespace CatalogueManager.DashboardTabs
{
    partial class DashboardLayoutUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardLayoutUI));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tbDashboardName = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.cbxAvailableControls = new System.Windows.Forms.ToolStripComboBox();
            this.btnAddDashboardControl = new System.Windows.Forms.ToolStripButton();
            this.btnEditMode = new System.Windows.Forms.ToolStripButton();
            this.btnDeleteDashboard = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tbDashboardName,
            this.toolStripSeparator1,
            this.toolStripLabel2,
            this.cbxAvailableControls,
            this.btnAddDashboardControl,
            this.btnEditMode,
            this.btnDeleteDashboard});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1334, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(42, 22);
            this.toolStripLabel1.Text = "Name:";
            // 
            // tbDashboardName
            // 
            this.tbDashboardName.Name = "tbDashboardName";
            this.tbDashboardName.Size = new System.Drawing.Size(150, 25);
            this.tbDashboardName.TextChanged += new System.EventHandler(this.tbDashboardName_TextChanged);
            this.tbDashboardName.LostFocus += new System.EventHandler(this.tbDashboardName_LostFocus);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(54, 22);
            this.toolStripLabel2.Text = "Add/Edit";
            // 
            // cbxAvailableControls
            // 
            this.cbxAvailableControls.Name = "cbxAvailableControls";
            this.cbxAvailableControls.Size = new System.Drawing.Size(321, 25);
            // 
            // btnAddDashboardControl
            // 
            this.btnAddDashboardControl.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddDashboardControl.Image = ((System.Drawing.Image)(resources.GetObject("btnAddDashboardControl.Image")));
            this.btnAddDashboardControl.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddDashboardControl.Name = "btnAddDashboardControl";
            this.btnAddDashboardControl.Size = new System.Drawing.Size(23, 22);
            this.btnAddDashboardControl.Text = "Add Control To Dashboard";
            this.btnAddDashboardControl.Click += new System.EventHandler(this.btnAddDashboardControl_Click);
            // 
            // btnEditMode
            // 
            this.btnEditMode.CheckOnClick = true;
            this.btnEditMode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnEditMode.Image = ((System.Drawing.Image)(resources.GetObject("btnEditMode.Image")));
            this.btnEditMode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditMode.Name = "btnEditMode";
            this.btnEditMode.Size = new System.Drawing.Size(23, 22);
            this.btnEditMode.Text = "Move Or Delete Controls";
            this.btnEditMode.CheckedChanged += new System.EventHandler(this.btnEditMode_CheckedChanged);
            // 
            // btnDeleteDashboard
            // 
            this.btnDeleteDashboard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDeleteDashboard.Image = ((System.Drawing.Image)(resources.GetObject("btnDeleteDashboard.Image")));
            this.btnDeleteDashboard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDeleteDashboard.Name = "btnDeleteDashboard";
            this.btnDeleteDashboard.Size = new System.Drawing.Size(23, 22);
            this.btnDeleteDashboard.Text = "Delete Dashboard";
            this.btnDeleteDashboard.Click += new System.EventHandler(this.btnDeleteDashboard_Click);
            // 
            // DashboardLayoutUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.toolStrip1);
            this.Name = "DashboardLayoutUI";
            this.Size = new System.Drawing.Size(1334, 844);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnEditMode;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox tbDashboardName;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox cbxAvailableControls;
        private System.Windows.Forms.ToolStripButton btnAddDashboardControl;
        private System.Windows.Forms.ToolStripButton btnDeleteDashboard;
    }
}
