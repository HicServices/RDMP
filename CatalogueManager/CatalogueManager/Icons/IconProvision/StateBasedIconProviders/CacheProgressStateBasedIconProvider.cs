using System.Drawing;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.Icons.IconOverlays;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class CacheProgressStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly IconOverlayProvider _overlayProvider;
        private Bitmap _cacheProgress;

        public CacheProgressStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _overlayProvider = overlayProvider;
            _cacheProgress = CatalogueIcons.CacheProgress;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var cp = o as CacheProgress;

            if (cp == null)
                return null;

            if (cp.PermissionWindow_ID != null && cp.PermissionWindow.LockedBecauseRunning)
                    return _overlayProvider.GetOverlay(_cacheProgress, OverlayKind.Locked);

            return _cacheProgress;
        }
    }
}