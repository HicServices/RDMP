using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Contains all the DatabaseType specific implementation logic required by DiscoveredTable.
    /// </summary>
    public interface IDiscoveredTableHelper
    {
        string GetTopXSqlForTable(IHasFullyQualifiedNameToo table, int topX);

        DiscoveredColumn[] DiscoverColumns(DiscoveredTable discoveredTable, IManagedConnection connection, string database);

        IDiscoveredColumnHelper GetColumnHelper();
        
        void DropTable(DbConnection connection, DiscoveredTable tableToDrop);
        void DropFunction(DbConnection connection, DiscoveredTableValuedFunction functionToDrop);
        void DropColumn(DbConnection connection, DiscoveredColumn columnToDrop);

        void AddColumn(DiscoveredTable table, DbConnection connection, string name, string dataType, bool allowNulls,int timeout);

        int GetRowCount(DbConnection connection, IHasFullyQualifiedNameToo table, DbTransaction dbTransaction = null);

        DiscoveredParameter[] DiscoverTableValuedFunctionParameters(DbConnection connection, DiscoveredTableValuedFunction discoveredTableValuedFunction, DbTransaction transaction);

        IBulkCopy BeginBulkInsert(DiscoveredTable discoveredTable, IManagedConnection connection);
        
        void TruncateTable(DiscoveredTable discoveredTable);
        void MakeDistinct(DiscoveredTable discoveredTable);

        /// <inheritdoc cref="DiscoveredTable.ScriptTableCreation"/>
        string ScriptTableCreation(DiscoveredTable constraints, bool dropPrimaryKeys, bool dropNullability, bool convertIdentityToInt, DiscoveredTable toCreateTable = null);
        bool IsEmpty(DbConnection connection, DiscoveredTable discoveredTable, DbTransaction transaction);
        void RenameTable(DiscoveredTable discoveredTable, string newName, IManagedConnection connection);
        void CreatePrimaryKey(DiscoveredTable columns, DiscoveredColumn[] discoverColumns, IManagedConnection connection);
    }
}
