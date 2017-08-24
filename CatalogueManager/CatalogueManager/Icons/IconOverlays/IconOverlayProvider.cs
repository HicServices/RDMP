using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CatalogueManager.Icons.IconProvision;

namespace CatalogueManager.Icons.IconOverlays
{
    public class IconOverlayProvider
    {
        readonly Dictionary<Bitmap,List<CachedOverlayResult>> _resultCache = new Dictionary<Bitmap, List<CachedOverlayResult>>();

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
