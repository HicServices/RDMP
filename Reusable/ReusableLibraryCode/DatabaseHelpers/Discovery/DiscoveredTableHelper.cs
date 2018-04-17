using System.Data.Common;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

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

        public virtual void AddColumn(DiscoveredTable table, DbConnection connection, string name, string dataType, bool allowNulls)
        {
            table.Database.Server.GetCommand("ALTER TABLE " + table + " ADD " + name + " " + dataType + " " + (allowNulls ? "NULL" : "NOT NULL"), connection).ExecuteNonQuery();
        }

        public abstract int GetRowCount(DbConnection connection, IHasFullyQualifiedNameToo table, DbTransaction dbTransaction = null);

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

        public abstract void MakeDistinct(DiscoveredTable discoveredTable);
    }
}