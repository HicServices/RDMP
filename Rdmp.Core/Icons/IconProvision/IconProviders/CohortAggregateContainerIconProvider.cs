using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.ReusableLibraryCode.Checks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Icons.IconProvision.IconProviders
{
    internal class CohortAggregateContainerIconProvider : IIconProvider
    {
        public static Image<Rgba32> GetIcon(object concept, ReusableLibraryCode.Icons.IconProvision.OverlayKind kind = ReusableLibraryCode.Icons.IconProvision.OverlayKind.None)
        {
            if(concept is CohortAggregateContainer cr)
            {
                if (cr.Operation == SetOperation.UNION) return Image.Load<Rgba32>(CatalogueIcons.UNIONCohortAggregate);
                if (cr.Operation == SetOperation.INTERSECT) return Image.Load<Rgba32>(CatalogueIcons.INTERSECTCohortAggregate);
                if (cr.Operation == SetOperation.EXCEPT) return Image.Load<Rgba32>(CatalogueIcons.EXCEPTCohortAggregate);
                //if (cr is CheckResult.Fail) return Image.Load<Rgba32>(CatalogueIcons.Failed);
                //if (cr is CheckResult.Warning) return Image.Load<Rgba32>(CatalogueIcons.Warning);
                //if (cr is CheckResult.Success) return Image.Load<Rgba32>(CatalogueIcons.Tick);
            }
            throw new NotImplementedException();
        }
    }
}
