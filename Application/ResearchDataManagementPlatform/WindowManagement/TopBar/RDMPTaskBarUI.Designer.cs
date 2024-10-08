

namespace ResearchDataManagementPlatform.WindowManagement.TopBar
{
    partial class RDMPTaskBarUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RDMPTaskBarUI));
            btnHome = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            btnCatalogues = new System.Windows.Forms.ToolStripButton();
            btnCohorts = new System.Windows.Forms.ToolStripButton();
            btnDataExport = new System.Windows.Forms.ToolStripButton();
            btnTables = new System.Windows.Forms.ToolStripButton();
            btnConfiguration = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            btnBack = new System.Windows.Forms.ToolStripSplitButton();
            btnForward = new System.Windows.Forms.ToolStripButton();
            btnFavourites = new System.Windows.Forms.ToolStripButton();
            btnSavedCohorts = new System.Windows.Forms.ToolStripButton();
            btnLoads = new System.Windows.Forms.ToolStripButton();
            toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            cbxLayouts = new System.Windows.Forms.ToolStripComboBox();
            btnSaveWindowLayout = new System.Windows.Forms.ToolStripButton();
            btnDeleteLayout = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            cbCommits = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnHome
            // 
            btnHome.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnHome.Name = "btnHome";
            btnHome.Size = new System.Drawing.Size(44, 22);
            btnHome.Text = "Home";
            btnHome.Click += btnHome_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnCatalogues
            // 
            btnCatalogues.Image = (System.Drawing.Image)resources.GetObject("btnCatalogues.Image");
            btnCatalogues.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnCatalogues.Name = "btnCatalogues";
            btnCatalogues.Size = new System.Drawing.Size(86, 22);
            btnCatalogues.Text = "Catalogues";
            btnCatalogues.Click += ToolboxButtonClicked;
            // 
            // btnCohorts
            // 
            btnCohorts.Image = (System.Drawing.Image)resources.GetObject("btnCohorts.Image");
            btnCohorts.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnCohorts.Name = "btnCohorts";
            btnCohorts.Size = new System.Drawing.Size(104, 22);
            btnCohorts.Text = "Cohort Builder";
            btnCohorts.Click += ToolboxButtonClicked;
            // 
            // btnDataExport
            // 
            btnDataExport.Image = (System.Drawing.Image)resources.GetObject("btnDataExport.Image");
            btnDataExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnDataExport.Name = "btnDataExport";
            btnDataExport.Size = new System.Drawing.Size(69, 22);
            btnDataExport.Text = "Projects";
            btnDataExport.Click += ToolboxButtonClicked;
            // 
            // btnTables
            // 
            btnTables.Image = (System.Drawing.Image)resources.GetObject("btnTables.Image");
            btnTables.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnTables.Name = "btnTables";
            btnTables.Size = new System.Drawing.Size(123, 22);
            btnTables.Text = "Tables (Advanced)";
            btnTables.Click += ToolboxButtonClicked;
            // 
            // btnConfiguration
            // 
            btnConfiguration.Image = (System.Drawing.Image)resources.GetObject("btnConfiguration.Image");
            btnConfiguration.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnConfiguration.Name = "btnConfiguration";
            btnConfiguration.Size = new System.Drawing.Size(71, 22);
            btnConfiguration.Text = "Configuration";
            btnConfiguration.Click += ToolboxButtonClicked;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator
            // 
            toolStripSeparator.Name = "toolStripSeparator";
            toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnBack, btnForward, btnHome, toolStripSeparator1, btnFavourites, btnCatalogues, btnCohorts, btnSavedCohorts, btnDataExport, toolStripSeparator, btnTables, btnLoads, btnConfiguration, toolStripSeparator2, toolStripLabel2, cbxLayouts, btnSaveWindowLayout, btnDeleteLayout, toolStripSeparator4, cbCommits, toolStripSeparator3 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1539, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnBack
            // 
            btnBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnBack.Enabled = false;
            btnBack.Image = (System.Drawing.Image)resources.GetObject("btnBack.Image");
            btnBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnBack.Name = "btnBack";
            btnBack.Size = new System.Drawing.Size(32, 22);
            btnBack.Text = "Back";
            btnBack.ButtonClick += btnBack_ButtonClick;
            btnBack.DropDownOpening += btnBack_DropDownOpening;
            // 
            // btnForward
            // 
            btnForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnForward.Enabled = false;
            btnForward.Image = (System.Drawing.Image)resources.GetObject("btnForward.Image");
            btnForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnForward.Name = "btnForward";
            btnForward.Size = new System.Drawing.Size(23, 22);
            btnForward.Text = "Forward";
            btnForward.Click += btnForward_Click;
            // 
            // btnFavourites
            // 
            btnFavourites.Image = (System.Drawing.Image)resources.GetObject("btnFavourites.Image");
            btnFavourites.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnFavourites.Name = "btnFavourites";
            btnFavourites.Size = new System.Drawing.Size(81, 22);
            btnFavourites.Text = "Favourites";
            btnFavourites.Click += ToolboxButtonClicked;
            // 
            // btnSavedCohorts
            // 
            btnSavedCohorts.Image = (System.Drawing.Image)resources.GetObject("btnSavedCohorts.Image");
            btnSavedCohorts.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnSavedCohorts.Name = "btnSavedCohorts";
            btnSavedCohorts.Size = new System.Drawing.Size(103, 22);
            btnSavedCohorts.Text = "Saved Cohorts";
            btnSavedCohorts.Click += ToolboxButtonClicked;
            // 
            // btnLoads
            // 
            btnLoads.Image = (System.Drawing.Image)resources.GetObject("btnLoads.Image");
            btnLoads.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnLoads.Name = "btnLoads";
            btnLoads.Size = new System.Drawing.Size(80, 22);
            btnLoads.Text = "Data Load";
            btnLoads.Click += ToolboxButtonClicked;
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new System.Drawing.Size(43, 22);
            toolStripLabel2.Text = "Layout";
            // 
            // cbxLayouts
            // 
            cbxLayouts.Name = "cbxLayouts";
            cbxLayouts.Size = new System.Drawing.Size(174, 25);
            cbxLayouts.DropDownClosed += cbx_DropDownClosed;
            cbxLayouts.SelectedIndexChanged += cbx_SelectedIndexChanged;
            // 
            // btnSaveWindowLayout
            // 
            btnSaveWindowLayout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnSaveWindowLayout.Enabled = false;
            btnSaveWindowLayout.Image = (System.Drawing.Image)resources.GetObject("btnSaveWindowLayout.Image");
            btnSaveWindowLayout.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnSaveWindowLayout.Name = "btnSaveWindowLayout";
            btnSaveWindowLayout.Size = new System.Drawing.Size(23, 22);
            btnSaveWindowLayout.Text = "Save Window Layout";
            btnSaveWindowLayout.Click += btnSaveWindowLayout_Click;
            // 
            // btnDeleteLayout
            // 
            btnDeleteLayout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnDeleteLayout.Enabled = false;
            btnDeleteLayout.Image = (System.Drawing.Image)resources.GetObject("btnDeleteLayout.Image");
            btnDeleteLayout.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnDeleteLayout.Name = "btnDeleteLayout";
            btnDeleteLayout.Size = new System.Drawing.Size(23, 22);
            btnDeleteLayout.Text = "Delete Layout";
            btnDeleteLayout.Click += btnDelete_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // cbCommits
            // 
            cbCommits.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            cbCommits.Image = (System.Drawing.Image)resources.GetObject("cbCommits.Image");
            cbCommits.ImageTransparentColor = System.Drawing.Color.Magenta;
            cbCommits.Name = "cbCommits";
            cbCommits.Size = new System.Drawing.Size(23, 22);
            cbCommits.Text = "Use Commits";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // RDMPTaskBarUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(toolStrip1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "RDMPTaskBarUI";
            Size = new System.Drawing.Size(1539, 29);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStripButton btnHome;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnCatalogues;
        private System.Windows.Forms.ToolStripButton btnCohorts;
        private System.Windows.Forms.ToolStripButton btnDataExport;
        private System.Windows.Forms.ToolStripButton btnTables;
        private System.Windows.Forms.ToolStripButton btnConfiguration;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnSavedCohorts;
        private System.Windows.Forms.ToolStripButton btnFavourites;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox cbxLayouts;
        private System.Windows.Forms.ToolStripButton btnDeleteLayout;
        private System.Windows.Forms.ToolStripButton btnSaveWindowLayout;
        private System.Windows.Forms.ToolStripButton btnForward;
        private System.Windows.Forms.ToolStripSplitButton btnBack;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton cbCommits;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnLoads;
    }
}
