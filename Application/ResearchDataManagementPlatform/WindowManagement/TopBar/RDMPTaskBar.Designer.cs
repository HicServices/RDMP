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
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnSavedCohorts = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.tbFind = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnFavourites = new System.Windows.Forms.ToolStripButton();
            this.btnLaunchNavigateTo = new ResearchDataManagementPlatform.WindowManagement.TopBar.SimpleCounterButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnHome
            // 
            this.btnHome.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(44, 22);
            this.btnHome.Text = "Home";
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnCatalogues
            // 
            this.btnCatalogues.Image = ((System.Drawing.Image)(resources.GetObject("btnCatalogues.Image")));
            this.btnCatalogues.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCatalogues.Name = "btnCatalogues";
            this.btnCatalogues.Size = new System.Drawing.Size(125, 22);
            this.btnCatalogues.Text = "Data Management";
            this.btnCatalogues.Click += new System.EventHandler(this.ToolboxButtonClicked);
            // 
            // btnCohorts
            // 
            this.btnCohorts.Image = ((System.Drawing.Image)(resources.GetObject("btnCohorts.Image")));
            this.btnCohorts.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCohorts.Name = "btnCohorts";
            this.btnCohorts.Size = new System.Drawing.Size(104, 22);
            this.btnCohorts.Text = "Cohort Builder";
            this.btnCohorts.Click += new System.EventHandler(this.ToolboxButtonClicked);
            // 
            // btnDataExport
            // 
            this.btnDataExport.Image = ((System.Drawing.Image)(resources.GetObject("btnDataExport.Image")));
            this.btnDataExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDataExport.Name = "btnDataExport";
            this.btnDataExport.Size = new System.Drawing.Size(87, 22);
            this.btnDataExport.Text = "Data Export";
            this.btnDataExport.Click += new System.EventHandler(this.ToolboxButtonClicked);
            // 
            // btnTables
            // 
            this.btnTables.Image = ((System.Drawing.Image)(resources.GetObject("btnTables.Image")));
            this.btnTables.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTables.Name = "btnTables";
            this.btnTables.Size = new System.Drawing.Size(125, 22);
            this.btnTables.Text = "Tables (Advanced)";
            this.btnTables.Click += new System.EventHandler(this.ToolboxButtonClicked);
            // 
            // btnLoad
            // 
            this.btnLoad.Image = ((System.Drawing.Image)(resources.GetObject("btnLoad.Image")));
            this.btnLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(80, 22);
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
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnHome,
            this.toolStripSeparator1,
            this.btnFavourites,
            this.btnCatalogues,
            this.btnCohorts,
            this.btnSavedCohorts,
            this.btnDataExport,
            this.toolStripSeparator,
            this.btnTables,
            this.btnLoad,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.cbxDashboards,
            this.btnAddDashboard,
            this.toolStripSeparator3,
            this.toolStripLabel2,
            this.tbFind,
            this.btnLaunchNavigateTo,
            this.toolStripSeparator4});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1179, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnSavedCohorts
            // 
            this.btnSavedCohorts.Image = ((System.Drawing.Image)(resources.GetObject("btnSavedCohorts.Image")));
            this.btnSavedCohorts.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSavedCohorts.Name = "btnSavedCohorts";
            this.btnSavedCohorts.Size = new System.Drawing.Size(103, 22);
            this.btnSavedCohorts.Text = "Saved Cohorts";
            this.btnSavedCohorts.Click += new System.EventHandler(this.ToolboxButtonClicked);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(33, 22);
            this.toolStripLabel2.Text = "Find:";
            // 
            // tbFind
            // 
            this.tbFind.Name = "tbFind";
            this.tbFind.Size = new System.Drawing.Size(100, 25);
            this.tbFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFind_KeyDown);
            this.tbFind.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbFind_KeyUp);
            this.tbFind.TextChanged += new System.EventHandler(this.tbFind_TextChanged);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // btnFavourites
            // 
            this.btnFavourites.Image = ((System.Drawing.Image)(resources.GetObject("btnFavourites.Image")));
            this.btnFavourites.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFavourites.Name = "btnFavourites";
            this.btnFavourites.Size = new System.Drawing.Size(81, 22);
            this.btnFavourites.Text = "Favourites";
            this.btnFavourites.Click += new System.EventHandler(this.ToolboxButtonClicked);
            // 
            // btnLaunchNavigateTo
            // 
            this.btnLaunchNavigateTo.Count = null;
            this.btnLaunchNavigateTo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLaunchNavigateTo.Image = ((System.Drawing.Image)(resources.GetObject("btnLaunchNavigateTo.Image")));
            this.btnLaunchNavigateTo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLaunchNavigateTo.Name = "btnLaunchNavigateTo";
            this.btnLaunchNavigateTo.Size = new System.Drawing.Size(23, 22);
            this.btnLaunchNavigateTo.Text = "Launch Advanced Find";
            this.btnLaunchNavigateTo.Click += new System.EventHandler(this.btnLaunchNavigateTo_Click);
            // 
            // RDMPTaskBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip1);
            this.Name = "RDMPTaskBar";
            this.Size = new System.Drawing.Size(1179, 25);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnSavedCohorts;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox tbFind;
        private SimpleCounterButton btnLaunchNavigateTo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnFavourites;

    }
}
