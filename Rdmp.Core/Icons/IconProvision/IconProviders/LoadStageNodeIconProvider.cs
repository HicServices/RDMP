using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
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
    public class LoadStageNodeIconProvider : IIconProvider
    {
        public static Image<Rgba32> GetIcon(object concept, ReusableLibraryCode.Icons.IconProvision.OverlayKind kind = ReusableLibraryCode.Icons.IconProvision.OverlayKind.None)
        {
            if (concept is LoadStageNode lsn)
            {
                switch (lsn.LoadStage)
                {
                    case Curation.Data.DataLoad.LoadStage.GetFiles:
                        return Image.Load<Rgba32>(CatalogueIcons.LoadStage1);
                    case Curation.Data.DataLoad.LoadStage.Mounting:
                        return Image.Load<Rgba32>(CatalogueIcons.LoadStage2);
                    case Curation.Data.DataLoad.LoadStage.AdjustRaw:
                        return Image.Load<Rgba32>(CatalogueIcons.LoadStage3);
                    case Curation.Data.DataLoad.LoadStage.AdjustStaging:
                        return Image.Load<Rgba32>(CatalogueIcons.LoadStage4);
                    case Curation.Data.DataLoad.LoadStage.PostLoad:
                        return Image.Load<Rgba32>(CatalogueIcons.LoadStage5);
                }
            }
            if (concept is LoadStage ls)
            {
                switch (ls)
                {
                    case Curation.Data.DataLoad.LoadStage.GetFiles:
                        return Image.Load<Rgba32>(CatalogueIcons.LoadStage1);
                    case Curation.Data.DataLoad.LoadStage.Mounting:
                        return Image.Load<Rgba32>(CatalogueIcons.LoadStage2);
                    case Curation.Data.DataLoad.LoadStage.AdjustRaw:
                        return Image.Load<Rgba32>(CatalogueIcons.LoadStage3);
                    case Curation.Data.DataLoad.LoadStage.AdjustStaging:
                        return Image.Load<Rgba32>(CatalogueIcons.LoadStage4);
                    case Curation.Data.DataLoad.LoadStage.PostLoad:
                        return Image.Load<Rgba32>(CatalogueIcons.LoadStage5);
                }
            }
            throw new NotImplementedException();
        }
    }
}
