using CatalogueManager.Refreshing;

namespace DataExportManager.CohortUI
{
    partial class ExtractableCohortCollection : ILifetimeSubscriber
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
            this.lbCohortDatabaseTable = new BrightIdeasSoftware.ObjectListView();
            this.olvDescription = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvSource = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvOriginID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCount = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCountDistinct = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvPrivateIdentifier = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvReleaseIdentifier = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvProjectNumber = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvVersion = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCreationDate = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvViewLog = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label2 = new System.Windows.Forms.Label();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.olvID = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.lbCohortDatabaseTable)).BeginInit();
            this.SuspendLayout();
            // 
            // lbCohortDatabaseTable
            // 
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvDescription);
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvSource);
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvOriginID);
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvCount);
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvCountDistinct);
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvPrivateIdentifier);
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvReleaseIdentifier);
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvProjectNumber);
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvVersion);
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvCreationDate);
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvViewLog);
            this.lbCohortDatabaseTable.AllColumns.Add(this.olvID);
            this.lbCohortDatabaseTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbCohortDatabaseTable.CellEditUseWholeCell = false;
            this.lbCohortDatabaseTable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvDescription,
            this.olvOriginID,
            this.olvCount,
            this.olvCountDistinct,
            this.olvPrivateIdentifier,
            this.olvReleaseIdentifier,
            this.olvProjectNumber,
            this.olvVersion,
            this.olvCreationDate,
            this.olvViewLog,
            this.olvID});
            this.lbCohortDatabaseTable.Cursor = System.Windows.Forms.Cursors.Default;
            this.lbCohortDatabaseTable.FullRowSelect = true;
            this.lbCohortDatabaseTable.HideSelection = false;
            this.lbCohortDatabaseTable.Location = new System.Drawing.Point(3, 0);
            this.lbCohortDatabaseTable.Name = "lbCohortDatabaseTable";
            this.lbCohortDatabaseTable.Size = new System.Drawing.Size(568, 709);
            this.lbCohortDatabaseTable.TabIndex = 2;
            this.lbCohortDatabaseTable.UseCompatibleStateImageBehavior = false;
            this.lbCohortDatabaseTable.View = System.Windows.Forms.View.Details;
            this.lbCohortDatabaseTable.SelectionChanged += new System.EventHandler(this.lbCohortDatabaseTable_SelectionChanged);
            this.lbCohortDatabaseTable.ItemActivate += new System.EventHandler(this.lbCohortDatabaseTable_ItemActivate);
            this.lbCohortDatabaseTable.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbCohortDatabaseTable_KeyUp);
            // 
            // olvDescription
            // 
            this.olvDescription.AspectName = "Description";
            this.olvDescription.Text = "Description";
            this.olvDescription.Width = 200;
            // 
            // olvSource
            // 
            this.olvSource.AspectName = "SourceName";
            this.olvSource.DisplayIndex = 0;
            this.olvSource.IsVisible = false;
            this.olvSource.Text = "SourceName";
            // 
            // olvOriginID
            // 
            this.olvOriginID.AspectName = "OriginID";
            this.olvOriginID.Text = "OriginID";
            // 
            // olvCount
            // 
            this.olvCount.AspectName = "Count";
            this.olvCount.Text = "Count";
            // 
            // olvCountDistinct
            // 
            this.olvCountDistinct.AspectName = "CountDistinct";
            this.olvCountDistinct.Text = "Count Distinct";
            this.olvCountDistinct.Width = 90;
            // 
            // olvPrivateIdentifier
            // 
            this.olvPrivateIdentifier.AspectName = "PrivateIdentifier";
            this.olvPrivateIdentifier.Text = "Private Identifier";
            this.olvPrivateIdentifier.Width = 88;
            // 
            // olvReleaseIdentifier
            // 
            this.olvReleaseIdentifier.AspectName = "ReleaseIdentifier";
            this.olvReleaseIdentifier.Text = "Release Identifier";
            this.olvReleaseIdentifier.Width = 94;
            // 
            // olvProjectNumber
            // 
            this.olvProjectNumber.AspectName = "ProjectNumber";
            this.olvProjectNumber.Text = "Project Number";
            this.olvProjectNumber.Width = 92;
            // 
            // olvVersion
            // 
            this.olvVersion.AspectName = "Version";
            this.olvVersion.Text = "Version";
            // 
            // olvCreationDate
            // 
            this.olvCreationDate.AspectName = "CreationDate";
            this.olvCreationDate.Text = "CreationDate";
            this.olvCreationDate.Width = 100;
            // 
            // olvViewLog
            // 
            this.olvViewLog.ButtonSizing = BrightIdeasSoftware.OLVColumn.ButtonSizingMode.CellBounds;
            this.olvViewLog.IsButton = true;
            this.olvViewLog.Text = "Audit Log";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 718);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Filter:";
            // 
            // tbFilter
            // 
            this.tbFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilter.Location = new System.Drawing.Point(44, 715);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(524, 20);
            this.tbFilter.TabIndex = 9;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // olvID
            // 
            this.olvID.Text = "ID";
            // 
            // ExtractableCohortCollection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbFilter);
            this.Controls.Add(this.lbCohortDatabaseTable);
            this.Name = "ExtractableCohortCollection";
            this.Size = new System.Drawing.Size(571, 738);
            ((System.ComponentModel.ISupportInitialize)(this.lbCohortDatabaseTable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.ObjectListView lbCohortDatabaseTable;
        private BrightIdeasSoftware.OLVColumn olvDescription;
        private BrightIdeasSoftware.OLVColumn olvOriginID;
        private BrightIdeasSoftware.OLVColumn olvCount;
        private BrightIdeasSoftware.OLVColumn olvCountDistinct;
        private BrightIdeasSoftware.OLVColumn olvPrivateIdentifier;
        private BrightIdeasSoftware.OLVColumn olvReleaseIdentifier;
        private BrightIdeasSoftware.OLVColumn olvProjectNumber;
        private BrightIdeasSoftware.OLVColumn olvVersion;
        private BrightIdeasSoftware.OLVColumn olvCreationDate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbFilter;
        private BrightIdeasSoftware.OLVColumn olvSource;
        private BrightIdeasSoftware.OLVColumn olvViewLog;
        private BrightIdeasSoftware.OLVColumn olvID;
    }
}
