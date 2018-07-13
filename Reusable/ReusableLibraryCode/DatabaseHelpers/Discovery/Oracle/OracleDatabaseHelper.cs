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

        public override string GetCreateTableSql(DiscoveredDatabase database, string tableName, DatabaseColumnRequest[] columns,
            Dictionary<DatabaseColumnRequest, DiscoveredColumn> foreignKeyPairs, bool cascadeDelete)
        {
            var bodySql = base.GetCreateTableSql(database, tableName, columns, foreignKeyPairs, cascadeDelete);


            var server = database.Server;
            var syntaxHelper = server.GetQuerySyntaxHelper();

            //the name sans brackets (hopefully they didn't pass any brackets)
            var tableRuntimeName = syntaxHelper.GetRuntimeName(tableName);
            var tableNameFullyQualified = database.ExpectTable(tableRuntimeName).GetFullyQualifiedName();
            
            var autoIncrementColumn = columns.SingleOrDefault(c => c.IsAutoIncrement);

            if (autoIncrementColumn == null)
                return bodySql;
            
            var sequenceName = string.Format("\"{0}\".\"{1}_{2}_seq\"", database.GetRuntimeName() , tableRuntimeName , autoIncrementColumn.ColumnName );
            var triggerName = string.Format("\"{0}\".\"{1}_bir\"",database.GetRuntimeName() ,tableRuntimeName);

            bodySql += "GO" + Environment.NewLine;

            bodySql += string.Format("CREATE SEQUENCE {0} START WITH 1",sequenceName) + Environment.NewLine;

            bodySql += "GO" + Environment.NewLine;

            bodySql += string.Format(
@"CREATE OR REPLACE TRIGGER {0} 
BEFORE INSERT ON {1} 
FOR EACH ROW

BEGIN
  SELECT {2}.NEXTVAL
  INTO   :new.{3}
  FROM   dual;
END;", triggerName, tableNameFullyQualified, sequenceName, autoIncrementColumn.ColumnName) + Environment.NewLine;

            return bodySql;

        }

        public override DirectoryInfo Detach(DiscoveredDatabase database)
        {
            throw new NotImplementedException();
        }

        public override void CreateBackup(DiscoveredDatabase discoveredDatabase, string backupName)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<DiscoveredTable> ListTables(DiscoveredDatabase parent, IQuerySyntaxHelper querySyntaxHelper, DbConnection connection, string database, bool includeViews, DbTransaction transaction = null)
        {
            List<DiscoveredTable> tables = new List<DiscoveredTable>();
            

            /*Maybe add this WHERE statement?
             * 
             * where owner not in 
(
'SYS','SYSTEM','MDSYS','OUTLN','CTXSYS','OLAPSYS','FLOWS_FILES','DVSYS','AUDSYS','DBSNMP','GSMADMIN_INTERNAL','OJVMSYS','ORDSYS','APPQOSSYS','XDB','ORDDATA','WMSYS','LBACSYS'
)*/

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
