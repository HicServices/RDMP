using Dashboard.CatalogueSummary.LoadEvents;

namespace Dashboard.CatalogueSummary
{
    partial class CatalogueSummaryScreen
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.timePeriodicityChart1 = new Dashboard.CatalogueSummary.DataQualityReporting.TimePeriodicityChart();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.dqePivotCategorySelector1 = new Dashboard.CatalogueSummary.DataQualityReporting.SubComponents.DQEPivotCategorySelector();
            this.columnStatesChart1 = new Dashboard.CatalogueSummary.DataQualityReporting.ColumnStatesChart();
            this.evaluationTrackBar1 = new Dashboard.CatalogueSummary.DataQualityReporting.SubComponents.EvaluationTrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.timePeriodicityChart1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(1109, 605);
            this.splitContainer1.SplitterDistance = 302;
            this.splitContainer1.TabIndex = 3;
            // 
            // timePeriodicityChart1
            // 
            this.timePeriodicityChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timePeriodicityChart1.Location = new System.Drawing.Point(0, 0);
            this.timePeriodicityChart1.Name = "timePeriodicityChart1";
            this.timePeriodicityChart1.Size = new System.Drawing.Size(1105, 298);
            this.timePeriodicityChart1.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Location = new System.Drawing.Point(3, 3);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.dqePivotCategorySelector1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.columnStatesChart1);
            this.splitContainer3.Size = new System.Drawing.Size(1105, 295);
            this.splitContainer3.SplitterDistance = 176;
            this.splitContainer3.TabIndex = 1;
            // 
            // dqePivotCategorySelector1
            // 
            this.dqePivotCategorySelector1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dqePivotCategorySelector1.Location = new System.Drawing.Point(0, 0);
            this.dqePivotCategorySelector1.Name = "dqePivotCategorySelector1";
            this.dqePivotCategorySelector1.Size = new System.Drawing.Size(176, 295);
            this.dqePivotCategorySelector1.TabIndex = 0;
            // 
            // columnStatesChart1
            // 
            this.columnStatesChart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.columnStatesChart1.Location = new System.Drawing.Point(0, 0);
            this.columnStatesChart1.Name = "columnStatesChart1";
            this.columnStatesChart1.Size = new System.Drawing.Size(925, 295);
            this.columnStatesChart1.TabIndex = 2;
            // 
            // evaluationTrackBar1
            // 
            this.evaluationTrackBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.evaluationTrackBar1.Evaluations = null;
            this.evaluationTrackBar1.Location = new System.Drawing.Point(8, 614);
            this.evaluationTrackBar1.Name = "evaluationTrackBar1";
            this.evaluationTrackBar1.Size = new System.Drawing.Size(1098, 71);
            this.evaluationTrackBar1.TabIndex = 4;
            this.evaluationTrackBar1.EvaluationSelected += new Dashboard.CatalogueSummary.DataQualityReporting.SubComponents.EvaluationSelectedHandler(this.evaluationTrackBar1_EvaluationSelected);
            // 
            // CatalogueSummaryScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.evaluationTrackBar1);
            this.Controls.Add(this.splitContainer1);
            this.Name = "CatalogueSummaryScreen";
            this.Size = new System.Drawing.Size(1115, 692);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DataQualityReporting.TimePeriodicityChart timePeriodicityChart1;
        private DataQualityReporting.ColumnStatesChart columnStatesChart1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private DataQualityReporting.SubComponents.DQEPivotCategorySelector dqePivotCategorySelector1;
        private DataQualityReporting.SubComponents.EvaluationTrackBar evaluationTrackBar1;
    }
}
