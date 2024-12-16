using Rdmp.UI.ChecksUI;
using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.AggregationUIs
{
    partial class AggregateGraphUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AggregateGraphUI));
            label1 = new System.Windows.Forms.Label();
            pbLoading = new System.Windows.Forms.PictureBox();
            tabControl1 = new System.Windows.Forms.TabControl();
            tpGraph = new System.Windows.Forms.TabPage();
            ragSmiley1 = new RAGSmiley();
            lblLoadStage = new System.Windows.Forms.Label();
            llCancel = new System.Windows.Forms.LinkLabel();
            tpCode = new System.Windows.Forms.TabPage();
            tpDataTable = new System.Windows.Forms.TabPage();
            btnClearFromCache = new System.Windows.Forms.Button();
            lblCannotLoadData = new System.Windows.Forms.Label();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            tpHeatmap = new System.Windows.Forms.TabPage();
            heatmapUI = new HeatmapUI();
            ((System.ComponentModel.ISupportInitialize)pbLoading).BeginInit();
            tabControl1.SuspendLayout();
            tpGraph.SuspendLayout();
            tpDataTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tpHeatmap.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(4, 529);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.MaximumSize = new System.Drawing.Size(828, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(38, 45);
            label1.TabIndex = 1;
            label1.Text = "label1\r\n\r\nbob\r\n";
            // 
            // pbLoading
            // 
            pbLoading.Anchor = System.Windows.Forms.AnchorStyles.Top;
            pbLoading.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            pbLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pbLoading.Image = (System.Drawing.Image)resources.GetObject("pbLoading.Image");
            pbLoading.InitialImage = null;
            pbLoading.Location = new System.Drawing.Point(334, 192);
            pbLoading.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pbLoading.Name = "pbLoading";
            pbLoading.Size = new System.Drawing.Size(122, 118);
            pbLoading.TabIndex = 2;
            pbLoading.TabStop = false;
            pbLoading.Visible = false;
            // 
            // tabControl1
            // 
            tabControl1.Alignment = System.Windows.Forms.TabAlignment.Right;
            tabControl1.Controls.Add(tpGraph);
            tabControl1.Controls.Add(tpCode);
            tabControl1.Controls.Add(tpDataTable);
            tabControl1.Controls.Add(tpHeatmap);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabControl1.Multiline = true;
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(814, 586);
            tabControl1.TabIndex = 3;
            // 
            // tpGraph
            // 
            tpGraph.Controls.Add(ragSmiley1);
            tpGraph.Controls.Add(lblLoadStage);
            tpGraph.Controls.Add(llCancel);
            tpGraph.Controls.Add(pbLoading);
            tpGraph.Controls.Add(label1);
            tpGraph.Location = new System.Drawing.Point(4, 4);
            tpGraph.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tpGraph.Name = "tpGraph";
            tpGraph.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tpGraph.Size = new System.Drawing.Size(783, 578);
            tpGraph.TabIndex = 0;
            tpGraph.Text = "Graph";
            tpGraph.UseVisualStyleBackColor = true;
            // 
            // ragSmiley1
            // 
            ragSmiley1.AlwaysShowHandCursor = false;
            ragSmiley1.Anchor = System.Windows.Forms.AnchorStyles.None;
            ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            ragSmiley1.Location = new System.Drawing.Point(369, 233);
            ragSmiley1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            ragSmiley1.Name = "ragSmiley1";
            ragSmiley1.Size = new System.Drawing.Size(43, 44);
            ragSmiley1.TabIndex = 6;
            ragSmiley1.Visible = false;
            // 
            // lblLoadStage
            // 
            lblLoadStage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblLoadStage.Location = new System.Drawing.Point(7, 123);
            lblLoadStage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblLoadStage.Name = "lblLoadStage";
            lblLoadStage.Size = new System.Drawing.Size(769, 20);
            lblLoadStage.TabIndex = 5;
            lblLoadStage.Text = "label2";
            lblLoadStage.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            lblLoadStage.Visible = false;
            // 
            // llCancel
            // 
            llCancel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            llCancel.AutoSize = true;
            llCancel.Location = new System.Drawing.Point(365, 307);
            llCancel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            llCancel.Name = "llCancel";
            llCancel.Size = new System.Drawing.Size(43, 15);
            llCancel.TabIndex = 4;
            llCancel.TabStop = true;
            llCancel.Text = "Cancel";
            llCancel.LinkClicked += llCancel_LinkClicked;
            // 
            // tpCode
            // 
            tpCode.Location = new System.Drawing.Point(4, 4);
            tpCode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tpCode.Name = "tpCode";
            tpCode.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tpCode.Size = new System.Drawing.Size(783, 549);
            tpCode.TabIndex = 1;
            tpCode.Text = "SQL Code";
            tpCode.UseVisualStyleBackColor = true;
            // 
            // tpDataTable
            // 
            tpDataTable.Controls.Add(btnClearFromCache);
            tpDataTable.Controls.Add(lblCannotLoadData);
            tpDataTable.Controls.Add(dataGridView1);
            tpDataTable.Location = new System.Drawing.Point(4, 4);
            tpDataTable.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tpDataTable.Name = "tpDataTable";
            tpDataTable.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tpDataTable.Size = new System.Drawing.Size(783, 549);
            tpDataTable.TabIndex = 2;
            tpDataTable.Text = "Data";
            tpDataTable.UseVisualStyleBackColor = true;
            // 
            // btnClearFromCache
            // 
            btnClearFromCache.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnClearFromCache.Location = new System.Drawing.Point(4, 540);
            btnClearFromCache.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnClearFromCache.Name = "btnClearFromCache";
            btnClearFromCache.Size = new System.Drawing.Size(132, 27);
            btnClearFromCache.TabIndex = 7;
            btnClearFromCache.Text = "Clear From Cache";
            btnClearFromCache.UseVisualStyleBackColor = true;
            btnClearFromCache.Click += btnClearFromCache_Click;
            // 
            // lblCannotLoadData
            // 
            lblCannotLoadData.Anchor = System.Windows.Forms.AnchorStyles.None;
            lblCannotLoadData.AutoSize = true;
            lblCannotLoadData.ForeColor = System.Drawing.Color.Red;
            lblCannotLoadData.Location = new System.Drawing.Point(322, 237);
            lblCannotLoadData.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblCannotLoadData.Name = "lblCannotLoadData";
            lblCannotLoadData.Size = new System.Drawing.Size(109, 15);
            lblCannotLoadData.TabIndex = 1;
            lblCannotLoadData.Text = "lblCannotLoadData";
            lblCannotLoadData.Visible = false;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridView1.Location = new System.Drawing.Point(4, 3);
            dataGridView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.Size = new System.Drawing.Size(775, 543);
            dataGridView1.TabIndex = 0;
            // 
            // tpHeatmap
            // 
            tpHeatmap.AutoScroll = true;
            tpHeatmap.Controls.Add(heatmapUI);
            tpHeatmap.Location = new System.Drawing.Point(4, 4);
            tpHeatmap.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tpHeatmap.Name = "tpHeatmap";
            tpHeatmap.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tpHeatmap.Size = new System.Drawing.Size(783, 549);
            tpHeatmap.TabIndex = 3;
            tpHeatmap.Text = "Heat Map";
            tpHeatmap.UseVisualStyleBackColor = true;
            // 
            // heatmapUI
            // 
            heatmapUI.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            heatmapUI.AutoSize = true;
            heatmapUI.Location = new System.Drawing.Point(4, 3);
            heatmapUI.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            heatmapUI.Name = "heatmapUI";
            heatmapUI.Size = new System.Drawing.Size(769, 509);
            heatmapUI.TabIndex = 1;
            // 
            // AggregateGraphUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            Controls.Add(tabControl1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AggregateGraphUI";
            Size = new System.Drawing.Size(814, 586);
            ((System.ComponentModel.ISupportInitialize)pbLoading).EndInit();
            tabControl1.ResumeLayout(false);
            tpGraph.ResumeLayout(false);
            tpGraph.PerformLayout();
            tpDataTable.ResumeLayout(false);
            tpDataTable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tpHeatmap.ResumeLayout(false);
            tpHeatmap.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.Button btnClearFromCache;
        private System.Windows.Forms.Label lblLoadStage;
        private System.Windows.Forms.TabPage tpHeatmap;
        public HeatmapUI heatmapUI;
        private RAGSmiley ragSmiley1;
    }
}
