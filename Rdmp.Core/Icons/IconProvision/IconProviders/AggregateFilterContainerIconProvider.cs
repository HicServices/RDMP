using Rdmp.Core.Curation.Data.Aggregation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Icons.IconProvision.IconProviders
{
    /// <summary>
    /// Provides custom Icons
    /// </summary>
    internal class AggregateFilterContainerIconProvider : IIconProvider
    {
        public static Image<Rgba32> GetIcon(object concept, ReusableLibraryCode.Icons.IconProvision.OverlayKind kind = ReusableLibraryCode.Icons.IconProvision.OverlayKind.None)
        {
            if(concept is AggregateFilterContainer afc)
            {
                if (afc.Operation == Curation.Data.FilterContainerOperation.AND) return Image.Load<Rgba32>(CatalogueIcons.AggregateFilterContainerAND);
                if (afc.Operation == Curation.Data.FilterContainerOperation.OR) return Image.Load<Rgba32>(CatalogueIcons.AggregateFilterContainerOR);
            }
            throw new NotImplementedException();
        }
    }
}
