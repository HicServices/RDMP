using System;
using System.Data;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlBulkCopy : BulkCopy
    {
        public MySqlBulkCopy(DiscoveredTable targetTable, IManagedConnection connection) : base(targetTable, connection)
        {
            
        }

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
            
            loader.Columns.AddRange(dt.Columns.Cast<DataColumn>().Select(c=>c.ColumnName));

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
    }
}