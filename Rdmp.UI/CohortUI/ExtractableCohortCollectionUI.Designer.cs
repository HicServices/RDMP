using Rdmp.UI.Refreshing;

namespace Rdmp.UI.CohortUI
{
    partial class ExtractableCohortCollectionUI : ILifetimeSubscriber
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
            this.olvDescription = new BrightIdeasSoftware.OLVColumn();
            this.olvSource = new BrightIdeasSoftware.OLVColumn();
            this.olvOriginID = new BrightIdeasSoftware.OLVColumn();
            this.olvCount = new BrightIdeasSoftware.OLVColumn();
            this.olvCountDistinct = new BrightIdeasSoftware.OLVColumn();
            this.olvPrivateIdentifier = new BrightIdeasSoftware.OLVColumn();
            this.olvReleaseIdentifier = new BrightIdeasSoftware.OLVColumn();
            this.olvProjectNumber = new BrightIdeasSoftware.OLVColumn();
            this.olvVersion = new BrightIdeasSoftware.OLVColumn();
            this.olvCreationDate = new BrightIdeasSoftware.OLVColumn();
            this.olvViewLog = new BrightIdeasSoftware.OLVColumn();
            this.olvID = new BrightIdeasSoftware.OLVColumn();
            this.tbFilter = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.lbCohortDatabaseTable)).BeginInit();
            this.groupBox1.SuspendLayout();
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
            this.lbCohortDatabaseTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbCohortDatabaseTable.FullRowSelect = true;
            this.lbCohortDatabaseTable.HideSelection = false;
            this.lbCohortDatabaseTable.Location = new System.Drawing.Point(0, 0);
            this.lbCohortDatabaseTable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lbCohortDatabaseTable.Name = "lbCohortDatabaseTable";
            this.lbCohortDatabaseTable.Size = new System.Drawing.Size(666, 803);
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
            // olvID
            // 
            this.olvID.Text = "ID";
            // 
            // tbFilter
            // 
            this.tbFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbFilter.Location = new System.Drawing.Point(3, 19);
            this.tbFilter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbFilter.Name = "tbFilter";
            this.tbFilter.Size = new System.Drawing.Size(660, 23);
            this.tbFilter.TabIndex = 9;
            this.tbFilter.TextChanged += new System.EventHandler(this.tbFilter_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tbFilter);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 803);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(666, 49);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Filter:";
            // 
            // ExtractableCohortCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbCohortDatabaseTable);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ExtractableCohortCollectionUI";
            this.Size = new System.Drawing.Size(666, 852);
            ((System.ComponentModel.ISupportInitialize)(this.lbCohortDatabaseTable)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.TextBox tbFilter;
        private BrightIdeasSoftware.OLVColumn olvSource;
        private BrightIdeasSoftware.OLVColumn olvViewLog;
        private BrightIdeasSoftware.OLVColumn olvID;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}
