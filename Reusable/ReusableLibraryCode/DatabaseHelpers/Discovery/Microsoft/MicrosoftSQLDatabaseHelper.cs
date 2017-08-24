using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft
{
    public class MicrosoftSQLDatabaseHelper: IDiscoveredDatabaseHelper
    {
        public IEnumerable<DiscoveredTable> ListTables(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, bool includeViews, DbTransaction transaction = null)
        {
            if (connection.State == ConnectionState.Closed)
                throw new InvalidOperationException("Expected connection to be open");

            List<DiscoveredTable> tables = new List<DiscoveredTable>();
            
            var cmd = new SqlCommand("use [" + database + "]; EXEC sp_tables", (SqlConnection)connection);
            cmd.Transaction = transaction as SqlTransaction;

            using (var r = cmd.ExecuteReader())
                while (r.Read())
                {
                    //its a system table
                    if (r["TABLE_OWNER"] as string == "sys")
                        continue;

                    if (r["TABLE_OWNER"] as string == "INFORMATION_SCHEMA")
                        continue;

                    //add views if we are including them
                    if (includeViews && r["TABLE_TYPE"].Equals("VIEW"))
                        tables.Add(new DiscoveredTable(parent,(string)r["TABLE_NAME"],querySyntaxHelper,(string) r["TABLE_OWNER"],TableType.View));

                    //add tables
                    if (r["TABLE_TYPE"].Equals("TABLE"))
                        tables.Add(new DiscoveredTable(parent, (string)r["TABLE_NAME"], querySyntaxHelper, (string)r["TABLE_OWNER"], TableType.Table));
                }
            
            return tables.ToArray();
        }

        public IEnumerable<DiscoveredTableValuedFunction> ListTableValuedFunctions(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, DbTransaction transaction = null)
        {
            List<DiscoveredTableValuedFunction> functionsToReturn = new List<DiscoveredTableValuedFunction>();

            DbCommand cmd = new SqlCommand("use [" + database + @"]; select name from sys.objects 
WHERE type_desc = 'SQL_TABLE_VALUED_FUNCTION' OR type_desc ='CLR_TABLE_VALUED_FUNCTION'", (SqlConnection)connection);

            cmd.Transaction = transaction;

            using (DbDataReader r = cmd.ExecuteReader())
                while (r.Read())
                    functionsToReturn.Add(new DiscoveredTableValuedFunction(parent,r["name"].ToString(),querySyntaxHelper));


            return functionsToReturn.ToArray();
        }
        
        public  DiscoveredStoredprocedure[] ListStoredprocedures(DbConnectionStringBuilder builder, string database)
        {
            List<DiscoveredStoredprocedure> toReturn = new List<DiscoveredStoredprocedure>();

            using (var con = new SqlConnection(builder.ConnectionString))
            {
                con.Open();
                SqlCommand cmdFindStoredprocedure = new SqlCommand("use [" + database + @"];  SELECT * FROM sys.procedures", con);

                var result = cmdFindStoredprocedure.ExecuteReader();

                while (result.Read())
                    toReturn.Add(new DiscoveredStoredprocedure((string)result["name"]));
            }

            return toReturn.ToArray();
        }

        public IDiscoveredTableHelper GetTableHelper()
        {
            return new MicrosoftSQLTableHelper();
        }

        public void DropDatabase(DiscoveredDatabase database)
        {
            bool userIsCurrentlyInDatabase = database.Server.GetCurrentDatabase().GetRuntimeName().Equals(database.GetRuntimeName());

            var serverConnectionBuilder = new SqlConnectionStringBuilder(database.Server.Builder.ConnectionString);
            if (userIsCurrentlyInDatabase)
                serverConnectionBuilder.InitialCatalog = "master";

            // Create a new server so we don't mutate database.Server and cause a whole lot of side-effects in other code, e.g. attachers
            var server = new DiscoveredServer(serverConnectionBuilder);
            var databaseToDrop = database.GetRuntimeName();

            string sql = "ALTER DATABASE [" + databaseToDrop + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE" + Environment.NewLine;
            sql += "DROP DATABASE [" + databaseToDrop + "]";

            using (var con = (SqlConnection) server.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(sql, con);
                cmd.ExecuteNonQuery();
            }
        }

        public Dictionary<string, string> DescribeDatabase(DbConnectionStringBuilder builder, string database)
        {
            using (var con = new SqlConnection(builder.ConnectionString))
            {
                con.Open();
                con.ChangeDatabase(database);
                SqlCommand cmd = new SqlCommand("exec sp_spaceused", con);

                DataSet ds = new DataSet();

                new SqlDataAdapter(cmd).Fill(ds);

                var toReturn = new Dictionary<string, string>();

                toReturn.Add(ds.Tables[0].Columns[0].ColumnName, ds.Tables[0].Rows[0][0].ToString());
                toReturn.Add(ds.Tables[0].Columns[1].ColumnName, ds.Tables[1].Rows[0][1].ToString());

                toReturn.Add(ds.Tables[1].Columns[0].ColumnName, ds.Tables[1].Rows[0][0].ToString());
                toReturn.Add(ds.Tables[1].Columns[1].ColumnName, ds.Tables[1].Rows[0][1].ToString());
                toReturn.Add(ds.Tables[1].Columns[2].ColumnName, ds.Tables[1].Rows[0][2].ToString());

                return toReturn;
            }
        }
    }
}
