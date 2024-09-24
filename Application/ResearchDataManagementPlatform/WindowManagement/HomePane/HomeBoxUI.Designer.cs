namespace ResearchDataManagementPlatform.WindowManagement.HomePane
{
    partial class HomeBoxUI
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HomeBoxUI));
            lblTitle = new System.Windows.Forms.Label();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            btnNew = new System.Windows.Forms.ToolStripButton();
            btnNewDropdown = new System.Windows.Forms.ToolStripDropDownButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            btnOpen = new System.Windows.Forms.ToolStripButton();
            olvRecent = new BrightIdeasSoftware.TreeListView();
            olvName = new BrightIdeasSoftware.OLVColumn();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)olvRecent).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblTitle.ForeColor = System.Drawing.SystemColors.Desktop;
            lblTitle.Location = new System.Drawing.Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(71, 33);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Title";
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnNew, btnNewDropdown, toolStripSeparator1, btnOpen });
            toolStrip1.Location = new System.Drawing.Point(0, 33);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(480, 25);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnNew
            // 
            btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnNew.Image = (System.Drawing.Image)resources.GetObject("btnNew.Image");
            btnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnNew.Name = "btnNew";
            btnNew.Size = new System.Drawing.Size(23, 22);
            btnNew.Text = "toolStripButton1";
            // 
            // btnNewDropdown
            // 
            btnNewDropdown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnNewDropdown.Image = (System.Drawing.Image)resources.GetObject("btnNewDropdown.Image");
            btnNewDropdown.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnNewDropdown.Name = "btnNewDropdown";
            btnNewDropdown.Size = new System.Drawing.Size(29, 22);
            btnNewDropdown.Text = "newDropdown";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnOpen
            // 
            btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnOpen.Image = (System.Drawing.Image)resources.GetObject("btnOpen.Image");
            btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new System.Drawing.Size(23, 22);
            btnOpen.Text = "toolStripButton1";
            // 
            // olvRecent
            // 
            olvRecent.AllColumns.Add(olvName);
            olvRecent.CellEditUseWholeCell = false;
            olvRecent.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { olvName });
            olvRecent.Cursor = System.Windows.Forms.Cursors.Default;
            olvRecent.Dock = System.Windows.Forms.DockStyle.Fill;
            olvRecent.FullRowSelect = true;
            olvRecent.HideSelection = false;
            olvRecent.Location = new System.Drawing.Point(0, 58);
            olvRecent.Name = "olvRecent";
            olvRecent.RowHeight = 19;
            olvRecent.ShowGroups = false;
            olvRecent.Size = new System.Drawing.Size(480, 441);
            olvRecent.TabIndex = 3;
            olvRecent.UseCompatibleStateImageBehavior = false;
            olvRecent.View = System.Windows.Forms.View.Details;
            olvRecent.VirtualMode = true;
            // 
            // olvName
            // 
            olvName.FillsFreeSpace = true;
            olvName.Groupable = false;
            olvName.Text = "Recent";
            // 
            // HomeBoxUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(olvRecent);
            Controls.Add(toolStrip1);
            Controls.Add(lblTitle);
            Name = "HomeBoxUI";
            Size = new System.Drawing.Size(480, 499);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)olvRecent).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton btnNewDropdown;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnNew;
        private BrightIdeasSoftware.TreeListView olvRecent;
        private BrightIdeasSoftware.OLVColumn olvName;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}
