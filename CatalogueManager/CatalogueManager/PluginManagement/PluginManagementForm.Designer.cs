using BrightIdeasSoftware;

namespace CatalogueManager.PluginManagement
{
    partial class PluginManagementForm
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
            this.components = new System.ComponentModel.Container();
            this.treeListView = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.pluginDependencyVisualisation1 = new CatalogueManager.PluginManagement.PluginDependencyVisualisation();
            this.checksUI1 = new ReusableUIComponents.ChecksUI.ChecksUI();
            this.pbAnalysing = new System.Windows.Forms.ProgressBar();
            this.lblProgressAnalysing = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnSaveToRemote = new System.Windows.Forms.Button();
            this.btnExportToDisk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).BeginInit();
            this.SuspendLayout();
            // 
            // treeListView
            // 
            this.treeListView.AllColumns.Add(this.olvName);
            this.treeListView.AllowDrop = true;
            this.treeListView.CellEditUseWholeCell = false;
            this.treeListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.treeListView.Location = new System.Drawing.Point(12, 48);
            this.treeListView.Name = "treeListView";
            this.treeListView.ShowGroups = false;
            this.treeListView.Size = new System.Drawing.Size(654, 393);
            this.treeListView.TabIndex = 0;
            this.treeListView.UseCompatibleStateImageBehavior = false;
            this.treeListView.View = System.Windows.Forms.View.Details;
            this.treeListView.VirtualMode = true;
            this.treeListView.ItemActivate += new System.EventHandler(this.treeListView_ItemActivate);
            this.treeListView.SelectedIndexChanged += new System.EventHandler(this.treeListView_SelectedIndexChanged);
            this.treeListView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyUp);
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.Text = "Plugin Name";
            this.olvName.Width = 201;
            // 
            // pluginDependencyVisualisation1
            // 
            this.pluginDependencyVisualisation1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pluginDependencyVisualisation1.AutoSize = true;
            this.pluginDependencyVisualisation1.Location = new System.Drawing.Point(673, 12);
            this.pluginDependencyVisualisation1.Name = "pluginDependencyVisualisation1";
            this.pluginDependencyVisualisation1.Size = new System.Drawing.Size(702, 708);
            this.pluginDependencyVisualisation1.TabIndex = 1;
            // 
            // checksUI1
            // 
            this.checksUI1.Location = new System.Drawing.Point(12, 492);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(655, 228);
            this.checksUI1.TabIndex = 2;
            // 
            // pbAnalysing
            // 
            this.pbAnalysing.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbAnalysing.Location = new System.Drawing.Point(12, 726);
            this.pbAnalysing.Name = "pbAnalysing";
            this.pbAnalysing.Size = new System.Drawing.Size(1363, 10);
            this.pbAnalysing.TabIndex = 3;
            // 
            // lblProgressAnalysing
            // 
            this.lblProgressAnalysing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblProgressAnalysing.AutoSize = true;
            this.lblProgressAnalysing.Location = new System.Drawing.Point(12, 739);
            this.lblProgressAnalysing.Name = "lblProgressAnalysing";
            this.lblProgressAnalysing.Size = new System.Drawing.Size(110, 13);
            this.lblProgressAnalysing.TabIndex = 4;
            this.lblProgressAnalysing.Text = "No Plugins Found Yet";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(12, 19);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(107, 23);
            this.btnBrowse.TabIndex = 5;
            this.btnBrowse.Text = "Add Plugin...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnSaveToRemote
            // 
            this.btnSaveToRemote.Location = new System.Drawing.Point(12, 447);
            this.btnSaveToRemote.Name = "btnSaveToRemote";
            this.btnSaveToRemote.Size = new System.Drawing.Size(350, 39);
            this.btnSaveToRemote.TabIndex = 3;
            this.btnSaveToRemote.Text = "Save to Remotes";
            this.btnSaveToRemote.UseVisualStyleBackColor = true;
            this.btnSaveToRemote.Click += new System.EventHandler(this.btnSaveToRemote_Click);
            // 
            // btnExportToDisk
            // 
            this.btnExportToDisk.Location = new System.Drawing.Point(368, 447);
            this.btnExportToDisk.Name = "btnExportToDisk";
            this.btnExportToDisk.Size = new System.Drawing.Size(298, 39);
            this.btnExportToDisk.TabIndex = 6;
            this.btnExportToDisk.Text = "Export All To Disk";
            this.btnExportToDisk.UseVisualStyleBackColor = true;
            this.btnExportToDisk.Click += new System.EventHandler(this.btnExportToDisk_Click);
            // 
            // PluginManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1387, 759);
            this.Controls.Add(this.btnExportToDisk);
            this.Controls.Add(this.btnSaveToRemote);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblProgressAnalysing);
            this.Controls.Add(this.pbAnalysing);
            this.Controls.Add(this.checksUI1);
            this.Controls.Add(this.pluginDependencyVisualisation1);
            this.Controls.Add(this.treeListView);
            this.Name = "PluginManagementForm";
            this.Text = "PluginManagementForm";
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TreeListView treeListView;
        private OLVColumn olvName;
        private PluginDependencyVisualisation pluginDependencyVisualisation1;
        private ReusableUIComponents.ChecksUI.ChecksUI checksUI1;
        private System.Windows.Forms.ProgressBar pbAnalysing;
        private System.Windows.Forms.Label lblProgressAnalysing;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnSaveToRemote;
        private System.Windows.Forms.Button btnExportToDisk;
    }
}