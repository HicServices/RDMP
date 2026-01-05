using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision.IconProviders;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.ItemActivation;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;

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
            if (this.RowObject is ExtractionConfiguration ec)
            {
                DrawBackground(g, r);
                int xOffset = 0;
                if (ec.IsReleased)
                {
                    xOffset += RenderChip("Frozen", StatusColours.Frozen, StatusColours.FrozenCompliment, r, xOffset, g);
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
            if (!(base.Aspect is IConvertible convertible))
            {
                return xOffset;
            }

            double num = convertible.ToDouble(NumberFormatInfo.InvariantInfo);
            Rectangle rect = Rectangle.Inflate(inner, -1, -1);

            var rounded = GetRoundedRect(inner, inner.Height/2);
            g.FillPath(new SolidBrush(backgroundColour), rounded);
            g.DrawString(_text, this.Font, new SolidBrush(textColor), inner, fmt);
            g.DrawPath(new Pen(new SolidBrush(backgroundColour)), rounded);
            return xOffset + inner.Width;
        }
        private static GraphicsPath GetRoundedRect(RectangleF rect, float diameter)
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
