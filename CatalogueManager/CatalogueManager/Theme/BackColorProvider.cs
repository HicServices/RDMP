using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.Collections;

namespace CatalogueManager.Theme
{
    /// <summary>
    /// Determines which colour to use for the background on controls relating to this collection concept
    /// </summary>
    public class BackColorProvider
    {
        public const int IndiciatorBarSuggestedHeight = 4;

        public Color GetColor(RDMPCollection collection)
        {
            switch (collection)
            {
                case RDMPCollection.None:
                    return SystemColors.Control;
                case RDMPCollection.Tables:
                    return Color.FromArgb(255, 220, 255);
                case RDMPCollection.Catalogue:
                    return Color.FromArgb(255, 255, 220);
                case RDMPCollection.DataExport:
                    return Color.FromArgb(200,255,220);
                case RDMPCollection.SavedCohorts:
                    return Color.FromArgb(255, 220, 220);
                case RDMPCollection.Favourites:
                    return SystemColors.Control;
                case RDMPCollection.Cohort:
                    return Color.FromArgb(210, 240, 255);
                case RDMPCollection.DataLoad:
                    return Color.DarkGray;
                default:
                    throw new ArgumentOutOfRangeException("collection");
            }
        }

        public Bitmap DrawBottomBar(Bitmap image,RDMPCollection collection)
        {
            var newImage = new Bitmap(image.Width, image.Height);
            using (var g = Graphics.FromImage(newImage))
            {
                g.FillRectangle(new SolidBrush(GetColor(collection)), 0, 0, newImage.Width, newImage.Height);
                g.DrawImage(image,0,0);
            }

            return newImage;
        }


        public Image GetBackgroundImage(Size size, RDMPCollection collection)
        {
            var bmp = new Bitmap(size.Width, size.Height);

            using (var g = Graphics.FromImage(bmp))
                g.FillRectangle(new SolidBrush(GetColor(collection)), 5, size.Height - IndiciatorBarSuggestedHeight, size.Width - 10, IndiciatorBarSuggestedHeight);

            return bmp;
        }

    }
}
