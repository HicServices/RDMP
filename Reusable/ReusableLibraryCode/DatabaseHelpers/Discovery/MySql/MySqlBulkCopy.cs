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
        private DbConnection _connection;
        
        public MySqlBulkCopy(DiscoveredTable discoveredTable, DbConnection connection, DbTransaction transaction)
        {
            _discoveredTable = discoveredTable;
            _connection = connection;
        }

        public int Upload(DataTable dt)
        {
            var loader = new MySqlBulkLoader((MySqlConnection)_connection);
            loader.TableName = _discoveredTable.GetRuntimeName();
            
            var tempFile = Path.GetTempFileName();
            loader.FieldTerminator = ",";
            loader.LineTerminator = "\r\n";
            loader.FieldQuotationCharacter = '"';
            
            var sw = new StreamWriter(tempFile);
            Rfc4180Writer.WriteDataTable(dt,sw,false);
            sw.Flush();
            sw.Close();

            loader.FileName = tempFile;
            try
            {
                return loader.Load();
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        public void Dispose()
        {
            
        }
    }
}