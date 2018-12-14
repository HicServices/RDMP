namespace Dashboard.PieCharts
{
    partial class GoodBadCataloguePieChart
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GoodBadCataloguePieChart));
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.gbWhatThisIs = new System.Windows.Forms.GroupBox();
            this.btnViewDataTable = new System.Windows.Forms.Button();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.lblNoIssues = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.ddChartType = new System.Windows.Forms.ToolStripComboBox();
            this.btnAllCatalogues = new System.Windows.Forms.ToolStripButton();
            this.btnSingleCatalogue = new System.Windows.Forms.ToolStripButton();
            this.btnShowLabels = new System.Windows.Forms.ToolStripButton();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.gbWhatThisIs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chart1
            // 
            this.chart1.BackColor = System.Drawing.Color.DarkGray;
            this.chart1.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.TopBottom;
            this.chart1.BorderSkin.PageColor = System.Drawing.Color.Black;
            chartArea1.Area3DStyle.Enable3D = true;
            chartArea1.Area3DStyle.WallWidth = 10;
            chartArea1.BackColor = System.Drawing.Color.DarkGray;
            chartArea1.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.TopBottom;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.BackColor = System.Drawing.Color.Gainsboro;
            legend1.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.TopBottom;
            legend1.BorderColor = System.Drawing.Color.White;
            legend1.BorderWidth = 2;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(3, 16);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            this.chart1.PaletteCustomColors = new System.Drawing.Color[] {
        System.Drawing.Color.Red,
        System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))))};
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series1.CustomProperties = "PieDrawingStyle=SoftEdge, PieLabelStyle=Disabled";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(387, 203);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            // 
            // gbWhatThisIs
            // 
            this.gbWhatThisIs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbWhatThisIs.Controls.Add(this.btnViewDataTable);
            this.gbWhatThisIs.Controls.Add(this.pbLoading);
            this.gbWhatThisIs.Controls.Add(this.lblNoIssues);
            this.gbWhatThisIs.Controls.Add(this.chart1);
            this.gbWhatThisIs.Location = new System.Drawing.Point(0, 28);
            this.gbWhatThisIs.Name = "gbWhatThisIs";
            this.gbWhatThisIs.Size = new System.Drawing.Size(393, 222);
            this.gbWhatThisIs.TabIndex = 1;
            this.gbWhatThisIs.TabStop = false;
            this.gbWhatThisIs.Text = "What This Is";
            // 
            // btnViewDataTable
            // 
            this.btnViewDataTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnViewDataTable.Image = ((System.Drawing.Image)(resources.GetObject("btnViewDataTable.Image")));
            this.btnViewDataTable.Location = new System.Drawing.Point(360, 193);
            this.btnViewDataTable.Name = "btnViewDataTable";
            this.btnViewDataTable.Size = new System.Drawing.Size(27, 23);
            this.btnViewDataTable.TabIndex = 4;
            this.btnViewDataTable.UseVisualStyleBackColor = true;
            this.btnViewDataTable.Click += new System.EventHandler(this.btnViewDataTable_Click);
            // 
            // pbLoading
            // 
            this.pbLoading.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pbLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pbLoading.Image = ((System.Drawing.Image)(resources.GetObject("pbLoading.Image")));
            this.pbLoading.InitialImage = null;
            this.pbLoading.Location = new System.Drawing.Point(120, 77);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(105, 102);
            this.pbLoading.TabIndex = 3;
            this.pbLoading.TabStop = false;
            this.pbLoading.Visible = false;
            // 
            // lblNoIssues
            // 
            this.lblNoIssues.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblNoIssues.AutoSize = true;
            this.lblNoIssues.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoIssues.Location = new System.Drawing.Point(100, 111);
            this.lblNoIssues.Name = "lblNoIssues";
            this.lblNoIssues.Size = new System.Drawing.Size(153, 13);
            this.lblNoIssues.TabIndex = 1;
            this.lblNoIssues.Text = "No Issues Reported In Dataset";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.ddChartType,
            this.btnAllCatalogues,
            this.btnSingleCatalogue,
            this.btnShowLabels,
            this.btnRefresh});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(393, 25);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(36, 22);
            this.toolStripLabel1.Text = "Type:";
            // 
            // ddChartType
            // 
            this.ddChartType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddChartType.Name = "ddChartType";
            this.ddChartType.Size = new System.Drawing.Size(121, 25);
            // 
            // btnAllCatalogues
            // 
            this.btnAllCatalogues.Image = ((System.Drawing.Image)(resources.GetObject("btnAllCatalogues.Image")));
            this.btnAllCatalogues.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAllCatalogues.Name = "btnAllCatalogues";
            this.btnAllCatalogues.Size = new System.Drawing.Size(41, 22);
            this.btnAllCatalogues.Text = "All";
            this.btnAllCatalogues.Click += new System.EventHandler(this.btnAllCatalogues_Click);
            // 
            // btnSingleCatalogue
            // 
            this.btnSingleCatalogue.Image = ((System.Drawing.Image)(resources.GetObject("btnSingleCatalogue.Image")));
            this.btnSingleCatalogue.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSingleCatalogue.Name = "btnSingleCatalogue";
            this.btnSingleCatalogue.Size = new System.Drawing.Size(59, 22);
            this.btnSingleCatalogue.Text = "Single";
            this.btnSingleCatalogue.Click += new System.EventHandler(this.btnSingleCatalogue_Click);
            // 
            // btnShowLabels
            // 
            this.btnShowLabels.CheckOnClick = true;
            this.btnShowLabels.Image = ((System.Drawing.Image)(resources.GetObject("btnShowLabels.Image")));
            this.btnShowLabels.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowLabels.Name = "btnShowLabels";
            this.btnShowLabels.Size = new System.Drawing.Size(60, 22);
            this.btnShowLabels.Text = "Labels";
            this.btnShowLabels.CheckStateChanged += new System.EventHandler(this.btnShowLabels_CheckStateChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(23, 22);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // GoodBadCataloguePieChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.gbWhatThisIs);
            this.Name = "GoodBadCataloguePieChart";
            this.Size = new System.Drawing.Size(393, 250);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.gbWhatThisIs.ResumeLayout(false);
            this.gbWhatThisIs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.GroupBox gbWhatThisIs;
        private System.Windows.Forms.Label lblNoIssues;
        private System.Windows.Forms.PictureBox pbLoading;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripComboBox ddChartType;
        private System.Windows.Forms.ToolStripButton btnSingleCatalogue;
        private System.Windows.Forms.ToolStripButton btnAllCatalogues;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton btnShowLabels;
        private System.Windows.Forms.Button btnViewDataTable;
    }
}
