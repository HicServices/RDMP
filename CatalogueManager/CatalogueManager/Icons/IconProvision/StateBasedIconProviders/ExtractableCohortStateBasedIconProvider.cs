using System.Drawing;
using CatalogueManager.Icons.IconOverlays;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ExtractableCohortStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly IconOverlayProvider _overlayProvider;
        private Bitmap _basicIcon;

        public ExtractableCohortStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _overlayProvider = overlayProvider;
            _basicIcon = CatalogueIcons.ExtractableCohort;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var cohort = o as ExtractableCohort;

            if (cohort != null)
                return cohort.IsDeprecated
                    ? _overlayProvider.GetOverlay(_basicIcon, OverlayKind.Deprecated)
                    : _basicIcon;

            return null;
        }
    }
}