// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataViewing;
using Terminal.Gui;
using Terminal.Gui.Graphs;
using static Terminal.Gui.TabView;
using Attribute = Terminal.Gui.Attribute;
using Color = Terminal.Gui.Color;
using Point = Terminal.Gui.Point;
using PointF = Terminal.Gui.PointF;

namespace Rdmp.Core.CommandLine.Gui;

internal class ConsoleGuiViewGraph : ConsoleGuiSqlEditor
{
    private readonly AggregateConfiguration aggregate;
    private GraphView graphView;
    private Tab graphTab;

    public ConsoleGuiViewGraph(IBasicActivateItems activator, AggregateConfiguration aggregate) :
        base(activator, new ViewAggregateExtractUICollection(aggregate) { TopX = null })
    {
        graphView = new GraphView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        ColorScheme = ConsoleMainWindow.ColorScheme;
        TabView.AddTab(graphTab = new Tab("Graph", graphView), false);
        this.aggregate = aggregate;
    }


    protected override void OnQueryCompleted(DataTable dt)
    {
        base.OnQueryCompleted(dt);

        TabView.SelectedTab = graphTab;

        string valueColumnName;

        try
        {
            valueColumnName = aggregate.GetQuerySyntaxHelper().GetRuntimeName(aggregate.CountSQL);
        }
        catch (Exception)
        {
            valueColumnName = "Count";
        }

        AggregateConfiguration.AdjustGraphDataTable(dt);

        PopulateGraphResults(dt, valueColumnName, aggregate.GetAxisIfAny());
    }

    private void PopulateGraphResults(DataTable dt, string countColumnName, AggregateContinuousDateAxis axis)
    {
        //       if (chart1.Legends.Count == 0)
        //         chart1.Legends.Add(new Legend());

        // clear any lingering settings
        graphView.Reset();

        // Work out how much screen real estate we have
        var boundsWidth = graphView.Bounds.Width;
        var boundsHeight = graphView.Bounds.Height;

        if (boundsWidth == 0) boundsWidth = TabView.Bounds.Width - 4;
        if (boundsHeight == 0) boundsHeight = TabView.Bounds.Height - 4;

        var titleWidth = aggregate.Name.Sum(c => Rune.ColumnWidth(c));
        var titleStartX = boundsWidth / 2 - titleWidth / 2;

        var title = new TextAnnotation
        {
            ScreenPosition = new Point(titleStartX, 0),
            Text = aggregate.Name,
            BeforeSeries = false
        };

        graphView.Annotations.Add(title);

        // if no time axis then we have a regular bar chart
        if (axis == null)
        {
            if (dt.Columns.Count == 2)
                SetupBarSeries(dt, countColumnName, boundsWidth, boundsHeight);
            else
                SetupMultiBarSeries(dt, countColumnName, boundsWidth, boundsHeight);
        }
        else
        {
            SetupLineGraph(dt, axis, countColumnName, boundsWidth, boundsHeight);
        }
    }

    private void SetupLineGraph(DataTable dt, AggregateContinuousDateAxis axis, string countColumnName, int boundsWidth,
        int boundsHeight)
    {
        graphView.AxisY.Text = countColumnName;
        graphView.GraphColor = Driver.MakeAttribute(Color.White, Color.Black);

        var xIncrement = 1f / (boundsWidth / (float)dt.Rows.Count);

        graphView.MarginBottom = 2;
        graphView.AxisX.Increment = xIncrement * 10;
        graphView.AxisX.ShowLabelsEvery = 1;
        graphView.AxisX.Text = axis.AxisIncrement.ToString();
        graphView.AxisX.LabelGetter = v =>
        {
            var x = (int)v.Value;
            return x < 0 || x >= dt.Rows.Count ? "" : dt.Rows[x][0].ToString();
        };

        float minY = 0;
        float maxY = 1;

        var colors = GetColors(dt.Columns.Count - 1);

        for (var i = 1; i < dt.Columns.Count; i++)
        {
            var series = new PathAnnotation { LineColor = colors[i - 1], BeforeSeries = true };
            var row = 0;

            foreach (DataRow dr in dt.Rows)
            {
                // Treat nulls as 0
                var yVal = dr[i] == DBNull.Value ? 0 : (float)Convert.ToDouble(dr[i]);

                minY = Math.Min(minY, yVal);
                maxY = Math.Max(maxY, yVal);

                series.Points.Add(new PointF(row++, yVal));
            }

            graphView.Annotations.Add(series);
        }

        var yIncrement = 1 / ((boundsHeight - graphView.MarginBottom) / (maxY - minY));

        graphView.CellSize = new PointF(xIncrement, yIncrement);

        graphView.AxisY.LabelGetter = v => FormatValue(v.Value, minY);
        graphView.MarginLeft = (uint)Math.Max(FormatValue(maxY, minY).Length, FormatValue(minY, minY).Length) + 1;

        var legend = GetLegend(dt, boundsWidth, boundsHeight);

        for (var i = 1; i < dt.Columns.Count; i++)
            legend.AddEntry(new GraphCellToRender('.', colors[i - 1]), dt.Columns[i].ColumnName);
    }

