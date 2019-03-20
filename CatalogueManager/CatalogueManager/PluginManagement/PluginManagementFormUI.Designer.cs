using BrightIdeasSoftware;

namespace CatalogueManager.PluginManagement
{
    partial class PluginManagementFormUI
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
            this.olvPlugins = new BrightIdeasSoftware.ObjectListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvPluginName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.pluginDependencyVisualisation1 = new CatalogueManager.PluginManagement.PluginDependencyVisualisationUI();
            this.pbAnalysing = new System.Windows.Forms.ProgressBar();
            this.lblProgressAnalysing = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnSaveToRemote = new System.Windows.Forms.Button();
            this.btnExportToDisk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.olvLegacyPlugins = new BrightIdeasSoftware.ObjectListView();
            this.olvLegacyName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvLegacyPluginName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvLegacyVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.olvPlugins)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvLegacyPlugins)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // olvPlugins
            // 
            this.olvPlugins.AllColumns.Add(this.olvName);
            this.olvPlugins.AllColumns.Add(this.olvPluginName);
            this.olvPlugins.AllColumns.Add(this.olvVersion);
            this.olvPlugins.AllowDrop = true;
            this.olvPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvPlugins.CellEditUseWholeCell = false;
            this.olvPlugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvVersion});
            this.olvPlugins.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvPlugins.FullRowSelect = true;
            this.olvPlugins.Location = new System.Drawing.Point(3, 21);
            this.olvPlugins.Name = "olvPlugins";
            this.olvPlugins.Size = new System.Drawing.Size(646, 318);
            this.olvPlugins.TabIndex = 0;
            this.olvPlugins.UseCompatibleStateImageBehavior = false;
            this.olvPlugins.View = System.Windows.Forms.View.Details;
            this.olvPlugins.ItemActivate += new System.EventHandler(this.treeListView_ItemActivate);
            this.olvPlugins.SelectedIndexChanged += new System.EventHandler(this.treeListView_SelectedIndexChanged);
            this.olvPlugins.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyUp);
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.Text = "Dll";
            this.olvName.Width = 201;
            // 
            // olvPluginName
            // 
            this.olvPluginName.DisplayIndex = 1;
            this.olvPluginName.IsVisible = false;
            this.olvPluginName.Text = "Plugin";
            // 
            // olvVersion
            // 
            this.olvVersion.AspectName = "";
            this.olvVersion.Text = "Version";
            this.olvVersion.Width = 120;
            // 
            // pluginDependencyVisualisation1
            // 
            this.pluginDependencyVisualisation1.AutoSize = true;
            this.pluginDependencyVisualisation1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pluginDependencyVisualisation1.Location = new System.Drawing.Point(0, 0);
            this.pluginDependencyVisualisation1.Name = "pluginDependencyVisualisation1";
            this.pluginDependencyVisualisation1.Size = new System.Drawing.Size(707, 672);
            this.pluginDependencyVisualisation1.TabIndex = 1;
            // 
            // pbAnalysing
            // 
            this.pbAnalysing.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbAnalysing.Location = new System.Drawing.Point(18, 726);
            this.pbAnalysing.Name = "pbAnalysing";
            this.pbAnalysing.Size = new System.Drawing.Size(1357, 10);
            this.pbAnalysing.TabIndex = 3;
            // 
            // lblProgressAnalysing
            // 
            this.lblProgressAnalysing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblProgressAnalysing.AutoSize = true;
            this.lblProgressAnalysing.Location = new System.Drawing.Point(15, 739);
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
            this.btnSaveToRemote.Location = new System.Drawing.Point(125, 19);
            this.btnSaveToRemote.Name = "btnSaveToRemote";
            this.btnSaveToRemote.Size = new System.Drawing.Size(107, 23);
            this.btnSaveToRemote.TabIndex = 3;
            this.btnSaveToRemote.Text = "Save to Remotes";
            this.btnSaveToRemote.UseVisualStyleBackColor = true;
            this.btnSaveToRemote.Click += new System.EventHandler(this.btnSaveToRemote_Click);
            // 
            // btnExportToDisk
            // 
            this.btnExportToDisk.Location = new System.Drawing.Point(238, 19);
            this.btnExportToDisk.Name = "btnExportToDisk";
            this.btnExportToDisk.Size = new System.Drawing.Size(132, 23);
            this.btnExportToDisk.TabIndex = 6;
            this.btnExportToDisk.Text = "Export All To Disk";
            this.btnExportToDisk.UseVisualStyleBackColor = true;
            this.btnExportToDisk.Click += new System.EventHandler(this.btnExportToDisk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Loaded Plugins";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Legacy Plugins (Not Loaded)";
            // 
            // olvLegacyPlugins
            // 
            this.olvLegacyPlugins.AllColumns.Add(this.olvLegacyName);
            this.olvLegacyPlugins.AllColumns.Add(this.olvLegacyPluginName);
            this.olvLegacyPlugins.AllColumns.Add(this.olvLegacyVersion);
            this.olvLegacyPlugins.AllowDrop = true;
            this.olvLegacyPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvLegacyPlugins.CellEditUseWholeCell = false;
            this.olvLegacyPlugins.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvLegacyName,
            this.olvLegacyVersion});
            this.olvLegacyPlugins.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvLegacyPlugins.FullRowSelect = true;
            this.olvLegacyPlugins.Location = new System.Drawing.Point(6, 21);
            this.olvLegacyPlugins.Name = "olvLegacyPlugins";
            this.olvLegacyPlugins.Size = new System.Drawing.Size(643, 300);
            this.olvLegacyPlugins.TabIndex = 9;
            this.olvLegacyPlugins.UseCompatibleStateImageBehavior = false;
            this.olvLegacyPlugins.View = System.Windows.Forms.View.Details;
            this.olvLegacyPlugins.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyUp);
            // 
            // olvLegacyName
            // 
            this.olvLegacyName.AspectName = "ToString";
            this.olvLegacyName.FillsFreeSpace = true;
            this.olvLegacyName.Text = "Dll";
            this.olvLegacyName.Width = 201;
            // 
            // olvLegacyPluginName
            // 
            this.olvLegacyPluginName.DisplayIndex = 1;
            this.olvLegacyPluginName.IsVisible = false;
            this.olvLegacyPluginName.Text = "Plugin";
            // 
            // olvLegacyVersion
            // 
            this.olvLegacyVersion.AspectName = "";
            this.olvLegacyVersion.Text = "Version";
            this.olvLegacyVersion.Width = 120;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 48);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pluginDependencyVisualisation1);
            this.splitContainer1.Size = new System.Drawing.Size(1363, 672);
            this.splitContainer1.SplitterDistance = 652;
            this.splitContainer1.TabIndex = 10;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.olvPlugins);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.label2);
            this.splitContainer2.Panel2.Controls.Add(this.olvLegacyPlugins);
            this.splitContainer2.Size = new System.Drawing.Size(652, 672);
            this.splitContainer2.SplitterDistance = 342;
            this.splitContainer2.TabIndex = 0;
            // 
            // PluginManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1387, 759);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnExportToDisk);
            this.Controls.Add(this.btnSaveToRemote);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblProgressAnalysing);
            this.Controls.Add(this.pbAnalysing);
            this.Name = "PluginManagementForm";
            this.Text = "PluginManagementForm";
            ((System.ComponentModel.ISupportInitialize)(this.olvPlugins)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvLegacyPlugins)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ObjectListView olvPlugins;
        private OLVColumn olvName;
        private PluginDependencyVisualisationUI pluginDependencyVisualisation1;
        private System.Windows.Forms.ProgressBar pbAnalysing;
        private System.Windows.Forms.Label lblProgressAnalysing;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnSaveToRemote;
        private System.Windows.Forms.Button btnExportToDisk;
        private OLVColumn olvVersion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private OLVColumn olvPluginName;
        private ObjectListView olvLegacyPlugins;
        private OLVColumn olvLegacyName;
        private OLVColumn olvLegacyPluginName;
        private OLVColumn olvLegacyVersion;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
    }
}