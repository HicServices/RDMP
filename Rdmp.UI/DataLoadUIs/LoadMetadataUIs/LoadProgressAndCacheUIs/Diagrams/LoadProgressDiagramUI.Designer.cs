using BrightIdeasSoftware;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.LinkLabels;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs.Diagrams
{
    partial class LoadProgressDiagramUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadProgressDiagramUI));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.cacheState = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.ragSmiley1 = new RAGSmiley();
            this.cataloguesRowCountChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.olvDQERuns = new BrightIdeasSoftware.ObjectListView();
            this.olvCatalogue = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvLastDQERun = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvExecute = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.pathLinkLabel1 = new PathLinkLabel();
            this.pbFolder = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.cacheState)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cataloguesRowCountChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvDQERuns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFolder)).BeginInit();
            this.SuspendLayout();
            // 
            // cacheState
            // 
            this.cacheState.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.cacheState.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.cacheState.Legends.Add(legend1);
            this.cacheState.Location = new System.Drawing.Point(3, 3);
            this.cacheState.Name = "cacheState";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.cacheState.Series.Add(series1);
            this.cacheState.Size = new System.Drawing.Size(916, 125);
            this.cacheState.TabIndex = 3;
            this.cacheState.Text = "catalogueStackChart";
            this.cacheState.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            this.cacheState.ChartAreas[0].CursorX.AutoScroll = true;
            this.cacheState.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
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
            this.splitContainer1.Panel1.Controls.Add(this.btnRefresh);
            this.splitContainer1.Panel1.Controls.Add(this.ragSmiley1);
            this.splitContainer1.Panel1.Controls.Add(this.cataloguesRowCountChart);
            this.splitContainer1.Panel1.Controls.Add(this.olvDQERuns);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pathLinkLabel1);
            this.splitContainer1.Panel2.Controls.Add(this.pbFolder);
            this.splitContainer1.Panel2.Controls.Add(this.cacheState);
            this.splitContainer1.Size = new System.Drawing.Size(924, 371);
            this.splitContainer1.SplitterDistance = 234;
            this.splitContainer1.TabIndex = 4;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.Location = new System.Drawing.Point(694, 167);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(26, 26);
            this.btnRefresh.TabIndex = 7;
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(663, 168);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 6;
            // 
            // cataloguesRowCountChart
            // 
            this.cataloguesRowCountChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea2.Name = "ChartArea1";
            this.cataloguesRowCountChart.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.cataloguesRowCountChart.Legends.Add(legend2);
            this.cataloguesRowCountChart.Location = new System.Drawing.Point(-2, -1);
            this.cataloguesRowCountChart.Name = "cataloguesRowCountChart";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.cataloguesRowCountChart.Series.Add(series2);
            this.cataloguesRowCountChart.Size = new System.Drawing.Size(922, 163);
            this.cataloguesRowCountChart.TabIndex = 5;
            this.cataloguesRowCountChart.Text = "catalogueStackChart";
            this.cataloguesRowCountChart.AnnotationPositionChanged += new System.EventHandler(this.cataloguesRowCountChart_AnnotationPositionChanged);
            this.cataloguesRowCountChart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            this.cataloguesRowCountChart.ChartAreas[0].CursorX.AutoScroll = true;
            this.cataloguesRowCountChart.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            // 
            // olvDQERuns
            // 
            this.olvDQERuns.AllColumns.Add(this.olvCatalogue);
            this.olvDQERuns.AllColumns.Add(this.olvLastDQERun);
            this.olvDQERuns.AllColumns.Add(this.olvExecute);
            this.olvDQERuns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvDQERuns.CellEditUseWholeCell = false;
            this.olvDQERuns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvCatalogue,
            this.olvLastDQERun,
            this.olvExecute});
            this.olvDQERuns.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvDQERuns.Location = new System.Drawing.Point(5, 168);
            this.olvDQERuns.Name = "olvDQERuns";
            this.olvDQERuns.ShowGroups = false;
            this.olvDQERuns.Size = new System.Drawing.Size(652, 61);
            this.olvDQERuns.TabIndex = 4;
            this.olvDQERuns.Text = "label1";
            this.olvDQERuns.UseCompatibleStateImageBehavior = false;
            this.olvDQERuns.View = System.Windows.Forms.View.Details;
            // 
            // olvCatalogue
            // 
            this.olvCatalogue.AspectName = "Name";
            this.olvCatalogue.Text = "Catalogue";
            this.olvCatalogue.Width = 250;
            // 
            // olvLastDQERun
            // 
            this.olvLastDQERun.Text = "Last Data Quality Engine Run";
            this.olvLastDQERun.Width = 200;
            // 
            // olvExecute
            // 
            this.olvExecute.Text = "Run DQE";
            this.olvExecute.Width = 100;
            // 
            // pathLinkLabel1
            // 
            this.pathLinkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pathLinkLabel1.Location = new System.Drawing.Point(26, 115);
            this.pathLinkLabel1.Name = "pathLinkLabel1";
            this.pathLinkLabel1.Size = new System.Drawing.Size(893, 15);
            this.pathLinkLabel1.TabIndex = 5;
            this.pathLinkLabel1.Text = "Cache Path";
            // 
            // pbFolder
            // 
            this.pbFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pbFolder.Image = ((System.Drawing.Image)(resources.GetObject("pbFolder.Image")));
            this.pbFolder.Location = new System.Drawing.Point(5, 115);
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
            ((System.ComponentModel.ISupportInitialize)(this.cacheState)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cataloguesRowCountChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvDQERuns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFolder)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart cacheState;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pbFolder;
        private PathLinkLabel pathLinkLabel1;
        private System.Windows.Forms.DataVisualization.Charting.Chart cataloguesRowCountChart;
        private ObjectListView olvDQERuns;
        private OLVColumn olvCatalogue;
        private OLVColumn olvLastDQERun;
        private OLVColumn olvExecute;
        private RAGSmiley ragSmiley1;
        private System.Windows.Forms.Button btnRefresh;
    }
}
