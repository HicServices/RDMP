using Org.BouncyCastle.Pqc.Crypto.Utilities;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.IconProviders;

public class RDMPConceptIconProvider : IIconProvider
{

    public static Image<Rgba32> GetIcon(object concept, OverlayKind kind = OverlayKind.None)
    {
        var _concept = (RDMPConcept)concept;
        switch (_concept)
        {
            case RDMPConcept.CatalogueFolder:
                return Image.Load<Rgba32>(CatalogueIcons.CatalogueFolder);
            default:
                return Image.Load<Rgba32>(CatalogueIcons.NoIconAvailable);
        }
    }
}
