namespace ResearchDataManagementPlatform.WindowManagement.TopBar
{
    partial class RDMPTaskBar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RDMPTaskBar));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnHome = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnCatalogues = new System.Windows.Forms.ToolStripButton();
            this.btnCohorts = new System.Windows.Forms.ToolStripButton();
            this.btnDataExport = new System.Windows.Forms.ToolStripButton();
            this.btnTables = new System.Windows.Forms.ToolStripButton();
            this.btnLoad = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.cbxDashboards = new System.Windows.Forms.ToolStripComboBox();
            this.btnAddDashboard = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnHome,
            this.toolStripSeparator1,
            this.btnCatalogues,
            this.btnCohorts,
            this.btnDataExport,
            this.btnTables,
            this.btnLoad,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.cbxDashboards,
            this.btnAddDashboard,
            this.toolStripSeparator3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(755, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnHome
            // 
            this.btnHome.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHome.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(23, 22);
            this.btnHome.Text = "Show Home Screen";
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnCatalogues
            // 
            this.btnCatalogues.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCatalogues.Image = ((System.Drawing.Image)(resources.GetObject("btnCatalogues.Image")));
            this.btnCatalogues.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCatalogues.Name = "btnCatalogues";
            this.btnCatalogues.Size = new System.Drawing.Size(23, 22);
            this.btnCatalogues.Text = "Catalogues";
            this.btnCatalogues.Click += new System.EventHandler(this.ToolboxButtonClicked);
            // 
            // btnCohorts
            // 
            this.btnCohorts.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCohorts.Image = ((System.Drawing.Image)(resources.GetObject("btnCohorts.Image")));
            this.btnCohorts.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCohorts.Name = "btnCohorts";
            this.btnCohorts.Size = new System.Drawing.Size(23, 22);
            this.btnCohorts.Text = "Cohort Builder";
            this.btnCohorts.Click += new System.EventHandler(this.ToolboxButtonClicked);
            // 
            // btnDataExport
            // 
            this.btnDataExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDataExport.Image = ((System.Drawing.Image)(resources.GetObject("btnDataExport.Image")));
            this.btnDataExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDataExport.Name = "btnDataExport";
            this.btnDataExport.Size = new System.Drawing.Size(23, 22);
            this.btnDataExport.Text = "Data Export";
            this.btnDataExport.Click += new System.EventHandler(this.ToolboxButtonClicked);
            // 
            // btnTables
            // 
            this.btnTables.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnTables.Image = ((System.Drawing.Image)(resources.GetObject("btnTables.Image")));
            this.btnTables.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTables.Name = "btnTables";
            this.btnTables.Size = new System.Drawing.Size(23, 22);
            this.btnTables.Text = "Tables";
            this.btnTables.Click += new System.EventHandler(this.ToolboxButtonClicked);
            // 
            // btnLoad
            // 
            this.btnLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLoad.Image = ((System.Drawing.Image)(resources.GetObject("btnLoad.Image")));
            this.btnLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(23, 22);
            this.btnLoad.Text = "Data Load";
            this.btnLoad.Click += new System.EventHandler(this.ToolboxButtonClicked);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(69, 22);
            this.toolStripLabel1.Text = "Dashboards";
            // 
            // cbxDashboards
            // 
            this.cbxDashboards.Name = "cbxDashboards";
            this.cbxDashboards.Size = new System.Drawing.Size(121, 25);
            this.cbxDashboards.DropDownClosed += new System.EventHandler(this.cbxDashboards_DropDownClosed);
            // 
            // btnAddDashboard
            // 
            this.btnAddDashboard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddDashboard.Image = ((System.Drawing.Image)(resources.GetObject("btnAddDashboard.Image")));
            this.btnAddDashboard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddDashboard.Name = "btnAddDashboard";
            this.btnAddDashboard.Size = new System.Drawing.Size(23, 22);
            this.btnAddDashboard.Text = "toolStripButton1";
            this.btnAddDashboard.Click += new System.EventHandler(this.btnAddDashboard_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // RDMPTaskBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip1);
            this.Name = "RDMPTaskBar";
            this.Size = new System.Drawing.Size(755, 25);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnHome;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnCatalogues;
        private System.Windows.Forms.ToolStripButton btnCohorts;
        private System.Windows.Forms.ToolStripButton btnDataExport;
        private System.Windows.Forms.ToolStripButton btnTables;
        private System.Windows.Forms.ToolStripButton btnLoad;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox cbxDashboards;
        private System.Windows.Forms.ToolStripButton btnAddDashboard;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}
