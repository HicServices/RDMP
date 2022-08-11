// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.Linq;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconOverlays
{
    public class IconOverlayProvider
    {
        readonly Dictionary<Image,List<CachedOverlayResult>> _resultCache = new Dictionary<Image, List<CachedOverlayResult>>();

        readonly Dictionary<Image, Dictionary<Image,Image>>  _resultCacheCustom = new Dictionary<Image, Dictionary<Image, Image>>();

        readonly Dictionary<Image,Image> _greyscaleCache = new Dictionary<Image, Image>();

        private readonly EnumImageCollection<OverlayKind> _images;

        public IconOverlayProvider()
        {
            _images = new EnumImageCollection<OverlayKind>(Overlays.ResourceManager);
        }

        public Image GetOverlay(Image forImage, OverlayKind overlayKind)
        {
            //make sure the input image is added to the cache if it is novel
            if(!_resultCache.ContainsKey(forImage))
                _resultCache.Add(forImage,new List<CachedOverlayResult>());

            //is there a cached image for this overlay ?
            var cachedResult = _resultCache[forImage].SingleOrDefault(c => c.Kind == overlayKind);
                
            //yes
            if (cachedResult != null)
                return cachedResult.Result;
            
            var clone = GetOverlayNoCache(forImage, overlayKind);

            //and cache it
            var newCacheEntry = new CachedOverlayResult(overlayKind, clone);
            _resultCache[forImage].Add(newCacheEntry);

            return newCacheEntry.Result;
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
                var clone = (Image)forImage.CloneAs<Rgba32>();

                var graphics = Graphics.FromImage(clone);
                graphics.DrawImage(customOverlay, new Rectangle(0, 0, clone.Width, clone.Height));

                //and cache it
                _resultCacheCustom[forImage].Add(customOverlay,clone);
                
            }

            //now it is cached for sure
            return _resultCacheCustom[forImage][customOverlay];
        }

        public Image GetGrayscale(Image forImage)
        {
            if (!_greyscaleCache.ContainsKey(forImage))
                _greyscaleCache.Add(forImage, MakeGrayscale(forImage));

            return _greyscaleCache[forImage];
        }

        /// <summary>
        /// From https://stackoverflow.com/a/2265990/4824531
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        private static Image MakeGrayscale(Image original)
        {
            //create a blank Image the same size as original
            Image newBitmap = new Image<Rgba32>(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newImage);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][] 
      {
         new float[] {.3f, .3f, .3f, 0, 0},
         new float[] {.59f, .59f, .59f, 0, 0},
         new float[] {.11f, .11f, .11f, 0, 0},
         new float[] {0, 0, 0, 1, 0},
         new float[] {0, 0, 0, 0, 1}
      });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        public Image GetOverlayNoCache(Image forImage, OverlayKind overlayKind)
        {
            //cached result does not exist so we must draw it
            var overlay = _images[overlayKind];

            var clone = (Image)forImage.CloneAs<Rgba32>();
            
            var graphics = Graphics.FromImage(clone);
            graphics.DrawImage(overlay, new Rectangle(0, 0, clone.Width, clone.Height));

            return clone;
        }
    }
}
