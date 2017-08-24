using System.Drawing;
using CatalogueLibrary.Data;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class TableInfoStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _tableInfo;
        private Bitmap _tableInfoTableValuedFunction;

        public TableInfoStateBasedIconProvider()
        {
            _tableInfo = CatalogueIcons.TableInfo;
            _tableInfoTableValuedFunction = CatalogueIcons.TableInfoTableValuedFunction;
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            var tableInfo = o as TableInfo;

            if (tableInfo == null)
                return null;

            return tableInfo.IsTableValuedFunction ? _tableInfoTableValuedFunction : _tableInfo;
        }
    }
}