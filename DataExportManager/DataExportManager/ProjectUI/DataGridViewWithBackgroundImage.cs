using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataExportManager.ProjectUI
{
    public class DataGridViewWithBackgroundImage : DataGridView
    {
        public Color BackgroundTextColor { get; set; }
        public Font BackgroundTextFont { get; set; }
        public string BackgroundText { get; set; }

        public override Image BackgroundImage { get; set; }

        protected override void PaintBackground(Graphics graphics, Rectangle clipBounds, Rectangle gridBounds)
        {
            base.PaintBackground(graphics, clipBounds, gridBounds);

            if (((this.BackgroundImage != null)))
            {
                graphics.FillRectangle(Brushes.Black, gridBounds);
                graphics.DrawImage(this.BackgroundImage, gridBounds);
            }

            if (!string.IsNullOrWhiteSpace(BackgroundText))
            {
                int height = gridBounds.Height;
                int width = gridBounds.Width;

                var textSize = graphics.MeasureString(BackgroundText, BackgroundTextFont);

                int topLeftPointX = (int) ((width/2) - (textSize.Width/2));
                int topLeftPointY = (int) ((height/2) - (textSize.Height/2));

                graphics.DrawString(BackgroundText, BackgroundTextFont,new SolidBrush(BackgroundTextColor),topLeftPointX,topLeftPointY );
            }
        }
    }
}
