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
            this.btnStartAll = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbTimeout = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.refreshThreadCountPeriodically = new System.Windows.Forms.Timer(this.components);
            this.lblThreadCount = new System.Windows.Forms.Label();
            this.btnStartSelected = new System.Windows.Forms.Button();
            this.btnCacheSelected = new System.Windows.Forms.Button();
            this.btnClearCacheForSelected = new System.Windows.Forms.Button();
            this.cbIncludeCumulative = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnClearCacheAll = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cbAutoCache = new System.Windows.Forms.CheckBox();
            this.ddOptimisation = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.tlvConfiguration)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // otvConfiguration
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
            this.tlvConfiguration.Name = "otvConfiguration";
            this.tlvConfiguration.ShowGroups = false;
            this.tlvConfiguration.Size = new System.Drawing.Size(536, 538);
            this.tlvConfiguration.TabIndex = 0;
            this.tlvConfiguration.UseCompatibleStateImageBehavior = false;
            this.tlvConfiguration.View = System.Windows.Forms.View.Details;
            this.tlvConfiguration.VirtualMode = true;
            this.tlvConfiguration.CellRightClick += new System.EventHandler<BrightIdeasSoftware.CellRightClickEventArgs>(this.otvConfiguration_CellRightClick);
            this.tlvConfiguration.SelectionChanged += new System.EventHandler(this.otvConfiguration_SelectionChanged);
            this.tlvConfiguration.ItemActivate += new System.EventHandler(this.otvConfiguration_ItemActivate);
            this.tlvConfiguration.SelectedIndexChanged += new System.EventHandler(this.otvConfiguration_SelectedIndexChanged);
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
            // btnStartAll
            // 
            this.btnStartAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnStartAll.Location = new System.Drawing.Point(6, 14);
            this.btnStartAll.Name = "btnStartAll";
            this.btnStartAll.Size = new System.Drawing.Size(78, 21);
            this.btnStartAll.TabIndex = 4;
            this.btnStartAll.Text = "All";
            this.btnStartAll.UseVisualStyleBackColor = true;
            this.btnStartAll.Click += new System.EventHandler(this.btnStartAll_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(264, 611);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Timeout(s):";
            // 
            // tbTimeout
            // 
            this.tbTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbTimeout.Location = new System.Drawing.Point(324, 608);
            this.tbTimeout.Name = "tbTimeout";
            this.tbTimeout.Size = new System.Drawing.Size(94, 20);
            this.tbTimeout.TabIndex = 6;
            this.tbTimeout.Text = "300";
            this.tbTimeout.TextChanged += new System.EventHandler(this.tbTimeout_TextChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(210, 585);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(54, 39);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel All";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
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
            this.lblThreadCount.Location = new System.Drawing.Point(7, 541);
            this.lblThreadCount.Name = "lblThreadCount";
            this.lblThreadCount.Size = new System.Drawing.Size(81, 13);
            this.lblThreadCount.TabIndex = 8;
            this.lblThreadCount.Text = "Thread Count:0";
            // 
            // btnStartSelected
            // 
            this.btnStartSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnStartSelected.Enabled = false;
            this.btnStartSelected.Location = new System.Drawing.Point(6, 35);
            this.btnStartSelected.Name = "btnStartSelected";
            this.btnStartSelected.Size = new System.Drawing.Size(78, 20);
            this.btnStartSelected.TabIndex = 9;
            this.btnStartSelected.Text = "Selected";
            this.btnStartSelected.UseVisualStyleBackColor = true;
            this.btnStartSelected.Click += new System.EventHandler(this.btnStartSelected_Click);
            // 
            // btnCacheSelected
            // 
            this.btnCacheSelected.Enabled = false;
            this.btnCacheSelected.Location = new System.Drawing.Point(6, 29);
            this.btnCacheSelected.Name = "btnCacheSelected";
            this.btnCacheSelected.Size = new System.Drawing.Size(95, 23);
            this.btnCacheSelected.TabIndex = 9;
            this.btnCacheSelected.Text = "Cache Selected";
            this.btnCacheSelected.UseVisualStyleBackColor = true;
            this.btnCacheSelected.Click += new System.EventHandler(this.btnCacheSelected_Click);
            // 
            // btnClearCacheForSelected
            // 
            this.btnClearCacheForSelected.Enabled = false;
            this.btnClearCacheForSelected.Location = new System.Drawing.Point(6, 16);
            this.btnClearCacheForSelected.Name = "btnClearCacheForSelected";
            this.btnClearCacheForSelected.Size = new System.Drawing.Size(94, 19);
            this.btnClearCacheForSelected.TabIndex = 10;
            this.btnClearCacheForSelected.Text = "Clear ";
            this.btnClearCacheForSelected.UseVisualStyleBackColor = true;
            this.btnClearCacheForSelected.Click += new System.EventHandler(this.btnClearCacheForSelected_Click);
            // 
            // cbIncludeCumulative
            // 
            this.cbIncludeCumulative.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbIncludeCumulative.AutoSize = true;
            this.cbIncludeCumulative.Location = new System.Drawing.Point(270, 587);
            this.cbIncludeCumulative.Name = "cbIncludeCumulative";
            this.cbIncludeCumulative.Size = new System.Drawing.Size(148, 17);
            this.cbIncludeCumulative.TabIndex = 11;
            this.cbIncludeCumulative.Text = "Include Cumulative Totals";
            this.cbIncludeCumulative.UseVisualStyleBackColor = true;
            this.cbIncludeCumulative.CheckedChanged += new System.EventHandler(this.cbIncludeCumulative_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.btnClearCacheAll);
            this.groupBox1.Controls.Add(this.btnClearCacheForSelected);
            this.groupBox1.Location = new System.Drawing.Point(424, 571);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(113, 58);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Clear Cache";
            // 
            // btnClearCacheAll
            // 
            this.btnClearCacheAll.Enabled = false;
            this.btnClearCacheAll.Location = new System.Drawing.Point(6, 38);
            this.btnClearCacheAll.Name = "btnClearCacheAll";
            this.btnClearCacheAll.Size = new System.Drawing.Size(94, 19);
            this.btnClearCacheAll.TabIndex = 10;
            this.btnClearCacheAll.Text = "Clear All";
            this.btnClearCacheAll.UseVisualStyleBackColor = true;
            this.btnClearCacheAll.Click += new System.EventHandler(this.btnClearCacheAll_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.btnStartAll);
            this.groupBox2.Controls.Add(this.btnStartSelected);
            this.groupBox2.Location = new System.Drawing.Point(4, 571);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(93, 58);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Start";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.cbAutoCache);
            this.groupBox3.Controls.Add(this.btnCacheSelected);
            this.groupBox3.Location = new System.Drawing.Point(103, 574);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(105, 55);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Cache";
            // 
            // cbAutoCache
            // 
            this.cbAutoCache.AutoSize = true;
            this.cbAutoCache.Location = new System.Drawing.Point(6, 13);
            this.cbAutoCache.Name = "cbAutoCache";
            this.cbAutoCache.Size = new System.Drawing.Size(82, 17);
            this.cbAutoCache.TabIndex = 10;
            this.cbAutoCache.Text = "Auto Cache";
            this.cbAutoCache.UseVisualStyleBackColor = true;
            // 
            // ddOptimisation
            // 
            this.ddOptimisation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ddOptimisation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddOptimisation.FormattingEnabled = true;
            this.ddOptimisation.Location = new System.Drawing.Point(416, 544);
            this.ddOptimisation.Name = "ddOptimisation";
            this.ddOptimisation.Size = new System.Drawing.Size(121, 21);
            this.ddOptimisation.TabIndex = 16;
            // 
            // CohortCompilerUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ddOptimisation);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.lblThreadCount);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cbIncludeCumulative);
            this.Controls.Add(this.tbTimeout);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.tlvConfiguration);
            this.Name = "CohortCompilerUI";
            this.Size = new System.Drawing.Size(540, 632);
            ((System.ComponentModel.ISupportInitialize)(this.tlvConfiguration)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TreeListView tlvConfiguration;
        private OLVColumn olvAggregate;
        private OLVColumn olvIdentifierCount;
        private OLVColumn olvWorking;
        private System.Windows.Forms.Button btnStartAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbTimeout;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Timer refreshThreadCountPeriodically;
        private System.Windows.Forms.Label lblThreadCount;
        private OLVColumn olvCatalogue;
        private OLVColumn olvOrder;
        private OLVColumn olvTime;
        private OLVColumn olvCachedQueryUseCount;
        private System.Windows.Forms.Button btnStartSelected;
        private System.Windows.Forms.Button btnCacheSelected;
        private System.Windows.Forms.Button btnClearCacheForSelected;
        private OLVColumn olvCumulativeTotal;
        private System.Windows.Forms.CheckBox cbIncludeCumulative;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnClearCacheAll;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox cbAutoCache;
        private System.Windows.Forms.ComboBox ddOptimisation;
    }
}
