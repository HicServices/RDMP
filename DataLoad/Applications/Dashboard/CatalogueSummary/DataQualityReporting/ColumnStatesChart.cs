using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dashboard.CatalogueSummary.DataQualityReporting.SubComponents;
using DataQualityEngine.Data;

namespace Dashboard.CatalogueSummary.DataQualityReporting
{
    /// <summary>
    /// Only visible after running the data quality engine on a dataset (Catalogue).   Shows each extractable column in the dataset with a horizontal bar indicating what proportion
    /// of the values in the dataset that are in that column are passing validation.  By comparing this chart with the TimePeriodicityChart you can if validation problems in the 
    /// dataset are attributable to specific columns or whether the quality of the entire dataset is bad.  For example if the entire TimePeriodicityChart is red (failing validation)
    /// but only one column in the ColumnStatesChart is showing red then you know that the scope of the problem is limited only to that column.
    /// 
    /// Also included in each column bar (underneath the main colour) is a black/grey bar which shows what proportion of the values in the column were null.
    /// </summary>
    public partial class ColumnStatesChart : UserControl, IDataQualityReportingChart
    {
        public ColumnStatesChart()
        {
            InitializeComponent();
        }

        public void ClearGraph()
        {
            tableLayoutPanel1.Controls.Clear();
        }

        public void SelectEvaluation(Evaluation evaluation, string pivotCategoryValue)
        {
            GenerateChart(evaluation, pivotCategoryValue);
        }

        private void GenerateChart(Evaluation evaluation, string pivotCategoryValue)
        {
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowCount = evaluation.ColumnStates.Count();

            tableLayoutPanel1.SuspendLayout();
            int row = 0;
            foreach (var targetProperty in evaluation.ColumnStates.Select(c=>c.TargetProperty).Distinct())
            {
                Label l = new Label();
                l.Text = targetProperty;
                tableLayoutPanel1.Controls.Add(l,0,row);

                l.Width = l.PreferredWidth;
                tableLayoutPanel1.ColumnStyles[0].SizeType = SizeType.Absolute;
                tableLayoutPanel1.ColumnStyles[0].Width = Math.Max(tableLayoutPanel1.ColumnStyles[0].Width,l.PreferredWidth);
                string property = targetProperty;
                

                ConsequenceBar bar = new ConsequenceBar();

                bar.Correct = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property)).Sum(s => s.PivotCategory.Equals(pivotCategoryValue)?s.CountCorrect:0);
                bar.Invalid = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property)).Sum(s => s.PivotCategory.Equals(pivotCategoryValue)?s.CountInvalidatesRow:0);
                bar.Missing = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property)).Sum(s => s.PivotCategory.Equals(pivotCategoryValue)?s.CountMissing:0);
                bar.Wrong = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property)).Sum(s => s.PivotCategory.Equals(pivotCategoryValue)?s.CountWrong:0);
                bar.DBNull = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property)).Sum(s => s.PivotCategory.Equals(pivotCategoryValue)?s.CountDBNull:0);
                bar.GenerateToolTip();

                tableLayoutPanel1.Controls.Add(bar, 1, row);
                row++;
            }

            foreach (RowStyle r in tableLayoutPanel1.RowStyles)
                r.SizeType = SizeType.AutoSize;

            tableLayoutPanel1.ResumeLayout(true);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return new Size(tableLayoutPanel1.PreferredSize.Width + 200, proposedSize.Height);
        }
    }
}
