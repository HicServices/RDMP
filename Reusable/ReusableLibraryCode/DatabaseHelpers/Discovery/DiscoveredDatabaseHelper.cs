using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public abstract class DiscoveredDatabaseHelper:IDiscoveredDatabaseHelper
    {
        public abstract IEnumerable<DiscoveredTable> ListTables(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection,
            string database, bool includeViews, DbTransaction transaction = null);

        public abstract IEnumerable<DiscoveredTableValuedFunction> ListTableValuedFunctions(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper,
            DbConnection connection, string database, DbTransaction transaction = null);

        public abstract DiscoveredStoredprocedure[] ListStoredprocedures(DbConnectionStringBuilder builder, string database);
        public abstract IDiscoveredTableHelper GetTableHelper();
        public abstract void DropDatabase(DiscoveredDatabase database);
        public abstract Dictionary<string, string> DescribeDatabase(DbConnectionStringBuilder builder, string database);

        public DiscoveredTable CreateTable(DiscoveredDatabase database, string tableName, DataTable dt, DatabaseColumnRequest[] explicitColumnDefinitions = null, bool createEmpty = false)
        {
            List<DatabaseColumnRequest> columns = new List<DatabaseColumnRequest>();
            List<DatabaseColumnRequest> customRequests = explicitColumnDefinitions != null
                ? explicitColumnDefinitions.ToList()
                : new List<DatabaseColumnRequest>();

            foreach (DataColumn column in dt.Columns)
            {
                //do we have an explicit overriding column definition?
                var overriding = customRequests.SingleOrDefault(c => c.ColumnName.Equals(column.ColumnName,StringComparison.CurrentCultureIgnoreCase));

                //yes
                if (overriding != null)
                {
                    columns.Add(overriding);
                    customRequests.Remove(overriding);
                }
                else
                {
                    //no, work out the column definition using a datatype computer
                    DataTypeComputer computer = new DataTypeComputer(column);
                    columns.Add(new DatabaseColumnRequest(column.ColumnName, computer.GetTypeRequest(), column.AllowDBNull) { IsPrimaryKey = dt.PrimaryKey.Contains(column)});
                }
            }

            var tbl = CreateTable(database, tableName, columns.ToArray(),null,false);

            //unless we are being asked to create it empty then upload the DataTable to it
            if(!createEmpty)
                tbl.BeginBulkInsert().Upload(dt);
            
            return tbl;
        }
        
        public DiscoveredTable CreateTable(DiscoveredDatabase database, string tableName, DatabaseColumnRequest[] columns, Dictionary<DatabaseColumnRequest, DiscoveredColumn> foreignKeyPairs,bool cascadeDelete)
        {
            string bodySql = GetCreateTableSql(database, tableName, columns, foreignKeyPairs, cascadeDelete);

            var server = database.Server;

            using(var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand(bodySql, con).ExecuteNonQuery();
            }
            
            return database.ExpectTable(tableName);
        }

        public virtual string GetCreateTableSql(DiscoveredDatabase database, string tableName, DatabaseColumnRequest[] columns, Dictionary<DatabaseColumnRequest, DiscoveredColumn> foreignKeyPairs, bool cascadeDelete)
        {
            string bodySql = "";

            var server = database.Server;
            var syntaxHelper = server.GetQuerySyntaxHelper();

            bodySql += "CREATE TABLE " + tableName + "(" + Environment.NewLine;

            foreach (var col in columns)
            {
                var datatype = col.GetSQLDbType(syntaxHelper.TypeTranslater);

                //add the column name and accompanying datatype
                bodySql += syntaxHelper.GetRuntimeName(col.ColumnName) + " " + datatype + (col.AllowNulls && !col.IsPrimaryKey ? " NULL" : " NOT NULL") + "," + Environment.NewLine;
            }

            var pks = columns.Where(c => c.IsPrimaryKey).ToArray();
            if (pks.Any())
                bodySql += GetPrimaryKeyDeclarationSql(tableName, pks);
            
            if (foreignKeyPairs != null)
            {
                bodySql += Environment.NewLine + GetForeignKeyConstraintSql(tableName, syntaxHelper, foreignKeyPairs, cascadeDelete) + Environment.NewLine;
            }

            bodySql = bodySql.TrimEnd('\r', '\n', ',');

            bodySql += ")" + Environment.NewLine;

            return bodySql;
        }

        private string GetForeignKeyConstraintSql(string tableName, IQuerySyntaxHelper syntaxHelper, Dictionary<DatabaseColumnRequest, DiscoveredColumn> foreignKeyPairs, bool cascadeDelete)
        {
            //@"    CONSTRAINT FK_PersonOrder FOREIGN KEY (PersonID) REFERENCES Persons(PersonID) on delete cascade";
            var otherTable = foreignKeyPairs.Values.Select(v => v.Table).Distinct().Single();

            string constraintName = "FK_" + tableName + "_" + otherTable;

            return string.Format(
@"CONSTRAINT {0} FOREIGN KEY ({1})
REFERENCES {2}({3}) {4}",
                                               constraintName,
                string.Join(",",foreignKeyPairs.Keys.Select(k=>syntaxHelper.GetRuntimeName(k.ColumnName))),
                otherTable.GetRuntimeName(),
                string.Join(",",foreignKeyPairs.Values.Select(v=>v.GetRuntimeName())),
                cascadeDelete ? " on delete cascade": ""
                );
        }

        public abstract DirectoryInfo Detach(DiscoveredDatabase database);

        public abstract void CreateBackup(DiscoveredDatabase discoveredDatabase, string backupName);

        protected virtual string GetPrimaryKeyDeclarationSql(string tableName, DatabaseColumnRequest[] pks)
        {
            return string.Format(" CONSTRAINT PK_{0} PRIMARY KEY ({1})",tableName,string.Join(",",pks.Select(c=>c.ColumnName))) + "," + Environment.NewLine;
        }
    }
}