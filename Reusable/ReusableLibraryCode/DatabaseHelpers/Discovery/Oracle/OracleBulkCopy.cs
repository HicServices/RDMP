using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;


namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle
{
    class OracleBulkCopy : IBulkCopy
    {
        private readonly DiscoveredTable _discoveredTable;
        private readonly IManagedConnection _connection;
        private readonly DiscoveredServer _server;
        private readonly DiscoveredColumn[] _discoveredColumns;

        private const char ParameterSymbol = ':';

        public OracleBulkCopy(DiscoveredTable discoveredTable, IManagedConnection connection)
        {
            _discoveredTable = discoveredTable;
            _connection = connection;
            _server = discoveredTable.Database.Server;

            _discoveredColumns = _discoveredTable.DiscoverColumns(connection.ManagedTransaction);
            
        }

        public void Dispose()
        {
        }

        public int Upload(DataTable dt)
        {
            int affectedRows = 0;

            var availableColumns = new HashSet<string>(dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName),System.StringComparer.CurrentCultureIgnoreCase);

            var sql = string.Format("INSERT INTO " + _discoveredTable.GetFullyQualifiedName() + "({0}) VALUES ({1})",
                string.Join(",", availableColumns),
                string.Join(",", availableColumns.Select(c => ParameterSymbol + c))
                );

            var tt = _server.GetQuerySyntaxHelper().TypeTranslater;
            
            using(var cmd = _server.GetCommand(sql, _connection))
            {

                foreach (var c in availableColumns)
                {
                    var p = _server.AddParameterWithValueToCommand(ParameterSymbol + c, cmd, DBNull.Value);

                    var matching = _discoveredColumns.SingleOrDefault(d => d.GetRuntimeName().Equals(c,StringComparison.CurrentCultureIgnoreCase));

                    if(matching == null)
                        throw new Exception("Unmatched column '" + c + "' in DataTable when compared to table " + _discoveredTable + " (with columns:" + string.Join(",",availableColumns) +")");

                    p.DbType = tt.GetDbTypeForSQLDBType(matching.DataType.SQLType);
                }

                cmd.Prepare();

                foreach (DataRow dataRow in dt.Rows)
                {
                    //populate parameters for current row
                    foreach (var col in availableColumns)
                    {
                        var param = cmd.Parameters[ParameterSymbol + col];

                        //oracle isn't too bright when it comes to these kinds of things, see Test CreateDateColumnFromDataTable
                        if (param.DbType == DbType.DateTime)
                            param.Value = Convert.ToDateTime(dataRow[col]);
                        else
                        {
                            param.Value = dataRow[col];
                        }
                    }

                    //send query
                    affectedRows += cmd.ExecuteNonQuery();
                }
                
            }
            return affectedRows;
        }

        public int Timeout { get; set; }
    }
}
