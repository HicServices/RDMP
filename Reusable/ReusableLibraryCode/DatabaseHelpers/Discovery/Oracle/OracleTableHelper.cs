using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using ReusableLibraryCode.DatabaseHelpers.Discovery.MySql;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle
{
    public class OracleTableHelper : DiscoveredTableHelper
    {

        public override string GetTopXSqlForTable(IHasFullyQualifiedNameToo table, int topX)
        {
            return "SELECT * FROM " + table.GetFullyQualifiedName() + " WHERE ROWNUM <= " + topX;
        }

        public override DiscoveredColumn[] DiscoverColumns(DiscoveredTable discoveredTable, IManagedConnection connection, string database, string tableName)
        {
            List<DiscoveredColumn> columns = new List<DiscoveredColumn>();

            

                DbCommand cmd = DatabaseCommandHelper.GetCommand(@"SELECT *
FROM   all_tab_cols
WHERE  table_name = :table_name
", connection.Connection);
                cmd.Transaction = connection.Transaction;

                DbParameter p = new OracleParameter("table_name", OracleDbType.Varchar2);
                p.Value = tableName;
                cmd.Parameters.Add(p);

                using (var r = cmd.ExecuteReader())
                {
                    if (!r.HasRows)
                        throw new Exception("Could not find any columns for table " + tableName +
                                            " in database " + database);

                    while (r.Read())
                    {

                        var toAdd = new DiscoveredColumn(discoveredTable, (string)r["COLUMN_NAME"], r["NULLABLE"].ToString() != "N") { Format = r["CHARACTER_SET_NAME"] as string };
                        toAdd.DataType = new DiscoveredDataType(r, GetSQLType_From_all_tab_cols_Result(r), toAdd);
                        columns.Add(toAdd);
                    }

                }

                //get primary key information 
                cmd = new OracleCommand(@"SELECT cols.table_name, cols.column_name, cols.position, cons.status, cons.owner
FROM all_constraints cons, all_cons_columns cols
WHERE cols.table_name = :table_name
AND cons.constraint_type = 'P'
AND cons.constraint_name = cols.constraint_name
AND cons.owner = cols.owner
ORDER BY cols.table_name, cols.position", (OracleConnection) connection.Connection);
                cmd.Transaction = connection.Transaction;


                p = new OracleParameter("table_name",OracleDbType.Varchar2);
                p.Value = tableName;
                cmd.Parameters.Add(p);


                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        columns.Single(c => c.GetRuntimeName().Equals(r["COLUMN_NAME"])).IsPrimaryKey = true;//mark all primary keys as primary
                }

                return columns.ToArray();
        }


        public override IDiscoveredColumnHelper GetColumnHelper()
        {
            return new OracleColumnHelper();
        }

        public override void DropTable(DbConnection connection, DiscoveredTable table)
        {
            var cmd = new OracleCommand("DROP TABLE " +table.GetFullyQualifiedName(), (OracleConnection)connection);
            cmd.ExecuteNonQuery();
        }

        public override void DropColumn(DbConnection connection, DiscoveredColumn columnToDrop)
        {
            throw new NotImplementedException();
        }

        public override int GetRowCount(DbConnection connection, IHasFullyQualifiedNameToo table, DbTransaction dbTransaction = null)
        {
            var cmd = new OracleCommand("select count(*) from " + table.GetFullyQualifiedName(), (OracleConnection) connection);
            cmd.Transaction = dbTransaction as OracleTransaction;
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        
        private string SensibleTypeFromOracleType(DbDataReader r)
        {
            int? precision = null;
            int? scale = null;

            if (r["DATA_SCALE"] != DBNull.Value)
                scale = Convert.ToInt32(r["DATA_SCALE"]);
            if (r["DATA_PRECISION"] != DBNull.Value)
                precision = Convert.ToInt32(r["DATA_PRECISION"]);


            switch (r["DATA_TYPE"] as string)
            {
                case "VARCHAR2": return "varchar";
                case "NUMBER":
                    if (scale == 0 && precision == null)
                        return "int";
                    else if (precision != null && scale != null)
                        return "decimal";
                    else
                        throw new Exception(
                            string.Format("Found Oracle NUMBER datatype with scale {0} and precision {1}, did not know what datatype to use to represent it",
                            scale != null ? scale.ToString() : "DBNull.Value",
                            precision != null ? precision.ToString() : "DBNull.Value"));
                case "FLOAT":
                    return "double";
                default:
                    return r["DATA_TYPE"].ToString().ToLower();
            }
        }

        private string GetSQLType_From_all_tab_cols_Result(DbDataReader r)
        {
            string columnType = SensibleTypeFromOracleType(r);

            string lengthQualifier = "";
            
            if (UsefulStuff.HasPrecisionAndScale(columnType))
                lengthQualifier = "(" + r["DATA_PRECISION"] + "," + r["DATA_SCALE"] + ")";
            else
                if (UsefulStuff.RequiresLength(columnType))
                    lengthQualifier = "(" + r["DATA_LENGTH"] + ")";

            return columnType + lengthQualifier;
        }

        public override void DropFunction(DbConnection connection, DiscoveredTableValuedFunction functionToDrop)
        {
            throw new NotImplementedException();
        }

        public override DiscoveredColumn[] DiscoverColumns(DiscoveredTableValuedFunction discoveredTableValuedFunction,
            IManagedConnection connection, string database, string tableName)
        {
            throw new NotImplementedException();
        }

        public override DiscoveredParameter[] DiscoverTableValuedFunctionParameters(DbConnection connection,
            DiscoveredTableValuedFunction discoveredTableValuedFunction, DbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public override IBulkCopy BeginBulkInsert(DiscoveredTable discoveredTable, IManagedConnection connection)
        {
            return new OracleBulkCopy(discoveredTable,connection);
        }

        protected override string GetRenameTableSql(DiscoveredTable discoveredTable, string newName)
        {
            return string.Format(@"alter table {0} rename to {1};", discoveredTable.GetRuntimeName(),newName);
        }

        public override void MakeDistinct(DiscoveredTable discoveredTable)
        {
            throw new NotImplementedException();
        }
    }
}