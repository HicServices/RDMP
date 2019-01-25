using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CatalogueLibrary.Data;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using Dashboard.CatalogueSummary.DataQualityReporting.SubComponents;
using DataQualityEngine.Data;
using HIC.Common.Validation.Constraints;
using ReusableUIComponents;
using Cursor = System.Windows.Forms.Cursor;

namespace Dashboard.CatalogueSummary.DataQualityReporting
{
    /// <summary>
    /// Only visible after running the Data Quality Engine at least once on a given dataset (Catalogue).  Shows the number of records each month in the dataset that are passing/failing
    /// validation as a stack chart.  The Data tab will show you the raw counts that power the graph.  See SecondaryConstraintUI for validation configuration and ConsequenceKey for the
    /// meanings of each consequence classification.
    /// </summary>
    public partial class TimePeriodicityChart : RDMPUserControl,IDataQualityReportingChart
    {
        private readonly ChartLookAndFeelSetter _chartLookAndFeelSetter =  new ChartLookAndFeelSetter();

        public TimePeriodicityChart()
        {
            InitializeComponent();
            
            chart1.PaletteCustomColors = new Color[]
            {
             ConsequenceBar.CorrectColor,
             ConsequenceBar.InvalidColor,
             ConsequenceBar.MissingColor,
             ConsequenceBar.WrongColor
            };
            chart1.Palette = ChartColorPalette.None;
            chart1.KeyUp += chart1_KeyUp;
        }



        public void ClearGraph()
        {
            chart1.Series.Clear();
        }

        private string _pivotCategoryValue = null;
        public void SelectEvaluation(Evaluation evaluation, string pivotCategoryValue)
        {
            _pivotCategoryValue = pivotCategoryValue;
            GenerateChart(evaluation, pivotCategoryValue);
        }

        private void GenerateChart(Evaluation evaluation, string pivotCategoryValue)
        {
            _currentEvaluation = evaluation;

            chart1.Visible = false;
            var dt = PeriodicityState.GetPeriodicityForDataTableForEvaluation(evaluation, pivotCategoryValue, true);
            chart1.DataSource = dt;
            dataGridView1.DataSource = dt;

            chart1.Series.Clear();


            if (dt.Rows.Count != 0)
                _chartLookAndFeelSetter.PopulateYearMonthChart(chart1, dt, "Data Quality");
            
            chart1.DataBind();
            chart1.Visible = true;
            
            ReGenerateAnnotations();
        }

        private void ReGenerateAnnotations()
        {

            chart1.Annotations.Clear();

            AddUserAnnotations(_currentEvaluation);

            if (cbShowGaps.Checked)
                AddGapAnnotations(chart1.DataSource as DataTable);

        }

        private void AddGapAnnotations(DataTable dt)
        {
            DateTime lastBucket = DateTime.MinValue;
            int bucketNumber = 0;

            foreach (DataRow dr in dt.Rows)
            {
                DateTime currentBucket = new DateTime((int) dr["Year"],(int) dr["Month"],1);
                var diff = currentBucket.Subtract(lastBucket);

                bucketNumber++;
                
                if (lastBucket != DateTime.MinValue && diff.TotalDays >31)
                {
                    //add gap annotation
                    var line = new LineAnnotation();
                    line.IsSizeAlwaysRelative = false;
                    line.AxisX = chart1.ChartAreas[0].AxisX;
                    line.AxisY = chart1.ChartAreas[0].AxisY;
                    line.AnchorX = bucketNumber;
                    line.AnchorY = 0;
                    line.IsInfinitive = true;
                    line.LineWidth = 1;
                    line.LineDashStyle = ChartDashStyle.Dot;
                    line.Width = 0;
                    line.LineWidth = 2;
                    line.StartCap = LineAnchorCapStyle.None;
                    line.EndCap = LineAnchorCapStyle.None;

                    var text = new TextAnnotation();
                    text.Text = diff.TotalDays +"d gap";
                    text.IsSizeAlwaysRelative = false;
                    text.AxisX = chart1.ChartAreas[0].AxisX;
                    text.AxisY = chart1.ChartAreas[0].AxisY;
                    text.AnchorX = bucketNumber;
                    text.AnchorY = 0;

                    chart1.Annotations.Add(line);
                    chart1.Annotations.Add(text);
                }

                lastBucket = new DateTime((int) dr["Year"],(int) dr["Month"],1);
            }
        }

        private void AddUserAnnotations(Evaluation evaluation)
        {
            //clear old annotations
            chart1.Annotations.Clear();

            var evaluations = evaluation.GetAllDQEGraphAnnotations(_pivotCategoryValue).Where(a => a.AnnotationIsForGraph == DQEGraphType.TimePeriodicityGraph);
            
            foreach (DQEGraphAnnotation annotation in evaluations)
            {
                var a = new DQEGraphAnnotationUI(annotation,chart1);
                chart1.Annotations.Add(a.Annotation);
                chart1.Annotations.Add(a.TextAnnotation);
            }
        }

        private int GetOffset(string yearMonth)
        {
            var match = Regex.Match(yearMonth, @"\d{4}-([A-Za-z]*)");

            if(!match.Success)
                throw new Exception("Regex did not match!");

            return DateTime.ParseExact(match.Groups[1].Value, "MMMM", CultureInfo.CurrentCulture).Month;
        }


        private double pointStartX = 0;
        private double pointStartY = 0;

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!chart1.Focused)
                chart1.Focus();

            if (!annotating)
                return;
            
            pointStartX = chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
            pointStartY = chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);
        }

        private void chart1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!annotating)
                return;

            var pointEndX = chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
            var pointEndY = chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);

          
            TypeTextOrCancelDialog inputBox = new TypeTextOrCancelDialog("Enter Annotation Text", "Type some annotation text (will be saved to the database for other data analysts to see)",500);
            if (DialogResult.OK == inputBox.ShowDialog())
            {
                //create new annotation in the database
                new DQEGraphAnnotation(_currentEvaluation.DQERepository,pointStartX, pointStartY, pointEndX, pointEndY, inputBox.ResultText, _currentEvaluation, DQEGraphType.TimePeriodicityGraph, _pivotCategoryValue);
                
                //refresh the annotations
                AddUserAnnotations(_currentEvaluation);
            }
        }

        private bool annotating = false;
        private Evaluation _currentEvaluation;

        private void btnAnnotator_Click(object sender, EventArgs e)
        {
            annotating = true;
            btnAnnotator.Enabled = false;
            btnPointer.Enabled = true;
            this.Cursor = Cursors.Cross;
        }

        private void btnPointer_Click(object sender, EventArgs e)
        {
            annotating = false;
            btnAnnotator.Enabled = true;
            btnPointer.Enabled = false;
            this.Cursor = DefaultCursor;
        }

        void chart1_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
                foreach (DQEGraphAnnotationUI ui in
                    chart1.Annotations.Where(a => a.IsSelected && a.Tag is DQEGraphAnnotationUI) //get the selected ones
                           .Select(t => t.Tag) //get all the appropriately typed annotations
                           .Distinct() //distinct because we get a line and a text for each - works because all .Equals are on ID of underlying object
                           .ToArray())//use ToArray so we can modify it in for loop
                {
                    if(MessageBox.Show("Delete annotation '" + ui.TextAnnotation.Text +"'", "Confirm deleting annotation from database",MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                        ui.Delete(chart1);//delete it is what we are actually doing
                }
        }

        private void cbShowGaps_CheckedChanged(object sender, EventArgs e)
        {
            ReGenerateAnnotations();
        }
    }
}
