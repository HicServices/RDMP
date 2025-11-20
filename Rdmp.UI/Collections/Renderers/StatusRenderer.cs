using BrightIdeasSoftware;
using DnsClient;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
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

        private IActivateItems _activator;

        public StatusRenderer(IActivateItems activator) : base()
        {
            _activator = activator;
        }

        private readonly StringFormat fmt = new StringFormat(StringFormatFlags.NoWrap)
        {
            Trimming = StringTrimming.EllipsisCharacter,
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Near,
        };

        public override bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, object rowObject)
        {
            if (rowObject is Catalogue c)
            {
                bool isInternal = c.IsInternalDataset;
                bool isProjectSpecific = c.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository);
                bool isDeprecated = c.IsDeprecated;
                DrawBackground(g, cellBounds);
                int xOffset = 0;
                if (isInternal)
                {
                    xOffset += RenderStatus("Internal", StatusColours.Internal, StatusColours.InternalCompliment, cellBounds, xOffset, g);
                    xOffset += 5;
                }
                if (isProjectSpecific)
                {
                    xOffset += RenderStatus("Project Specific", StatusColours.ProjectSpecific, StatusColours.ProjectSpecificCompliment, cellBounds, xOffset, g);
                    xOffset += 5;
                }
                if (isDeprecated)
                {
                    xOffset += RenderStatus("Deprecated", StatusColours.Deprecated, StatusColours.DeprecatedCompliment, cellBounds, xOffset, g);
                    xOffset += 5;
                }
                return true;
            }
            if (rowObject is CatalogueItem ci)
            {
                DrawBackground(g, cellBounds);
                int xOffset = 0;

                if (ci.ExtractionInformation.IsExtractionIdentifier)
                {
                    xOffset += RenderStatus("Extraction Identifier", StatusColours.ExtractionIdentifier, StatusColours.ExtractionIdentifierCompliment, cellBounds, xOffset, g);
                    xOffset += 5;
                }
                if (ci.ExtractionInformation.IsPrimaryKey)
                {
                    xOffset += RenderStatus("Primary Key", StatusColours.PrimaryKey, StatusColours.PrimaryKeyCompliment, cellBounds, xOffset, g);
                    xOffset += 5;
                }
                if (ci.ExtractionInformation.HashOnDataRelease)
                {
                    xOffset += RenderStatus("Hash on Release", StatusColours.HashOnRelease, StatusColours.HashOnReleaseCompliment, cellBounds, xOffset, g);
                    xOffset += 5;
                }
                return true;
            }
            return false;
        }

        private static void DrawBackground(Graphics g, Rectangle r)
        {
            Color backgroundColor = Color.White;

            using (Brush brush = new SolidBrush(backgroundColor))
            {
                g.FillRectangle(brush, r.X - 1, r.Y - 1, r.Width + 2, r.Height + 2);
            }
        }


        private int RenderStatus(string text, Color colour, Color textColour, Rectangle cellBounds, int offset, Graphics g)
        {
            using Font font = new Font("Roboto", 8);
            RectangleF textBoxRect = cellBounds;
            textBoxRect.X += offset;
            textBoxRect.Width = 50;

            SizeF size = g.MeasureString(text, font, cellBounds.Width, fmt);
            textBoxRect.Height = 16;
            textBoxRect.Width = size.Width + 4;
            textBoxRect.Y += (cellBounds.Height - size.Height) / 2;
            var textArc = GetRoundedRect(textBoxRect, textBoxRect.Height / 2);
            textArc.CloseFigure();

            g.DrawPath(new Pen(colour), textArc);
            g.FillPath(new SolidBrush(colour), textArc);
            g.Clip = new Region(cellBounds);
            textBoxRect.Y += 2;
            textBoxRect.Height -= 4;
            g.DrawString(text, font, new SolidBrush(textColour), textBoxRect, fmt);

            return (int)Math.Ceiling(textBoxRect.Width);
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
