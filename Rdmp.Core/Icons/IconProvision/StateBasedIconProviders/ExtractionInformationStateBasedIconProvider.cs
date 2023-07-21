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

public class ExtractionInformationStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private readonly Image<Rgba32> _extractionInformation_Core;
    private readonly Image<Rgba32> _extractionInformation_Deprecated;
    private readonly Image<Rgba32> _extractionInformation_InternalOnly;
    private readonly Image<Rgba32> _extractionInformation_ProjectSpecific;
    private readonly Image<Rgba32> _extractionInformation_SpecialApproval;
    private readonly Image<Rgba32> _extractionInformation_Supplemental;
    private readonly Image<Rgba32> _noIconAvailable;
    private readonly IconOverlayProvider _overlayProvider;

    public ExtractionInformationStateBasedIconProvider()
    {
        _extractionInformation_Core = Image.Load<Rgba32>(CatalogueIcons.ExtractionInformation);
        _extractionInformation_Supplemental = Image.Load<Rgba32>(CatalogueIcons.ExtractionInformation_Supplemental);
        _extractionInformation_SpecialApproval =
            Image.Load<Rgba32>(CatalogueIcons.ExtractionInformation_SpecialApproval);
        _extractionInformation_ProjectSpecific =
            Image.Load<Rgba32>(CatalogueIcons.ExtractionInformation_ProjectSpecific);
        _overlayProvider = new IconOverlayProvider();
        _extractionInformation_InternalOnly =
            _overlayProvider.GetOverlayNoCache(_extractionInformation_SpecialApproval, OverlayKind.Internal);
        _extractionInformation_Deprecated =
            _overlayProvider.GetOverlayNoCache(_extractionInformation_Core, OverlayKind.Deprecated);

        _noIconAvailable = Image.Load<Rgba32>(CatalogueIcons.NoIconAvailable);
    }

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        if (o is ExtractionCategory cat)
            return GetImage(cat);

        if (o is not ExtractionInformation ei) return null;

        var toReturn = GetImage(ei.ExtractionCategory);

        if (ei.IsExtractionIdentifier)
            toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.IsExtractionIdentifier);

        if (ei.IsPrimaryKey)
            toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Key);

        if (ei.HashOnDataRelease)
            toReturn = _overlayProvider.GetOverlay(toReturn, OverlayKind.Hashed);

        return toReturn;
    }

    private Image<Rgba32> GetImage(ExtractionCategory category)
    {
        return category switch
        {
            ExtractionCategory.Core => _extractionInformation_Core,
            ExtractionCategory.Supplemental => _extractionInformation_Supplemental,
            ExtractionCategory.SpecialApprovalRequired => _extractionInformation_SpecialApproval,
            ExtractionCategory.Internal => _extractionInformation_InternalOnly,
            ExtractionCategory.Deprecated => _extractionInformation_Deprecated,
            ExtractionCategory.ProjectSpecific => _extractionInformation_ProjectSpecific,
            ExtractionCategory.Any => _noIconAvailable,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}