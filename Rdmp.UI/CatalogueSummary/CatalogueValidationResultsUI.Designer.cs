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
            splitContainer3 = new System.Windows.Forms.SplitContainer();
            dqePivotCategorySelector1 = new DQEPivotCategorySelector();
            timePeriodicityChart1 = new TimePeriodicityChartNoControls();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
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
            evaluationTrackBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            evaluationTrackBar1.Location = new System.Drawing.Point(0, 291);
            evaluationTrackBar1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            evaluationTrackBar1.Name = "evaluationTrackBar1";
            evaluationTrackBar1.Size = new System.Drawing.Size(1206, 100);
            evaluationTrackBar1.TabIndex = 4;
            this.evaluationTrackBar1.EvaluationValidationSelected += new EvaluationSelectedValidationHandler(this.evaluationTrackBar1_EvaluationSelected);

            // 
            // splitContainer3
            // 
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
            dqePivotCategorySelector1.Size = new System.Drawing.Size(131, 287);
            dqePivotCategorySelector1.TabIndex = 0;
            // 
            // timePeriodicityChart1
            // 
            timePeriodicityChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            timePeriodicityChart1.Location = new System.Drawing.Point(0, 0);
            timePeriodicityChart1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            timePeriodicityChart1.Name = "timePeriodicityChart1";
            timePeriodicityChart1.Size = new System.Drawing.Size(1062, 287);
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
            splitContainer1.Size = new System.Drawing.Size(1206, 291);
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
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CatalogueValidationResultsUI";
            Size = new System.Drawing.Size(1206, 391);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion
        private DataQualityReporting.SubComponents.EvaluationTrackBar evaluationTrackBar1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private DQEPivotCategorySelector dqePivotCategorySelector1;
        private TimePeriodicityChartNoControls timePeriodicityChart1;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}
