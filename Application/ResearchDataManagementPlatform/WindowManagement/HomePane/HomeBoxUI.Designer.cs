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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HomeBoxUI));
            this.lblTitle = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnNew = new System.Windows.Forms.ToolStripButton();
            this.btnNewDropdown = new System.Windows.Forms.ToolStripDropDownButton();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.olvRecent = new BrightIdeasSoftware.ObjectListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvRecent)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.SystemColors.Desktop;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(71, 33);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Title";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNew,
            this.btnNewDropdown,
            this.btnOpen});
            this.toolStrip1.Location = new System.Drawing.Point(0, 33);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(480, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnNew
            // 
            this.btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNew.Image = ((System.Drawing.Image)(resources.GetObject("btnNew.Image")));
            this.btnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(23, 22);
            this.btnNew.Text = "toolStripButton1";
            // 
            // btnNewDropdown
            // 
            this.btnNewDropdown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNewDropdown.Image = ((System.Drawing.Image)(resources.GetObject("btnNewDropdown.Image")));
            this.btnNewDropdown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNewDropdown.Name = "btnNewDropdown";
            this.btnNewDropdown.Size = new System.Drawing.Size(29, 22);
            this.btnNewDropdown.Text = "newDropdown";
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(23, 22);
            this.btnOpen.Text = "toolStripButton1";
            // 
            // olvRecent
            // 
            this.olvRecent.AllColumns.Add(this.olvName);
            this.olvRecent.CellEditUseWholeCell = false;
            this.olvRecent.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.olvRecent.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvRecent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvRecent.FullRowSelect = true;
            this.olvRecent.HideSelection = false;
            this.olvRecent.Location = new System.Drawing.Point(0, 58);
            this.olvRecent.Name = "olvRecent";
            this.olvRecent.RowHeight = 19;
            this.olvRecent.Size = new System.Drawing.Size(480, 441);
            this.olvRecent.TabIndex = 3;
            this.olvRecent.UseCompatibleStateImageBehavior = false;
            this.olvRecent.View = System.Windows.Forms.View.Details;
            // 
            // olvName
            // 
            this.olvName.FillsFreeSpace = true;
            this.olvName.Groupable = false;
            this.olvName.Text = "Recent";
            // 
            // HomeBoxUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.olvRecent);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.lblTitle);
            this.Name = "HomeBoxUI";
            this.Size = new System.Drawing.Size(480, 499);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvRecent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton btnNewDropdown;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnNew;
        private BrightIdeasSoftware.ObjectListView olvRecent;
        private BrightIdeasSoftware.OLVColumn olvName;
    }
}
