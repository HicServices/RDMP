using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle
{
    public class OracleDatabaseHelper : DiscoveredDatabaseHelper
    {
        public override IDiscoveredTableHelper GetTableHelper()
        {
            return new OracleTableHelper();
        }

        public override void DropDatabase(DiscoveredDatabase database)
        {
             using(var con = (OracleConnection)database.Server.GetConnection())
             {
                 con.Open();
                 var cmd = new OracleCommand("DROP USER " + database.GetRuntimeName() + " CASCADE ",con);
                 cmd.ExecuteNonQuery();
             }
        }

        public override Dictionary<string, string> DescribeDatabase(DbConnectionStringBuilder builder, string database)
        {
            throw new NotImplementedException();
        }

        public override DirectoryInfo Detach(DiscoveredDatabase database)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<DiscoveredTable> ListTables(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, bool includeViews, DbTransaction transaction = null)
        {
            List<DiscoveredTable> tables = new List<DiscoveredTable>();
            
            var cmd = new OracleCommand("SELECT table_name FROM all_tables where owner='" + database + "'", (OracleConnection) connection);
            cmd.Transaction = transaction as OracleTransaction;

            var r = cmd.ExecuteReader();

            while (r.Read())
                tables.Add(new DiscoveredTable(parent,r["table_name"].ToString(),querySyntaxHelper));

            return tables.ToArray();
        }

        public override IEnumerable<DiscoveredTableValuedFunction> ListTableValuedFunctions(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper,
            DbConnection connection, string database, DbTransaction transaction = null)
        {
            return new DiscoveredTableValuedFunction[0];
        }
        
        public override DiscoveredStoredprocedure[] ListStoredprocedures(DbConnectionStringBuilder builder, string database)
        {
            return new DiscoveredStoredprocedure[0];
        }


    }
}