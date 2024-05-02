// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Rdmp.Core.Icons.IconOverlays;

public static class IconOverlayProvider
{
    private static readonly ConcurrentDictionary<ValueTuple<Image<Rgba32>, OverlayKind>, Image<Rgba32>> Cache = new();

    private static readonly ConcurrentDictionary<ValueTuple<Image<Rgba32>, Image<Rgba32>>, Image<Rgba32>>
        ResultCacheCustom = new();

    private static readonly ConcurrentDictionary<Image<Rgba32>, Image<Rgba32>> GreyscaleCache = new();

    private static readonly EnumImageCollection<OverlayKind> Images = new(Overlays.ResourceManager);

    public static Image<Rgba32> GetOverlay(Image<Rgba32> forImage, OverlayKind overlayKind)
    {
        return Cache.GetOrAdd((forImage, overlayKind), _ => GetOverlayNoCache(forImage, overlayKind));
    }

    public static Image<Rgba32> GetOverlay(Image<Rgba32> forImage, Image<Rgba32> customOverlay)
    {
        return ResultCacheCustom.GetOrAdd((forImage, customOverlay),
            _ => forImage.Clone(x => x.DrawImage(customOverlay, 1.0f)));
    }

    public static Image<Rgba32> GetGreyscale(Image<Rgba32> forImage)
    {
        return GreyscaleCache.GetOrAdd(forImage, MakeGreyscale);
    }

    /// <summary>
    ///     Use ImageSharp's greyscale converter
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    private static Image<Rgba32> MakeGreyscale(Image<Rgba32> original)
    {
        return original.Clone(static x => x.Grayscale());
    }

    public static Image<Rgba32> GetOverlayNoCache(Image<Rgba32> forImage, OverlayKind overlayKind)
    {
        return forImage.Clone(x => x.DrawImage(Images[overlayKind], 1.0f));
    }
}