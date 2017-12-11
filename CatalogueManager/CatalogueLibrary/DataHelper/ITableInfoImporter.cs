using CatalogueLibrary.Data;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.DataHelper
{
    /// <summary>
    /// Shared interface for the two classes which create TableInfos from tables (namely TableInfoImporter and TableValuedFunctionImporter).  A TableInfo the RDMP class that
    /// documents a persistent reference to a querable table (See TableInfo).
    /// </summary>
    public interface ITableInfoImporter
    {
        void DoImport(out TableInfo tableInfoCreated, out ColumnInfo[] columnInfosCreated);
        ColumnInfo CreateNewColumnInfo(TableInfo parent, DiscoveredColumn discoveredColumn);
    }
}
