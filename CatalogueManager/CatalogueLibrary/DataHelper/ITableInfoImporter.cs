using CatalogueLibrary.Data;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.DataHelper
{
    public interface ITableInfoImporter
    {
        void DoImport(out TableInfo tableInfoCreated, out ColumnInfo[] columnInfosCreated);
        ColumnInfo CreateNewColumnInfo(TableInfo parent, DiscoveredColumn discoveredColumn);
    }
}
