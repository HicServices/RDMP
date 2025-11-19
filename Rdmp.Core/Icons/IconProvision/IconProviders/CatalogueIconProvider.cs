using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Icons.IconProvision.IconProviders
{
    public class CatalogueIconProvider: IIconProvider
    {
        public static Image<Rgba32> GetIcon(object concept, OverlayKind kind = OverlayKind.None)
        {
            return Image.Load<Rgba32>(CatalogueIcons.Catalogue);
        }
    }
}
