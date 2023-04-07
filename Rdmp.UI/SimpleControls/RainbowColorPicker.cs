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

        List<Color> baseColors = new List<Color>();  // create a color list
        baseColors.Add(Color.RoyalBlue);
        baseColors.Add(Color.LightSkyBlue);
        baseColors.Add(Color.LightGreen);
        baseColors.Add(Color.Yellow);
        baseColors.Add(Color.Orange);
        baseColors.Add(Color.Red);
        Colors = interpolateColors(baseColors, numberOfColors);
    }

    public RainbowColorPicker(Color color1, Color color2, int numberOfColors)
    {
        Colors = new List<Color>();

        List<Color> baseColors = new List<Color>();  // create a color list
        baseColors.Add(color1);
        baseColors.Add(color2);
        Colors = interpolateColors(baseColors, numberOfColors);
    }

    List<Color> interpolateColors(List<Color> stopColors, int count)
    {
        SortedDictionary<float, Color> gradient = new SortedDictionary<float, Color>();
        for (int i = 0; i < stopColors.Count; i++)
            gradient.Add(1f * i / (stopColors.Count - 1), stopColors[i]);
        List<Color> ColorList = new List<Color>();

        using (Bitmap bmp = new Bitmap(count, 1))
        using (Graphics G = Graphics.FromImage(bmp))
        {
            Rectangle bmpCRect = new Rectangle(Point.Empty, bmp.Size);
            LinearGradientBrush br = new LinearGradientBrush
                (bmpCRect, Color.Empty, Color.Empty, 0, false);
            ColorBlend cb = new ColorBlend();
            cb.Positions = new float[gradient.Count];
            for (int i = 0; i < gradient.Count; i++)
                cb.Positions[i] = gradient.ElementAt(i).Key;
            cb.Colors = gradient.Values.ToArray();
            br.InterpolationColors = cb;
            G.FillRectangle(br, bmpCRect);
            for (int i = 0; i < count; i++) ColorList.Add(bmp.GetPixel(i, 0));
            br.Dispose();
        }
        return ColorList;
    }
}