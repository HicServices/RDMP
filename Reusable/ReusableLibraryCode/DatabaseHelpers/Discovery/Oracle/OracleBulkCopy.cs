using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Oracle.ManagedDataAccess.Client;


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
            var dateColumns = new HashSet<string>();

            var sql = string.Format("INSERT INTO " + _discoveredTable.GetFullyQualifiedName() + "({0}) VALUES ({1})",
                string.Join(",", availableColumns),
                string.Join(",", availableColumns.Select(c => ParameterSymbol + c))
                );

            var tt = _server.GetQuerySyntaxHelper().TypeTranslater;
            
            using(OracleCommand cmd = (OracleCommand) _server.GetCommand(sql, _connection))
            {
                //send all the data at once
                cmd.ArrayBindCount = dt.Rows.Count;

                foreach (var c in availableColumns)
                {
                    var p = _server.AddParameterWithValueToCommand(ParameterSymbol + c, cmd, DBNull.Value);

                    var matching = _discoveredColumns.SingleOrDefault(d => d.GetRuntimeName().Equals(c,StringComparison.CurrentCultureIgnoreCase));

                    if(matching == null)
                        throw new Exception("Unmatched column '" + c + "' in DataTable when compared to table " + _discoveredTable + " (with columns:" + string.Join(",",availableColumns) +")");

                    p.DbType = tt.GetDbTypeForSQLDBType(matching.DataType.SQLType);

                    if (p.DbType == DbType.DateTime)
                        dateColumns.Add(c);
                }
                
                var values = new Dictionary<string, List<object>>();

                foreach (string c in availableColumns)
                    values.Add(c, new List<object>());


                foreach (DataRow dataRow in dt.Rows)
                {
                    //populate parameters for current row
                    foreach (var col in availableColumns)
                    {
                        var val = dataRow[col];

                        if (val is string && string.IsNullOrWhiteSpace((string) val))
                            val = null;
                        else
                        if (val == null || val == DBNull.Value)
                            val = null;
                        else if (dateColumns.Contains(col))
                            val = Convert.ToDateTime(dataRow[col]);
                        
                        values[col].Add(val);
                    }
                }

                foreach (string col in availableColumns)
                {
                    var param = cmd.Parameters[ParameterSymbol + col];
                    param.Value = values[col].ToArray();
                }

                //send query
                affectedRows += cmd.ExecuteNonQuery();
            }
            return affectedRows;
        }

        public int Timeout { get; set; }
    }
}
