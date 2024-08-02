namespace Rdmp.UI.PieCharts
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
            this.lblNoIssues = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.gbWhatThisIs.SuspendLayout();
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
            this.chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            this.chart1.ChartAreas[0].CursorX.AutoScroll = true;
            this.chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            // 
            // gbWhatThisIs
            // 
            this.gbWhatThisIs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbWhatThisIs.Controls.Add(this.btnViewDataTable);
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
            // GoodBadCataloguePieChart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbWhatThisIs);
            this.Name = "GoodBadCataloguePieChart";
            this.Size = new System.Drawing.Size(393, 250);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.gbWhatThisIs.ResumeLayout(false);
            this.gbWhatThisIs.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.GroupBox gbWhatThisIs;
        private System.Windows.Forms.Label lblNoIssues;
        private System.Windows.Forms.Button btnViewDataTable;
    }
}
