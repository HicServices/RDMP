// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

internal sealed class ExtractionInformationStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private static readonly Image<Rgba32> ExtractionInformationCore =
        Image.Load<Rgba32>(CatalogueIcons.ExtractionInformation);

    private static readonly Image<Rgba32> ExtractionInformationSupplemental =
        Image.Load<Rgba32>(CatalogueIcons.ExtractionInformation_Supplemental);

    private static readonly Image<Rgba32> ExtractionInformationSpecialApproval =
        Image.Load<Rgba32>(CatalogueIcons.ExtractionInformation_SpecialApproval);

    private static readonly Image<Rgba32> ExtractionInformationInternalOnly =
        IconOverlayProvider.GetOverlayNoCache(ExtractionInformationSpecialApproval, OverlayKind.Internal);

    private static readonly Image<Rgba32> ExtractionInformationDeprecated =
        IconOverlayProvider.GetOverlayNoCache(ExtractionInformationCore, OverlayKind.Deprecated);

    private static readonly Image<Rgba32> ExtractionInformationProjectSpecific =
        Image.Load<Rgba32>(CatalogueIcons.ExtractionInformation_ProjectSpecific);

    private static readonly Image<Rgba32> NoIconAvailable = Image.Load<Rgba32>(CatalogueIcons.NoIconAvailable);

    private static readonly Image<Rgba32> ExtractionInformationNotExtractable = IconOverlayProvider.GetOverlayNoCache(ExtractionInformationCore, OverlayKind.Delete);

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        if (o is ExtractionCategory cat)
            return GetImage(cat);

        if (o is not ExtractionInformation ei) return null;

        var toReturn = GetImage(ei.ExtractionCategory);

        if (ei.IsExtractionIdentifier)
            toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.IsExtractionIdentifier);

        if (ei.IsPrimaryKey)
            toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Key);

        if (ei.HashOnDataRelease)
            toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Hashed);

        return toReturn;
    }

    private static Image<Rgba32> GetImage(ExtractionCategory category)
    {
        return category switch
        {
            ExtractionCategory.Core => ExtractionInformationCore,
            ExtractionCategory.Supplemental => ExtractionInformationSupplemental,
            ExtractionCategory.SpecialApprovalRequired => ExtractionInformationSpecialApproval,
            ExtractionCategory.Internal => ExtractionInformationInternalOnly,
            ExtractionCategory.Deprecated => ExtractionInformationDeprecated,
            ExtractionCategory.ProjectSpecific => ExtractionInformationProjectSpecific,
            ExtractionCategory.Any => NoIconAvailable,
            ExtractionCategory.NotExtractable => ExtractionInformationNotExtractable,
            _ => throw new ArgumentOutOfRangeException(nameof(category))
        };
    }
}