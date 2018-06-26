using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlTableHelper : DiscoveredTableHelper
    {

        public override DiscoveredColumn[] DiscoverColumns(DiscoveredTable discoveredTable, IManagedConnection connection, string database)
        {
            List<DiscoveredColumn> columns = new List<DiscoveredColumn>();
            var tableName = discoveredTable.GetRuntimeName();

            DbCommand cmd = DatabaseCommandHelper.GetCommand("DESCRIBE `" + database + "`.`" + tableName + "`", connection.Connection);
            cmd.Transaction = connection.Transaction;

            using(DbDataReader r = cmd.ExecuteReader())
            {
                if (!r.HasRows)
                    throw new Exception("Could not find any columns using sp_columns for table " + tableName + " in database " + database);

                while (r.Read())
                {
                    var toAdd = new DiscoveredColumn(discoveredTable, (string) r["Field"],YesNoToBool(r["Null"]));

                    if (r["Key"].Equals("PRI"))
                        toAdd.IsPrimaryKey = true;
                    
                    toAdd.IsAutoIncrement = r["Extra"] as string == "auto_increment";

                    toAdd.DataType = new DiscoveredDataType(r,SensibleTypeFromMySqlType(r["Type"].ToString()),toAdd);
                    columns.Add(toAdd);

                }

                r.Close();
            }

            return columns.ToArray();
            
        }

        private bool YesNoToBool(object o)
        {
            if (o is bool)
                return (bool)o;

            if (o == null || o == DBNull.Value)
                return false;

            if (o.ToString() == "NO")
                return false;
            
            if (o.ToString() == "YES")
                return true;

            return Convert.ToBoolean(o);
        }

        private string SensibleTypeFromMySqlType(string type)
        {
            if (type.StartsWith("int"))//for some reason mysql likes to put parenthesis and the byte size.. ??? after the number
                return "int";

            if (type.StartsWith("smallint"))//for some reason mysql likes to put parenthesis and the byte size.. ??? after the number
                return "smallint";

            if (type.Equals("bit(1)"))
                return "bit";

            return type;
        }

        public override IDiscoveredColumnHelper GetColumnHelper()
        {
            return new MySqlColumnHelper();
        }

        public override void DropTable(DbConnection connection, DiscoveredTable table)
        {
            var cmd = new MySqlCommand("drop table " + table.GetFullyQualifiedName(), (MySqlConnection)connection);
            cmd.ExecuteNonQuery();
        }

        public override void DropColumn(DbConnection connection, DiscoveredColumn columnToDrop)
        {
            var cmd = new MySqlCommand("alter table " + columnToDrop.Table.GetFullyQualifiedName() + " drop column " + columnToDrop.GetRuntimeName(), (MySqlConnection)connection);
            cmd.ExecuteNonQuery();
        }

        public override int GetRowCount(DbConnection connection, IHasFullyQualifiedNameToo table, DbTransaction dbTransaction = null)
        {
            var cmd = new MySqlCommand("select count(*) from " + table.GetFullyQualifiedName(),(MySqlConnection) connection);
            cmd.Transaction = dbTransaction as MySqlTransaction;
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public override DiscoveredParameter[] DiscoverTableValuedFunctionParameters(DbConnection connection,
            DiscoveredTableValuedFunction discoveredTableValuedFunction, DbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public override IBulkCopy BeginBulkInsert(DiscoveredTable discoveredTable,IManagedConnection connection)
        {
            return new MySqlBulkCopy(discoveredTable, connection);
        }

        protected override string GetRenameTableSql(DiscoveredTable discoveredTable, string newName)
        {
            return string.Format("RENAME TABLE `{0}` TO `{1}`;", discoveredTable.GetRuntimeName(), newName);
        }

        public override string GetTopXSqlForTable(IHasFullyQualifiedNameToo table, int topX)
        {
            return "SELECT * FROM " + table.GetFullyQualifiedName() + " LIMIT " + topX;
        }


        public override void DropFunction(DbConnection connection, DiscoveredTableValuedFunction functionToDrop)
        {
            throw new NotImplementedException();
        }
    }
}