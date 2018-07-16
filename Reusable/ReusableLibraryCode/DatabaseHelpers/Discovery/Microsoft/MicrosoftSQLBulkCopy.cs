using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft
{
    public class MicrosoftSQLBulkCopy : BulkCopy
    {
        private SqlBulkCopy _bulkcopy;

        public MicrosoftSQLBulkCopy(DiscoveredTable targetTable, IManagedConnection connection): base(targetTable, connection)
        {
            _bulkcopy = new SqlBulkCopy((SqlConnection)connection.Connection, SqlBulkCopyOptions.KeepIdentity, (SqlTransaction)connection.Transaction);
            _bulkcopy.BulkCopyTimeout = 50000;
            _bulkcopy.DestinationTableName = targetTable.GetFullyQualifiedName();
        }

        public override int Upload(DataTable dt)
        {
            _bulkcopy.BulkCopyTimeout = Timeout;

            _bulkcopy.ColumnMappings.Clear();

            foreach (KeyValuePair<DataColumn, DiscoveredColumn> kvp in GetMapping(dt.Columns.Cast<DataColumn>()))
                _bulkcopy.ColumnMappings.Add(kvp.Key.ColumnName, kvp.Value.GetRuntimeName());
            
            return UsefulStuff.BulkInsertWithBetterErrorMessages(_bulkcopy, dt, TargetTable.Database.Server);
        }
    }
}