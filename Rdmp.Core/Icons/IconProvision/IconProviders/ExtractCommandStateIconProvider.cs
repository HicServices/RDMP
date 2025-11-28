using NPOI.OpenXmlFormats.Dml;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Icons.IconProvision.IconProviders
{
    internal class ExtractCommandStateIconProvider : IIconProvider
    {
        public static Image<Rgba32> GetIcon(object concept, ReusableLibraryCode.Icons.IconProvision.OverlayKind kind = ReusableLibraryCode.Icons.IconProvision.OverlayKind.None)
        {
            if (concept is ExtractCommandState ecs)
            {
                if (ecs is ExtractCommandState.NotLaunched)
                {
                    return null;
                }
                if (ecs is ExtractCommandState.WaitingToExecute || ecs is ExtractCommandState.WaitingForSQLServer)
                {
                    return Image.Load<Rgba32>(CatalogueIcons.Waiting);

                }
                if (ecs is ExtractCommandState.WritingMetadata || ecs is ExtractCommandState.WritingToFile)
                {
                    return Image.Load<Rgba32>(CatalogueIcons.Edit);

                }
                if (ecs is ExtractCommandState.Completed)
                {
                    return Image.Load<Rgba32>(CatalogueIcons.Tick);

                }
                if (ecs is ExtractCommandState.Warning)
                {
                    return Image.Load<Rgba32>(CatalogueIcons.Warning);

                }
                if (ecs is ExtractCommandState.UserAborted || ecs is ExtractCommandState.Crashed)
                {
                    return Image.Load<Rgba32>(CatalogueIcons.Failed);
                }
            }
            throw new NotImplementedException();
        }
    }
}
