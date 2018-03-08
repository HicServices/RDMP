using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Triggers;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Migration;
using log4net;
using MapsDirectlyToDatabaseTable;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Column = Microsoft.SqlServer.Management.Smo.Column;

namespace DataLoadEngine.DatabaseManagement
{
    
    /// <summary>
    /// SMO (Microsoft.SqlServer.Management.Smo) powered class for scripting tables, creating constraint free copies (e.g. RAW bubble) etc.  This is
    /// Microsoft only stuff (as opposed to the ReusableLibraryCode.DatabaseHelpers.Discovery namespace).  The class powers the data load engine.
    /// </summary>
    [Obsolete("This functionality should be ported to ReusableLibraryCode.DatabaseHelpers.Discovery namespace and made non Microsoft / SMO specific")]
    public class DatabaseOperations
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (DatabaseOperations));

        public static bool CheckTableExists(string tableName, DiscoveredDatabase dbInfo)
        {
            using (var conn = (SqlConnection)dbInfo.Server.GetConnection())
            {
                conn.Open();
                var cmd =
                    new SqlCommand(
                        "SELECT CASE WHEN EXISTS (SELECT TABLE_NAME FROM [" + dbInfo.GetRuntimeName() +
                        "].INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + tableName + "') THEN 1 ELSE 0 END", conn);
                try
                {
                    return (int)cmd.ExecuteScalar() == 1;
                }
                catch (Exception e)
                {
                    throw new Exception("CheckDatabaseExistsOnServer: Could not check if '" + dbInfo.GetRuntimeName() + "' exists: " + e);
                }
            }
        }

        public static bool CheckTablesAreEmptyInDatabaseOnServer(DiscoveredDatabase dbInfo)
        {
            using (var conn = (SqlConnection)dbInfo.Server.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("USE [" + dbInfo.GetRuntimeName() + "]; SELECT " +
                                         "t.name table_name, " +
                                         "sum(p.rows) total_rows " +
                                         "FROM " +
                                         "sys.tables t " +
                                         "JOIN sys.partitions p ON (t.object_id = p.object_id) " +
                                         "WHERE p.index_id IN (0,1) " +
                                         "GROUP BY t.name " +
                                         "HAVING SUM(p.rows) != 0;", conn);
                SqlDataReader result;
                try
                {
                    result = cmd.ExecuteReader();
                }
                catch (SqlException exception)
                {
                    Log.Fatal("Could not run the command to check if tables are empty", exception);
                    throw new Exception("Could not run the command to check if tables are empty: " + exception);
                }

                return !result.HasRows;
            }
        }

        public static void CreateTableInDatabase(string tableName, string columnDefinitions, DiscoveredDatabase database)
        {
            using (var conn = (SqlConnection)database.Server.GetConnection())
            {
                conn.Open();

                var sql = "CREATE TABLE [" + database.GetRuntimeName() + "]..[" + tableName + "] (" + columnDefinitions + ")";
                new SqlCommand(sql, conn).ExecuteNonQuery();
            }
        }

        public static ScriptingOptions GetScriptingOptionsForDeploymentStage(LoadBubble stage)
        {
            var options = GetDefaultScriptingOptions();

            switch (stage)
            {
                case LoadBubble.Raw:
                    options.DriAll = false;
                    options.DriAllConstraints = false;
                    options.DriPrimaryKey = false;
                    options.DriIndexes = false;
                    options.DriChecks = false;
                    options.NoFileGroup = true;
                    options.DriAllKeys = false;
                    options.DriDefaults = true;
                    options.Default = true;
                    options.Bindings = true;
                    options.DriUniqueKeys = false;
                    break;
                case LoadBubble.Staging:
                    options.DriAll = false;
                    options.DriAllConstraints = false;

                    options.DriAllKeys = false;
                    options.DriPrimaryKey = true;
                    
                    options.DriIndexes = true;
                    options.NoFileGroup = true;

                    //foreign key constraints (which could include keys into other tables that we don't want to script)
                    options.DriChecks = false;
                    options.DriUniqueKeys = false;
                    options.DriForeignKeys = false;
                    options.SchemaQualifyForeignKeysReferences = false;

                    //defaults
                    options.DriDefaults = true;
                    options.Default = true;
                    options.Bindings = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("No scripting option configuration for stage: " + stage);
            }

            return options;
        }

        public static void CloneDatabaseTables(DiscoveredDatabase srcDatabaseInfo, DiscoveredDatabase destDatabaseInfo, Dictionary<string, string> tableMappings, bool dropHICColumns, bool dropNotNullConstraints, ScriptingOptions options = null)
        {
            var srcServer = CreateSMOServer(srcDatabaseInfo);
            var srcDatabase = srcServer.Databases[srcDatabaseInfo.GetRuntimeName()];

            var destServer = CreateSMOServer(destDatabaseInfo);
            var destDatabase = destServer.Databases[destDatabaseInfo.GetRuntimeName()];


            if (options == null && dropNotNullConstraints)
            {
                options = GetDefaultScriptingOptions();
                options.DriAllConstraints = false;
                options.DriPrimaryKey = false;
                options.DriIndexes = false;
                options.DriChecks = false;
                options.NoIdentities = true;
            }

            if (tableMappings != null && tableMappings.Any(t => !srcDatabase.Tables.Contains(t.Key)))
                throw new Exception("Explicitly stated table(s) " +
                                    String.Join(", ", tableMappings.Where(t => !srcDatabase.Tables.Contains(t.Key)).Select(pair => pair.Key)) + " were not found in the source database" + srcDatabase.Name + " on " + srcDatabase.Parent.Name);

            // Script each of the tables, observing whether specific tables have been requested through the 'tableMappings'
            foreach (Table table in srcDatabase.Tables)
            {
                if (tableMappings == null || tableMappings.ContainsKey(table.Name))
                {
                    var scriptOfTable = ScriptTableSchema(table, options);
                    foreach (var sql in scriptOfTable)
                    {
                        string mutatedSql = sql;

                        // Hacky modification of constraints SQL
                        if (dropNotNullConstraints)
                        {
                            if (sql.Contains("CREATE"))
                                mutatedSql = mutatedSql.Replace("NOT NULL", "NULL");
                        }

                        destDatabase.ExecuteNonQuery(mutatedSql);
                    }
                }
            }

            // Drop HIC columns if required
            foreach (Table table in destDatabase.Tables)
            {
                List<Column> toDrop = new List<Column>();

                foreach (Column column in table.Columns)
                {
                    if (dropHICColumns && column.Name.StartsWith("hic_", StringComparison.CurrentCultureIgnoreCase))
                        toDrop.Add(column);
                    
                    if (column.Identity)
                        toDrop.Add(column);
                }

                foreach (var column in toDrop)
                    column.Drop();
            }

            // If we have a list of table name mappings, rename the requested tables
            if (tableMappings != null)
                foreach (var mapping in tableMappings)
                {
                    var table = destDatabase.Tables[mapping.Key];
                    table.Rename(mapping.Value);
                }
        }

        public static void CloneTable(DiscoveredDatabase srcDatabaseInfo, DiscoveredDatabase destDatabaseInfo, string srcTableName, string destTableName, bool dropHICColumns, bool dropIdentityColumns, bool allowNulls, ScriptingOptions options, Dictionary<string, DataType> columnAlterDictionary)
        {
            var srcServer = CreateSMOServer(srcDatabaseInfo);
            var srcDatabase = srcServer.Databases[srcDatabaseInfo.GetRuntimeName()];

            var destServer = CreateSMOServer(destDatabaseInfo);
            var destDatabase = destServer.Databases[destDatabaseInfo.GetRuntimeName()];

            if(srcDatabase == null)
                throw new Exception("Database " + srcDatabaseInfo.GetRuntimeName() + " not found on server " + srcDatabaseInfo.Server.Name);

            if (destDatabase == null)
                throw new Exception("Database " + destDatabaseInfo.GetRuntimeName() + " not found on server " + destDatabaseInfo.Server.Name);

            if (!srcDatabase.Tables.Contains(srcTableName))
                throw new Exception("Table " + srcTableName + " does not exist on " + srcDatabase.Name);

            var srcTable = srcDatabase.Tables[srcTableName];

            var sql = ScriptTableSchema(srcTable, options);


            using (var con = destDatabaseInfo.Server.GetConnection())
            {
                con.Open();
                foreach (string s in sql)
                    UsefulStuff.ExecuteBatchNonQuery(s, con);
            }
            
            Table finalTable;
            destDatabase.Refresh();

            if (!srcTableName.Equals(destTableName))
            {
                var newTable = destDatabase.Tables[srcTableName];
                
                if (newTable == null)
                    throw new Exception("Table '" + srcTableName + "' not found in " + destDatabase.Name + " on " + destDatabase.Parent.Name);

                newTable.Rename(destTableName);
                finalTable = newTable;

            }
            else
            {
                finalTable = destDatabase.Tables[destTableName];
            }
            
            foreach (Column column in finalTable.Columns)
            {

                if (column.Identity && dropIdentityColumns)
                    column.MarkForDrop(true);
                if(column.Name.StartsWith("hic_") && dropHICColumns)
                    column.MarkForDrop(true);

                //override nullability to true if flag is set
                if (column.Nullable == false && allowNulls)
                    column.Nullable = true;

                //drop the data load run ID field and validFrom fields, we don't need them in STAGING or RAW, it will be hard coded in the MERGE migration with a fixed value anyway.
                if(column.Name.Equals(SpecialFieldNames.DataLoadRunID) || column.Name.Equals(SpecialFieldNames.ValidFrom))
                    column.MarkForDrop(true);

                if (columnAlterDictionary.ContainsKey(column.Name))
                    column.DataType = columnAlterDictionary[column.Name];


            }

            finalTable.Alter();
        }

        private static StringCollection ScriptTableSchema(Table table, ScriptingOptions options = null)
        {
            if(options == null)
                options = GetDefaultScriptingOptions();
            return table.Script(options);
        }

     

        private static ScriptingOptions GetDefaultScriptingOptions()
        {
            return new ScriptingOptions
            {
                ScriptData = false,
                IncludeDatabaseContext = false
            };
        }

        public static IEnumerable<StringCollection> ScriptTableSchema(DiscoveredDatabase dbInfo, string tableName, bool includeDatabaseContext,bool includePrimaryKeys)
        {
            var server = CreateSMOServer(dbInfo);
            var database = server.Databases[dbInfo.GetRuntimeName()];

            var scriptOptions = new ScriptingOptions
            {
                ScriptData = false,
                IncludeDatabaseContext = includeDatabaseContext,
                DriPrimaryKey = includePrimaryKeys,
                NoFileGroup = true
                
            };

            foreach (Table table in database.Tables)
                if(table.Name.Equals(tableName))
                    yield return ScriptTableSchema(table, scriptOptions);
        }

        public static IEnumerable<StringCollection> ScriptAllTableSchemas(DiscoveredDatabase dbInfo, bool includeDatabaseContext = true)
        {
            var server = CreateSMOServer(dbInfo);
            var database = server.Databases[dbInfo.GetRuntimeName()];

            var scriptOptions = new ScriptingOptions
            {
                ScriptData = false,
                IncludeDatabaseContext = includeDatabaseContext
            };

            foreach (Table table in database.Tables)
                  yield return ScriptTableSchema(table,scriptOptions);
        }

        public static DataTable CreateDataTableFromDbOnServer(DiscoveredDatabase dbInfo, string tableName)
        {
            var dt = new DataTable();
            var table = dbInfo.ExpectTable(tableName);

            if(!table.Exists())
                throw new Exception("Could not find table " + tableName + " on database " + dbInfo);

            using (var conn = (SqlConnection)dbInfo.Server.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT TOP 0 * FROM "+ table.GetFullyQualifiedName(), conn);
                dt.Load(cmd.ExecuteReader());
            }
            return dt;
        }
        


        public static bool DoesColumnHaveDefault(DiscoveredTable table, string columnName)
        {
            string query = "USE " + table.Database.GetRuntimeName() + @"; 
SELECT object_definition(default_object_id) AS definition
FROM   sys.columns
WHERE  name      ='" +columnName+@"'
AND    object_id = object_id('" + table.GetRuntimeName()+ "');";
            try
            {
                using (var conn = (SqlConnection)table.Database.Server.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(query, conn);
                    object result = cmd.ExecuteScalar();
                    
                    if (result == DBNull.Value)
                        return false;

                    return !string.IsNullOrWhiteSpace(result as string);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to check default of column " + columnName + ": " + e);
            }
        }

        public static bool CheckTableContainsColumns(DiscoveredDatabase dbInfo, string tableName, List<string> columnNamesToCheck)
        {
            var cols = dbInfo.ExpectTable(tableName).DiscoverColumns().ToArray();
            var missingColumns = columnNamesToCheck.Except(cols.Select(c => c.GetRuntimeName())).ToArray();

            if(missingColumns.Any())
                throw new Exception( dbInfo + " does not contain columns: " + String.Join(", ", missingColumns));

            return true;
        }


        public static SqlDataReader GetReaderForTableInDatabase(DiscoveredDatabase dbInfo, string tableName)
        {
            var conn = (SqlConnection)dbInfo.Server.GetConnection();
            
            conn.Open();
            var command = new SqlCommand("SELECT distinct * FROM " + tableName, conn);
            command.CommandTimeout = 50000;//distinct can take a long time for some big tables
                
            return  command.ExecuteReader();

        }


        public static SqlDataAdapter GetAdapterForTableInDatabase(DiscoveredDatabase dbInfo, string tableName)
        {
            var conn = (SqlConnection)dbInfo.Server.GetConnection();

            conn.Open();
            var command = new SqlCommand("SELECT distinct * FROM " + tableName, conn);
            command.CommandTimeout = 50000;//distinct can take a long time for some big tables

            return new SqlDataAdapter(command);
        }

        public static void AddColumnToTable(DiscoveredDatabase discoveredDatabase, string tableName, string desiredColumnName, string desiredColumnType)
        {
            using (var conn = (SqlConnection)discoveredDatabase.Server.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("Alter table " + tableName + " ADD [" + desiredColumnName + "] " + desiredColumnType ,conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }


        public static void DropColumnFromTable(DiscoveredDatabase server, string tableName, string columnName)
        {
            var todrop = server.ExpectTable(tableName).DiscoverColumn(columnName);
            server.ExpectTable(tableName).DropColumn(todrop);
        }

        private static Server CreateSMOServer(DiscoveredDatabase dbInfo)
        {
            return new Server(new ServerConnection((SqlConnection) dbInfo.Server.GetConnection()));
        }

        public static void RemoveTablesFromDatabase(IEnumerable<string> tableNames, DiscoveredDatabase dbInfo)
        {
            if (!IsNukable(dbInfo))
                throw new Exception("This method loops through every table in a database and nukes it! for obvious reasons this is only allowed on databases with a suffix _STAGING/_RAW");

            foreach (var tableName in tableNames)
                dbInfo.ExpectTable(tableName).Drop();
        }

        public static void RemoveTableFromDatabase(string tableName, DiscoveredDatabase dbInfo)
        {
            if (!IsNukable(dbInfo))
                throw new Exception("This method nukes a table in a database! for obvious reasons this is only allowed on databases with a suffix _STAGING/_RAW");

            dbInfo.ExpectTable(tableName).Drop();
        }

        private static bool IsNukable(DiscoveredDatabase dbInfo)
        {
            return dbInfo.GetRuntimeName().EndsWith("_STAGING") || dbInfo.GetRuntimeName().EndsWith("_RAW");
        }
    }
}
