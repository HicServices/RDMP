// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Rdmp.Core.CatalogueAnalysisTools.Data;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.CatalogueSummary.DataQualityReporting;

/// <summary>
/// Only visible after running the Data Quality Engine at least once on a given dataset (Catalogue).  Shows the number of records each month in the dataset that are passing/failing
/// validation as a stack chart.  The Data tab will show you the raw counts that power the graph.  See SecondaryConstraintUI for validation configuration and ConsequenceKey for the
/// meanings of each consequence classification.
/// </summary>
public partial class TimePeriodicityChart : RDMPUserControl, IDataQualityReportingChart
{
    private readonly ChartLookAndFeelSetter _chartLookAndFeelSetter = new();

    public TimePeriodicityChart()
    {
        InitializeComponent();

        chart1.PaletteCustomColors = new Color[]
        {
            ConsequenceBar.CorrectColor,
            ConsequenceBar.WrongColor,
            ConsequenceBar.MissingColor,
            ConsequenceBar.InvalidColor
        };
        chart1.Palette = ChartColorPalette.None;
        chart1.KeyUp += chart1_KeyUp;

        dataGridView1.ColumnAdded += (s, e) => e.Column.FillWeight = 1;
    }


    /// <inheritdoc/>
    public void ClearGraph()
    {
        chart1.Series.Clear();
    }

    private string _pivotCategoryValue;

    /// <inheritdoc/>
    public void SelectEvaluation(Evaluation evaluation, string pivotCategoryValue)
    {
        _pivotCategoryValue = pivotCategoryValue;
        GenerateChart(evaluation, pivotCategoryValue);
    }


    public void SelectEvaluation(CatalogueValidation evaluation, string pivotCategoryValue)
    {
        _pivotCategoryValue = pivotCategoryValue;
        GenerateChart(evaluation, pivotCategoryValue);
    }

    private void GenerateChart(Evaluation evaluation, string pivotCategoryValue)
    {
        _currentEvaluation = evaluation;

        chart1.Visible = false;
        var dt = PeriodicityState.GetPeriodicityForDataTableForEvaluation(evaluation, pivotCategoryValue, true);

        if (dt == null)
        {
            ClearGraph();
            chart1.Visible = true;
            return;
        }

        chart1.DataSource = dt;
        dataGridView1.DataSource = dt;

        chart1.Series.Clear();


        if (dt.Rows.Count != 0)
            ChartLookAndFeelSetter.PopulateYearMonthChart(chart1, dt, "Data Quality");

        chart1.DataBind();
        chart1.Visible = true;
        if (_currentEvaluation is not null)
        {
            ReGenerateAnnotations();
        }
    }

    private void GenerateChart(CatalogueValidation evaluation, string pivotCategoryValue)
    {
        //_currentEvaluation = evaluation;

        chart1.Visible = false;
        var dt = evaluation.GenerateDataTable();

        if (dt == null)
        {
            ClearGraph();
            chart1.Visible = true;
            return;
        }

        chart1.DataSource = dt;
        dataGridView1.DataSource = dt;

        chart1.Series.Clear();


        if (dt.Rows.Count != 0)
            ChartLookAndFeelSetter.PopulateYearMonthChart(chart1, dt, "Data Quality");

        chart1.DataBind();
        chart1.Visible = true;
        label1.Visible = false;
        label2.Visible = false;
        cbShowGaps.Visible = false;
        btnAnnotator.Visible = false;
        btnPointer.Visible = false;
        //ReGenerateAnnotations();
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
        var lastBucket = DateTime.MinValue;
        var bucketNumber = 0;

        foreach (DataRow dr in dt.Rows)
        {
            var currentBucket = new DateTime((int)dr["Year"], (int)dr["Month"], 1);
            var diff = currentBucket.Subtract(lastBucket);

            bucketNumber++;

            if (lastBucket != DateTime.MinValue && diff.TotalDays > 31)
            {
                //add gap annotation
                var line = new LineAnnotation
                {
                    IsSizeAlwaysRelative = false,
                    AxisX = chart1.ChartAreas[0].AxisX,
                    AxisY = chart1.ChartAreas[0].AxisY,
                    AnchorX = bucketNumber,
                    AnchorY = 0,
                    IsInfinitive = true,
                    LineWidth = 1,
                    LineDashStyle = ChartDashStyle.Dot,
                    Width = 0
                };
                line.LineWidth = 2;
                line.StartCap = LineAnchorCapStyle.None;
                line.EndCap = LineAnchorCapStyle.None;

                var text = new TextAnnotation
                {
                    Text = $"{diff.TotalDays}d gap",
                    IsSizeAlwaysRelative = false,
                    AxisX = chart1.ChartAreas[0].AxisX,
                    AxisY = chart1.ChartAreas[0].AxisY,
                    AnchorX = bucketNumber,
                    AnchorY = 0
                };

                chart1.Annotations.Add(line);
                chart1.Annotations.Add(text);
            }

            lastBucket = new DateTime((int)dr["Year"], (int)dr["Month"], 1);
        }
    }

