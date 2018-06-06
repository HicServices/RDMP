using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

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

        public virtual void AddColumn(DiscoveredTable table, DbConnection connection, string name, string dataType, bool allowNulls,int timeout)
        {
            var cmd = table.Database.Server.GetCommand("ALTER TABLE " + table + " ADD " + name + " " + dataType + " " + (allowNulls ? "NULL" : "NOT NULL"), connection);
            cmd.CommandTimeout = timeout;
            cmd.ExecuteNonQuery();
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

        public string ScriptTableCreation(DiscoveredTable table, bool dropPrimaryKeys, bool dropNullability, bool convertIdentityToInt)
        {
            List<DatabaseColumnRequest> columns = new List<DatabaseColumnRequest>();

            foreach (DiscoveredColumn c in table.DiscoverColumns())
            {
                string sqlType = c.DataType.SQLType;

                if (c.DataType.IsIdentity() && convertIdentityToInt)
                    sqlType = "int";

                var colRequest = new DatabaseColumnRequest(c.GetRuntimeName(),sqlType , c.AllowNulls || dropNullability);
                colRequest.IsPrimaryKey = c.IsPrimaryKey && !dropPrimaryKeys;
                
                columns.Add(colRequest);
            }

            return table.Database.Helper.GetCreateTableSql(table.Database, table.GetRuntimeName(), columns.ToArray(),null,false);
        }

        public virtual bool IsEmpty(DbConnection connection, DiscoveredTable discoveredTable, DbTransaction transaction)
        {
            return GetRowCount(connection, discoveredTable, transaction) == 0;
        }

        public virtual void RenameTable(DiscoveredTable discoveredTable, string newName, IManagedConnection connection)
        {
            DbCommand cmd = DatabaseCommandHelper.GetCommand(GetRenameTableSql(discoveredTable, newName), connection.Connection, connection.Transaction);
            cmd.ExecuteNonQuery();
        }

        public virtual void CreatePrimaryKey(DiscoveredTable table, DiscoveredColumn[] discoverColumns, IManagedConnection connection)
        {
            string sql = string.Format("ALTER TABLE {0} ADD PRIMARY KEY ({1});",
                     table.GetFullyQualifiedName(),
                    string.Join(",", discoverColumns.Select(c => c.GetRuntimeName()))
                    );
            

            DbCommand cmd = DatabaseCommandHelper.GetCommand(sql,connection.Connection,connection.Transaction);
            cmd.ExecuteNonQuery();
        }

        protected abstract string GetRenameTableSql(DiscoveredTable discoveredTable, string newName);

        public abstract void MakeDistinct(DiscoveredTable discoveredTable);
    }
}