using Rdmp.Core.Curation.Data.Pipelines;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Icons.IconProvision.IconProviders
{
    public class PipelineComponentIconProvider : IIconProvider
    {
        public static Image<Rgba32> GetIcon(object concept, ReusableLibraryCode.Icons.IconProvision.OverlayKind kind = ReusableLibraryCode.Icons.IconProvision.OverlayKind.None)
        {
            if(concept is PipelineComponent pc)
            {
                if (((Pipeline)pc.Pipeline).SourcePipelineComponent_ID == pc.ID)
                {
                    return Image.Load<Rgba32>(CatalogueIcons.PipelineComponentStart);

                }
                if (((Pipeline)pc.Pipeline).DestinationPipelineComponent_ID == pc.ID)
                {
                    return Image.Load<Rgba32>(CatalogueIcons.PipelineComponentEnd);

                }
                return Image.Load<Rgba32>(CatalogueIcons.PipelineComponentMiddle);
            }
            throw new NotImplementedException();
        }
    }
}
