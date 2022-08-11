// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;

namespace Rdmp.Core.Icons.IconProvision
{
    /// <summary>
    /// Creates <see cref="Icon"/> instances based on provided <see cref="Bitmap"/> (with support for caching).
    /// </summary>
    public class IconFactory
    {
        private readonly Dictionary<Image, Icon> _iconDictionary = new Dictionary<Image, Icon>();

        /// <summary>
        /// Returns an <see cref="Icon"/> depicting the provided <paramref name="bmp"/>.  Calling this
        /// method multiple times with the same <see cref="Icon"/> will return the same <see cref="Icon"/> instance.
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public Icon GetIcon(Image bmp)
        {
            if (_iconDictionary.ContainsKey(bmp))
                return _iconDictionary[bmp];

            return CreateIcon(bmp);
        }

        private Icon CreateIcon(Image bmp)
        {
            // Get an Hicon for myBitmap.
            IntPtr Hicon = bmp.GetHicon();

            // Create a new icon from the handle. 
            Icon newIcon = Icon.FromHandle(Hicon);

            //now that we have created the icon don't create it again for the same Image (to avoid memory leaks)
            _iconDictionary.Add(bmp, newIcon);

            return newIcon;
        }

    }
}
