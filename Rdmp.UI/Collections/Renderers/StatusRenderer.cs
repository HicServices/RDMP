using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision.IconProviders;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.ItemActivation;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Windows.Forms;
using static ScintillaNET.Style;

namespace Rdmp.UI.Collections.Renderers
{
    /// <summary>
    /// Renders status items for objects
    /// </summary>
    public class StatusRenderer : BaseRenderer
    {

        public override void Render(Graphics g, Rectangle r)
        {
            DrawBackground(g, r);
            if (this.RowObject is Catalogue c)
            {
                bool isInternal = c.IsInternalDataset;
                bool isProjectSpecific = c.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository);
                bool isDeprecated = c.IsDeprecated;
                DrawBackground(g, r);
                int xOffset = 0;
                if (isInternal)
                {
                    xOffset += RenderChip("Internal", StatusColours.Internal, StatusColours.InternalCompliment, r, xOffset, g);
                    xOffset += 5;
                }
                if (isProjectSpecific)
                {
                    xOffset += RenderChip("Project Specific", StatusColours.ProjectSpecific, StatusColours.ProjectSpecificCompliment, r, xOffset, g);
                    xOffset += 5;
                }
                if (isDeprecated)
                {
                    xOffset += RenderChip("Deprecated", StatusColours.Deprecated, StatusColours.DeprecatedCompliment, r, xOffset, g);
                    xOffset += 5;
                }
            }
            if (this.RowObject is CatalogueItem ci)
            {
                DrawBackground(g, r);
                int xOffset = 0;

                if (ci.ExtractionInformation.IsExtractionIdentifier)
                {
                    xOffset += RenderChip("Extraction Identifier", StatusColours.ExtractionIdentifier, StatusColours.ExtractionIdentifierCompliment, r, xOffset, g);
                    xOffset += 5;
                }
                if (ci.ExtractionInformation.IsPrimaryKey)
                {
                    xOffset += RenderChip("Primary Key", StatusColours.PrimaryKey, StatusColours.PrimaryKeyCompliment, r, xOffset, g);
                    xOffset += 5;
                }
                if (ci.ExtractionInformation.HashOnDataRelease)
                {
                    xOffset += RenderChip("Hash on Release", StatusColours.HashOnRelease, StatusColours.HashOnReleaseCompliment, r, xOffset, g);
                    xOffset += 5;
                }
                if (ci.ExtractionInformation.ExtractionCategory == ExtractionCategory.Supplemental)
                {
                    xOffset += RenderChip("Supplemental", StatusColours.Supplemental, StatusColours.SupplementalCompliment, r, xOffset, g);
                    xOffset += 5;
                }
                if (ci.ExtractionInformation.ExtractionCategory == ExtractionCategory.SpecialApprovalRequired)
                {
                    xOffset += RenderChip("Special Approval", StatusColours.SpecialistApproval, StatusColours.SpecialistApprovalCompliment, r, xOffset, g);
                    xOffset += 5;
                }
                if (ci.ExtractionInformation.ExtractionCategory == ExtractionCategory.Internal)
                {
                    xOffset += RenderChip("Internal", StatusColours.Internal, StatusColours.InternalCompliment, r, xOffset, g);
                    xOffset += 5;
                }
                if (ci.ExtractionInformation.ExtractionCategory == ExtractionCategory.Deprecated)
                {
                    xOffset += RenderChip("Deprecated", StatusColours.Deprecated, StatusColours.DeprecatedCompliment, r, xOffset, g);
                    xOffset += 5;
                }
            }
            if (this.RowObject is CohortIdentificationConfiguration cic)
            {
                DrawBackground(g, r);
                int xOffset = 0;
                if (cic.IsTemplate)
                {
                    xOffset += RenderChip("Template", StatusColours.Template, StatusColours.TemplateCompliment, r, xOffset, g);
                    xOffset += 5;
                }
                if (cic.Frozen)
                {
                    xOffset += RenderChip("Frozen", StatusColours.Frozen, StatusColours.FrozenCompliment, r, xOffset, g);
                    xOffset += 5;
                }
                if (cic.IsAssociatedToAProject(_activator.CoreChildProvider as DataExportChildProvider))
                {
                    xOffset += RenderChip("Project Specific", StatusColours.ProjectSpecific, StatusColours.ProjectSpecificCompliment, r, xOffset, g);
                    xOffset += 5;
                }
            }
        }
        private IActivateItems _activator;

