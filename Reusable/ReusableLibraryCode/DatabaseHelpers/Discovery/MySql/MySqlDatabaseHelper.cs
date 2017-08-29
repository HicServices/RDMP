using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using MySql.Data.MySqlClient;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlDatabaseHelper : IDiscoveredDatabaseHelper
    {
        public IEnumerable<DiscoveredTableValuedFunction> ListTableValuedFunctions(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper,
            DbConnection connection, string database, DbTransaction transaction = null)
        {
            return Enumerable.Empty<DiscoveredTableValuedFunction>();
        }

        public DiscoveredStoredprocedure[] ListStoredprocedures(DbConnectionStringBuilder builder, string database)
        {
            throw new NotImplementedException();
        }

        public IDiscoveredTableHelper GetTableHelper()
        {
            return new MySqlTableHelper();
        }

        public void DropDatabase(DiscoveredDatabase database)
        {
            using (var con = (MySqlConnection) database.Server.GetConnection())
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand("DROP DATABASE " + database.GetRuntimeName(),con);
                cmd.ExecuteNonQuery();
            }
        }

        public Dictionary<string, string> DescribeDatabase(DbConnectionStringBuilder builder, string database)
        {
            throw new NotImplementedException();
        }



        public IEnumerable<DiscoveredTable> ListTables(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, bool includeViews, DbTransaction transaction = null)
        {
            if (connection.State == ConnectionState.Closed)
                throw new InvalidOperationException("Expected connection to be open");

            List<DiscoveredTable> tables = new List<DiscoveredTable>();

            var cmd = new MySqlCommand("SHOW TABLES in " + database, (MySqlConnection) connection);
            cmd.Transaction = transaction as MySqlTransaction;

            var r = cmd.ExecuteReader();
            while (r.Read())
                tables.Add(new DiscoveredTable(parent,r[0] as string,querySyntaxHelper));//this table fieldname will be something like Tables_in_mydbwhatevernameitis
            
            return tables.ToArray();
        }

    }
}