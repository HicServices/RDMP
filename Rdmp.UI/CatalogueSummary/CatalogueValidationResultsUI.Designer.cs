using Rdmp.UI.CatalogueSummary.DataQualityReporting;
using Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents;

namespace Rdmp.UI.CatalogueSummary
{
    partial class CatalogueValidationResultsUI
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
            evaluationTrackBar1 = new EvaluationTrackBar();
            panel1 = new System.Windows.Forms.Panel();
            tblCompletionRates = new System.Windows.Forms.TableLayoutPanel();
            lblExtractionIdentifiersCount = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            lblRecordCount = new System.Windows.Forms.Label();
            splitContainer3 = new System.Windows.Forms.SplitContainer();
            timePeriodicityChart1 = new TimePeriodicityChartNoControls();
            dqePivotCategorySelector1 = new DQEPivotCategorySelector();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // evaluationTrackBar1
            // 
            evaluationTrackBar1.AutoSize = true;
            evaluationTrackBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            evaluationTrackBar1.Location = new System.Drawing.Point(0, 374);
            evaluationTrackBar1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            evaluationTrackBar1.Name = "evaluationTrackBar1";
            evaluationTrackBar1.Size = new System.Drawing.Size(1206, 73);
            evaluationTrackBar1.TabIndex = 4;
            evaluationTrackBar1.EvaluationValidationSelected += evaluationTrackBar1_EvaluationSelected;
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.BackColor = System.Drawing.SystemColors.Control;
            panel1.Controls.Add(tblCompletionRates);
            panel1.Controls.Add(lblExtractionIdentifiersCount);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(label11);
            panel1.Controls.Add(lblRecordCount);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1206, 443);
            panel1.TabIndex = 3;
            // 
            // tblCompletionRates
            // 
            tblCompletionRates.AutoSize = true;
            tblCompletionRates.BackColor = System.Drawing.SystemColors.Control;
            tblCompletionRates.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            tblCompletionRates.ColumnCount = 2;
            tblCompletionRates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tblCompletionRates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tblCompletionRates.Location = new System.Drawing.Point(344, 16);
            tblCompletionRates.Name = "tblCompletionRates";
            tblCompletionRates.RowCount = 2;
            tblCompletionRates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tblCompletionRates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tblCompletionRates.Size = new System.Drawing.Size(200, 45);
            tblCompletionRates.TabIndex = 11;
            // 
            // lblExtractionIdentifiersCount
            // 
            lblExtractionIdentifiersCount.AutoSize = true;
            lblExtractionIdentifiersCount.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblExtractionIdentifiersCount.Location = new System.Drawing.Point(173, 31);
            lblExtractionIdentifiersCount.Name = "lblExtractionIdentifiersCount";
            lblExtractionIdentifiersCount.Size = new System.Drawing.Size(24, 30);
            lblExtractionIdentifiersCount.TabIndex = 10;
            lblExtractionIdentifiersCount.Text = "0";
            lblExtractionIdentifiersCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(22, 16);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(80, 15);
            label8.TabIndex = 7;
            label8.Text = "Record Count";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(163, 16);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(146, 15);
            label11.TabIndex = 9;
            label11.Text = "Extraction Identifier Count";
            // 
            // lblRecordCount
            // 
            lblRecordCount.AutoSize = true;
            lblRecordCount.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblRecordCount.Location = new System.Drawing.Point(34, 31);
            lblRecordCount.Name = "lblRecordCount";
            lblRecordCount.Size = new System.Drawing.Size(24, 30);
            lblRecordCount.TabIndex = 8;
            lblRecordCount.Text = "0";
            lblRecordCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // splitContainer3
            // 
            splitContainer3.BackColor = System.Drawing.SystemColors.Control;
            splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer3.Location = new System.Drawing.Point(0, 0);
            splitContainer3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Size = new System.Drawing.Size(1062, 3000);
            splitContainer3.SplitterDistance = 167;
            splitContainer3.SplitterWidth = 5;
            splitContainer3.TabIndex = 1;
            // 
            // timePeriodicityChart1
            // 
            timePeriodicityChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            timePeriodicityChart1.Location = new System.Drawing.Point(0, 0);
            timePeriodicityChart1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            timePeriodicityChart1.Name = "timePeriodicityChart1";
            timePeriodicityChart1.Size = new System.Drawing.Size(1206, 447);
            timePeriodicityChart1.TabIndex = 0;
            // 
            // dqePivotCategorySelector1
            // 
            dqePivotCategorySelector1.Dock = System.Windows.Forms.DockStyle.Left;
            dqePivotCategorySelector1.Location = new System.Drawing.Point(0, 0);
            dqePivotCategorySelector1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            dqePivotCategorySelector1.Name = "dqePivotCategorySelector1";
            dqePivotCategorySelector1.Size = new System.Drawing.Size(131, 374);
            dqePivotCategorySelector1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.MinimumSize = new System.Drawing.Size(0, 800);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(dqePivotCategorySelector1);
            splitContainer2.Panel1.Controls.Add(evaluationTrackBar1);
            splitContainer2.Panel1.Controls.Add(timePeriodicityChart1);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(panel1);
            splitContainer2.Size = new System.Drawing.Size(1206, 894);
            splitContainer2.SplitterDistance = 447;
            splitContainer2.TabIndex = 5;
            // 
            // CatalogueValidationResultsUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSize = true;
            BackColor = System.Drawing.SystemColors.Control;
            Controls.Add(splitContainer2);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(0, 800);
            Name = "CatalogueValidationResultsUI";
            Size = new System.Drawing.Size(1206, 894);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion
        private DataQualityReporting.SubComponents.EvaluationTrackBar evaluationTrackBar1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.Label lblExtractionIdentifiersCount;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblRecordCount;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tblCompletionRates;
        private TimePeriodicityChartNoControls timePeriodicityChart1;
        private DQEPivotCategorySelector dqePivotCategorySelector1;
        private System.Windows.Forms.SplitContainer splitContainer2;
    }
}