        private bool _useShortStrings = UserSettings.UseShortStatusChips;

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

        private int RenderChip(string text, Color backgroundColour, Color textColor, Rectangle r, int xOffset, Graphics g)
        {
            string _text = text;
            if (_useShortStrings)
            {
                _text = String.Join("", _text.Split(' ').Select(s => s[0]));
            }
            r = ApplyCellPadding(r);
            SizeF size = g.MeasureString(_text, this.Font, r.Width, fmt);
            Rectangle inner = new Rectangle(xOffset + r.X, r.Y+2, (int)Math.Ceiling(size.Width),16);
            //inner = AlignRectangle(r, inner);
            if (!(base.Aspect is IConvertible convertible))
            {
                return xOffset;
            }

            double num = convertible.ToDouble(NumberFormatInfo.InvariantInfo);
            Rectangle rect = Rectangle.Inflate(inner, -1, -1);

            g.FillRectangle(new SolidBrush(backgroundColour), inner);
            g.DrawString(_text, this.Font, new SolidBrush(textColor), inner, fmt);
            g.DrawRectangle(new Pen(new SolidBrush(backgroundColour)), inner);
            return xOffset + inner.Width;
        }

        //public override bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, object rowObject)
        //{
        //    if (rowObject is Catalogue c)
        //    {
        //        bool isInternal = c.IsInternalDataset;
        //        bool isProjectSpecific = c.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository);
        //        bool isDeprecated = c.IsDeprecated;
        //        DrawBackground(g, cellBounds);
        //        int xOffset = 0;
        //        if (isInternal)
        //        {
        //            xOffset += RenderStatus("Internal", StatusColours.Internal, StatusColours.InternalCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        if (isProjectSpecific)
        //        {
        //            xOffset += RenderStatus("Project Specific", StatusColours.ProjectSpecific, StatusColours.ProjectSpecificCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        if (isDeprecated)
        //        {
        //            xOffset += RenderStatus("Deprecated", StatusColours.Deprecated, StatusColours.DeprecatedCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        return true;
        //    }
        //    if (rowObject is CatalogueItem ci)
        //    {
        //        DrawBackground(g, cellBounds);
        //        int xOffset = 0;

