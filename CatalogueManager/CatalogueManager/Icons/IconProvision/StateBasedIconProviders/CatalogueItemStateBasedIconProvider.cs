using System;
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

            var ei = ci.ExtractionInformation;

            //it's extractable
            if (ei != null)
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


            return toReturn;
        }
    }
}