using System.Windows.Forms.DataVisualization.Charting;

namespace Rdmp.UI.CatalogueAnalysisUIs.Charts
{
    partial class MultiPurposeChart
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
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            groupBox1 = new System.Windows.Forms.GroupBox();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox1.Location = new System.Drawing.Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(150, 150);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
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
            // MultiPurposeChart
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            groupBox1.Controls.Add(chart1);
            Controls.Add(groupBox1);
            Name = "MultiPurposeChart";
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}
