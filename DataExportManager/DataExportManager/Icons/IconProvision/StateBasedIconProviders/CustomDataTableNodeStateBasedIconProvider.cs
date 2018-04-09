using System.Drawing;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using DataExportLibrary.Providers.Nodes;

namespace DataExportManager.Icons.IconProvision.StateBasedIconProviders
{
    public class CustomDataTableNodeStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _normal;
        private Bitmap _disabled;

        public CustomDataTableNodeStateBasedIconProvider()
        {
            _normal = CatalogueIcons.CustomDataTableNode;
            _disabled = CatalogueIcons.CustomDataTableNotActive;
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            var node = o as CustomDataTableNode;
            if (node == null)
                return null;

            return node.Active ? _normal : _disabled;

        }
    }
}