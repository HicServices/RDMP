using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
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
    /// <para>Also included in each column bar (underneath the main colour) is a black/grey bar which shows what proportion of the values in the column were null.</para>
    /// </summary>
    public partial class ColumnStatesChart : UserControl, IDataQualityReportingChart
    {
        public ColumnStatesChart()
        {
            InitializeComponent();
        }

        public void ClearGraph()
        {
            panel1.Controls.Clear();
        }

        public void SelectEvaluation(Evaluation evaluation, string pivotCategoryValue)
        {
            GenerateChart(evaluation, pivotCategoryValue);
        }

        private void GenerateChart(Evaluation evaluation, string pivotCategoryValue)
        {
            panel1.Controls.Clear();

            int row = 0;

            foreach (var property in evaluation.ColumnStates.Select(c=>c.TargetProperty).Distinct())
            {
                ConsequenceBar bar = new ConsequenceBar();
                bar.Label = property;
                bar.Correct = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property)).Sum(s => s.PivotCategory.Equals(pivotCategoryValue)?s.CountCorrect:0);
                bar.Invalid = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property)).Sum(s => s.PivotCategory.Equals(pivotCategoryValue)?s.CountInvalidatesRow:0);
                bar.Missing = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property)).Sum(s => s.PivotCategory.Equals(pivotCategoryValue)?s.CountMissing:0);
                bar.Wrong = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property)).Sum(s => s.PivotCategory.Equals(pivotCategoryValue)?s.CountWrong:0);
                bar.DBNull = evaluation.ColumnStates.Where(c => c.TargetProperty.Equals(property)).Sum(s => s.PivotCategory.Equals(pivotCategoryValue)?s.CountDBNull:0);
                
                bar.Width = panel1.Width;
                bar.Location = new Point(0,23*row++);
                
                bar.Anchor = AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;

                bar.GenerateToolTip();

                panel1.Controls.Add(bar);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            foreach (ConsequenceBar bar in panel1.Controls.OfType<ConsequenceBar>())
                bar.Invalidate();
        }
    }
}
