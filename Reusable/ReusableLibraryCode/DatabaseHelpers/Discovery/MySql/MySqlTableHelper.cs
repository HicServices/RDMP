using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlTableHelper : IDiscoveredTableHelper
    {

        public DiscoveredColumn[] DiscoverColumns(DiscoveredTable discoveredTable, IManagedConnection connection, string database, string tableName)
        {
            List<DiscoveredColumn> columns = new List<DiscoveredColumn>();

            DbCommand cmd = DatabaseCommandHelper.GetCommand("DESCRIBE `" + database + "`.`" + tableName + "`", connection.Connection);
            cmd.Transaction = connection.Transaction;

            using(DbDataReader r = cmd.ExecuteReader())
            {
                if (!r.HasRows)
                    throw new Exception("Could not find any columns using sp_columns for table " + tableName + " in database " + database);

                while (r.Read())
                {
                    var toAdd = new DiscoveredColumn(discoveredTable, (string) r["Field"],(bool)r["Null"]);

                    if (r["Key"].Equals("PRI"))
                        toAdd.IsPrimaryKey = true;

                    toAdd.DataType = new DiscoveredDataType(r,SensibleTypeFromMySqlType(r["Type"].ToString()),toAdd);
                    columns.Add(toAdd);

                }

                r.Close();
            }

            return columns.ToArray();
            
        }
        private string SensibleTypeFromMySqlType(string type)
        {
            if (type.StartsWith("int"))//for some reason mysql likes to put parenthesis and the byte size.. ??? after the number
                return "int";

            if (type.StartsWith("smallint"))//for some reason mysql likes to put parenthesis and the byte size.. ??? after the number
                return "smallint";
            
            return type;
        }

        public IDiscoveredColumnHelper GetColumnHelper()
        {
            return new MySqlColumnHelper();
        }

        public void DropTable(DbConnection connection, DiscoveredTable table, DbTransaction dbTransaction = null)
        {
            var cmd = new MySqlCommand("drop table " + table.GetFullyQualifiedName(), (MySqlConnection)connection);
            cmd.Transaction = dbTransaction as MySqlTransaction;
            cmd.ExecuteNonQuery();
        }

        public void DropColumn(DbConnection connection, DiscoveredTable discoveredTable, DiscoveredColumn columnToDrop,
            DbTransaction dbTransaction)
        {
            throw new NotImplementedException();
        }

        public int GetRowCount(DbConnection connection, IHasFullyQualifiedNameToo table, DbTransaction dbTransaction = null)
        {
            var cmd = new MySqlCommand("select count(*) from " + table.GetFullyQualifiedName(),(MySqlConnection) connection);
            cmd.Transaction = dbTransaction as MySqlTransaction;
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public string WrapStatementWithIfTableExistanceMatches(bool existanceDesiredForExecution, StringLiteralSqlInContext bodySql, string tableName)
        {

            if(bodySql.IsDynamic)
                throw new NotSupportedException("Cannot wrap dynamic sql sorry");
            
            var syntaxHelper = new MySqlQuerySyntaxHelper();
            tableName = syntaxHelper.GetRuntimeName(tableName);


            var matchCreate = new Regex("CREATE TABLE (.*\\.?" + tableName +")",RegexOptions.IgnoreCase).Match(bodySql.Sql);//brackets indicate capture group 1 which is 'db.table' but might just be 'table'
            var matchDrop = new Regex("DROP TABLE (.*\\.?" + tableName + ")", RegexOptions.IgnoreCase).Match(bodySql.Sql);

            string existanceString = existanceDesiredForExecution ? "" : "NOT";

            if (matchCreate.Success)
                return bodySql.Sql.Replace(matchCreate.Groups[0].Value,
                    "CREATE TABLE IF " + existanceString + " EXISTS " + matchCreate.Groups[1].Value);
            else if (matchDrop.Success)
                return bodySql.Sql.Replace(matchDrop.Groups[0].Value,
                                    "DROP TABLE IF " + existanceString + " EXISTS " + matchDrop.Groups[1].Value);
            else
                throw new NotImplementedException("Expected bodysql to contain CREATE or DROP TABLE " + tableName);
        }

        public DiscoveredParameter[] DiscoverTableValuedFunctionParameters(DbConnection connection,
            DiscoveredTableValuedFunction discoveredTableValuedFunction, DbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public string GetTopXSqlForTable(IHasFullyQualifiedNameToo table, int topX)
        {
            throw new NotImplementedException();
        }


        public void DropFunction(DbConnection connection, DiscoveredTableValuedFunction functionToDrop, DbTransaction dbTransaction)
        {
            throw new NotImplementedException();
        }


        public DiscoveredColumn[] DiscoverColumns(DiscoveredTableValuedFunction discoveredTableValuedFunction,
            IManagedConnection connection, string database, string tableName)
        {
            throw new NotImplementedException();
        }

    }
}