    /// <summary>
    /// Creates a new empty legend based on the column names of <paramref name="dt"/>
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="boundsWidth"></param>
    /// <param name="boundsHeight"></param>
    /// <returns></returns>
    private LegendAnnotation GetLegend(DataTable dt, int boundsWidth, int boundsHeight)
    {
        // Configure legend
        var seriesNames = dt.Columns.Cast<DataColumn>().Skip(1).Select(c => c.ColumnName).ToArray();

        var legendWidth = Math.Min(seriesNames.Max(s => s.Length) + 4, boundsWidth / 10);
        var legendHeight = Math.Min(seriesNames.Length + 2, (int)(boundsHeight * 0.9));

        var legend = new LegendAnnotation(new Rect(boundsWidth - legendWidth, 0, legendWidth, legendHeight));
        graphView.Annotations.Add(legend);

        return legend;
    }

    private void SetupBarSeries(DataTable dt, string countColumnName, int boundsWidth, int boundsHeight)
    {
        var barSeries = new BarSeries();

        var softStiple = new GraphCellToRender('\u2591');
        var mediumStiple = new GraphCellToRender('\u2592');

        var row = 0;
        var widestCategory = 0;

        float min = 0;
        float max = 1;

        foreach (DataRow dr in dt.Rows)
        {
            var label = dr[0].ToString();

            if (string.IsNullOrWhiteSpace(label)) label = "<Null>";

            // treat nulls as 0
            var val = dr[1] == DBNull.Value ? 0 : (float)Convert.ToDouble(dr[1]);

            min = Math.Min(min, val);
            max = Math.Max(max, val);

            widestCategory = Math.Max(widestCategory, label.Length);

            barSeries.Bars.Add(new BarSeries.Bar(label, row++ % 2 == 0 ? softStiple : mediumStiple, val));
        }

        // show bars alphabetically (graph is rendered y=0 at bottom)
        barSeries.Bars = barSeries.Bars.OrderByDescending(b => b.Text).ToList();


        // Configure Axis, Margins etc

        // make sure whole graph fits on axis
        var xIncrement = (max - min) / boundsWidth;

        // 1 bar per row of console
        graphView.CellSize = new PointF(xIncrement, 1);

        graphView.Series.Add(barSeries);
        graphView.AxisY.Increment = 0;
        barSeries.Orientation = Orientation.Horizontal;
        graphView.MarginBottom = 2;
        graphView.MarginLeft = (uint)widestCategory + 1;

        // work out how to space x axis without scrolling
        graphView.AxisX.Increment = 10 * xIncrement;
        graphView.AxisX.ShowLabelsEvery = 1;
        graphView.AxisX.LabelGetter = v => FormatValue(v.Value, min);
        graphView.AxisX.Text = countColumnName;

        graphView.AxisY.Increment = 1;
        graphView.AxisY.ShowLabelsEvery = 1;

        // scroll to the top of the bar chart so that the natural scroll direction (down) is preserved
        graphView.ScrollOffset = new PointF(0, barSeries.Bars.Count - boundsHeight + 4);
    }

