using System;
using System.Drawing;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.Icons.IconOverlays;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class HICProjectDirectoryStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private IconOverlayProvider _overlayProvider;
        private Bitmap _basicImage;

        public HICProjectDirectoryStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _overlayProvider = overlayProvider;
            _basicImage = CatalogueIcons.HICProjectDirectoryNode;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var node = o as HICProjectDirectoryNode;

            if (node == null)
                return null;

            if (node.IsEmpty)
                return _overlayProvider.GetOverlay(_basicImage, OverlayKind.Problem);

            return _basicImage;
        }
    }
}