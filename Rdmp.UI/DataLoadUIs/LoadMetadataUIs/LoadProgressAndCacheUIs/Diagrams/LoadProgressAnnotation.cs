// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Rdmp.Core.Curation.Data;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs.Diagrams;

/// <summary>
/// Line on a <see cref="Chart"/> indicating how much progress has been made towards various <see cref="LoadProgress"/> / <see cref="CacheProgressUI"/>
/// goals.
/// </summary>
internal class LoadProgressAnnotation
{
    private readonly LoadProgress _lp;
    private readonly DataTable _dt;

    public LineAnnotation LineAnnotationOrigin { get; private set; }
    public TextAnnotation TextAnnotationOrigin { get; private set; }

    public LineAnnotation LineAnnotationFillProgress { get; private set; }
    public TextAnnotation TextAnnotationFillProgress { get; private set; }

    public LineAnnotation LineAnnotationCacheProgress { get; private set; }
    public TextAnnotation TextAnnotationCacheProgress { get; private set; }

    public LoadProgressAnnotation(LoadProgress lp, DataTable dt, Chart chart)
    {
        _lp = lp;
        _dt = dt;

        GetAnnotations("OriginDate", 0.9, lp.OriginDate, chart, out var line, out var text);
        LineAnnotationOrigin = line;
        TextAnnotationOrigin = text;


        GetAnnotations("Progress", 0.7, lp.DataLoadProgress, chart, out var line2, out var text2);
        LineAnnotationFillProgress = line2;
        TextAnnotationFillProgress = text2;

        var cp = lp.CacheProgress;

        if (cp != null)
        {
            GetAnnotations("Cache Fill", 0.50, cp.CacheFillProgress, chart, out var line3, out var text3);
            LineAnnotationCacheProgress = line3;
            TextAnnotationCacheProgress = text3;
        }
    }

    private void GetAnnotations(string label, double fractionalHeightOfLabel, DateTime? date, Chart chart,
        out LineAnnotation line, out TextAnnotation text)
    {
        string originText;
        double anchorX;
        var maxY = GetMaxY(_dt);

        //display the text labels half way along the chart
        var textAnchorY = maxY * fractionalHeightOfLabel;

        if (date == null)
        {
            originText = "unknown";
            anchorX = 1;
        }
        else
        {
            originText = date.Value.ToString("yyyy-MM-dd");
            anchorX = GetXForDate(_dt, date.Value) + 1;
        }

        line = new LineAnnotation
        {
            IsSizeAlwaysRelative = false,
            AxisX = chart.ChartAreas[0].AxisX,
            AxisY = chart.ChartAreas[0].AxisY,
            AnchorX = anchorX,
            AnchorY = 0,
            Height = maxY * 2,
            LineWidth = 1,
            LineDashStyle = ChartDashStyle.Dot,
            Width = 0
        };
        line.LineWidth = 2;
        line.StartCap = LineAnchorCapStyle.None;
        line.EndCap = LineAnchorCapStyle.None;
        line.AllowSelecting = true;
        line.AllowMoving = true;

        text = new TextAnnotation
        {
            Text = $"{label}:{Environment.NewLine}{originText}",
            IsSizeAlwaysRelative = false,
            AxisX = chart.ChartAreas[0].AxisX,
            AxisY = chart.ChartAreas[0].AxisY,
            AnchorX = anchorX,
            AnchorY = textAnchorY,

            ForeColor = date == null ? Color.Red : Color.Green
        };
        text.Font = new Font(text.Font, FontStyle.Bold);
    }

    private static int GetXForDate(DataTable dt, DateTime value)
    {
        var year = value.Year;
        var month = value.Month;

        for (var i = 0; i < dt.Rows.Count; i++)
        {
            var currentYear = Convert.ToInt32(dt.Rows[i]["Year"]);
            var currentMonth = Convert.ToInt32(dt.Rows[i]["Month"]);

            //we have overstepped
            if (year < currentYear)
                return i;

            //if we are on the month or have overstepped it (axis can have gaps)
            if (year == currentYear && month <= currentMonth)
                return i;

            //if there is no more data, return the last row
            if (i + 1 == dt.Rows.Count)
                return i;
        }

        throw new Exception("Should have returned the last row or the first row by now");
    }

    private static double GetMaxY(DataTable dt)
    {
        var max = 0;
        var colCount = dt.Columns.Count;
        foreach (DataRow r in dt.Rows)
        {
            var totalForRow = 0;
            for (var i = 3; i < colCount; i++)
                totalForRow += Convert.ToInt32(r[i]);

            max = Math.Max(max, totalForRow);
        }

        return max;
    }

    public void OnAnnotationPositionChanged(object sender, EventArgs eventArgs)
    {
        if (sender == LineAnnotationOrigin)
        {
            var newDate = GetDateFromX((int)LineAnnotationOrigin.X);

            if (MessageBox.Show($"Set new LoadProgress Origin date to {newDate}?", "Change Origin Date?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _lp.OriginDate = newDate;
                _lp.SaveToDatabase();
            }
        }

        if (sender == LineAnnotationFillProgress)
        {
            var newDate = GetDateFromX((int)LineAnnotationFillProgress.X);

            if (MessageBox.Show($"Set new LoadProgress Fill date to {newDate}?", "Change Fill Date?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _lp.DataLoadProgress = newDate;
                _lp.SaveToDatabase();
            }
        }

        if (sender == LineAnnotationCacheProgress)
        {
            var cp = _lp.CacheProgress;

            if (cp == null)
                return;

            var newDate = GetDateFromX((int)LineAnnotationCacheProgress.X);

            if (MessageBox.Show($"Set new CacheProgress date to {newDate}?", "Change Cache Progress Date?",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                cp.CacheFillProgress = newDate;
                cp.SaveToDatabase();
            }
        }
    }

    private DateTime? GetDateFromX(int x)
    {
        x = Math.Max(0,
            x - 1); //subtract 1 because X axis on chart starts at 1 but data table starts at 0, also prevents them dragging it super negative
        x = Math.Min(x, _dt.Rows.Count - 1);

        var year = Convert.ToInt32(_dt.Rows[x]["Year"]);
        var month = Convert.ToInt32(_dt.Rows[x]["Month"]);

        return new DateTime(year, month, 1);
    }
}