// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReusableLibraryCode.Icons
{
    public static class ImageSharpExtensions
    {
        public static System.Drawing.Bitmap ToBitmap(this Image image)
        {
            using (var memoryStream = new MemoryStream())
            {
                var imageEncoder = image.GetConfiguration().ImageFormatsManager.FindEncoder(PngFormat.Instance);
                image.Save(memoryStream, imageEncoder);

                memoryStream.Seek(0, SeekOrigin.Begin);

                return new System.Drawing.Bitmap(memoryStream);
            }
        }

        public static Image<TPixel> ToImageSharpImage<TPixel>(this System.Drawing.Bitmap bitmap) where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

                memoryStream.Seek(0, SeekOrigin.Begin);

                return Image.Load<TPixel>(memoryStream);
            }
        }
    }
}
