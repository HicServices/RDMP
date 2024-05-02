// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;

/// <summary>
///     Provides 19x19 pixel images for the given object which could be an RDMPConcept, class instance or Type.
/// </summary>
public interface IIconProvider
{
    Image<Rgba32> ImageUnknown { get; }

    Image<Rgba32> GetImage(object concept, OverlayKind kind = OverlayKind.None);
}