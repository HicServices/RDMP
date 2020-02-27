using System;
using System.Drawing;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Icons.IconProvision.StateBasedIconProviders
{
    public class ExtractableColumnStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly Bitmap basicImage;
        private readonly IconOverlayProvider _overlayProvider;

        public ExtractableColumnStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            basicImage = CatalogueIcons.ExtractableColumn;
            _overlayProvider = overlayProvider;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var col = o as ExtractableColumn;

            if (col == null)
                return null;

            Bitmap toReturn = basicImage;
            
            //if the current state is to hash add the overlay
            if (col.HashOnDataRelease) 
                toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Hashed);
            
            var ei = col.CatalogueExtractionInformation;

            //it's parent ExtractionInformation still exists then we can determine it's category
            if (ei != null)
            {
                switch (ei.ExtractionCategory)
                {
                    case ExtractionCategory.ProjectSpecific:
                    case ExtractionCategory.Core:
                        toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Extractable);
                        break;
                    case ExtractionCategory.Supplemental:
                        toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Extractable_Supplemental);
                        break;
                    case ExtractionCategory.SpecialApprovalRequired:
                        toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Extractable_SpecialApproval);
                        break;
                    case ExtractionCategory.Internal:
                        toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Extractable_Internal);
                        break;
                    case ExtractionCategory.Deprecated:
                        toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Extractable);
                        toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Deprecated);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return toReturn;
        }
    }
}