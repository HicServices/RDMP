

using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI;

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
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            btnBack = new System.Windows.Forms.ToolStripSplitButton();
            btnForward = new System.Windows.Forms.ToolStripButton();
            btnLoads = new System.Windows.Forms.ToolStripButton();
            btnSavedCohorts = new System.Windows.Forms.ToolStripButton();
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
            btnHome.Text = "";
            btnHome.Click += btnHome_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnCatalogues
            // 
            btnCatalogues.Image = CatalogueIcons.Catalogue.ImageToBitmap();
            btnCatalogues.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnCatalogues.Name = "btnCatalogues";
            btnCatalogues.Size = new System.Drawing.Size(86, 22);
            btnCatalogues.Text = "Catalogues";
            btnCatalogues.Click += ToolboxButtonClicked;
            // 
            // btnCohorts
            // 
            btnCohorts.Image = CatalogueIcons.CohortIdentificationConfiguration.ImageToBitmap();
            btnCohorts.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnCohorts.Name = "btnCohorts";
            btnCohorts.Size = new System.Drawing.Size(104, 22);
            btnCohorts.Text = "Cohort Builder";
            btnCohorts.Click += ToolboxButtonClicked;
            // 
            // btnDataExport
            // 
            btnDataExport.Image = CatalogueIcons.Project.ImageToBitmap();
            btnDataExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnDataExport.Name = "btnDataExport";
            btnDataExport.Size = new System.Drawing.Size(69, 22);
            btnDataExport.Text = "Projects";
            btnDataExport.Click += ToolboxButtonClicked;
            // 
            // btnTables
            // 
            btnTables.Image = CatalogueIcons.Settings.ImageToBitmap();
            btnTables.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnTables.Name = "btnTables";
            btnTables.Size = new System.Drawing.Size(65, 22);
            btnTables.Text = "System";
            btnTables.Click += ToolboxButtonClicked;
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
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnBack, btnForward, btnHome, toolStripSeparator1, btnLoads, btnCatalogues, btnCohorts, btnDataExport, toolStripSeparator, btnTables, btnSavedCohorts, toolStripSeparator2, cbCommits, toolStripSeparator3 });
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
            btnBack.Image = CatalogueIcons.Back.ImageToBitmap();
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
            btnForward.Image = CatalogueIcons.Forward.ImageToBitmap();
            btnForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnForward.Name = "btnForward";
            btnForward.Size = new System.Drawing.Size(23, 22);
            btnForward.Text = "Forward";
            btnForward.Click += btnForward_Click;
            // 
            // btnLoads
            // 
            btnLoads.Image = CatalogueIcons.LoadMetadata.ImageToBitmap();
            btnLoads.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnLoads.Name = "btnLoads";
            btnLoads.Size = new System.Drawing.Size(80, 22);
            btnLoads.Text = "Data Load";
            btnLoads.Click += ToolboxButtonClicked;
            // 
            // btnSavedCohorts
            // 
            btnSavedCohorts.Image = CatalogueIcons.CohortIdentificationConfiguration.ImageToBitmap();
            btnSavedCohorts.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnSavedCohorts.Name = "btnSavedCohorts";
            btnSavedCohorts.Size = new System.Drawing.Size(103, 22);
            btnSavedCohorts.Text = "Saved Cohorts";
            btnSavedCohorts.Click += ToolboxButtonClicked;
            // 
            // cbCommits
            // 
            cbCommits.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            cbCommits.Image = CatalogueIcons.Commit.ImageToBitmap();
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnSavedCohorts;
        private System.Windows.Forms.ToolStripButton btnForward;
        private System.Windows.Forms.ToolStripSplitButton btnBack;
        private System.Windows.Forms.ToolStripButton cbCommits;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnLoads;
    }
}
