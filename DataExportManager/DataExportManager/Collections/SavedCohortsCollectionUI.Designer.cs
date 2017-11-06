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
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tlvSavedCohorts)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvSavedCohorts
            // 
            this.tlvSavedCohorts.AllColumns.Add(this.olvName);
            this.tlvSavedCohorts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvSavedCohorts.CellEditUseWholeCell = false;
            this.tlvSavedCohorts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.tlvSavedCohorts.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvSavedCohorts.FullRowSelect = true;
            this.tlvSavedCohorts.HideSelection = false;
            this.tlvSavedCohorts.Location = new System.Drawing.Point(7, -2);
            this.tlvSavedCohorts.Name = "tlvSavedCohorts";
            this.tlvSavedCohorts.ShowGroups = false;
            this.tlvSavedCohorts.Size = new System.Drawing.Size(389, 620);
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
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(40, 624);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(353, 20);
            this.tbFilter.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 627);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Filter:";
            // 
            // SavedCohortsCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tlvSavedCohorts);
            this.Name = "SavedCohortsCollectionUI";
            this.Size = new System.Drawing.Size(396, 647);
            ((System.ComponentModel.ISupportInitialize)(this.tlvSavedCohorts)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.TreeListView tlvSavedCohorts;
        private BrightIdeasSoftware.OLVColumn olvName;
        private System.Windows.Forms.TextBox tbFilter;
        private System.Windows.Forms.Label label1;
    }
}
