using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Triggers;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.DatabaseManagement
{
    
    /// <summary>
    /// SMO (Microsoft.SqlServer.Management.Smo) powered class for scripting tables, creating constraint free copies (e.g. RAW bubble) etc.  This is
    /// Microsoft only stuff (as opposed to the ReusableLibraryCode.DatabaseHelpers.Discovery namespace).  The class powers the data load engine.
    /// </summary>
    [Obsolete("This functionality should be ported to ReusableLibraryCode.DatabaseHelpers.Discovery namespace and made non Microsoft / SMO specific")]
    public class DatabaseOperations
    {
        public static void CloneTable(DiscoveredDatabase srcDatabaseInfo, DiscoveredDatabase destDatabaseInfo, string srcTableName, string destTableName, bool dropHICColumns, bool dropIdentityColumns, bool allowNulls,PreLoadDiscardedColumn[]  dillutionColumns)
        {
            DiscoveredTable discoveredSourceTable = srcDatabaseInfo.ExpectTable(srcTableName);
            
            if (!discoveredSourceTable.Exists())
                throw new Exception("Table " + srcTableName + " does not exist on " + srcDatabaseInfo);

            var sql = discoveredSourceTable.ScriptTableCreation(allowNulls, allowNulls, false /*False because we want to drop these columns entirely not just flip to int*/); 

            using (var con = destDatabaseInfo.Server.GetConnection())
            {
                con.Open();
                var cmd = destDatabaseInfo.Server.GetCommand(sql, con);
                cmd.ExecuteNonQuery();
            }

            //new table will start with the same name as the as the old scripted one
            DiscoveredTable newTable = destDatabaseInfo.ExpectTable(srcTableName);

            if (!newTable.Exists())
                throw new Exception("Table '" + srcTableName + "' not found in " + destDatabaseInfo + " despite running table creation SQL!");

            //if user wants to rename it
            if (!srcTableName.Equals(destTableName))
                newTable.Rename(destTableName);
            
            foreach (DiscoveredColumn column in newTable.DiscoverColumns())
            {
                bool drop = false;
                var colName = column.GetRuntimeName();

                if (column.DataType.IsIdentity())
                    drop = true;

                if (colName.StartsWith("hic_") && dropHICColumns)
                    drop = true;

                //drop the data load run ID field and validFrom fields, we don't need them in STAGING or RAW, it will be hard coded in the MERGE migration with a fixed value anyway.
                if (colName.Equals(SpecialFieldNames.DataLoadRunID) || colName.Equals(SpecialFieldNames.ValidFrom))
                    drop = true;

                var dillution = dillutionColumns.SingleOrDefault(c => c.GetRuntimeName().Equals(colName));

                if (dillution != null)
                    column.DataType.AlterTypeTo(dillution.Data_type);

                if(drop)
                    newTable.DropColumn(column);
            }
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
