using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle
{
    public class OracleDatabaseHelper : IDiscoveredDatabaseHelper
    {
        public IDiscoveredTableHelper GetTableHelper()
        {
            return new OracleTableHelper();
        }

        public void  DropDatabase(DiscoveredDatabase database)
        {
            //todo should I be worrying about dropping a database I am currently using (where server is pointed at)
            var cmd = new OracleCommand("DROP USER " + database.GetRuntimeName() + " CASCADE ", (OracleConnection)database.Server.GetConnection());
            cmd.ExecuteNonQuery();
        }

        public Dictionary<string, string> DescribeDatabase(DbConnectionStringBuilder builder, string database)
        {
            throw new NotImplementedException();
        }
        
        public IEnumerable<DiscoveredTable> ListTables(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, bool includeViews, DbTransaction transaction = null)
        {
            List<DiscoveredTable> tables = new List<DiscoveredTable>();
            
            var cmd = new OracleCommand("SELECT table_name FROM dba_tables where owner='" + database + "'", (OracleConnection) connection);
            cmd.Transaction = transaction as OracleTransaction;

            var r = cmd.ExecuteReader();

            while (r.Read())
                tables.Add(new DiscoveredTable(parent,r["table_name"].ToString(),querySyntaxHelper));

            return tables.ToArray();
        }

        public IEnumerable<DiscoveredTableValuedFunction> ListTableValuedFunctions(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper,
            DbConnection connection, string database, DbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }

        public string[] ListTableValuedFunctions(DbConnectionStringBuilder builder, string database)
        {
            return new string[0];
        }


        public DiscoveredStoredprocedure[] ListStoredprocedures(DbConnectionStringBuilder builder, string database)
        {
            throw new NotImplementedException();
        }


    }
}