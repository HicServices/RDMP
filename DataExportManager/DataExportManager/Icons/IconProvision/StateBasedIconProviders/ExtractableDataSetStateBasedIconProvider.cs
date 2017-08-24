using System.Drawing;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using DataExportLibrary.Data.DataTables;

namespace DataExportManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ExtractableDataSetStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _normal;
        private Bitmap _disabled;

        public ExtractableDataSetStateBasedIconProvider()
        {
            _normal = CatalogueIcons.ExtractableDataSet;
            _disabled = CatalogueIcons.ExtractableDataSetDisabled;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var ds = o as ExtractableDataSet;
            if (ds == null)
                return null;
            
            return ds.IsCatalogueDeprecated || ds.DisableExtraction ? _disabled : _normal;
        }
    }
}