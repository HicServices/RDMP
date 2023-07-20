// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using SixLabors.ImageSharp;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public sealed class CatalogueItemStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private static readonly Image<Rgba32> BasicImage = Image.Load<Rgba32>(CatalogueIcons.CatalogueItem);
    private static readonly Image<Rgba32> TransformImage = Image.Load<Rgba32>(CatalogueIcons.CatalogueItemTransform);

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        if (o is not CatalogueItem ci)
            return null;

        var ei = ci.ExtractionInformation;
        var toReturn = ei?.IsProperTransform() ?? false ? TransformImage: BasicImage;

        //it's not extractable:
        if (ei == null) return toReturn;

        if (ei.HashOnDataRelease)
            toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Hashed);

        if (ei.IsExtractionIdentifier)
            toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.IsExtractionIdentifier);

        if (ei.IsPrimaryKey)
            toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Key);

        switch (ei.ExtractionCategory)
        {
            case ExtractionCategory.ProjectSpecific:
            case ExtractionCategory.Core:
                toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Extractable);
                break;
            case ExtractionCategory.Supplemental:
                toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Extractable_Supplemental);
                break;
            case ExtractionCategory.SpecialApprovalRequired:
                toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Extractable_SpecialApproval);
                break;
            case ExtractionCategory.Internal:
                toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Extractable_Internal);
                break;
            case ExtractionCategory.Deprecated:
                toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Extractable);
                toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Deprecated);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(o));
        }


        return toReturn;
    }
}