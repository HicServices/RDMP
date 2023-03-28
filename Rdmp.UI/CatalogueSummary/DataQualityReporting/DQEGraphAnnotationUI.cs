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
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DQEGraphAnnotationUI) obj);
    }
    protected bool Equals(DQEGraphAnnotationUI other)
    {
        return Equals(_underlyingAnnotationObject, other._underlyingAnnotationObject);
    }

    public override int GetHashCode()
    {
        return _underlyingAnnotationObject.ID.GetHashCode();
    }

    public DQEGraphAnnotationUI(DQEGraphAnnotation a, Chart chart)
    {
        _underlyingAnnotationObject = a;
        Annotation = new LineAnnotation();
        Annotation.IsSizeAlwaysRelative = false;
        Annotation.AxisX = chart.ChartAreas[0].AxisX;
        Annotation.AxisY = chart.ChartAreas[0].AxisY;
        Annotation.AnchorX = a.EndX;
        Annotation.AnchorY = a.EndY;
        Annotation.Height = a.StartY - a.EndY;
        Annotation.Width = a.StartX - a.EndX;
        Annotation.LineWidth = 2;
        Annotation.StartCap = LineAnchorCapStyle.Arrow;
        Annotation.EndCap = LineAnchorCapStyle.None;
        Annotation.AllowSelecting = true;
        Annotation.Tag = this;

        TextAnnotation = new TextAnnotation();
        TextAnnotation.Text = a.Text;
        TextAnnotation.IsSizeAlwaysRelative = false;
        TextAnnotation.AxisX = chart.ChartAreas[0].AxisX;
        TextAnnotation.AxisY = chart.ChartAreas[0].AxisY;
        TextAnnotation.AnchorX = a.StartX;
        TextAnnotation.AnchorY = a.StartY;
        TextAnnotation.AllowSelecting = true;
        TextAnnotation.Tag = this;
            
    }

    public void Delete(Chart chart)
    {
        _underlyingAnnotationObject.DeleteInDatabase();
        chart.Annotations.Remove(Annotation);
        chart.Annotations.Remove(TextAnnotation);
    }


}