    private static List<Attribute> GetColors(int numberNeeded)
    {
        var colors = new Attribute[15];

        colors[0] = Driver.MakeAttribute(Color.White, Color.Black);
        colors[1] = Driver.MakeAttribute(Color.Green, Color.Black);
        colors[2] = Driver.MakeAttribute(Color.Blue, Color.Black);
        colors[3] = Driver.MakeAttribute(Color.Cyan, Color.Black);
        colors[4] = Driver.MakeAttribute(Color.Red, Color.Black);
        colors[5] = Driver.MakeAttribute(Color.Magenta, Color.Black);
        colors[6] = Driver.MakeAttribute(Color.Brown, Color.Black);
        colors[7] = Driver.MakeAttribute(Color.Gray, Color.Black);
        colors[8] = Driver.MakeAttribute(Color.DarkGray, Color.Black);
        colors[9] = Driver.MakeAttribute(Color.BrightBlue, Color.Black);
        colors[10] = Driver.MakeAttribute(Color.BrightGreen, Color.Black);
        colors[11] = Driver.MakeAttribute(Color.BrightCyan, Color.Black);
        colors[12] = Driver.MakeAttribute(Color.BrightRed, Color.Black);
        colors[13] = Driver.MakeAttribute(Color.BrightMagenta, Color.Black);
        colors[14] = Driver.MakeAttribute(Color.BrightYellow, Color.Black);

        var toReturn = new List<Attribute>();

        for (var i = 0; i < numberNeeded; i++) toReturn.Add(colors[i % colors.Length]);

        return toReturn;
    }

    private void SetupMultiBarSeries(DataTable dt, string countColumnName, int boundsWidth, int boundsHeight)
    {
        var numberOfBars = dt.Columns.Count - 1;
        var colors = GetColors(numberOfBars).ToArray();
        const char mediumStiple = '\u2592';
        graphView.GraphColor = Driver.MakeAttribute(Color.White, Color.Black);

        // Configure legend
        var legend = GetLegend(dt, boundsWidth, boundsHeight);

        for (var i = 1; i < dt.Columns.Count; i++)
            legend.AddEntry(new GraphCellToRender(mediumStiple, colors[i - 1]), dt.Columns[i].ColumnName);

        // Configure multi bar series
        var barSeries = new MultiBarSeries(numberOfBars, numberOfBars + 1, 1, colors);

        float min = 0;
        float max = 1;

        foreach (DataRow dr in dt.Rows)
        {
            var label = dr[0].ToString();

            if (string.IsNullOrWhiteSpace(label)) label = "<Null>";
            var vals = dr.ItemArray.Skip(1)
                .Select(v => v == DBNull.Value ? 0 : (float)Convert.ToDouble(v)).ToArray();

            barSeries.AddBars(label, mediumStiple, vals);

            foreach (var val in vals)
            {
                min = Math.Min(min, val);
                max = Math.Max(max, val);
            }
        }

        // Configure Axis, Margins etc

        // make sure whole graph fits on axis
        var yIncrement = (max - min) / (boundsHeight - 2 /*MarginBottom*/);

        // 1 bar per row of console
        graphView.CellSize = new PointF(1, yIncrement);

        graphView.Series.Add(barSeries);
        graphView.MarginBottom = 2;
        graphView.MarginLeft = (uint)Math.Max(FormatValue(max, min).Length, FormatValue(min, min).Length) + 1;

        // work out how to space x axis without scrolling
        graphView.AxisY.Increment = yIncrement * 5;
        graphView.AxisY.ShowLabelsEvery = 1;
        graphView.AxisY.LabelGetter = v => FormatValue(v.Value, min);
        graphView.AxisY.Text = countColumnName;

        graphView.AxisX.Increment = numberOfBars + 1;
        graphView.AxisX.ShowLabelsEvery = 1;
        graphView.AxisX.Increment = 0;
        graphView.AxisX.Text = dt.Columns[0].ColumnName;
    }

    private static string FormatValue(float val, float min)
    {
        return val < min
            ? ""
            : val switch
            {
                > 1 => val.ToString("N0"),
                >= 0.01f => val.ToString("N2"),
                > 0.0001f => val.ToString("N4"),
                > 0.000001f => val.ToString("N6"),
                _ => val.ToString(CultureInfo.InvariantCulture)
            };
    }
}