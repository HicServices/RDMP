// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Rdmp.Core.Icons.IconOverlays
{
    public class IconOverlayProvider
    {
        readonly Dictionary<ValueTuple<Image<Argb32>,OverlayKind>,Image<Argb32>> _cache=new ();

        readonly Dictionary<Image, Dictionary<Image,Image>>  _resultCacheCustom = new Dictionary<Image, Dictionary<Image, Image>>();

        readonly Dictionary<Image<Argb32>, Image<Argb32>> _greyscaleCache = new();

        private readonly EnumImageCollection<OverlayKind> _images;

        public IconOverlayProvider()
        {
            _images = new EnumImageCollection<OverlayKind>(Overlays.ResourceManager);
        }

        public Image<Argb32> GetOverlay(Image<Argb32> forImage, OverlayKind overlayKind)
        {
            var key = (forImage, overlayKind);

            //make sure the input image is added to the cache if it is novel
            if (_cache.TryGetValue(key, out var hit))
                return hit;
            
            var clone = GetOverlayNoCache(forImage, overlayKind);
            _cache.Add(key,clone);

            return clone;
        }


        public Image GetOverlay(Image forImage, Image customOverlay)
        {
            //make sure the input image is added to the cache if it is novel
            if (!_resultCacheCustom.ContainsKey(forImage))
                _resultCacheCustom.Add(forImage, new Dictionary<Image, Image>());

            //is there a cached image for this overlay ?
            if (!_resultCacheCustom[forImage].ContainsKey(customOverlay))
            {
                //no

                //draw it
                var clone = forImage.CloneAs<Rgba32>();
                clone.Mutate(x=>x.DrawImage(customOverlay,1.0f));

                //and cache it
                _resultCacheCustom[forImage].Add(customOverlay,clone);
                
            }

            //now it is cached for sure
            return _resultCacheCustom[forImage][customOverlay];
        }

        public Image<Argb32> GetGrayscale(Image<Argb32> forImage)
        {
            if (!_greyscaleCache.ContainsKey(forImage))
                _greyscaleCache.Add(forImage, MakeGrayscale(forImage));

            return _greyscaleCache[forImage];
        }

        /// <summary>
        /// Use ImageSharp's grayscale converter
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private static Image<Argb32> MakeGrayscale(Image<Argb32> original)
        {
            return original.Clone(x=>x.Grayscale());
        }

        public Image<Argb32> GetOverlayNoCache(Image forImage, OverlayKind overlayKind)
        {
            //cached result does not exist so we must draw it
            var overlay = _images[overlayKind];
            var clone = forImage.CloneAs<Argb32>();
            clone.Mutate(x=>x.DrawImage(overlay,1.0f));
            return clone;
        }
    }
}
