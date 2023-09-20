// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public sealed class ExtractableColumnStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private static readonly Image<Rgba32> BasicImage = Image.Load<Rgba32>(CatalogueIcons.ExtractableColumn);

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        if (o is not ExtractableColumn col)
            return null;

        var toReturn = BasicImage;

        //if the current state is to hash add the overlay
        if (col.HashOnDataRelease)
            toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Hashed);

        if (col.CatalogueExtractionInformation?.IsPrimaryKey ?? false)
            toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Key);
        if (col.CatalogueExtractionInformation?.IsExtractionIdentifier ?? false)
            toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.IsExtractionIdentifier);

        var ei = col.CatalogueExtractionInformation;

        //its parent ExtractionInformation still exists then we can determine its category
        return ei == null
            ? toReturn
            : ei.ExtractionCategory switch
            {
                ExtractionCategory.ProjectSpecific =>
                    IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Extractable),
                ExtractionCategory.Core => IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Extractable),
                ExtractionCategory.Supplemental => IconOverlayProvider.GetOverlay(toReturn,
                    OverlayKind.Extractable_Supplemental),
                ExtractionCategory.SpecialApprovalRequired => IconOverlayProvider.GetOverlay(toReturn,
                    OverlayKind.Extractable_SpecialApproval),
                ExtractionCategory.Internal => IconOverlayProvider.GetOverlay(toReturn,
                    OverlayKind.Extractable_Internal),
                ExtractionCategory.Deprecated => IconOverlayProvider.GetOverlay(
                    IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Extractable), OverlayKind.Deprecated),
                _ => throw new ArgumentOutOfRangeException(nameof(o))
            };
    }
}