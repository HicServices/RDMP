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
            areaChart1 = new CatalogueSummary.DataQualityReporting.AreaChart();
            panel2 = new System.Windows.Forms.Panel();
            lblLatestExtraction = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            lblLastDataLoad = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            cbTimeColumns = new System.Windows.Forms.ComboBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.Color.IndianRed;
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
            lblDescription.AutoSize = true;
            lblDescription.Location = new System.Drawing.Point(33, 81);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(38, 15);
            lblDescription.TabIndex = 3;
            lblDescription.Text = "label1";
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new System.Drawing.Point(31, 28);
            lblName.Name = "lblName";
            lblName.Size = new System.Drawing.Size(52, 15);
            lblName.TabIndex = 2;
            lblName.Text = "lblName";
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
            panel2.Controls.Add(lblLatestExtraction);
            panel2.Controls.Add(label2);
            panel2.Controls.Add(lblLastDataLoad);
            panel2.Controls.Add(label1);
            panel2.Location = new System.Drawing.Point(0, 424);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(1342, 207);
            panel2.TabIndex = 1;
            // 
            // lblLatestExtraction
            // 
            lblLatestExtraction.AutoSize = true;
            lblLatestExtraction.Location = new System.Drawing.Point(742, 107);
            lblLatestExtraction.Name = "lblLatestExtraction";
            lblLatestExtraction.Size = new System.Drawing.Size(38, 15);
            lblLatestExtraction.TabIndex = 3;
            lblLatestExtraction.Text = "label2";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(714, 37);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(94, 15);
            label2.TabIndex = 2;
            label2.Text = "Latest Extraction";
            // 
            // lblLastDataLoad
            // 
            lblLastDataLoad.AutoSize = true;
            lblLastDataLoad.Location = new System.Drawing.Point(92, 107);
            lblLastDataLoad.Name = "lblLastDataLoad";
            lblLastDataLoad.Size = new System.Drawing.Size(38, 15);
            lblLastDataLoad.TabIndex = 1;
            lblLastDataLoad.Text = "label2";
            lblLastDataLoad.Click += lblLastDataLoad_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(75, 37);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(99, 15);
            label1.TabIndex = 0;
            label1.Text = "Lastest Data Load";
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
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
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
    }
}