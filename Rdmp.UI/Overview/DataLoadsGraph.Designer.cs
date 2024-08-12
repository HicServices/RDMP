using BrightIdeasSoftware;
using Rdmp.UI.ChecksUI;

namespace Rdmp.UI.Overview
{
    partial class DataLoadsGraph
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
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataLoadsGraph));
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ragSmiley1 = new RAGSmiley();
            this.lblNoDataLoadsFound = new System.Windows.Forms.Label();
            this.pbLoading = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.olvDataLoads = new BrightIdeasSoftware.ObjectListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvCategory = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvStatus = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvLastRun = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvViewLog = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.ID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LoadMetadataName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Category = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvDataLoads)).BeginInit();
            this.SuspendLayout();
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Location = new System.Drawing.Point(6, 19);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(634, 261);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            this.chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            this.chart1.ChartAreas[0].CursorX.AutoScroll = true;
            this.chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ragSmiley1);
            this.groupBox1.Controls.Add(this.lblNoDataLoadsFound);
            this.groupBox1.Controls.Add(this.pbLoading);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.olvDataLoads);
            this.groupBox1.Controls.Add(this.chart1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(646, 562);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data Loads Graph";
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(6, 230);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 6;
            // 
            // lblNoDataLoadsFound
            // 
            this.lblNoDataLoadsFound.AutoSize = true;
            this.lblNoDataLoadsFound.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoDataLoadsFound.Location = new System.Drawing.Point(53, 98);
            this.lblNoDataLoadsFound.Name = "lblNoDataLoadsFound";
            this.lblNoDataLoadsFound.Size = new System.Drawing.Size(241, 13);
            this.lblNoDataLoadsFound.TabIndex = 5;
            this.lblNoDataLoadsFound.Text = "There have never been any data loads attempted";
            this.lblNoDataLoadsFound.Visible = false;
            // 
            // pbLoading
            // 
            this.pbLoading.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pbLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pbLoading.Image = ((System.Drawing.Image)(resources.GetObject("pbLoading.Image")));
            this.pbLoading.InitialImage = null;
            this.pbLoading.Location = new System.Drawing.Point(137, 99);
            this.pbLoading.Name = "pbLoading";
            this.pbLoading.Size = new System.Drawing.Size(104, 101);
            this.pbLoading.TabIndex = 4;
            this.pbLoading.TabStop = false;
            this.pbLoading.Visible = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 287);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Data Loads Summary:";
            // 
            // olvDataLoads
            // 
            this.olvDataLoads.AllColumns.Add(this.olvName);
            this.olvDataLoads.AllColumns.Add(this.olvCategory);
            this.olvDataLoads.AllColumns.Add(this.olvStatus);
            this.olvDataLoads.AllColumns.Add(this.olvLastRun);
            this.olvDataLoads.AllColumns.Add(this.olvViewLog);
            this.olvDataLoads.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.olvDataLoads.CellEditUseWholeCell = false;
            this.olvDataLoads.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvCategory,
            this.olvStatus,
            this.olvLastRun,
            this.olvViewLog});
            this.olvDataLoads.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvDataLoads.Location = new System.Drawing.Point(6, 303);
            this.olvDataLoads.Name = "olvDataLoads";
            this.olvDataLoads.RowHeight = 19;
            this.olvDataLoads.Size = new System.Drawing.Size(634, 253);
            this.olvDataLoads.TabIndex = 1;
            this.olvDataLoads.UseCompatibleStateImageBehavior = false;
            this.olvDataLoads.View = System.Windows.Forms.View.Details;
            // 
            // olvName
            // 
            this.olvName.AspectName = "Name";
            this.olvName.Groupable = false;
            this.olvName.Text = "Name";
            this.olvName.MinimumWidth = 100;
            // 
            // olvCategory
            // 
            this.olvCategory.AspectName = "Category";
            this.olvCategory.Groupable = false;
            this.olvCategory.Text = "Category";
            this.olvCategory.Width = 90;
            // 
            // olvStatus
            // 
            this.olvStatus.AspectName = "Status";
            this.olvStatus.Text = "Status";
            this.olvStatus.Width = 90;
            // 
            // olvLastRun
            // 
            this.olvLastRun.AspectName = "LastRun";
            this.olvLastRun.Groupable = false;
            this.olvLastRun.Text = "Last Run";
            this.olvLastRun.Width = 120;
            // 
            // olvViewLog
            // 
            this.olvViewLog.ButtonSizing = BrightIdeasSoftware.OLVColumn.ButtonSizingMode.CellBounds;
            this.olvViewLog.Groupable = false;
            this.olvViewLog.IsButton = true;
            this.olvViewLog.Text = "Logs";
            this.olvViewLog.Width = 70;
            // 
            // ID
            // 
            this.ID.Text = "ID";
            // 
            // LoadMetadataName
            // 
            this.LoadMetadataName.Text = "Load Metadata Name";
            // 
            // Category
            // 
            this.Category.Text = "Category";
            // 
            // DataLoadsGraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "DataLoadsGraph";
            this.Size = new System.Drawing.Size(646, 562);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLoading)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.olvDataLoads)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private ObjectListView olvDataLoads;
        private System.Windows.Forms.ColumnHeader ID;
        private System.Windows.Forms.ColumnHeader LoadMetadataName;
        private System.Windows.Forms.ColumnHeader Category;
        private System.Windows.Forms.PictureBox pbLoading;
        private System.Windows.Forms.Label lblNoDataLoadsFound;
        private RAGSmiley ragSmiley1;
        private OLVColumn olvName;
        private OLVColumn olvCategory;
        private OLVColumn olvStatus;
        private OLVColumn olvLastRun;
        private OLVColumn olvViewLog;
    }
}
