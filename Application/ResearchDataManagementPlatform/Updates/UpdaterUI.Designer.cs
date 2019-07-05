namespace ResearchDataManagementPlatform.Updates
{
    partial class UpdaterUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdaterUI));
            this.lblStatus = new System.Windows.Forms.Label();
            this.objectListView1 = new BrightIdeasSoftware.ObjectListView();
            this.olvVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvInstall = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.cbShowOlderVersions = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(3, 3);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(117, 13);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Checking for updates...";
            // 
            // objectListView1
            // 
            this.objectListView1.AllColumns.Add(this.olvVersion);
            this.objectListView1.AllColumns.Add(this.olvType);
            this.objectListView1.AllColumns.Add(this.olvInstall);
            this.objectListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectListView1.CellEditUseWholeCell = false;
            this.objectListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvVersion,
            this.olvType,
            this.olvInstall});
            this.objectListView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.objectListView1.Location = new System.Drawing.Point(3, 28);
            this.objectListView1.Name = "objectListView1";
            this.objectListView1.RowHeight = 19;
            this.objectListView1.Size = new System.Drawing.Size(683, 217);
            this.objectListView1.TabIndex = 3;
            this.objectListView1.UseCompatibleStateImageBehavior = false;
            this.objectListView1.View = System.Windows.Forms.View.Details;
            // 
            // olvVersion
            // 
            this.olvVersion.Groupable = false;
            this.olvVersion.Text = "Version";
            this.olvVersion.Width = 100;
            // 
            // olvType
            // 
            this.olvType.Text = "Type";
            this.olvType.Width = 100;
            // 
            // olvInstall
            // 
            this.olvInstall.ButtonSizing = BrightIdeasSoftware.OLVColumn.ButtonSizingMode.CellBounds;
            this.olvInstall.Groupable = false;
            this.olvInstall.IsButton = true;
            this.olvInstall.Text = "";
            // 
            // pbLoading
            // 
            this.pbLoading.Image = ((System.Drawing.Image)(resources.GetObject("pbLoading.Image")));
            this.pbLoading.Location = new System.Drawing.Point(6, 50);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(19, 19);
            this.pbLoading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLoading.TabIndex = 4;
            this.pbLoading.TabStop = false;
            // 
            // cbShowOlderVersions
            // 
            this.cbShowOlderVersions.AutoSize = true;
            this.cbShowOlderVersions.Location = new System.Drawing.Point(571, 5);
            this.cbShowOlderVersions.Name = "cbShowOlderVersions";
            this.cbShowOlderVersions.Size = new System.Drawing.Size(115, 17);
            this.cbShowOlderVersions.TabIndex = 5;
            this.cbShowOlderVersions.Text = "Show Old Versions";
            this.cbShowOlderVersions.UseVisualStyleBackColor = true;
            this.cbShowOlderVersions.CheckedChanged += new System.EventHandler(this.CbShowOlderVersions_CheckedChanged);
            // 
            // UpdaterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbShowOlderVersions);
            this.Controls.Add(this.pbLoading);
            this.Controls.Add(this.objectListView1);
            this.Controls.Add(this.lblStatus);
            this.Name = "UpdaterUI";
            this.Size = new System.Drawing.Size(693, 248);
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblStatus;
        private BrightIdeasSoftware.ObjectListView objectListView1;
        private BrightIdeasSoftware.OLVColumn olvVersion;
        private BrightIdeasSoftware.OLVColumn olvType;
        private BrightIdeasSoftware.OLVColumn olvInstall;
        private System.Windows.Forms.PictureBox pbLoading;
        private System.Windows.Forms.CheckBox cbShowOlderVersions;
    }
}
