// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
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
        private Bitmap _noIconAvailable;

        public ExtractionInformationStateBasedIconProvider()
        {
            _extractionInformation_Core = CatalogueIcons.ExtractionInformation;
            _extractionInformation_Supplemental = CatalogueIcons.ExtractionInformation_Supplemental;
            _extractionInformation_SpecialApproval = CatalogueIcons.ExtractionInformation_SpecialApproval;
            _extractionInformation_ProjectSpecific = CatalogueIcons.ExtractionInformation_ProjectSpecific;
            _overlayProvider = new IconOverlayProvider();
            _extractionInformation_InternalOnly = _overlayProvider.GetOverlayNoCache(_extractionInformation_SpecialApproval, OverlayKind.Internal);
            _extractionInformation_Deprecated = _overlayProvider.GetOverlayNoCache(_extractionInformation_Core,OverlayKind.Deprecated);

            _noIconAvailable = CatalogueIcons.NoIconAvailable;
        }
        
        public Bitmap GetImageIfSupportedObject(object o)
        {
            
            if(o is ExtractionCategory cat)
                return GetImage(cat);

            if (o is ExtractionInformation ei)
            {
                Bitmap toReturn = GetImage(ei.ExtractionCategory);
                
                if (ei.IsExtractionIdentifier)
                    toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.IsExtractionIdentifier);


                if (ei.IsPrimaryKey)
                    toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Key);

                if (ei.HashOnDataRelease) 
                    toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Hashed);

                return toReturn;

            }

            return null;
        }

        private Bitmap GetImage(ExtractionCategory category)
        {
            switch (category)
            {
                case ExtractionCategory.Core:
                    return _extractionInformation_Core;
                case ExtractionCategory.Supplemental:
                    return _extractionInformation_Supplemental;
                case ExtractionCategory.SpecialApprovalRequired:
                    return _extractionInformation_SpecialApproval;
                case ExtractionCategory.Internal:
                    return _extractionInformation_InternalOnly;
                case ExtractionCategory.Deprecated:
                    return _extractionInformation_Deprecated;
                case ExtractionCategory.ProjectSpecific:
                    return _extractionInformation_ProjectSpecific;
                case ExtractionCategory.Any:
                    return _noIconAvailable;
                default:
                    throw new ArgumentOutOfRangeException();//.Any is not valid for ExtractionInformations
            }
        }
    }
}