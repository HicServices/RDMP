// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.UI.Icons.IconOverlays;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Icons.IconProvision.StateBasedIconProviders
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