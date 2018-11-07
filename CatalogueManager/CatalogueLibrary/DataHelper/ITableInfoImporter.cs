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
        /// <summary>
        /// Creates references to all columns/tables found on the live database in the RDMP persistence database.
        /// </summary>
        /// <param name="tableInfoCreated"></param>
        /// <param name="columnInfosCreated"></param>
        void DoImport(out TableInfo tableInfoCreated, out ColumnInfo[] columnInfosCreated);

        /// <summary>
        /// For when a <paramref name="discoveredColumn"/> is not currently documented by an existing <see cref="ColumnInfo"/>
        /// in the <paramref name="parent"/>.  This method creates one.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="discoveredColumn"></param>
        /// <returns></returns>
        ColumnInfo CreateNewColumnInfo(TableInfo parent, DiscoveredColumn discoveredColumn);
    }
}
