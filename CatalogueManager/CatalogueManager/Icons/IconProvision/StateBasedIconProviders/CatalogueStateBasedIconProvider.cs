using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class CatalogueStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly Bitmap _basic;
        private Bitmap _projectSpecific;
        private readonly Bitmap _deprecated;
        private readonly Bitmap _deprecatedAndInternal;
        private readonly Bitmap _internal;
        private IconOverlayProvider _overlayProvider;


        public CatalogueStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _basic = CatalogueIcons.Catalogue;
            _projectSpecific = CatalogueIcons.ProjectCatalogue;

            _overlayProvider = overlayProvider;
            _deprecated = overlayProvider.GetOverlayNoCache(_basic, OverlayKind.Deprecated);
            _deprecatedAndInternal = overlayProvider.GetOverlayNoCache(_deprecated, OverlayKind.Internal);
            _internal = overlayProvider.GetOverlayNoCache(_basic, OverlayKind.Internal);
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var c = o as Catalogue;
            
            if (c == null)
                return null;

            var status = c.GetExtractabilityStatus(null);

            Bitmap img;
            if (status != null && status.IsExtractable && status.IsProjectSpecific)
                img = _projectSpecific;
            else
                img = _basic;
            
            if (c.IsDeprecated && c.IsInternalDataset)
                img = _deprecatedAndInternal;
            
            if (c.IsDeprecated)
                img = _deprecated;
            
            if (c.IsInternalDataset)
                img = _internal;


            if (status != null && status.IsExtractable)
                img = _overlayProvider.GetOverlay(img, OverlayKind.Extractable);

            return img;
        }
    }
}