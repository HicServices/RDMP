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
        private IconOverlayProvider _overlayProvider;


        public CatalogueStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _basic = CatalogueIcons.Catalogue;
            _projectSpecific = CatalogueIcons.ProjectCatalogue;

            _overlayProvider = overlayProvider;

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

            if (c.IsDeprecated)
                img = _overlayProvider.GetOverlay(img, OverlayKind.Deprecated);
            
            if (c.IsInternalDataset)
                img = _overlayProvider.GetOverlay(img, OverlayKind.Internal);
            
            if (status != null && status.IsExtractable)
                img = _overlayProvider.GetOverlay(img, OverlayKind.Extractable);

            return img;
        }
    }
}