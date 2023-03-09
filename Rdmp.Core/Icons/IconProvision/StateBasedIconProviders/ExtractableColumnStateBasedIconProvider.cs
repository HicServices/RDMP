// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using SixLabors.ImageSharp;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class ExtractableColumnStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly Image<Rgba32> _basicImage;
        private readonly IconOverlayProvider _overlayProvider;

        public ExtractableColumnStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _basicImage = Image.Load<Rgba32>(CatalogueIcons.ExtractableColumn);
            _overlayProvider = overlayProvider;
        }

        public Image<Rgba32> GetImageIfSupportedObject(object o)
        {
            if (o is not ExtractableColumn col)
                return null;

            var toReturn = _basicImage;
            
            //if the current state is to hash add the overlay
            if (col.HashOnDataRelease) 
                toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Hashed);

            if (col.CatalogueExtractionInformation?.IsPrimaryKey ?? false)
                toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Key);
            if (col.CatalogueExtractionInformation?.IsExtractionIdentifier ?? false)
                toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.IsExtractionIdentifier);

            var ei = col.CatalogueExtractionInformation;

            //its parent ExtractionInformation still exists then we can determine its category
            if (ei == null) return toReturn;

            return ei.ExtractionCategory switch
            {
                ExtractionCategory.ProjectSpecific =>
                    _overlayProvider.GetOverlay(toReturn, OverlayKind.Extractable),
                ExtractionCategory.Core => _overlayProvider.GetOverlay(toReturn, OverlayKind.Extractable),
                ExtractionCategory.Supplemental => _overlayProvider.GetOverlay(toReturn,
                    OverlayKind.Extractable_Supplemental),
                ExtractionCategory.SpecialApprovalRequired => _overlayProvider.GetOverlay(toReturn,
                    OverlayKind.Extractable_SpecialApproval),
                ExtractionCategory.Internal => _overlayProvider.GetOverlay(toReturn,
                    OverlayKind.Extractable_Internal),
                ExtractionCategory.Deprecated => _overlayProvider.GetOverlay(
                    _overlayProvider.GetOverlay(toReturn, OverlayKind.Extractable), OverlayKind.Deprecated),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}