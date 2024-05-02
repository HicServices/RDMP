// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Rdmp.Core.Icons.IconProvision;

public class EnumImageCollection<T> where T : struct, Enum, IConvertible
{
    private readonly Dictionary<T, Image<Rgba32>> _images = new();

    private static Image<Rgba32> LoadImage(byte[] ba)
    {
        return ba == null ? null : Image.Load<Rgba32>(ba);
    }

    public EnumImageCollection(ResourceManager resourceManager)
    {
        _images = Enum.GetValues<T>()
            .ToDictionary(s => s, s => LoadImage(resourceManager.GetObject(s.ToString()) as byte[]));
        var missingImages = _images.Where(i => i.Value is null).Select(p => p.Key).ToList();
        if (missingImages.Any())
            throw new IconProvisionException(
                $"The following expected images were missing from {resourceManager.BaseName}.resx{Environment.NewLine}{string.Join($",{Environment.NewLine}", missingImages)}");
    }

    public Image<Rgba32> this[T index] => _images[index];

    public Dictionary<string, Image<Rgba32>> ToStringDictionary(int newSizeInPixels = -1)
    {
        return _images.ToDictionary(
            k => k.Key.ToString(),
            v => newSizeInPixels == -1 ? v.Value : v.Value.Clone(x => x.Resize(newSizeInPixels, newSizeInPixels)));
    }
}