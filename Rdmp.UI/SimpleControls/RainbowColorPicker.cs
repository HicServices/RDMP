// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Rdmp.UI.SimpleControls;

/// <summary>
/// Generates colours on the visual spectrum between blue and red using interoplation.
/// </summary>
public class RainbowColorPicker
{
    public List<Color> Colors { get; private set; }

    public RainbowColorPicker(int numberOfColors)
    {
        Colors = new List<Color>();

        var baseColors = new List<Color>
        {
            Color.RoyalBlue,
            Color.LightSkyBlue,
            Color.LightGreen,
            Color.Yellow,
            Color.Orange,
            Color.Red
        }; // create a color list
        Colors = interpolateColors(baseColors, numberOfColors);
    }

    public RainbowColorPicker(Color color1, Color color2, int numberOfColors)
    {
        Colors = new List<Color>();

        var baseColors = new List<Color>
        {
            color1,
            color2
        }; // create a color list
        Colors = interpolateColors(baseColors, numberOfColors);
    }

    private List<Color> interpolateColors(List<Color> stopColors, int count)
    {
        var gradient = new SortedDictionary<float, Color>();
        for (var i = 0; i < stopColors.Count; i++)
            gradient.Add(1f * i / (stopColors.Count - 1), stopColors[i]);
        var ColorList = new List<Color>();

        using var bmp = new Bitmap(count, 1);
        using var G = Graphics.FromImage(bmp);
        var bmpCRect = new Rectangle(Point.Empty, bmp.Size);
        using (var br = new LinearGradientBrush
            (bmpCRect, Color.Empty, Color.Empty, 0, false))
        {
            var cb = new ColorBlend
            {
                Positions = new float[gradient.Count]
            };
            for (var i = 0; i < gradient.Count; i++)
                cb.Positions[i] = gradient.ElementAt(i).Key;
            cb.Colors = gradient.Values.ToArray();
            br.InterpolationColors = cb;
            G.FillRectangle(br, bmpCRect);
            for (var i = 0; i < count; i++) ColorList.Add(bmp.GetPixel(i, 0));
        }
        return ColorList;
    }
}