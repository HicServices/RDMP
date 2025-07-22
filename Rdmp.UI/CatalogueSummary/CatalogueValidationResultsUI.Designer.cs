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
            lblExtractionIdentifiersCount = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            lblRecordCount = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            splitContainer3 = new System.Windows.Forms.SplitContainer();
            dqePivotCategorySelector1 = new DQEPivotCategorySelector();
            timePeriodicityChart1 = new TimePeriodicityChartNoControls();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // evaluationTrackBar1
            // 
            evaluationTrackBar1.AutoSize = true;
            evaluationTrackBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            evaluationTrackBar1.Location = new System.Drawing.Point(0, 299);
            evaluationTrackBar1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            evaluationTrackBar1.Name = "evaluationTrackBar1";
            evaluationTrackBar1.Size = new System.Drawing.Size(1206, 73);
            evaluationTrackBar1.TabIndex = 4;
            evaluationTrackBar1.EvaluationValidationSelected += evaluationTrackBar1_EvaluationSelected;
            // 
            // panel1
            // 
            panel1.Controls.Add(lblExtractionIdentifiersCount);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(label11);
            panel1.Controls.Add(lblRecordCount);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 372);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1206, 100);
            panel1.TabIndex = 3;
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
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(22, 16);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(80, 15);
            label8.TabIndex = 7;
            label8.Text = "Record Count";
            // 
            // splitContainer3
            // 
            splitContainer3.BackColor = System.Drawing.SystemColors.Control;
            splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer3.Location = new System.Drawing.Point(0, 0);
            splitContainer3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Size = new System.Drawing.Size(1062, 633);
            splitContainer3.SplitterDistance = 167;
            splitContainer3.SplitterWidth = 5;
            splitContainer3.TabIndex = 1;
            // 
            // dqePivotCategorySelector1
            // 
            dqePivotCategorySelector1.Dock = System.Windows.Forms.DockStyle.Fill;
            dqePivotCategorySelector1.Location = new System.Drawing.Point(0, 0);
            dqePivotCategorySelector1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            dqePivotCategorySelector1.Name = "dqePivotCategorySelector1";
            dqePivotCategorySelector1.Size = new System.Drawing.Size(131, 295);
            dqePivotCategorySelector1.TabIndex = 0;
            // 
            // timePeriodicityChart1
            // 
            timePeriodicityChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            timePeriodicityChart1.Location = new System.Drawing.Point(0, 0);
            timePeriodicityChart1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            timePeriodicityChart1.Name = "timePeriodicityChart1";
            timePeriodicityChart1.Size = new System.Drawing.Size(1062, 295);
            timePeriodicityChart1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(dqePivotCategorySelector1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(timePeriodicityChart1);
            splitContainer1.Size = new System.Drawing.Size(1206, 299);
            splitContainer1.SplitterDistance = 135;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 3;
            splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
            // 
            // CatalogueValidationResultsUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Controls.Add(evaluationTrackBar1);
            Controls.Add(panel1);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CatalogueValidationResultsUI";
            Size = new System.Drawing.Size(1206, 472);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private DataQualityReporting.SubComponents.EvaluationTrackBar evaluationTrackBar1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private DQEPivotCategorySelector dqePivotCategorySelector1;
        private TimePeriodicityChartNoControls timePeriodicityChart1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblExtractionIdentifiersCount;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblRecordCount;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panel1;
    }
}
