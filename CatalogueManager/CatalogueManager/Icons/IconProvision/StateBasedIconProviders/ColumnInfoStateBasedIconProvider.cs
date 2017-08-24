using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ColumnInfoStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly IconOverlayProvider _overlayProvider;
        private Bitmap _columnInfo;
        private Bitmap _columnInfoWithANO;

        public ColumnInfoStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _overlayProvider = overlayProvider;
            _columnInfo = CatalogueIcons.ColumnInfo;
            _columnInfoWithANO = CatalogueIcons.ANOColumnInfo;
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            var columnInfo = o as ColumnInfo;

            if (columnInfo == null)
                return null;

            var basicIcon = columnInfo.ANOTable_ID != null ? _columnInfoWithANO : _columnInfo;

            if (columnInfo.IsPrimaryKey)
                return _overlayProvider.GetOverlay(basicIcon, OverlayKind.Key);
            
            return basicIcon;
        }
    }
}