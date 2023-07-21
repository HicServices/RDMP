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

public class IconOverlayProvider
{
    private readonly ConcurrentDictionary<ValueTuple<Image<Rgba32>, OverlayKind>, Image<Rgba32>> _cache =
        new();

    private readonly ConcurrentDictionary<Image<Rgba32>, Image<Rgba32>> _greyscaleCache =
        new();

    private readonly EnumImageCollection<OverlayKind> _images;

    private readonly ConcurrentDictionary<ValueTuple<Image<Rgba32>, Image<Rgba32>>, Image<Rgba32>> _resultCacheCustom =
        new();

    public IconOverlayProvider()
    {
        _images = new EnumImageCollection<OverlayKind>(Overlays.ResourceManager);
    }

    public Image<Rgba32> GetOverlay(Image<Rgba32> forImage, OverlayKind overlayKind)
    {
        var key = (forImage, overlayKind);

        //make sure the input image is added to the cache if it is novel
        if (_cache.TryGetValue(key, out var hit))
            return hit;

        var clone = GetOverlayNoCache(forImage, overlayKind);
        _cache.TryAdd(key, clone);

        return clone;
    }


    public Image<Rgba32> GetOverlay(Image<Rgba32> forImage, Image<Rgba32> customOverlay)
    {
        if (_resultCacheCustom.TryGetValue((forImage, customOverlay), out var hit))
            return hit;

        var clone = forImage.Clone(x => x.DrawImage(customOverlay, 1.0f));

        //and cache it
        _resultCacheCustom.TryAdd((forImage, customOverlay), clone);
        return clone;
    }

    public Image<Rgba32> GetGrayscale(Image<Rgba32> forImage)
    {
        _greyscaleCache.TryAdd(forImage, MakeGrayscale(forImage));

        return _greyscaleCache[forImage];
    }

    /// <summary>
    ///     Use ImageSharp's grayscale converter
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    private static Image<Rgba32> MakeGrayscale(Image<Rgba32> original)
    {
        return original.Clone(x => x.Grayscale());
    }

    public Image<Rgba32> GetOverlayNoCache(Image<Rgba32> forImage, OverlayKind overlayKind)
    {
        //cached result does not exist so we must draw it
        var overlay = _images[overlayKind];
        var clone = forImage.Clone(x => x.DrawImage(overlay, 1.0f));
        return clone;
    }
}