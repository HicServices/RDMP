namespace DataExportManager.Collections
{
    partial class SavedCohortsCollectionUI
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
            this.components = new System.ComponentModel.Container();
            this.tlvSavedCohorts = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.tlvSavedCohorts)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvSavedCohorts
            // 
            this.tlvSavedCohorts.AllColumns.Add(this.olvName);
            this.tlvSavedCohorts.CellEditUseWholeCell = false;
            this.tlvSavedCohorts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.tlvSavedCohorts.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvSavedCohorts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvSavedCohorts.FullRowSelect = true;
            this.tlvSavedCohorts.HideSelection = false;
            this.tlvSavedCohorts.Location = new System.Drawing.Point(0, 0);
            this.tlvSavedCohorts.Name = "tlvSavedCohorts";
            this.tlvSavedCohorts.ShowGroups = false;
            this.tlvSavedCohorts.Size = new System.Drawing.Size(396, 647);
            this.tlvSavedCohorts.TabIndex = 1;
            this.tlvSavedCohorts.UseCompatibleStateImageBehavior = false;
            this.tlvSavedCohorts.View = System.Windows.Forms.View.Details;
            this.tlvSavedCohorts.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.CellEditUseWholeCell = true;
            this.olvName.FillsFreeSpace = true;
            this.olvName.Text = "Saved Cohorts";
            // 
            // SavedCohortsCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlvSavedCohorts);
            this.Name = "SavedCohortsCollectionUI";
            this.Size = new System.Drawing.Size(396, 647);
            ((System.ComponentModel.ISupportInitialize)(this.tlvSavedCohorts)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.TreeListView tlvSavedCohorts;
        private BrightIdeasSoftware.OLVColumn olvName;
    }
}
