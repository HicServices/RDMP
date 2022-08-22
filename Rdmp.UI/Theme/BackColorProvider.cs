// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Rdmp.UI.Theme
{
    /// <summary>
    /// Determines which colour to use for the background on controls relating to this collection concept
    /// </summary>
    public class BackColorProvider
    {
        public const int IndicatorBarSuggestedHeight = 4;

        public Color GetColor(RDMPCollection collection)
        {
            return collection switch
            {
                RDMPCollection.None => Control,
                RDMPCollection.Tables => ColorFromArgb(255, 220, 255),
                RDMPCollection.Catalogue => ColorFromArgb(255, 255, 220),
                RDMPCollection.DataExport => ColorFromArgb(200, 255, 220),
                RDMPCollection.SavedCohorts => ColorFromArgb(255, 220, 220),
                RDMPCollection.Favourites => Control,
                RDMPCollection.Cohort => ColorFromArgb(210, 240, 255),
                RDMPCollection.DataLoad => Color.DarkGray,
                _ => throw new ArgumentOutOfRangeException(nameof(collection))
            };
        }

        private static readonly Color Control =
            Color.FromRgb(System.Drawing.SystemColors.Control.R,
                System.Drawing.SystemColors.Control.G,
                System.Drawing.SystemColors.Control.B);

        public static System.Drawing.Color LegacyColor(Color c)
        {
            Rgba32 rgb=new();
            c.ToPixel<Rgba32>().ToRgba32(ref rgb);
            return System.Drawing.Color.FromArgb(rgb.R, rgb.G, rgb.B);
        }

        private Color ColorFromArgb(byte r, byte g, byte b)
        {
            return Color.FromRgb(r,g,b);
        }

        public Image<Rgba32> DrawBottomBar(Image<Rgba32> image,RDMPCollection collection)
        {
            return image.Clone(x => x.Fill(GetColor(collection),
                new SixLabors.ImageSharp.Rectangle(0, 0, image.Width, image.Height)));
        }


        public Image<Rgba32> GetBackgroundImage(int width,int height, RDMPCollection collection)
        {
            var i=new Image<Rgba32>(width,height);
            i.Mutate(x=>x.Fill(GetColor(collection),new Rectangle(2,height-IndicatorBarSuggestedHeight,width-4,IndicatorBarSuggestedHeight)));
            return i;
        }

    }
}