    private void AddUserAnnotations(Evaluation evaluation)
    {
        //clear old annotations
        chart1.Annotations.Clear();

        var evaluations = evaluation.GetAllDQEGraphAnnotations(_pivotCategoryValue)
            .Where(a => a.AnnotationIsForGraph == DQEGraphType.TimePeriodicityGraph);

        foreach (var annotation in evaluations)
        {
            var a = new DQEGraphAnnotationUI(annotation, chart1);
            chart1.Annotations.Add(a.Annotation);
            chart1.Annotations.Add(a.TextAnnotation);
        }
    }


    private double pointStartX;
    private double pointStartY;

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

        // don't let them annotate emptiness!
        if (double.IsNaN(pointEndX) || double.IsNaN(pointEndY)) return;

        if (Activator.TypeText(new DialogArgs
        {
            WindowTitle = "Add Annotation",
            TaskDescription =
                    "Type some annotation text(will be saved to the database for other data analysts to see)",
            EntryLabel = "Annotation:"
        }, 500, null, out var result, false))
        {
            //create new annotation in the database
            new DQEGraphAnnotation(_currentEvaluation.DQERepository, pointStartX, pointStartY, pointEndX, pointEndY,
                result, _currentEvaluation, DQEGraphType.TimePeriodicityGraph, _pivotCategoryValue);

            //refresh the annotations
            AddUserAnnotations(_currentEvaluation);
        }
    }

    private bool annotating;
    private Evaluation _currentEvaluation;

    private void btnAnnotator_Click(object sender, EventArgs e)
    {
        annotating = true;
        btnAnnotator.Enabled = false;
        btnPointer.Enabled = true;
        Cursor = Cursors.Cross;
    }

    private void btnPointer_Click(object sender, EventArgs e)
    {
        annotating = false;
        btnAnnotator.Enabled = true;
        btnPointer.Enabled = false;
        Cursor = DefaultCursor;
    }

    private void chart1_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete)
            foreach (DQEGraphAnnotationUI ui in
                     chart1.Annotations
                         .Where(a => a.IsSelected && a.Tag is DQEGraphAnnotationUI) //get the selected ones
                         .Select(t => t.Tag) //get all the appropriately typed annotations
                         .Distinct() //distinct because we get a line and a text for each - works because all .Equals are on ID of underlying object
                         .ToArray()) //use ToArray so we can modify it in for loop
                if (Activator.YesNo($"Delete annotation '{ui.TextAnnotation.Text}'",
                        "Confirm deleting annotation from database"))
                    ui.Delete(chart1); //delete it is what we are actually doing
    }

    private void cbShowGaps_CheckedChanged(object sender, EventArgs e)
    {
        if (_currentEvaluation is not null && chart1.DataSource != null) ReGenerateAnnotations();
    }
}