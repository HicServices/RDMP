using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TableCreation;
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

        public DiscoveredTable CreateTable(CreateTableArgs args)
        {
            var typeDictionary = new Dictionary<string, DataTypeComputer>(StringComparer.CurrentCultureIgnoreCase);

            List<DatabaseColumnRequest> columns = new List<DatabaseColumnRequest>();
            List<DatabaseColumnRequest> customRequests = args.ExplicitColumnDefinitions != null
                ? args.ExplicitColumnDefinitions.ToList()
                : new List<DatabaseColumnRequest>();

            if(args.DataTable != null)
            {
//If we have a data table from which to create the table from
                foreach (DataColumn column in args.DataTable.Columns)
                {
                    //do we have an explicit overriding column definition?
                    DatabaseColumnRequest overriding = customRequests.SingleOrDefault(c => c.ColumnName.Equals(column.ColumnName,StringComparison.CurrentCultureIgnoreCase));

                    //yes
                    if (overriding != null)
                    {
                        columns.Add(overriding);
                        customRequests.Remove(overriding);

                        //Type reqeuested 
                        var request = overriding.TypeRequested;

                        //Type is for an explicit Type e.g. datetime
                        if(request == null)
                            if(!string.IsNullOrWhiteSpace(overriding.ExplicitDbType))
                            {
                                var tt = args.Database.Server.GetQuerySyntaxHelper().TypeTranslater;
                        
                                request = new DatabaseTypeRequest(
                                    tt.GetCSharpTypeForSQLDBType(overriding.ExplicitDbType),
                                    tt.GetLengthIfString(overriding.ExplicitDbType),
                                    tt.GetDigitsBeforeAndAfterDecimalPointIfDecimal(overriding.ExplicitDbType));
                            }
                            else
                                throw new Exception("explicitColumnDefinitions for column " + column + " did not contain either a TypeRequested or ExplicitDbType");
                    
                        typeDictionary.Add(overriding.ColumnName, new DataTypeComputer(request));
                    }
                    else
                    {
                        //no, work out the column definition using a datatype computer
                        DataTypeComputer computer = new DataTypeComputer(column);
                        typeDictionary.Add(column.ColumnName,computer);

                        columns.Add(new DatabaseColumnRequest(column.ColumnName, computer.GetTypeRequest(), column.AllowDBNull) { IsPrimaryKey = args.DataTable.PrimaryKey.Contains(column)});
                    }
                }
            }
            else
            {
                //If no DataTable is provided just use the explicitly requested columns
                columns = customRequests;
            }

            if(args.Adjuster != null)
                args.Adjuster.AdjustColumns(columns);

            //Get the table creation SQL
            string bodySql = GetCreateTableSql(args.Database, args.TableName, columns.ToArray(), args.ForeignKeyPairs, args.CascadeDelete, args.Schema);

            //connect to the server and send it
            var server = args.Database.Server;

            using (var con = server.GetConnection())
            {
                con.Open();

                UsefulStuff.ExecuteBatchNonQuery(bodySql, con);
            }

            //Get reference to the newly created table
            var tbl = args.Database.ExpectTable(args.TableName, args.Schema);

            //unless we are being asked to create it empty then upload the DataTable to it
            if(args.DataTable != null && !args.CreateEmpty)
                tbl.BeginBulkInsert().Upload(args.DataTable);

            args.OnTableCreated(typeDictionary);

            return tbl;
        }
        
        public virtual string GetCreateTableSql(DiscoveredDatabase database, string tableName, DatabaseColumnRequest[] columns, Dictionary<DatabaseColumnRequest, DiscoveredColumn> foreignKeyPairs, bool cascadeDelete, string schema)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException("Table name cannot be null", "tableName");

            string bodySql = "";

            var server = database.Server;
            var syntaxHelper = server.GetQuerySyntaxHelper();

            //the name sans brackets (hopefully they didn't pass any brackets)
            tableName = syntaxHelper.GetRuntimeName(tableName);

            //the name uflly specified e.g. [db]..[tbl] or `db`.`tbl` - See Test HorribleColumnNames
            var fullyQualifiedName = syntaxHelper.EnsureFullyQualified(database.GetRuntimeName(), schema, tableName);

            bodySql += "CREATE TABLE " + fullyQualifiedName + "(" + Environment.NewLine;

            foreach (var col in columns)
            {
                var datatype = col.GetSQLDbType(syntaxHelper.TypeTranslater);
                
                //add the column name and accompanying datatype
                bodySql += string.Format("{0} {1} {2} {3} {4} {5},"+ Environment.NewLine,
                    syntaxHelper.EnsureWrapped(col.ColumnName),
                    datatype,
                    col.Default != MandatoryScalarFunctions.None ? "default " + syntaxHelper.GetScalarFunctionSql(col.Default):"",
                    string.IsNullOrWhiteSpace(col.Collation) ?"": "COLLATE " + col.Collation,
                    col.AllowNulls && !col.IsPrimaryKey ? " NULL" : " NOT NULL",
                    col.IsAutoIncrement ? syntaxHelper.GetAutoIncrementKeywordIfAny():""
                    );
            }

            var pks = columns.Where(c => c.IsPrimaryKey).ToArray();
            if (pks.Any())
                bodySql += GetPrimaryKeyDeclarationSql(tableName, pks,syntaxHelper);
            
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
            
            string constraintName = MakeSaneConstraintName("FK_", tableName + "_" + otherTable);

            return string.Format(
@"CONSTRAINT {0} FOREIGN KEY ({1})
REFERENCES {2}({3}) {4}",
                                               constraintName,
                string.Join(",",foreignKeyPairs.Keys.Select(k=>syntaxHelper.EnsureWrapped(k.ColumnName))),
                otherTable.GetFullyQualifiedName(),
                string.Join(",",foreignKeyPairs.Values.Select(v=>syntaxHelper.EnsureWrapped(v.GetRuntimeName()))),
                cascadeDelete ? " on delete cascade": ""
                );
        }

        public abstract DirectoryInfo Detach(DiscoveredDatabase database);

        public abstract void CreateBackup(DiscoveredDatabase discoveredDatabase, string backupName);

        protected virtual string GetPrimaryKeyDeclarationSql(string tableName, DatabaseColumnRequest[] pks, IQuerySyntaxHelper syntaxHelper)
        {
            var constraintName = MakeSaneConstraintName("PK_", tableName);

            return string.Format(" CONSTRAINT {0} PRIMARY KEY ({1})", constraintName, string.Join(",", pks.Select(c => syntaxHelper.EnsureWrapped(c.ColumnName)))) + "," + Environment.NewLine;
        }

        private string MakeSaneConstraintName(string prefix, string tableName)
        {
            var constraintName = QuerySyntaxHelper.MakeHeaderNameSane(tableName);

            if (string.IsNullOrWhiteSpace(constraintName))
            {
                Random r = new Random();
                constraintName = "Constraint" + r.Next(10000);
            }

            return prefix + constraintName;
        }
    }
}