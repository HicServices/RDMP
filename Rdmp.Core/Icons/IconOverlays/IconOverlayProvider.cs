// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.Icons.IconOverlays
{
    public class IconOverlayProvider
    {
        readonly Dictionary<Bitmap,List<CachedOverlayResult>> _resultCache = new Dictionary<Bitmap, List<CachedOverlayResult>>();

        readonly Dictionary<Bitmap, Dictionary<Bitmap,Bitmap>>  _resultCacheCustom = new Dictionary<Bitmap, Dictionary<Bitmap, Bitmap>>();

        readonly Dictionary<Bitmap,Bitmap> _greyscaleCache = new Dictionary<Bitmap, Bitmap>();

        private readonly EnumImageCollection<OverlayKind> _images;

        public IconOverlayProvider()
        {
            _images = new EnumImageCollection<OverlayKind>(Overlays.ResourceManager);
        }

        public Bitmap GetOverlay(Bitmap forImage, OverlayKind overlayKind)
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


        public Bitmap GetOverlay(Bitmap forImage, Bitmap customOverlay)
        {
            //make sure the input image is added to the cache if it is novel
            if (!_resultCacheCustom.ContainsKey(forImage))
                _resultCacheCustom.Add(forImage, new Dictionary<Bitmap, Bitmap>());

            //is there a cached image for this overlay ?
            if (!_resultCacheCustom[forImage].ContainsKey(customOverlay))
            {
                //no

                //draw it
                var clone = (Bitmap)forImage.Clone();

                var graphics = Graphics.FromImage(clone);
                graphics.DrawImage(customOverlay, new Rectangle(0, 0, clone.Width, clone.Height));

                //and cache it
                _resultCacheCustom[forImage].Add(customOverlay,clone);
                
            }

            //now it is cached for sure
            return _resultCacheCustom[forImage][customOverlay];
        }

        public Bitmap GetGrayscale(Bitmap forImage)
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
        private static Bitmap MakeGrayscale(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

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

        public Bitmap GetOverlayNoCache(Bitmap forImage, OverlayKind overlayKind)
        {
            //cached result does not exist so we must draw it
            var overlay = _images[overlayKind];

            var clone = (Bitmap)forImage.Clone();

            var graphics = Graphics.FromImage(clone);
            graphics.DrawImage(overlay, new Rectangle(0, 0, clone.Width, clone.Height));

            return clone;
        }
    }
}
