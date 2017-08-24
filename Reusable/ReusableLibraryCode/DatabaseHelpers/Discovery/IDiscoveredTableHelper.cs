using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public interface IDiscoveredTableHelper
    {
        string GetTopXSqlForTable(IHasFullyQualifiedNameToo table, int topX);

        DiscoveredColumn[] DiscoverColumns(DiscoveredTable discoveredTable, IManagedConnection connection, string database, string tableName);
        DiscoveredColumn[] DiscoverColumns(DiscoveredTableValuedFunction discoveredTableValuedFunction, IManagedConnection connection, string database, string tableName);

        IDiscoveredColumnHelper GetColumnHelper();
        
        void DropTable(DbConnection connection, DiscoveredTable tableToDrop, DbTransaction dbTransaction = null);
        void DropFunction(DbConnection connection, DiscoveredTableValuedFunction functionToDrop, DbTransaction dbTransaction);

        void DropColumn(DbConnection connection, DiscoveredTable discoveredTable, DiscoveredColumn columnToDrop,DbTransaction dbTransaction);

        int GetRowCount(DbConnection connection, IHasFullyQualifiedNameToo table, DbTransaction dbTransaction = null);
        string WrapStatementWithIfTableExistanceMatches(bool existanceDesiredForExecution, StringLiteralSqlInContext bodySql, string tableName);

        DiscoveredParameter[] DiscoverTableValuedFunctionParameters(DbConnection connection, DiscoveredTableValuedFunction discoveredTableValuedFunction, DbTransaction transaction);
        
    }
}
