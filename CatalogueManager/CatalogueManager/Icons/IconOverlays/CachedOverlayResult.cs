using System.Drawing;

namespace CatalogueManager.Icons.IconOverlays
{
    /// <summary>
    /// Do not draw on these Bitmaps once you have created them, this will corrupt the image for future users of the cache
    /// </summary>
    public class CachedOverlayResult
    {
        public OverlayKind Kind { get; private set; }
        public Bitmap Result { get; private set; }

        public CachedOverlayResult(OverlayKind kind, Bitmap result)
        {
            Kind = kind;
            Result = result;
        }
    }
}
