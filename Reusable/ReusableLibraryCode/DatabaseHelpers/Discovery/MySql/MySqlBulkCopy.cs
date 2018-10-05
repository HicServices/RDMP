using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NuDoq;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlBulkCopy : BulkCopy
    {

        public static int BulkInsertBatchTimeoutInSeconds = 0;

        public MySqlBulkCopy(DiscoveredTable targetTable, IManagedConnection connection) : base(targetTable, connection)
        {
            
        }

        /* //Old method that used LOAD DATA IN FILE LOCAL (MySqlBulkLoader).  This requires local infile to be enabled on the server (and is generally a pain) : https://dev.mysql.com/doc/refman/5.7/en/load-data-local.html
        public override int Upload(DataTable dt)
        {
            //for all columns not appearing in the DataTable provided
            DiscoveredColumn[] unmatchedColumns;

            var matchedColumns = GetMapping(dt.Columns.Cast<DataColumn>(), out unmatchedColumns);
            
            var unmatchedPks = unmatchedColumns.Where(c => c.IsPrimaryKey).ToArray();

            if (unmatchedPks.Any())
            {
                if (!unmatchedPks.All(pk=>pk.IsAutoIncrement))
                    throw new Exception("Primary key columns " + string.Join(",", unmatchedPks.Select(c => c.GetRuntimeName())) + " did not appear in the DataTable and are not IsAutoIncrement");
            }
            else
            {
                //MySqlBulkLoader does upsert and ignore but no Throw option, so we have to enforce primary keys in memory instead
                if (dt.PrimaryKey.Length == 0)
                    dt.PrimaryKey = matchedColumns.Where(kvp => kvp.Value.IsPrimaryKey).Select(kvp=>kvp.Key).ToArray();
            }

            //make the column names in the data table match the destination columns in case etc
            foreach (KeyValuePair<DataColumn, DiscoveredColumn> kvp in matchedColumns)
                if (!kvp.Key.ColumnName.Equals(kvp.Value.GetRuntimeName()))
                    kvp.Key.ColumnName = kvp.Value.GetRuntimeName();

            var loader = new MySqlBulkLoader((MySqlConnection)Connection.Connection);
            loader.TableName = "`" + TargetTable.GetRuntimeName() +"`";
            
            var tempFile = Path.GetTempFileName();
            loader.FieldTerminator = ",";
            loader.LineTerminator = "\r\n";
            loader.FieldQuotationCharacter = '"';

            loader.Expressions.Clear();
            loader.Columns.Clear();

            //use the system default
            foreach (var column in unmatchedColumns)
                loader.Expressions.Add(column + " = DEFAULT");
            
            loader.Columns.AddRange(dt.Columns.Cast<DataColumn>().Select(c=>"`"+c.ColumnName+"`"));

            var sw = new StreamWriter(tempFile);
            Rfc4180Writer.WriteDataTable(dt,sw,false, new MySqlQuerySyntaxHelper());
            sw.Flush();
            sw.Close();

            loader.FileName = tempFile;
            loader.Timeout = Timeout;
            try
            {
                return loader.Load();
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
        */
        public override int Upload(DataTable dt)
        {
            var matchedColumns = GetMapping(dt.Columns.Cast<DataColumn>());

            MySqlCommand cmd = new MySqlCommand("", (MySqlConnection)Connection.Connection,(MySqlTransaction) Connection.Transaction);

            if (BulkInsertBatchTimeoutInSeconds != 0)
                cmd.CommandTimeout = BulkInsertBatchTimeoutInSeconds;

            string commandPrefix = string.Format("INSERT INTO {0}({1}) VALUES ", TargetTable.GetFullyQualifiedName(),string.Join(",", matchedColumns.Values.Select(c => "`" + c.GetRuntimeName() + "`")));

            StringBuilder sb = new StringBuilder();
                
            int affected = 0;
            int row = 0;

            foreach(DataRow dr in dt.Rows)
            {
                sb.Append('(');

                DataRow dr1 = dr;
                
                sb.Append(string.Join(",",matchedColumns.Keys.Select(k => ConstructIndividualValue(matchedColumns[k].DataType.SQLType,dr1[k]))));

                sb.AppendLine("),");
                row++;

                //don't let command get too long
                if (row%1000 == 0)
                {
                    cmd.CommandText = commandPrefix + sb.ToString().TrimEnd(',', '\r', '\n');
                    affected += cmd.ExecuteNonQuery();
                    sb.Clear();
                }
            }

            //send final batch
            if(sb.Length > 0)
            {
                cmd.CommandText = commandPrefix + sb.ToString().TrimEnd(',', '\r', '\n');
                affected += cmd.ExecuteNonQuery();
                sb.Clear();
            }

            return affected;
            
        }

        private string ConstructIndividualValue(string dataType, object value)
        {
            if(value == null || value == DBNull.Value)
                return "NULL";
            
            return ConstructIndividualValue(dataType,  value.ToString());
        }

        private string ConstructIndividualValue(string dataType, string value)
        {
            var type = dataType.ToUpper();
            type = Regex.Replace(type,"\\(.*\\)", "");

            switch (type.Trim())
            {
                case "BIT":
                    return value;

                //Numbers
                case "INT":
                case "TINYINT":
                case "SMALLINT":
                case "MEDIUMINT":
                case "BIGINT":
                case "FLOAT":
                case "DOUBLE":
                case "DECIMAL":
                    return string.Format("{0}", value);
                
                //Text
                case "CHAR":
                case "VARCHAR":
                case "BLOB":
                case "TEXT":
                case "TINYBLOB":
                case "TINYTEXT":
                case "MEDIUMBLOB":
                case "MEDIUMTEXT":
                case "LONGBLOB":
                case "LONGTEXT":
                case "ENUM":
                    return string.Format("'{0}'", MySqlHelper.EscapeString(value));
                
                //Dates/times
                case "DATE":
                    return String.Format("'{0:yyyy-MM-dd}'", value);
                case "TIMESTAMP":
                case "DATETIME":
                    DateTime date = DateTime.Parse(value);
                    return String.Format("'{0:yyyy-MM-dd HH:mm:ss}'", date);
                case "TIME":
                    return String.Format("'{0:HH:mm:ss}'", value);
                case "YEAR2":
                    return String.Format("'{0:yy}'", value);
                case "YEAR4":
                    return String.Format("'{0:yyyy}'", value);

                //Unknown
                default:
                    // we don't understand the format. to safegaurd the code, just enclose with ''
                    return string.Format("'{0}'", MySqlHelper.EscapeString(value));
            }
        }
    }
}