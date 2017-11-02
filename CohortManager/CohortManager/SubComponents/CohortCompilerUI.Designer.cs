using BrightIdeasSoftware;
using CatalogueManager.ItemActivation;

namespace CohortManager.SubComponents
{
    partial class CohortCompilerUI 
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
            this.tlvConfiguration = new BrightIdeasSoftware.TreeListView();
            this.olvAggregate = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCatalogue = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvIdentifierCount = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCumulativeTotal = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvWorking = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvOrder = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvTime = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCachedQueryUseCount = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.label1 = new System.Windows.Forms.Label();
            this.tbTimeout = new System.Windows.Forms.TextBox();
            this.refreshThreadCountPeriodically = new System.Windows.Forms.Timer(this.components);
            this.lblThreadCount = new System.Windows.Forms.Label();
            this.cbIncludeCumulative = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.tlvConfiguration)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvConfiguration
            // 
            this.tlvConfiguration.AllColumns.Add(this.olvAggregate);
            this.tlvConfiguration.AllColumns.Add(this.olvCatalogue);
            this.tlvConfiguration.AllColumns.Add(this.olvIdentifierCount);
            this.tlvConfiguration.AllColumns.Add(this.olvCumulativeTotal);
            this.tlvConfiguration.AllColumns.Add(this.olvWorking);
            this.tlvConfiguration.AllColumns.Add(this.olvOrder);
            this.tlvConfiguration.AllColumns.Add(this.olvTime);
            this.tlvConfiguration.AllColumns.Add(this.olvCachedQueryUseCount);
            this.tlvConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlvConfiguration.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tlvConfiguration.CellEditUseWholeCell = false;
            this.tlvConfiguration.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvAggregate,
            this.olvCatalogue,
            this.olvIdentifierCount,
            this.olvCumulativeTotal,
            this.olvWorking,
            this.olvTime,
            this.olvCachedQueryUseCount});
            this.tlvConfiguration.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvConfiguration.FullRowSelect = true;
            this.tlvConfiguration.HideSelection = false;
            this.tlvConfiguration.Location = new System.Drawing.Point(4, 0);
            this.tlvConfiguration.Name = "tlvConfiguration";
            this.tlvConfiguration.ShowGroups = false;
            this.tlvConfiguration.Size = new System.Drawing.Size(536, 603);
            this.tlvConfiguration.TabIndex = 0;
            this.tlvConfiguration.UseCompatibleStateImageBehavior = false;
            this.tlvConfiguration.View = System.Windows.Forms.View.Details;
            this.tlvConfiguration.VirtualMode = true;
            this.tlvConfiguration.ItemActivate += new System.EventHandler(this.otvConfiguration_ItemActivate);
            this.tlvConfiguration.KeyUp += new System.Windows.Forms.KeyEventHandler(this.otvConfiguration_KeyUp);
            // 
            // olvAggregate
            // 
            this.olvAggregate.AspectName = "ToString";
            this.olvAggregate.FillsFreeSpace = true;
            this.olvAggregate.Sortable = false;
            this.olvAggregate.Text = "Aggregate";
            // 
            // olvCatalogue
            // 
            this.olvCatalogue.AspectName = "GetCatalogueName";
            this.olvCatalogue.Sortable = false;
            this.olvCatalogue.Text = "Catalogue";
            this.olvCatalogue.Width = 70;
            // 
            // olvIdentifierCount
            // 
            this.olvIdentifierCount.AspectName = "FinalRowCount";
            this.olvIdentifierCount.Sortable = false;
            this.olvIdentifierCount.Text = "Identifier Count";
            this.olvIdentifierCount.Width = 90;
            // 
            // olvCumulativeTotal
            // 
            this.olvCumulativeTotal.AspectName = "CumulativeRowCount";
            this.olvCumulativeTotal.Text = "Cumulative Total";
            // 
            // olvWorking
            // 
            this.olvWorking.AspectName = "GetStateDescription";
            this.olvWorking.Sortable = false;
            this.olvWorking.Text = "Working";
            this.olvWorking.Width = 80;
            // 
            // olvOrder
            // 
            this.olvOrder.AspectName = "Order";
            this.olvOrder.AspectToStringFormat = "";
            this.olvOrder.DisplayIndex = 5;
            this.olvOrder.IsVisible = false;
            this.olvOrder.Sortable = false;
            this.olvOrder.Text = "Order";
            // 
            // olvTime
            // 
            this.olvTime.AspectName = "ElapsedTime";
            this.olvTime.Text = "Time";
            // 
            // olvCachedQueryUseCount
            // 
            this.olvCachedQueryUseCount.AspectName = "GetCachedQueryUseCount";
            this.olvCachedQueryUseCount.Text = "Cached";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(246, 612);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Timeout(s):";
            // 
            // tbTimeout
            // 
            this.tbTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbTimeout.Location = new System.Drawing.Point(306, 609);
            this.tbTimeout.Name = "tbTimeout";
            this.tbTimeout.Size = new System.Drawing.Size(94, 20);
            this.tbTimeout.TabIndex = 6;
            this.tbTimeout.Text = "3000";
            this.tbTimeout.TextChanged += new System.EventHandler(this.tbTimeout_TextChanged);
            // 
            // refreshThreadCountPeriodically
            // 
            this.refreshThreadCountPeriodically.Interval = 500;
            this.refreshThreadCountPeriodically.Tick += new System.EventHandler(this.refreshThreadCountPeriodically_Tick);
            // 
            // lblThreadCount
            // 
            this.lblThreadCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblThreadCount.AutoSize = true;
            this.lblThreadCount.Location = new System.Drawing.Point(1, 611);
            this.lblThreadCount.Name = "lblThreadCount";
            this.lblThreadCount.Size = new System.Drawing.Size(81, 13);
            this.lblThreadCount.TabIndex = 8;
            this.lblThreadCount.Text = "Thread Count:0";
            // 
            // cbIncludeCumulative
            // 
            this.cbIncludeCumulative.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbIncludeCumulative.AutoSize = true;
            this.cbIncludeCumulative.Location = new System.Drawing.Point(98, 611);
            this.cbIncludeCumulative.Name = "cbIncludeCumulative";
            this.cbIncludeCumulative.Size = new System.Drawing.Size(148, 17);
            this.cbIncludeCumulative.TabIndex = 11;
            this.cbIncludeCumulative.Text = "Include Cumulative Totals";
            this.cbIncludeCumulative.UseVisualStyleBackColor = true;
            this.cbIncludeCumulative.CheckedChanged += new System.EventHandler(this.cbIncludeCumulative_CheckedChanged);
            // 
            // CohortCompilerUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblThreadCount);
            this.Controls.Add(this.cbIncludeCumulative);
            this.Controls.Add(this.tbTimeout);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tlvConfiguration);
            this.Name = "CohortCompilerUI";
            this.Size = new System.Drawing.Size(540, 632);
            ((System.ComponentModel.ISupportInitialize)(this.tlvConfiguration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TreeListView tlvConfiguration;
        private OLVColumn olvAggregate;
        private OLVColumn olvIdentifierCount;
        private OLVColumn olvWorking;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbTimeout;
        private System.Windows.Forms.Timer refreshThreadCountPeriodically;
        private System.Windows.Forms.Label lblThreadCount;
        private OLVColumn olvCatalogue;
        private OLVColumn olvOrder;
        private OLVColumn olvTime;
        private OLVColumn olvCachedQueryUseCount;
        private OLVColumn olvCumulativeTotal;
        private System.Windows.Forms.CheckBox cbIncludeCumulative;
    }
}
