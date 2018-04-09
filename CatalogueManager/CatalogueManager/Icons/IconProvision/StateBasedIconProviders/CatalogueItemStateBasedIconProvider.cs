using System.Collections.Generic;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueManager.Icons.IconOverlays;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class CatalogueItemStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly Bitmap basicImage;
        private readonly IconOverlayProvider _overlayProvider;

        public CatalogueItemStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            basicImage = CatalogueIcons.CatalogueItem;
            _overlayProvider = overlayProvider;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var ci = o as CatalogueItem;

            if (ci == null)
                return null;

            Bitmap toReturn = basicImage;

            //it's extractable
            if (ci.ExtractionInformation != null)
                toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Extractable);
            

            return toReturn;
        }
    }
}