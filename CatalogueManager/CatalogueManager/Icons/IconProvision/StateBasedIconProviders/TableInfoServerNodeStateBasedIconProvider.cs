using System;
using System.Drawing;
using CatalogueLibrary.Nodes;
using CatalogueManager.Icons.IconOverlays;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class TableInfoServerNodeStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly IconOverlayProvider _overlayProvider;
        private DatabaseTypeIconProvider _databaseTypeIconProvider;
        private Bitmap _serverNode;

        public TableInfoServerNodeStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _overlayProvider = overlayProvider;
            _databaseTypeIconProvider = new DatabaseTypeIconProvider();

            _serverNode = CatalogueIcons.TableInfoServerNode;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var node = o as TableInfoServerNode;

            if (node == null)
                return null;

            return _overlayProvider.GetOverlay(_serverNode, _databaseTypeIconProvider.GetOverlay(node.DatabaseType));
        }
    }
}