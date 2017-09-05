using System.Data.Common;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public abstract class DiscoveredTableHelper :IDiscoveredTableHelper
    {
        public abstract string GetTopXSqlForTable(IHasFullyQualifiedNameToo table, int topX);

        public abstract DiscoveredColumn[] DiscoverColumns(DiscoveredTable discoveredTable, IManagedConnection connection, string database,
            string tableName);

        public abstract DiscoveredColumn[] DiscoverColumns(DiscoveredTableValuedFunction discoveredTableValuedFunction,
            IManagedConnection connection, string database, string tableName);

        public abstract IDiscoveredColumnHelper GetColumnHelper();
        public abstract void DropTable(DbConnection connection, DiscoveredTable tableToDrop);
        public abstract void DropFunction(DbConnection connection, DiscoveredTableValuedFunction functionToDrop);
        public abstract void DropColumn(DbConnection connection, DiscoveredColumn columnToDrop);
        public abstract int GetRowCount(DbConnection connection, IHasFullyQualifiedNameToo table, DbTransaction dbTransaction = null);

        public abstract string WrapStatementWithIfTableExistanceMatches(bool existanceDesiredForExecution, StringLiteralSqlInContext bodySql,
            string tableName);

        public abstract DiscoveredParameter[] DiscoverTableValuedFunctionParameters(DbConnection connection, DiscoveredTableValuedFunction discoveredTableValuedFunction, DbTransaction transaction);

        public abstract IBulkCopy BeginBulkInsert(DiscoveredTable discoveredTable, IManagedConnection connection);

        public virtual void TruncateTable(DiscoveredTable discoveredTable)
        {
            var server = discoveredTable.Database.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand("TRUNCATE TABLE " + discoveredTable.GetFullyQualifiedName(), con).ExecuteNonQuery();
            }
        }
    }
}