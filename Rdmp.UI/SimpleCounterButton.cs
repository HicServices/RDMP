// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Rdmp.UI;

/// <summary>
/// ToolStripButton with a public Property Count on it which displays a number up to 99 (after which it displays 99+)
/// </summary>
[TechnicalUI]
[DesignerCategory("")]
public class SimpleCounterButton : ToolStripButton
{
    private int? _count;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int? Count
    {
        get => _count;
        set
        {
            _count = value;
            Invalidate();
        }
    }

    public float EmSize = 6f;
    public int LabelPadding = 2;

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        base.OnPaint(e);

        var labelFont = new Font(FontFamily.GenericMonospace, EmSize);

        if (_count != null)
        {
            var label = Count.ToString();

            if (Count >= 100)
                label = "99+";

            var labelSize = e.Graphics.MeasureString(label, labelFont);

            var labelXStart = (Width - labelSize.Width) / 2;

            var labelRect = new RectangleF(new PointF(labelXStart, Height - (labelSize.Height + LabelPadding)),
                labelSize);

            e.Graphics.FillRectangle(Brushes.White, labelRect);
            e.Graphics.DrawRectangle(Pens.Gray, Rectangle.Round(labelRect));
            e.Graphics.DrawString(label, labelFont, Brushes.Black, labelRect);
        }
    }
}