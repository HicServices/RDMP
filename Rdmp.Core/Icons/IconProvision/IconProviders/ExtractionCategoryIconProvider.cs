using Rdmp.Core.Curation.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Icons.IconProvision.IconProviders
{
    public class ExtractionCategoryIconProvider : IIconProvider
    {
        public static Image<Rgba32> GetIcon(object concept, ReusableLibraryCode.Icons.IconProvision.OverlayKind kind = ReusableLibraryCode.Icons.IconProvision.OverlayKind.None)
        {
            var img = Image.Load<Rgba32>(CatalogueIcons.CatalogueItemsNode);
            if (concept is ExtractionCategory ec)
            {
                switch (ec)
                {
                    case ExtractionCategory.Supplemental:
                        Mutate(img, StatusColours.Supplemental);
                        break;
                    case ExtractionCategory.Core:
                        Mutate(img, StatusColours.Core);
                        break;
                    case ExtractionCategory.SpecialApprovalRequired:
                        Mutate(img, StatusColours.SpecialistApproval);
                        break;
                    case ExtractionCategory.Internal:
                        Mutate(img, StatusColours.Internal);
                        break;
                    case ExtractionCategory.Deprecated:
                        Mutate(img, StatusColours.Deprecated);
                        break;

                }
            }

            return img;
        }
        private static void Mutate(Image<Rgba32> image, System.Drawing.Color colour)
        {
            image.ProcessPixelRows(accessor =>
            {
                // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                Rgba32 newcolour = Color.FromRgba(colour.R, colour.G, colour.B, colour.A);

                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    if (y == 4)
                    {
                        pixelRow[3] = newcolour;
                        pixelRow[4] = newcolour;
                        pixelRow[5] = newcolour;
                    }
                    if (y == 5)
                    {
                        pixelRow[3] = newcolour;
                        pixelRow[4] = newcolour;
                        pixelRow[5] = newcolour;
                        pixelRow[6] = newcolour;
                    }
                    if (y > 5 && y < 12)
                    {
                        pixelRow[3] = newcolour;
                        pixelRow[4] = newcolour;
                        pixelRow[5] = newcolour;
                        pixelRow[6] = newcolour;
                        pixelRow[7] = newcolour;
                        pixelRow[8] = newcolour;
                        pixelRow[9] = newcolour;
                        pixelRow[10] = newcolour;
                        pixelRow[11] = newcolour;
                        pixelRow[12] = newcolour;
                    }
                }
            });
        }
    }
}
