using BrightIdeasSoftware;
using DnsClient;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ScintillaNET.Style;

namespace Rdmp.UI.Collections.Renderers
{
    public class StatusRenderer : AbstractRenderer
    {



        public override bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, object rowObject)
        {
            if (rowObject is Catalogue c)
            {
                bool isInternal = true;
                bool isProjectSpecific = true;
                bool isDeprecated = true;
                //const int rounding = 20;
                GraphicsPath path = new GraphicsPath();

                RectangleF arc = new RectangleF(cellBounds.X, cellBounds.Y, 20000, 16);
                path.AddRectangle(arc);
                path.CloseFigure();
                g.FillPath(Brushes.White, path);
                g.DrawPath(new Pen(Color.White), path);
                g.Clip = new Region(cellBounds);
                StringFormat fmt = new StringFormat(StringFormatFlags.NoWrap);
                fmt.Trimming = StringTrimming.EllipsisCharacter;
                fmt.Alignment = StringAlignment.Center;
                fmt.LineAlignment = StringAlignment.Near;
                int xOffset = 0;
                if (isInternal)
                {
                    xOffset += RenderStatus("Internal", Color.Red,Color.Black, cellBounds, xOffset, g);
                    xOffset += 5;
                }
                if (isProjectSpecific)
                {
                    xOffset += RenderStatus("Project Specific", Color.Blue,Color.White, cellBounds, xOffset, g);
                    xOffset += 5;
                }
                if (isDeprecated)
                {
                    xOffset += RenderStatus("Deprecated", Color.Gray,Color.Black, cellBounds, xOffset, g);
                    xOffset += 5;
                }
                return true;
            }
            return false;
        }

        private int RenderStatus(string text, Color colour,Color textColour, Rectangle cellBounds, int offset, Graphics g)
        {
            using (Font font = new Font("Tahoma", 8))
            {
                RectangleF textBoxRect = cellBounds;
                textBoxRect.X += offset;
                textBoxRect.Width = 50;
                StringFormat fmt = new StringFormat(StringFormatFlags.NoWrap);
                fmt.Trimming = StringTrimming.EllipsisCharacter;
                fmt.Alignment = StringAlignment.Center;
                fmt.LineAlignment = StringAlignment.Near;
                SizeF size = g.MeasureString(text, font, cellBounds.Width, fmt);
                textBoxRect.Height = size.Height;
                textBoxRect.Width = size.Width+2;
                fmt.Alignment = StringAlignment.Near;
                var textArc = GetRoundedRect(textBoxRect, 5);
                textArc.AddRectangle(textBoxRect);
                textArc.CloseFigure();
                g.DrawPath(new Pen(colour), textArc);
                g.FillPath(new SolidBrush(colour), textArc);
                g.Clip = new Region(cellBounds);
                g.DrawRectangle(new Pen(Color.Transparent), textBoxRect);
                g.FillRectangle(new SolidBrush(colour), textBoxRect);
                g.DrawString(text, font, new SolidBrush(textColour), textBoxRect, fmt);
                return (int)Math.Ceiling(textBoxRect.Width);
            }
        }

        public override bool RenderItem(DrawListViewItemEventArgs e, Graphics g, Rectangle itemBounds, object rowObject)
        {
            if (rowObject is Catalogue c)
            {
                using (LinearGradientBrush gradient = new LinearGradientBrush(itemBounds, Color.Black, Color.Fuchsia, 0.0))
                {
                    g.FillRectangle(gradient, itemBounds);
                }
                //StringFormat fmt = new StringFormat(StringFormatFlags.NoWrap);
                //fmt.LineAlignment = StringAlignment.Center;
                //fmt.Trimming = StringTrimming.EllipsisCharacter;
                //Rectangle rectangle1 = new Rectangle(0, 0, 50, 16);
                //g.DrawRectangle(Pens.Pink, rectangle1);
                //g.DrawString("Internal", e.Item.Font, new SolidBrush(Color.Black), rectangle1, fmt);
                //Rectangle rectangle2 = new Rectangle(55, 0, 50, 16);
                //g.DrawRectangle(Pens.Orange, rectangle2);
                //g.DrawString("Deprecated", this.Font, this.TextBrush, rectangle2, fmt);
                return true;
            }
            return false;
        }
        private GraphicsPath GetRoundedRect(RectangleF rect, float diameter)
        {
            GraphicsPath path = new GraphicsPath();

            RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
