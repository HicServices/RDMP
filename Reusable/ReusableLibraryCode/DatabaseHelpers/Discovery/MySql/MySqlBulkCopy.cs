using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.MySql
{
    public class MySqlBulkCopy : IBulkCopy
    {
        private readonly DiscoveredTable _discoveredTable;
        private IManagedConnection _connection;
        
        public MySqlBulkCopy(DiscoveredTable discoveredTable, IManagedConnection connection)
        {
            _discoveredTable = discoveredTable;
            _connection = connection;
        }

        public int Upload(DataTable dt)
        {
            var loader = new MySqlBulkLoader((MySqlConnection)_connection.Connection);
            loader.TableName = "`" + _discoveredTable.GetRuntimeName() +"`";
            
            var tempFile = Path.GetTempFileName();
            loader.FieldTerminator = ",";
            loader.LineTerminator = "\r\n";
            loader.FieldQuotationCharacter = '"';
            
            //for all columns not appearing in the DataTable provided
            var unmatchedColumns = _discoveredTable.DiscoverColumns().Where(c=>!dt.Columns.Contains(c.GetRuntimeName()));

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

        public int Timeout { get; set; }

        public void Dispose()
        {
           _connection.Dispose();
        }
    }
}