using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft
{
    public class MicrosoftSQLBulkCopy : IBulkCopy
    {
        private readonly DiscoveredTable _discoveredTable;
        private readonly IManagedConnection _connection;

        private SqlBulkCopy _bulkcopy;

        public MicrosoftSQLBulkCopy(DiscoveredTable discoveredTable, IManagedConnection connection)
        {
            _discoveredTable = discoveredTable;
            _connection = connection;

            _bulkcopy = new SqlBulkCopy((SqlConnection)connection.Connection, SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)connection.Transaction);
            _bulkcopy.BulkCopyTimeout = 50000;
            _bulkcopy.DestinationTableName = _discoveredTable.GetRuntimeName();

        }

        public int Upload(DataTable dt)
        {
            if (_bulkcopy.ColumnMappings.Count == 0)
                foreach (DataColumn col in dt.Columns)
                    _bulkcopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);

            return UsefulStuff.BulkInsertWithBetterErrorMessages(_bulkcopy, dt, _discoveredTable.Database.Server);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}