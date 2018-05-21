using System;
using System.Collections.Generic;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueManager.Icons.IconOverlays;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ExtractionInformationStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _extractionInformation_Core;
        private Bitmap _extractionInformation_Supplemental;
        private Bitmap _extractionInformation_SpecialApproval;
        private Bitmap _extractionInformation_InternalOnly;
        private Bitmap _extractionInformation_Deprecated;
        private Bitmap _extractionInformation_ProjectSpecific;
        private IconOverlayProvider _overlayProvider;

        public ExtractionInformationStateBasedIconProvider()
        {
            _extractionInformation_Core = CatalogueIcons.ExtractionInformation;
            _extractionInformation_Supplemental = CatalogueIcons.ExtractionInformation_Supplemental;
            _extractionInformation_SpecialApproval = CatalogueIcons.ExtractionInformation_SpecialApproval;
            _extractionInformation_ProjectSpecific = CatalogueIcons.ExtractionInformation_ProjectSpecific;
            _overlayProvider = new IconOverlayProvider();
            _extractionInformation_InternalOnly = _overlayProvider.GetOverlayNoCache(_extractionInformation_SpecialApproval, OverlayKind.Internal);
            _extractionInformation_Deprecated = _overlayProvider.GetOverlayNoCache(_extractionInformation_Core,OverlayKind.Deprecated);
        }
        
        public Bitmap GetImageIfSupportedObject(object o)
        {
            var ei = o as ExtractionInformation;

            if (ei == null)
                return null;

            Bitmap toReturn;
            switch (ei.ExtractionCategory)
            {
                case ExtractionCategory.Core:
                    toReturn = _extractionInformation_Core;
                    break;
                case ExtractionCategory.Supplemental:
                    toReturn = _extractionInformation_Supplemental;
                    break;
                case ExtractionCategory.SpecialApprovalRequired:
                    toReturn = _extractionInformation_SpecialApproval;
                    break;
                case ExtractionCategory.Internal:
                    toReturn = _extractionInformation_InternalOnly;
                    break;
                case ExtractionCategory.Deprecated:
                    toReturn = _extractionInformation_Deprecated;
                    break;
                case ExtractionCategory.ProjectSpecific:
                    toReturn = _extractionInformation_ProjectSpecific;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();//.Any is not valid for ExtractionInformations
            }

            if (ei.IsExtractionIdentifier)
                return _overlayProvider.GetOverlay(toReturn, OverlayKind.Key);
            
            return toReturn;
        }
    }
}