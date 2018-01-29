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
        private Dictionary<int, CatalogueItemClassification> _classifications;

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

            //fetch from cached classifications if you can 
            if (_classifications != null && _classifications.ContainsKey(ci.ID))
            {
                //it's extractable
                if (_classifications[ci.ID].ExtractionInformation_ID != null)
                    toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Extractable);

                //it has no ColumnInfo but an ExtractionInformation!
                if (_classifications[ci.ID].IsExtractionInformationOrphan())
                    return _overlayProvider.GetOverlay(toReturn, OverlayKind.Problem);
            }

            return toReturn;
        }

        public void SetClassifications(Dictionary<int, CatalogueItemClassification> classifications)
        {
            _classifications = classifications;
        }
    }
}