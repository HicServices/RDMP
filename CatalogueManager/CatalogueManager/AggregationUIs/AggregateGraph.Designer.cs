using ReusableUIComponents.Heatmapping;

namespace CatalogueManager.AggregationUIs
{
    partial class AggregateGraph
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AggregateGraph));
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label1 = new System.Windows.Forms.Label();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpGraph = new System.Windows.Forms.TabPage();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.lblLoadStage = new System.Windows.Forms.Label();
            this.llCancel = new System.Windows.Forms.LinkLabel();
            this.tpCode = new System.Windows.Forms.TabPage();
            this.tpDataTable = new System.Windows.Forms.TabPage();
            this.btnClearFromCache = new System.Windows.Forms.Button();
            this.btnPublishToWebsite = new System.Windows.Forms.Button();
            this.lblCannotLoadData = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tpHeatmap = new System.Windows.Forms.TabPage();
            this.heatmapUI = new ReusableUIComponents.Heatmapping.HeatmapUI();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnSaveImages = new System.Windows.Forms.ToolStripButton();
            this.btnClipboard = new System.Windows.Forms.ToolStripButton();
            this.btnResendQuery = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tbTimeout = new System.Windows.Forms.ToolStripTextBox();
            this.rdmpObjectsRibbonUI1 = new CatalogueManager.ObjectVisualisation.RDMPObjectsRibbonUI();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tpGraph.SuspendLayout();
            this.tpDataTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tpHeatmap.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Area;
            series1.Legend = "Legend1";
            series1.MarkerSize = 1;
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(680, 404);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 405);
            this.label1.MaximumSize = new System.Drawing.Size(710, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 39);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1\r\n\r\nbob\r\n";
            // 
            // pbLoading
            // 
            this.pbLoading.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pbLoading.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pbLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pbLoading.Image = ((System.Drawing.Image)(resources.GetObject("pbLoading.Image")));
            this.pbLoading.InitialImage = null;
            this.pbLoading.Location = new System.Drawing.Point(283, 166);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(105, 102);
            this.pbLoading.TabIndex = 2;
            this.pbLoading.TabStop = false;
            this.pbLoading.Visible = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Right;
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tpGraph);
            this.tabControl1.Controls.Add(this.tpCode);
            this.tabControl1.Controls.Add(this.tpDataTable);
            this.tabControl1.Controls.Add(this.tpHeatmap);
            this.tabControl1.Location = new System.Drawing.Point(3, 50);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(692, 455);
            this.tabControl1.TabIndex = 3;
            // 
            // tpGraph
            // 
            this.tpGraph.Controls.Add(this.ragSmiley1);
            this.tpGraph.Controls.Add(this.lblLoadStage);
            this.tpGraph.Controls.Add(this.llCancel);
            this.tpGraph.Controls.Add(this.pbLoading);
            this.tpGraph.Controls.Add(this.chart1);
            this.tpGraph.Controls.Add(this.label1);
            this.tpGraph.Location = new System.Drawing.Point(4, 4);
            this.tpGraph.Name = "tpGraph";
            this.tpGraph.Padding = new System.Windows.Forms.Padding(3);
            this.tpGraph.Size = new System.Drawing.Size(665, 447);
            this.tpGraph.TabIndex = 0;
            this.tpGraph.Text = "Graph";
            this.tpGraph.UseVisualStyleBackColor = true;
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(313, 175);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(37, 38);
            this.ragSmiley1.TabIndex = 6;
            this.ragSmiley1.Visible = false;
            // 
            // lblLoadStage
            // 
            this.lblLoadStage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLoadStage.Location = new System.Drawing.Point(6, 107);
            this.lblLoadStage.Name = "lblLoadStage";
            this.lblLoadStage.Size = new System.Drawing.Size(653, 17);
            this.lblLoadStage.TabIndex = 5;
            this.lblLoadStage.Text = "label2";
            this.lblLoadStage.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblLoadStage.Visible = false;
            // 
            // llCancel
            // 
            this.llCancel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.llCancel.AutoSize = true;
            this.llCancel.Location = new System.Drawing.Point(310, 266);
            this.llCancel.Name = "llCancel";
            this.llCancel.Size = new System.Drawing.Size(40, 13);
            this.llCancel.TabIndex = 4;
            this.llCancel.TabStop = true;
            this.llCancel.Text = "Cancel";
            this.llCancel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llCancel_LinkClicked);
            // 
            // tpCode
            // 
            this.tpCode.Location = new System.Drawing.Point(4, 4);
            this.tpCode.Name = "tpCode";
            this.tpCode.Padding = new System.Windows.Forms.Padding(3);
            this.tpCode.Size = new System.Drawing.Size(665, 447);
            this.tpCode.TabIndex = 1;
            this.tpCode.Text = "SQL Code";
            this.tpCode.UseVisualStyleBackColor = true;
            // 
            // tpDataTable
            // 
            this.tpDataTable.Controls.Add(this.btnClearFromCache);
            this.tpDataTable.Controls.Add(this.btnPublishToWebsite);
            this.tpDataTable.Controls.Add(this.lblCannotLoadData);
            this.tpDataTable.Controls.Add(this.dataGridView1);
            this.tpDataTable.Location = new System.Drawing.Point(4, 4);
            this.tpDataTable.Name = "tpDataTable";
            this.tpDataTable.Padding = new System.Windows.Forms.Padding(3);
            this.tpDataTable.Size = new System.Drawing.Size(665, 447);
            this.tpDataTable.TabIndex = 2;
            this.tpDataTable.Text = "Data";
            this.tpDataTable.UseVisualStyleBackColor = true;
            // 
            // btnClearFromCache
            // 
            this.btnClearFromCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClearFromCache.Location = new System.Drawing.Point(3, 468);
            this.btnClearFromCache.Name = "btnClearFromCache";
            this.btnClearFromCache.Size = new System.Drawing.Size(113, 23);
            this.btnClearFromCache.TabIndex = 7;
            this.btnClearFromCache.Text = "Clear From Cache";
            this.btnClearFromCache.UseVisualStyleBackColor = true;
            this.btnClearFromCache.Click += new System.EventHandler(this.btnClearFromCache_Click);
            // 
            // btnPublishToWebsite
            // 
            this.btnPublishToWebsite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPublishToWebsite.Image = ((System.Drawing.Image)(resources.GetObject("btnPublishToWebsite.Image")));
            this.btnPublishToWebsite.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnPublishToWebsite.Location = new System.Drawing.Point(6, 417);
            this.btnPublishToWebsite.Name = "btnPublishToWebsite";
            this.btnPublishToWebsite.Size = new System.Drawing.Size(110, 50);
            this.btnPublishToWebsite.TabIndex = 6;
            this.btnPublishToWebsite.Text = "Publish To Website";
            this.btnPublishToWebsite.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.btnPublishToWebsite.UseVisualStyleBackColor = true;
            this.btnPublishToWebsite.Click += new System.EventHandler(this.btnPublishToWebsite_Click);
            // 
            // lblCannotLoadData
            // 
            this.lblCannotLoadData.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblCannotLoadData.AutoSize = true;
            this.lblCannotLoadData.ForeColor = System.Drawing.Color.Red;
            this.lblCannotLoadData.Location = new System.Drawing.Point(276, 205);
            this.lblCannotLoadData.Name = "lblCannotLoadData";
            this.lblCannotLoadData.Size = new System.Drawing.Size(98, 13);
            this.lblCannotLoadData.TabIndex = 1;
            this.lblCannotLoadData.Text = "lblCannotLoadData";
            this.lblCannotLoadData.Visible = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(659, 486);
            this.dataGridView1.TabIndex = 0;
            // 
            // tpHeatmap
            // 
            this.tpHeatmap.AutoScroll = true;
            this.tpHeatmap.Controls.Add(this.heatmapUI);
            this.tpHeatmap.Location = new System.Drawing.Point(4, 4);
            this.tpHeatmap.Name = "tpHeatmap";
            this.tpHeatmap.Padding = new System.Windows.Forms.Padding(3);
            this.tpHeatmap.Size = new System.Drawing.Size(665, 447);
            this.tpHeatmap.TabIndex = 3;
            this.tpHeatmap.Text = "Heat Map";
            this.tpHeatmap.UseVisualStyleBackColor = true;
            // 
            // heatmapUI
            // 
            this.heatmapUI.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.heatmapUI.AutoSize = true;
            this.heatmapUI.Location = new System.Drawing.Point(3, 3);
            this.heatmapUI.Name = "heatmapUI";
            this.heatmapUI.Size = new System.Drawing.Size(659, 441);
            this.heatmapUI.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSaveImages,
            this.btnClipboard,
            this.btnResendQuery,
            this.toolStripLabel1,
            this.tbTimeout});
            this.toolStrip1.Location = new System.Drawing.Point(0, 22);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(698, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnSaveImages
            // 
            this.btnSaveImages.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveImages.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveImages.Image")));
            this.btnSaveImages.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveImages.Name = "btnSaveImages";
            this.btnSaveImages.Size = new System.Drawing.Size(23, 22);
            this.btnSaveImages.Text = "Save Chart";
            this.btnSaveImages.Click += new System.EventHandler(this.btnSaveImages_Click);
            // 
            // btnClipboard
            // 
            this.btnClipboard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnClipboard.Image = ((System.Drawing.Image)(resources.GetObject("btnClipboard.Image")));
            this.btnClipboard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClipboard.Name = "btnClipboard";
            this.btnClipboard.Size = new System.Drawing.Size(23, 22);
            this.btnClipboard.Text = "Copy Data Table";
            this.btnClipboard.Click += new System.EventHandler(this.btnClipboard_Click);
            // 
            // btnResendQuery
            // 
            this.btnResendQuery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnResendQuery.Image = ((System.Drawing.Image)(resources.GetObject("btnResendQuery.Image")));
            this.btnResendQuery.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnResendQuery.Name = "btnResendQuery";
            this.btnResendQuery.Size = new System.Drawing.Size(23, 22);
            this.btnResendQuery.Text = "Re Send Query";
            this.btnResendQuery.Click += new System.EventHandler(this.btnResendQuery_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(55, 22);
            this.toolStripLabel1.Text = "Timeout:";
            // 
            // tbTimeout
            // 
            this.tbTimeout.Name = "tbTimeout";
            this.tbTimeout.Size = new System.Drawing.Size(100, 25);
            this.tbTimeout.TextChanged += new System.EventHandler(this.tbTimeout_TextChanged);
            // 
            // rdmpObjectsRibbonUI1
            // 
            this.rdmpObjectsRibbonUI1.Dock = System.Windows.Forms.DockStyle.Top;
            this.rdmpObjectsRibbonUI1.Location = new System.Drawing.Point(0, 0);
            this.rdmpObjectsRibbonUI1.Margin = new System.Windows.Forms.Padding(0);
            this.rdmpObjectsRibbonUI1.Name = "rdmpObjectsRibbonUI1";
            this.rdmpObjectsRibbonUI1.Size = new System.Drawing.Size(698, 22);
            this.rdmpObjectsRibbonUI1.TabIndex = 4;
            // 
            // AggregateGraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.rdmpObjectsRibbonUI1);
            this.Controls.Add(this.tabControl1);
            this.Name = "AggregateGraph";
            this.Size = new System.Drawing.Size(698, 508);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tpGraph.ResumeLayout(false);
            this.tpGraph.PerformLayout();
            this.tpDataTable.ResumeLayout(false);
            this.tpDataTable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tpHeatmap.ResumeLayout(false);
            this.tpHeatmap.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pbLoading;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpGraph;
        private System.Windows.Forms.TabPage tpCode;
        private System.Windows.Forms.TabPage tpDataTable;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label lblCannotLoadData;
        private System.Windows.Forms.LinkLabel llCancel;
        private System.Windows.Forms.Button btnPublishToWebsite;
        private System.Windows.Forms.Button btnClearFromCache;
        private System.Windows.Forms.Label lblLoadStage;
        private System.Windows.Forms.TabPage tpHeatmap;
        public HeatmapUI heatmapUI;
        private ReusableUIComponents.RAGSmiley ragSmiley1;
        private ObjectVisualisation.RDMPObjectsRibbonUI rdmpObjectsRibbonUI1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnSaveImages;
        private System.Windows.Forms.ToolStripButton btnClipboard;
        private System.Windows.Forms.ToolStripButton btnResendQuery;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox tbTimeout;
    }
}