        //        if (ci.ExtractionInformation.IsExtractionIdentifier)
        //        {
        //            xOffset += RenderStatus("Extraction Identifier", StatusColours.ExtractionIdentifier, StatusColours.ExtractionIdentifierCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        if (ci.ExtractionInformation.IsPrimaryKey)
        //        {
        //            xOffset += RenderStatus("Primary Key", StatusColours.PrimaryKey, StatusColours.PrimaryKeyCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        if (ci.ExtractionInformation.HashOnDataRelease)
        //        {
        //            xOffset += RenderStatus("Hash on Release", StatusColours.HashOnRelease, StatusColours.HashOnReleaseCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        if(ci.ExtractionInformation.ExtractionCategory == ExtractionCategory.Supplemental)
        //        {
        //            xOffset += RenderStatus("Supplemental", StatusColours.Supplemental, StatusColours.SupplementalCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        if (ci.ExtractionInformation.ExtractionCategory == ExtractionCategory.SpecialApprovalRequired)
        //        {
        //            xOffset += RenderStatus("Special Approval", StatusColours.SpecialistApproval, StatusColours.SpecialistApprovalCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        if (ci.ExtractionInformation.ExtractionCategory == ExtractionCategory.Internal)
        //        {
        //            xOffset += RenderStatus("Internal", StatusColours.Internal, StatusColours.InternalCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        if (ci.ExtractionInformation.ExtractionCategory == ExtractionCategory.Deprecated)
        //        {
        //            xOffset += RenderStatus("Deprecated", StatusColours.Deprecated, StatusColours.DeprecatedCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        //if (ci.ExtractionInformation.ExtractionCategory == ExtractionCategory.ProjectSpecific)
        //        //{
        //        //    xOffset += RenderStatus("Project Specific", StatusColours.ProjectSpecific, StatusColours.ProjectSpecificCompliment, cellBounds, xOffset, g);
        //        //    xOffset += 5;
        //        //}
        //        return true;
        //    }
        //    if (rowObject is CohortIdentificationConfiguration cic)
        //    {
        //        DrawBackground(g, cellBounds);
        //        int xOffset = 0;
        //        if (cic.IsTemplate)
        //        {
        //            xOffset += RenderStatus("Template", StatusColours.Template, StatusColours.TemplateCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        if (cic.Frozen)
        //        {
        //            xOffset += RenderStatus("Frozen", StatusColours.Frozen, StatusColours.FrozenCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        if (cic.IsAssociatedToAProject(_activator.CoreChildProvider as DataExportChildProvider))
        //        {
        //            xOffset += RenderStatus("Project Specific", StatusColours.ProjectSpecific, StatusColours.ProjectSpecificCompliment, cellBounds, xOffset, g);
        //            xOffset += 5;
        //        }
        //        return true;
        //    }
        //    return true;
        //}

        //private static void DrawBackground(Graphics g, Rectangle r)
        //{
        //    //Color backgroundColor = Color.White;

        //    //using (Brush brush = new SolidBrush(backgroundColor))
        //    //{
        //    //    g.FillRectangle(brush, r.X - 1, r.Y - 1, r.Width + 2, r.Height + 2);
        //    //}
        //}


        //private int RenderStatus(string text, Color colour, Color textColour, Rectangle cellBounds, int offset, Graphics g)
        //{
        //    string _text = text;
        //    if (_useShortStrings)
        //    {
        //        _text = String.Join("", _text.Split(' ').Select(s => s[0]));
        //    }
        //    using Font font = new Font("Roboto", 8);
        //    RectangleF textBoxRect = cellBounds;
        //    textBoxRect.X += offset;
        //    textBoxRect.Width = 50;

        //    SizeF size = g.MeasureString(_text, font, cellBounds.Width, fmt);
        //    textBoxRect.Height = 16;
        //    textBoxRect.Width = size.Width + 4;
        //    textBoxRect.Y += (cellBounds.Height - size.Height) / 2;
        //    var textArc = GetRoundedRect(textBoxRect, textBoxRect.Height / 2);
        //    textArc.CloseFigure();

        //    g.DrawPath(new Pen(colour), textArc);
        //    g.FillPath(new SolidBrush(colour), textArc);
        //    g.Clip = new Region(cellBounds);
        //    textBoxRect.Y += 2;
        //    textBoxRect.Height -= 4;
        //    g.DrawString(_text, font, new SolidBrush(textColour), textBoxRect, fmt);

        //    return (int)Math.Ceiling(textBoxRect.Width);
        //}

        //private GraphicsPath GetRoundedRect(RectangleF rect, float diameter)
        //{
        //    GraphicsPath path = new GraphicsPath();

        //    RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
        //    path.AddArc(arc, 180, 90);
        //    arc.X = rect.Right - diameter;
        //    path.AddArc(arc, 270, 90);
        //    arc.Y = rect.Bottom - diameter;
        //    path.AddArc(arc, 0, 90);
        //    arc.X = rect.Left;
        //    path.AddArc(arc, 90, 90);
        //    path.CloseFigure();

        //    return path;
        //}
    }
}
