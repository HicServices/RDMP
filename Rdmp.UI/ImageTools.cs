// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using ImageFormat=System.Drawing.Imaging.ImageFormat;
using Bitmap = System.Drawing.Bitmap;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;

namespace Rdmp.UI;

public static class ImageTools
{

    private static ConcurrentDictionary<Image<Rgba32>, Bitmap> ImageToBitmapCacheRgba32 = new();

    public static Bitmap ImageToBitmap(this Image<Rgba32> img)
    {
        return ImageToBitmapCacheRgba32.GetOrAdd(img, (k) =>
        {
            using MemoryStream stream = new();
            k.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(stream);
        });
    }

    static ConcurrentDictionary<byte[], Bitmap> ImageToBitmapCacheByteArray = new ();

    public static Bitmap ImageToBitmap(this byte [] img)
    {
        return ImageToBitmapCacheByteArray.GetOrAdd(img, (k) =>
        {
            using MemoryStream ms = new(k);
            return new Bitmap(ms);
        });
    }

    public static Image<Rgba32> LegacyToImage(this System.Drawing.Image img)
    {
        using MemoryStream stream = new();
        img.Save(stream,ImageFormat.Png);
        stream.Seek(0, SeekOrigin.Begin);
        return Image.Load<Rgba32>(stream);
    }
}