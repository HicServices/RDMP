// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;

namespace Rdmp.UI;

/// <summary>
/// Formats X and Y axis of <see cref="Chart"/> objects in a consistent way (including axis increments etc).
/// </summary>
public class ChartLookAndFeelSetter
{
    /// <summary>
    /// Formats the X and Y axis of a <paramref name="chart"/> with sensible axis increments for the DataTable <paramref name="dt"/>. The
    /// table must have a first column called YearMonth in the format YYYY-MM
    /// </summary>
    /// <param name="chart"></param>
    /// <param name="dt"></param>
    /// <param name="yAxisTitle"></param>
    /// <param name="skip"></param>
    public static void PopulateYearMonthChart(Chart chart, DataTable dt, string yAxisTitle, int skip=3)
    {
        if (dt.Columns[0].ColumnName != "YearMonth")
            throw new ArgumentException(
                "Expected a graph with a first column YearMonth containing values expressed as YYYY-MM");

        chart.DataSource = dt;
        chart.Annotations.Clear();
        chart.Series.Clear();

        chart.ChartAreas[0].AxisX.MinorGrid.LineWidth = 1;
        chart.ChartAreas[0].AxisX.MinorGrid.Enabled = true;
        chart.ChartAreas[0].AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dot;

        chart.ChartAreas[0].AxisX.LineColor = Color.LightGray;
        chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
        chart.ChartAreas[0].AxisX.MinorGrid.LineColor = Color.LightGray;
        chart.ChartAreas[0].AxisX.TitleForeColor = Color.DarkGray;
        chart.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.DarkGray;


        chart.ChartAreas[0].AxisY.LineColor = Color.LightGray;
        chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
        chart.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.LightGray;
        chart.ChartAreas[0].AxisY.TitleForeColor = Color.DarkGray;
        chart.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.DarkGray;

        const int rowsPerYear = 12;
        var datasetLifespanInYears = dt.Rows.Count / rowsPerYear;

        if (dt.Rows.Count == 0)
            throw new Exception("There are no rows in the DQE database (dataset is empty?)");

        if (datasetLifespanInYears < 10)
        {
            chart.ChartAreas[0].AxisX.MinorGrid.Interval = 1;
            chart.ChartAreas[0].AxisX.MinorTickMark.Interval = 1;
            chart.ChartAreas[0].AxisX.MajorGrid.Interval = 12;
            chart.ChartAreas[0].AxisX.MajorTickMark.Interval = 4;
            chart.ChartAreas[0].AxisX.Interval = 4; //ever quarter

            //start at beginning of a quarter (%4)
            var monthYearStart = dt.Rows[0]["YearMonth"].ToString();
            var offset = GetOffset(monthYearStart);
            chart.ChartAreas[0].AxisX.IntervalOffset = offset % 4;
        }
        else if (datasetLifespanInYears < 100)
        {
            chart.ChartAreas[0].AxisX.MinorGrid.Interval = 4;
            chart.ChartAreas[0].AxisX.MinorTickMark.Interval = 4;
            chart.ChartAreas[0].AxisX.MajorGrid.Interval = 12;
            chart.ChartAreas[0].AxisX.MajorTickMark.Interval = 12;
            chart.ChartAreas[0].AxisX.Interval = 12; //every year

            //start at january
            var monthYearStart = dt.Rows[0]["YearMonth"].ToString();
            var offset = GetOffset(monthYearStart);
            chart.ChartAreas[0].AxisX.IntervalOffset = offset;
        }

        chart.ChartAreas[0].AxisX.IsMarginVisible = false;
        chart.ChartAreas[0].AxisX.Title = "Dataset Record Time (periodicity)";
        chart.ChartAreas[0].AxisY.Title = yAxisTitle;


        var cols = dt.Columns;

        foreach (var col in cols.Cast<DataColumn>().Skip(skip))
        {
            var s = new Series();
            chart.Series.Add(s);
            s.ChartType = SeriesChartType.StackedArea;
            s.XValueMember = "YearMonth";
            s.YValueMembers = col.ToString();
            s.Name = col.ToString();
        }

        // Set gradient background of the chart area
        chart.ChartAreas[0].BackColor = Color.White;
        chart.ChartAreas[0].BackSecondaryColor = Color.FromArgb(255, 230, 230, 230);
        chart.ChartAreas[0].BackGradientStyle = GradientStyle.DiagonalRight;
    }

    /// <summary>
    /// calculates how far offset the month is
    /// </summary>
    /// <param name="yearMonth"></param>
    /// <returns></returns>
    private static int GetOffset(string yearMonth)
    {
        var matchYearDigit = Regex.Match(yearMonth, @"^\d+$");
        if(matchYearDigit.Success)
        {
            return 0;
        }

        var matchMonthName = Regex.Match(yearMonth, @"\d+-([A-Za-z]+)");

        if (matchMonthName.Success)
        {
            var monthName = matchMonthName.Groups[1].Value;
            return DateTime.ParseExact(monthName, "MMMM", CultureInfo.CurrentCulture).Month;
        }

        var matchMonthDigit = Regex.Match(yearMonth, @"\d+-(\d+)(-(\d\d))?");

        return !matchMonthDigit.Success
            ? throw new Exception("Regex did not match expected YYYY-MM!")
            : int.Parse(matchMonthDigit.Groups[1].Value);
    }
}