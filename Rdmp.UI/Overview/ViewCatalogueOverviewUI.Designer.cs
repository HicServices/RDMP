using BrightIdeasSoftware;
using System.Windows.Forms.DataVisualization.Charting;

namespace Rdmp.UI.ExtractionUIs
{
    partial class ViewCatalogueOverviewUI
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
            panel1 = new System.Windows.Forms.Panel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            lblDescription = new System.Windows.Forms.Label();
            lblName = new System.Windows.Forms.Label();
            cbTimeColumns = new System.Windows.Forms.ComboBox();
            areaChart1 = new CatalogueSummary.DataQualityReporting.AreaChart();
            panel2 = new System.Windows.Forms.Panel();
            panel4 = new System.Windows.Forms.Panel();
            label2 = new System.Windows.Forms.Label();
            lblLatestExtraction = new System.Windows.Forms.Label();
            panel3 = new System.Windows.Forms.Panel();
            label1 = new System.Windows.Forms.Label();
            lblLastDataLoad = new System.Windows.Forms.Label();
            viewsqlAndResultsWithDataGridui1 = new DataViewing.ViewSQLAndResultsWithDataGridUI();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel2.SuspendLayout();
            panel4.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(splitContainer1);
            panel1.Location = new System.Drawing.Point(0, 25);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1342, 400);
            panel1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lblDescription);
            splitContainer1.Panel1.Controls.Add(lblName);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(cbTimeColumns);
            splitContainer1.Panel2.Controls.Add(areaChart1);
            splitContainer1.Size = new System.Drawing.Size(1342, 400);
            splitContainer1.SplitterDistance = 259;
            splitContainer1.TabIndex = 0;
            // 
            // lblDescription
            // 
            lblDescription.Location = new System.Drawing.Point(16, 108);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(240, 285);
            lblDescription.TabIndex = 3;
            lblDescription.Text = "label1";
            lblDescription.Click += lblDescription_Click;
            // 
            // lblName
            // 
            lblName.Font = new System.Drawing.Font("Segoe UI", 16F);
            lblName.Location = new System.Drawing.Point(16, 29);
            lblName.Name = "lblName";
            lblName.Size = new System.Drawing.Size(240, 79);
            lblName.TabIndex = 2;
            lblName.Text = "lblName";
            // 
            // cbTimeColumns
            // 
            cbTimeColumns.FormattingEnabled = true;
            cbTimeColumns.Location = new System.Drawing.Point(853, 370);
            cbTimeColumns.Name = "cbTimeColumns";
            cbTimeColumns.Size = new System.Drawing.Size(196, 23);
            cbTimeColumns.TabIndex = 4;
            cbTimeColumns.SelectedIndexChanged += cbTimeColumns_SelectedIndexChanged;
            // 
            // areaChart1
            // 
            areaChart1.Location = new System.Drawing.Point(4, 0);
            areaChart1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            areaChart1.Name = "areaChart1";
            areaChart1.Size = new System.Drawing.Size(1075, 400);
            areaChart1.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.Controls.Add(viewsqlAndResultsWithDataGridui1);
            panel2.Controls.Add(panel4);
            panel2.Controls.Add(panel3);
            panel2.Location = new System.Drawing.Point(0, 424);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(1342, 207);
            panel2.TabIndex = 1;
            // 
            // panel4
            // 
            panel4.Controls.Add(label2);
            panel4.Controls.Add(lblLatestExtraction);
            panel4.Location = new System.Drawing.Point(199, 7);
            panel4.Name = "panel4";
            panel4.Size = new System.Drawing.Size(182, 200);
            panel4.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Segoe UI", 16F);
            label2.Location = new System.Drawing.Point(3, 4);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(169, 30);
            label2.TabIndex = 2;
            label2.Text = "Latest Extraction";
            label2.Click += label2_Click;
            // 
            // lblLatestExtraction
            // 
            lblLatestExtraction.Location = new System.Drawing.Point(3, 63);
            lblLatestExtraction.Name = "lblLatestExtraction";
            lblLatestExtraction.Size = new System.Drawing.Size(166, 23);
            lblLatestExtraction.TabIndex = 3;
            lblLatestExtraction.Text = "label2";
            lblLatestExtraction.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblLatestExtraction.Click += lblLatestExtraction_Click;
            // 
            // panel3
            // 
            panel3.Controls.Add(label1);
            panel3.Controls.Add(lblLastDataLoad);
            panel3.Location = new System.Drawing.Point(3, 6);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(190, 201);
            panel3.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI", 16F);
            label1.Location = new System.Drawing.Point(3, 1);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(181, 30);
            label1.TabIndex = 0;
            label1.Text = "Lastest Data Load";
            // 
            // lblLastDataLoad
            // 
            lblLastDataLoad.Location = new System.Drawing.Point(3, 63);
            lblLastDataLoad.Name = "lblLastDataLoad";
            lblLastDataLoad.Size = new System.Drawing.Size(179, 25);
            lblLastDataLoad.TabIndex = 1;
            lblLastDataLoad.Text = "label2";
            lblLastDataLoad.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblLastDataLoad.Click += lblLastDataLoad_Click;
            // 
            // viewsqlAndResultsWithDataGridui1
            // 
            viewsqlAndResultsWithDataGridui1.Location = new System.Drawing.Point(496, 124);
            viewsqlAndResultsWithDataGridui1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            viewsqlAndResultsWithDataGridui1.Name = "viewsqlAndResultsWithDataGridui1";
            viewsqlAndResultsWithDataGridui1.Size = new System.Drawing.Size(8, 8);
            viewsqlAndResultsWithDataGridui1.TabIndex = 6;
            // 
            // ViewCatalogueOverviewUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(panel2);
            Controls.Add(panel1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ViewCatalogueOverviewUI";
            Size = new System.Drawing.Size(1342, 955);
            panel1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lblLastDataLoad;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblLatestExtraction;
        private CatalogueSummary.DataQualityReporting.AreaChart areaChart1;
        private System.Windows.Forms.ComboBox cbTimeColumns;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private DataViewing.ViewSQLAndResultsWithDataGridUI viewsqlAndResultsWithDataGridui1;
    }
}