// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using Rdmp.Core;

namespace Rdmp.UI.Theme;

/// <summary>
/// Determines which colour to use for the background on controls relating to this collection concept
/// </summary>
public static class BackColorProvider
{
    public const int IndicatorBarSuggestedHeight = 4;

    public static Color GetColor(RDMPCollection collection)
    {
        return collection switch
        {
            RDMPCollection.None => SystemColors.Control,
            RDMPCollection.Tables => Color.FromArgb(255, 220, 255),
            RDMPCollection.Catalogue => Color.FromArgb(255, 255, 220),
            RDMPCollection.DataExport => Color.FromArgb(200, 255, 220),
            RDMPCollection.SavedCohorts => Color.FromArgb(255, 220, 220),
            RDMPCollection.Favourites => SystemColors.Control,
            RDMPCollection.Cohort => Color.FromArgb(210, 240, 255),
            RDMPCollection.DataLoad => Color.DarkGray,
            RDMPCollection.Datasets => Color.PaleVioletRed,
            RDMPCollection.Configurations => Color.DarkSeaGreen,
            _ => throw new ArgumentOutOfRangeException(nameof(collection))
        };
    }

    public static Image GetBackgroundImage(Size size, RDMPCollection collection)
    {
        var bmp = new Bitmap(size.Width, size.Height);

        using var g = Graphics.FromImage(bmp);
        g.FillRectangle(new SolidBrush(GetColor(collection)), 2, size.Height - IndicatorBarSuggestedHeight,
            size.Width - 4, IndicatorBarSuggestedHeight);

        return bmp;
    }
}