using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public abstract class DiscoveredTableHelper :IDiscoveredTableHelper
    {
        public abstract string GetTopXSqlForTable(IHasFullyQualifiedNameToo table, int topX);

        public abstract DiscoveredColumn[] DiscoverColumns(DiscoveredTable discoveredTable, IManagedConnection connection, string database);
        
        public abstract IDiscoveredColumnHelper GetColumnHelper();
        public abstract void DropTable(DbConnection connection, DiscoveredTable tableToDrop);
        public abstract void DropFunction(DbConnection connection, DiscoveredTableValuedFunction functionToDrop);
        public abstract void DropColumn(DbConnection connection, DiscoveredColumn columnToDrop);

        public virtual void AddColumn(DiscoveredTable table, DbConnection connection, string name, string dataType, bool allowNulls,int timeout)
        {
            var cmd = table.Database.Server.GetCommand("ALTER TABLE " + table.GetFullyQualifiedName() + " ADD " + name + " " + dataType + " " + (allowNulls ? "NULL" : "NOT NULL"), connection);
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

        /// <inheritdoc/>
        public string ScriptTableCreation(DiscoveredTable table, bool dropPrimaryKeys, bool dropNullability, bool convertIdentityToInt, DiscoveredTable toCreateTable = null)
        {
            List<DatabaseColumnRequest> columns = new List<DatabaseColumnRequest>();

            foreach (DiscoveredColumn c in table.DiscoverColumns())
            {
                string sqlType = c.DataType.SQLType;

                if (c.IsAutoIncrement && convertIdentityToInt)
                    sqlType = "int";

                bool isToDifferentDatabaseType = toCreateTable != null && toCreateTable.Database.Server.DatabaseType != table.Database.Server.DatabaseType;


                //translate types
                if (isToDifferentDatabaseType)
                {
                    var fromtt = table.Database.Server.GetQuerySyntaxHelper().TypeTranslater;
                    var tott = toCreateTable.Database.Server.GetQuerySyntaxHelper().TypeTranslater;

                    sqlType = fromtt.TranslateSQLDBType(c.DataType.SQLType, tott);
                }

                var colRequest = new DatabaseColumnRequest(c.GetRuntimeName(),sqlType , c.AllowNulls || dropNullability);
                colRequest.IsPrimaryKey = c.IsPrimaryKey && !dropPrimaryKeys;

                //if there is a collation
                if (!string.IsNullOrWhiteSpace(c.Collation))
                {
                    //if the script is to be run on a database of the same type
                    if (toCreateTable == null || !isToDifferentDatabaseType)
                    {
                        //then specify that the column should use the live collation
                        colRequest.Collation = c.Collation;
                    }
                }

                columns.Add(colRequest);
            }

            var destinationTable = toCreateTable ?? table;

            return table.Database.Helper.GetCreateTableSql(destinationTable.Database, destinationTable.GetRuntimeName(), columns.ToArray(), null, false);
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
            string sql = string.Format("ALTER TABLE {0} ADD PRIMARY KEY ({1})",
                     table.GetFullyQualifiedName(),
                    string.Join(",", discoverColumns.Select(c => c.GetRuntimeName()))
                    );
            

            DbCommand cmd = DatabaseCommandHelper.GetCommand(sql,connection.Connection,connection.Transaction);
            cmd.ExecuteNonQuery();
        }

        public virtual int ExecuteInsertReturningIdentity(DiscoveredTable discoveredTable, DbCommand cmd, IManagedTransaction transaction=null)
        {
            cmd.CommandText += ";SELECT @@IDENTITY";

            var result = cmd.ExecuteScalar();

            if (result == DBNull.Value || result == null)
                return 0;

            return Convert.ToInt32(result);
        }

        protected abstract string GetRenameTableSql(DiscoveredTable discoveredTable, string newName);

        public virtual void MakeDistinct(DiscoveredTable discoveredTable)
        {
            var server = discoveredTable.Database.Server;

            //note to future developers, this method has horrible side effects e.g. column defaults might be recalculated, foreign key CASCADE Deletes might happen
            //to other tables we can help the user not make such mistakes with this check.
            if(discoveredTable.DiscoverColumns().Any(c => c.IsPrimaryKey))
                throw new NotSupportedException("Table "+discoveredTable+" has primary keys, why are you calling MakeDistinct on it!");

            var tableName = discoveredTable.GetFullyQualifiedName();
            var tempTable = discoveredTable.Database.ExpectTable(discoveredTable.GetRuntimeName() + "_DistinctingTemp").GetFullyQualifiedName();

            using (var con = server.BeginNewTransactedConnection())
            {
                var cmdDistinct = server.GetCommand(string.Format("CREATE TABLE {1} AS SELECT distinct * FROM {0}", tableName, tempTable), con);
                cmdDistinct.ExecuteNonQuery();

                var cmdTruncate = server.GetCommand(string.Format("DELETE FROM {0}", tableName), con);
                cmdTruncate.ExecuteNonQuery();

                var cmdBack = server.GetCommand(string.Format("INSERT INTO {0} (SELECT * FROM {1})", tableName, tempTable), con);
                cmdBack.ExecuteNonQuery();

                var cmdDropDistinctTable = server.GetCommand(string.Format("DROP TABLE {0}", tempTable), con);
                cmdDropDistinctTable.ExecuteNonQuery();
            }
        }
    }
}