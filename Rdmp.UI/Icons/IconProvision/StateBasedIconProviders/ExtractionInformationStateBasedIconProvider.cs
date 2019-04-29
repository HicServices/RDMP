// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.Icons.IconOverlays;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Icons.IconProvision.StateBasedIconProviders
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