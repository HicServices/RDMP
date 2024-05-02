// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

internal sealed class CatalogueItemsNodeStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private static readonly Image<Rgba32> Basic = Image.Load<Rgba32>(CatalogueIcons.CatalogueItemsNode);
    private static readonly Image<Rgba32> Core = IconOverlayProvider.GetOverlay(Basic, OverlayKind.Extractable);

    private static readonly Image<Rgba32> Internal =
        IconOverlayProvider.GetOverlay(Basic, OverlayKind.Extractable_Internal);

    private static readonly Image<Rgba32> Supplemental =
        IconOverlayProvider.GetOverlay(Basic, OverlayKind.Extractable_Supplemental);

    private static readonly Image<Rgba32> Special =
        IconOverlayProvider.GetOverlay(Basic, OverlayKind.Extractable_SpecialApproval);

    private static readonly Image<Rgba32> Deprecated = IconOverlayProvider.GetOverlay(Basic, OverlayKind.Deprecated);

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        if (o is not CatalogueItemsNode cin)
            return null;

        return cin.Category == null
            ? Basic
            : cin.Category.Value switch
            {
                ExtractionCategory.Core => Core,
                ExtractionCategory.Supplemental => Supplemental,
                ExtractionCategory.SpecialApprovalRequired => Special,
                ExtractionCategory.Internal => Internal,
                ExtractionCategory.Deprecated => Deprecated,
                _ => Basic
            };
    }
}