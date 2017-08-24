namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs.Diagrams
{
    partial class LoadProgressDiagram
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadProgressDiagram));
            this.cataloguesRowCountChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.checksUIIconOnly1 = new ReusableUIComponents.ChecksUI.ChecksUIIconOnly();
            this.cacheState = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pathLinkLabel1 = new ReusableUIComponents.LinkLabels.PathLinkLabel();
            this.pbFolder = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.cataloguesRowCountChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cacheState)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbFolder)).BeginInit();
            this.SuspendLayout();
            // 
            // cataloguesRowCountChart
            // 
            this.cataloguesRowCountChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.cataloguesRowCountChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.cataloguesRowCountChart.Legends.Add(legend1);
            this.cataloguesRowCountChart.Location = new System.Drawing.Point(3, 3);
            this.cataloguesRowCountChart.Name = "cataloguesRowCountChart";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.cataloguesRowCountChart.Series.Add(series1);
            this.cataloguesRowCountChart.Size = new System.Drawing.Size(916, 177);
            this.cataloguesRowCountChart.TabIndex = 0;
            this.cataloguesRowCountChart.Text = "catalogueStackChart";
            this.cataloguesRowCountChart.AnnotationPositionChanged += new System.EventHandler(this.cataloguesRowCountChart_AnnotationPositionChanged);
            // 
            // checksUIIconOnly1
            // 
            this.checksUIIconOnly1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checksUIIconOnly1.Location = new System.Drawing.Point(3, 160);
            this.checksUIIconOnly1.Name = "checksUIIconOnly1";
            this.checksUIIconOnly1.Size = new System.Drawing.Size(20, 20);
            this.checksUIIconOnly1.TabIndex = 2;
            // 
            // cacheState
            // 
            this.cacheState.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea2.Name = "ChartArea1";
            this.cacheState.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.cacheState.Legends.Add(legend2);
            this.cacheState.Location = new System.Drawing.Point(3, 3);
            this.cacheState.Name = "cacheState";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.cacheState.Series.Add(series2);
            this.cacheState.Size = new System.Drawing.Size(916, 174);
            this.cacheState.TabIndex = 3;
            this.cacheState.Text = "catalogueStackChart";
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.checksUIIconOnly1);
            this.splitContainer1.Panel1.Controls.Add(this.cataloguesRowCountChart);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pathLinkLabel1);
            this.splitContainer1.Panel2.Controls.Add(this.pbFolder);
            this.splitContainer1.Panel2.Controls.Add(this.cacheState);
            this.splitContainer1.Size = new System.Drawing.Size(924, 371);
            this.splitContainer1.SplitterDistance = 185;
            this.splitContainer1.TabIndex = 4;
            // 
            // pathLinkLabel1
            // 
            this.pathLinkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pathLinkLabel1.Location = new System.Drawing.Point(26, 166);
            this.pathLinkLabel1.Name = "pathLinkLabel1";
            this.pathLinkLabel1.Size = new System.Drawing.Size(605, 13);
            this.pathLinkLabel1.TabIndex = 5;
            this.pathLinkLabel1.Text = "Cache Path";
            // 
            // pbFolder
            // 
            this.pbFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pbFolder.Image = ((System.Drawing.Image)(resources.GetObject("pbFolder.Image")));
            this.pbFolder.Location = new System.Drawing.Point(5, 164);
            this.pbFolder.Name = "pbFolder";
            this.pbFolder.Size = new System.Drawing.Size(19, 20);
            this.pbFolder.TabIndex = 4;
            this.pbFolder.TabStop = false;
            // 
            // LoadProgressDiagram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "LoadProgressDiagram";
            this.Size = new System.Drawing.Size(924, 371);
            ((System.ComponentModel.ISupportInitialize)(this.cataloguesRowCountChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cacheState)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbFolder)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart cataloguesRowCountChart;
        private ReusableUIComponents.ChecksUI.ChecksUIIconOnly checksUIIconOnly1;
        private System.Windows.Forms.DataVisualization.Charting.Chart cacheState;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pbFolder;
        private ReusableUIComponents.LinkLabels.PathLinkLabel pathLinkLabel1;
    }
}
