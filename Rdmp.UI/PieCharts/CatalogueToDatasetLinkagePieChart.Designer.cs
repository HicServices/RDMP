using System.Windows.Forms.DataVisualization.Charting;

namespace Rdmp.UI.PieCharts
{
    partial class CatalogueToDatasetLinkagePieChart
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

        #region Windows Form Designer generated code

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
            btnViewDataTable = new System.Windows.Forms.Button();
            lblNoIssues = new System.Windows.Forms.Label();
            gbWhatThisIs = new System.Windows.Forms.GroupBox();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            gbWhatThisIs.SuspendLayout();
            SuspendLayout();
            // 
            // btnViewDataTable
            // 
            btnViewDataTable.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnViewDataTable.Location = new System.Drawing.Point(677, 379);
            btnViewDataTable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnViewDataTable.Name = "btnViewDataTable";
            btnViewDataTable.Size = new System.Drawing.Size(31, 27);
            btnViewDataTable.TabIndex = 4;
            btnViewDataTable.UseVisualStyleBackColor = true;
            // 
            // lblNoIssues
            // 
            lblNoIssues.Anchor = System.Windows.Forms.AnchorStyles.None;
            lblNoIssues.AutoSize = true;
            lblNoIssues.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            lblNoIssues.Location = new System.Drawing.Point(246, 206);
            lblNoIssues.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblNoIssues.Name = "lblNoIssues";
            lblNoIssues.Size = new System.Drawing.Size(153, 13);
            lblNoIssues.TabIndex = 1;
            lblNoIssues.Text = "No Issues Reported In Dataset";
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
            gbWhatThisIs.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbWhatThisIs.Controls.Add(btnViewDataTable);
            gbWhatThisIs.Controls.Add(lblNoIssues);
            gbWhatThisIs.Location = new System.Drawing.Point(171, 97);
            gbWhatThisIs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbWhatThisIs.Name = "gbWhatThisIs";
            gbWhatThisIs.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbWhatThisIs.Size = new System.Drawing.Size(458, 256);
            gbWhatThisIs.TabIndex = 2;
            gbWhatThisIs.TabStop = false;
            gbWhatThisIs.Text = "What This Is";
            // 
            // CatalogueToDatasetLinkagePieChart
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(gbWhatThisIs);
            Name = "CatalogueToDatasetLinkagePieChart";
            Size = new System.Drawing.Size(800, 450);
            gbWhatThisIs.ResumeLayout(false);
            gbWhatThisIs.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;

        private System.Windows.Forms.Button btnViewDataTable;
        private System.Windows.Forms.Label lblNoIssues;
        private System.Windows.Forms.GroupBox gbWhatThisIs;
    }
}