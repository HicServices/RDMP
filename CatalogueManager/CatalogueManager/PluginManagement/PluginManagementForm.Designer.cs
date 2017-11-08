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
            this.grpRemotes = new System.Windows.Forms.GroupBox();
            this.lblRemoteResult = new System.Windows.Forms.Label();
            this.barRemoteSave = new System.Windows.Forms.ProgressBar();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lblCredentials = new System.Windows.Forms.Label();
            this.lblEndpoint = new System.Windows.Forms.Label();
            this.btnSaveToRemote = new System.Windows.Forms.Button();
            this.ddCredentials = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).BeginInit();
            this.grpRemotes.SuspendLayout();
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
            this.treeListView.Size = new System.Drawing.Size(654, 424);
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
            this.pluginDependencyVisualisation1.Size = new System.Drawing.Size(702, 545);
            this.pluginDependencyVisualisation1.TabIndex = 1;
            // 
            // checksUI1
            // 
            this.checksUI1.Location = new System.Drawing.Point(12, 478);
            this.checksUI1.Name = "checksUI1";
            this.checksUI1.Size = new System.Drawing.Size(655, 242);
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
            // grpRemotes
            // 
            this.grpRemotes.Controls.Add(this.lblRemoteResult);
            this.grpRemotes.Controls.Add(this.barRemoteSave);
            this.grpRemotes.Controls.Add(this.textBox1);
            this.grpRemotes.Controls.Add(this.lblCredentials);
            this.grpRemotes.Controls.Add(this.lblEndpoint);
            this.grpRemotes.Controls.Add(this.btnSaveToRemote);
            this.grpRemotes.Controls.Add(this.ddCredentials);
            this.grpRemotes.Location = new System.Drawing.Point(673, 563);
            this.grpRemotes.Name = "grpRemotes";
            this.grpRemotes.Size = new System.Drawing.Size(365, 157);
            this.grpRemotes.TabIndex = 6;
            this.grpRemotes.TabStop = false;
            this.grpRemotes.Text = "Remotes";
            // 
            // lblRemoteResult
            // 
            this.lblRemoteResult.AutoSize = true;
            this.lblRemoteResult.Location = new System.Drawing.Point(6, 137);
            this.lblRemoteResult.Name = "lblRemoteResult";
            this.lblRemoteResult.Size = new System.Drawing.Size(35, 13);
            this.lblRemoteResult.TabIndex = 8;
            this.lblRemoteResult.Text = "label1";
            // 
            // barRemoteSave
            // 
            this.barRemoteSave.Location = new System.Drawing.Point(9, 107);
            this.barRemoteSave.MarqueeAnimationSpeed = 0;
            this.barRemoteSave.Name = "barRemoteSave";
            this.barRemoteSave.Size = new System.Drawing.Size(350, 23);
            this.barRemoteSave.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.barRemoteSave.TabIndex = 7;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(68, 25);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(291, 20);
            this.textBox1.TabIndex = 6;
            // 
            // lblCredentials
            // 
            this.lblCredentials.AutoSize = true;
            this.lblCredentials.Location = new System.Drawing.Point(6, 54);
            this.lblCredentials.Name = "lblCredentials";
            this.lblCredentials.Size = new System.Drawing.Size(59, 13);
            this.lblCredentials.TabIndex = 5;
            this.lblCredentials.Text = "Credentials";
            // 
            // lblEndpoint
            // 
            this.lblEndpoint.AutoSize = true;
            this.lblEndpoint.Location = new System.Drawing.Point(6, 25);
            this.lblEndpoint.Name = "lblEndpoint";
            this.lblEndpoint.Size = new System.Drawing.Size(49, 13);
            this.lblEndpoint.TabIndex = 5;
            this.lblEndpoint.Text = "Endpoint";
            // 
            // btnSaveToRemote
            // 
            this.btnSaveToRemote.Location = new System.Drawing.Point(9, 78);
            this.btnSaveToRemote.Name = "btnSaveToRemote";
            this.btnSaveToRemote.Size = new System.Drawing.Size(350, 23);
            this.btnSaveToRemote.TabIndex = 3;
            this.btnSaveToRemote.Text = "Save to Remotes";
            this.btnSaveToRemote.UseVisualStyleBackColor = true;
            this.btnSaveToRemote.Click += new System.EventHandler(this.btnSaveToRemote_Click);
            // 
            // ddCredentials
            // 
            this.ddCredentials.FormattingEnabled = true;
            this.ddCredentials.Location = new System.Drawing.Point(68, 51);
            this.ddCredentials.Name = "ddCredentials";
            this.ddCredentials.Size = new System.Drawing.Size(291, 21);
            this.ddCredentials.TabIndex = 4;
            // 
            // PluginManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1387, 759);
            this.Controls.Add(this.grpRemotes);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblProgressAnalysing);
            this.Controls.Add(this.pbAnalysing);
            this.Controls.Add(this.checksUI1);
            this.Controls.Add(this.pluginDependencyVisualisation1);
            this.Controls.Add(this.treeListView);
            this.Name = "PluginManagementForm";
            this.Text = "PluginManagementForm";
            ((System.ComponentModel.ISupportInitialize)(this.treeListView)).EndInit();
            this.grpRemotes.ResumeLayout(false);
            this.grpRemotes.PerformLayout();
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
        private System.Windows.Forms.GroupBox grpRemotes;
        private System.Windows.Forms.Label lblRemoteResult;
        private System.Windows.Forms.ProgressBar barRemoteSave;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lblCredentials;
        private System.Windows.Forms.Label lblEndpoint;
        private System.Windows.Forms.Button btnSaveToRemote;
        private System.Windows.Forms.ComboBox ddCredentials;
    }
}