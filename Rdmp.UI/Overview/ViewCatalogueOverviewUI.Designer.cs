using BrightIdeasSoftware;
using System.Windows.Forms.DataVisualization.Charting;

namespace Rdmp.UI.Overview
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
            lblTime = new System.Windows.Forms.Label();
            lblWhere = new System.Windows.Forms.Label();
            tbMainWhere = new System.Windows.Forms.TextBox();
            cbTimeColumns = new System.Windows.Forms.ComboBox();
            panel2 = new System.Windows.Forms.Panel();
            label5 = new System.Windows.Forms.Label();
            areaChart1 = new CatalogueSummary.DataQualityReporting.AreaChart(OnTabChange);
            cbFrequency = new System.Windows.Forms.ComboBox();
            panel6 = new System.Windows.Forms.Panel();
            label4 = new System.Windows.Forms.Label();
            lblDateRange = new System.Windows.Forms.Label();
            panel5 = new System.Windows.Forms.Panel();
            label3 = new System.Windows.Forms.Label();
            lblRecords = new System.Windows.Forms.Label();
            panel4 = new System.Windows.Forms.Panel();
            label2 = new System.Windows.Forms.Label();
            lblLatestExtraction = new System.Windows.Forms.Label();
            panel3 = new System.Windows.Forms.Panel();
            label1 = new System.Windows.Forms.Label();
            lblLastDataLoad = new System.Windows.Forms.Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel2.SuspendLayout();
            panel6.SuspendLayout();
            panel5.SuspendLayout();
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
            // lblTime
            // 
            lblTime.AutoSize = true;
            lblTime.Location = new System.Drawing.Point(781, 9);
            lblTime.Name = "lblTime";
            lblTime.Size = new System.Drawing.Size(82, 15);
            lblTime.TabIndex = 9;
            lblTime.Text = "Time Column:";
            // 
            // lblWhere
            // 
            lblWhere.AutoSize = true;
            lblWhere.Location = new System.Drawing.Point(265, 9);
            lblWhere.Name = "lblWhere";
            lblWhere.Size = new System.Drawing.Size(44, 15);
            lblWhere.TabIndex = 8;
            lblWhere.Text = "Where:";
            // 
            // tbMainWhere
            // 
            tbMainWhere.Location = new System.Drawing.Point(315, 6);
            tbMainWhere.Name = "tbMainWhere";
            tbMainWhere.Size = new System.Drawing.Size(460, 23);
            tbMainWhere.TabIndex = 7;
            tbMainWhere.TextChanged += tbMainWhere_TextChanged;
            // 
            // cbTimeColumns
            // 
            cbTimeColumns.FormattingEnabled = true;
            cbTimeColumns.Location = new System.Drawing.Point(869, 6);
            cbTimeColumns.Name = "cbTimeColumns";
            cbTimeColumns.Size = new System.Drawing.Size(196, 23);
            cbTimeColumns.TabIndex = 4;
            cbTimeColumns.SelectedIndexChanged += cbTimeColumns_SelectedIndexChanged;
            // 
            // panel2
            // 
            panel2.Controls.Add(label5);
            panel2.Controls.Add(cbFrequency);
            panel2.Controls.Add(panel6);
            panel2.Controls.Add(panel5);
            panel2.Controls.Add(tbMainWhere);
            panel2.Controls.Add(lblWhere);
            panel2.Controls.Add(lblTime);
            panel2.Controls.Add(cbTimeColumns);
            panel2.Controls.Add(panel4);
            panel2.Controls.Add(panel3);
            splitContainer1.Panel2.Controls.Add(areaChart1);
            panel2.Location = new System.Drawing.Point(0, 424);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(1342, 246);
            panel2.TabIndex = 1;
            // areaChart1
            //
            areaChart1.Location = new System.Drawing.Point(4, 0);
            areaChart1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            areaChart1.Name = "areaChart1";
            areaChart1.Size = new System.Drawing.Size(1075, 400);
            areaChart1.TabIndex = 0;
            areaChart1.Load += areaChart1_Load;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(1114, 9);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(65, 15);
            label5.TabIndex = 11;
            label5.Text = "Frequency:";
            // 
            // cbFrequency
            // 
            cbFrequency.FormattingEnabled = true;
            cbFrequency.Location = new System.Drawing.Point(1181, 6);
            cbFrequency.Name = "cbFrequency";
            cbFrequency.Size = new System.Drawing.Size(135, 23);
            cbFrequency.TabIndex = 10;
            cbFrequency.SelectedIndexChanged += cbFrequency_SelectedIndexChanged;
            // 
            // panel6
            // 
            panel6.AutoSize = true;
            panel6.Controls.Add(label4);
            panel6.Controls.Add(lblDateRange);
            panel6.Location = new System.Drawing.Point(593, 35);
            panel6.Name = "panel6";
            panel6.Size = new System.Drawing.Size(202, 200);
            panel6.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new System.Drawing.Font("Segoe UI", 16F);
            label4.Location = new System.Drawing.Point(3, 4);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(125, 30);
            label4.TabIndex = 2;
            label4.Text = "Date Range";
            // 
            // lblDateRange
            // 
            lblDateRange.AutoSize = true;
            lblDateRange.Location = new System.Drawing.Point(16, 63);
            lblDateRange.Name = "lblDateRange";
            lblDateRange.Size = new System.Drawing.Size(38, 15);
            lblDateRange.TabIndex = 3;
            lblDateRange.Text = "label2";
            lblDateRange.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel5
            // 
            panel5.Controls.Add(label3);
            panel5.Controls.Add(lblRecords);
            panel5.Location = new System.Drawing.Point(397, 35);
            panel5.Name = "panel5";
            panel5.Size = new System.Drawing.Size(182, 200);
            panel5.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Segoe UI", 16F);
            label3.Location = new System.Drawing.Point(34, 4);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(109, 30);
            label3.TabIndex = 2;
            label3.Text = "# Records";
            // 
            // lblRecords
            // 
            lblRecords.Location = new System.Drawing.Point(3, 63);
            lblRecords.Name = "lblRecords";
            lblRecords.Size = new System.Drawing.Size(166, 23);
            lblRecords.TabIndex = 3;
            lblRecords.Text = "label2";
            lblRecords.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel4
            // 
            panel4.Controls.Add(label2);
            panel4.Controls.Add(lblLatestExtraction);
            panel4.Location = new System.Drawing.Point(209, 35);
            panel4.Name = "panel4";
            panel4.Size = new System.Drawing.Size(182, 200);
            panel4.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Segoe UI", 16F);
            label2.Location = new System.Drawing.Point(10, 4);
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
            panel3.Location = new System.Drawing.Point(13, 35);
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
            // ViewCatalogueOverviewUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoScroll = true;
            Controls.Add(panel2);
            Controls.Add(panel1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ViewCatalogueOverviewUI";
            Size = new System.Drawing.Size(1342, 955);
            panel1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
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
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblWhere;
        private System.Windows.Forms.TextBox tbMainWhere;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblDateRange;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblRecords;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbFrequency;
    }
}