﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public sealed class FilterStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private static readonly Image<Rgba32> BasicIcon = Image.Load<Rgba32>(CatalogueIcons.Filter);

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        if (o is not ExtractionFilter f) return CatalogueIconProvider.ConceptIs(typeof(IFilter), o) ? BasicIcon : null;
        // has known parameter values?
        return f.ExtractionFilterParameterSets.Any()
            ? IconOverlayProvider.GetOverlay(BasicIcon, OverlayKind.Parameter)
            :
            // just a regular filter then
            BasicIcon;
    }
}