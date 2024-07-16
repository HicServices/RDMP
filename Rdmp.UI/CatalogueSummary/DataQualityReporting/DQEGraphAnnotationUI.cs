// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms.DataVisualization.Charting;
using Rdmp.Core.DataQualityEngine.Data;

namespace Rdmp.UI.CatalogueSummary.DataQualityReporting;

internal class DQEGraphAnnotationUI
{
    private readonly DQEGraphAnnotation _underlyingAnnotationObject;
    public LineAnnotation Annotation { get; set; }
    public TextAnnotation TextAnnotation { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DQEGraphAnnotationUI)obj);
    }

    protected bool Equals(DQEGraphAnnotationUI other) =>
        Equals(_underlyingAnnotationObject, other._underlyingAnnotationObject);

    public override int GetHashCode() => _underlyingAnnotationObject.ID.GetHashCode();

    public DQEGraphAnnotationUI(DQEGraphAnnotation a, Chart chart)
    {
        _underlyingAnnotationObject = a;
        Annotation = new LineAnnotation
        {
            IsSizeAlwaysRelative = false,
            AxisX = chart.ChartAreas[0].AxisX,
            AxisY = chart.ChartAreas[0].AxisY,
            AnchorX = a.EndX,
            AnchorY = a.EndY,
            Height = a.StartY - a.EndY,
            Width = a.StartX - a.EndX,
            LineWidth = 2,
            StartCap = LineAnchorCapStyle.Arrow,
            EndCap = LineAnchorCapStyle.None,
            AllowSelecting = true,
            Tag = this
        };

        TextAnnotation = new TextAnnotation
        {
            Text = a.Text,
            IsSizeAlwaysRelative = false,
            AxisX = chart.ChartAreas[0].AxisX,
            AxisY = chart.ChartAreas[0].AxisY,
            AnchorX = a.StartX,
            AnchorY = a.StartY,
            AllowSelecting = true,
            Tag = this
        };
    }

    public void Delete(Chart chart)
    {
        _underlyingAnnotationObject.DeleteInDatabase();
        chart.Annotations.Remove(Annotation);
        chart.Annotations.Remove(TextAnnotation);
    }

    public void UpdatePosition(double x, double y)
    {

    }
}