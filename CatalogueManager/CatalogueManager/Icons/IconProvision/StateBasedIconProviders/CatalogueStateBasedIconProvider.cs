using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class CatalogueStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly Bitmap _basic;
        private readonly Bitmap _deprecated;
        private readonly Bitmap _deprecatedAndInternal;
        private readonly Bitmap _internal;


        public CatalogueStateBasedIconProvider()
        {
            _basic = CatalogueIcons.Catalogue;

            var overlayProvider = new IconOverlayProvider();
            _deprecated = overlayProvider.GetOverlayNoCache(_basic, OverlayKind.Deprecated);
            _deprecatedAndInternal = overlayProvider.GetOverlayNoCache(_deprecated, OverlayKind.Internal);
            _internal = overlayProvider.GetOverlayNoCache(_basic, OverlayKind.Internal);
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var c = o as Catalogue;
            
            if (c == null)
                return null;

            if (c.IsDeprecated && c.IsInternalDataset)
                return _deprecatedAndInternal;

            if (c.IsDeprecated)
                return _deprecated;

            if (c.IsInternalDataset)
                return _internal;

            return _basic;
        }
    }
}