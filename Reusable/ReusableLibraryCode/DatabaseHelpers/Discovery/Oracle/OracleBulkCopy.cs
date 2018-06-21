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
        private DbCommand _cmd;
        private DiscoveredColumn[] _columns;
        private DiscoveredServer _server;

        private const char ParameterSymbol = ':';

        public OracleBulkCopy(DiscoveredTable discoveredTable, IManagedConnection connection)
        {
            _discoveredTable = discoveredTable;

            _server = discoveredTable.Database.Server;

            _columns = discoveredTable.DiscoverColumns(connection.ManagedTransaction);

            var sql = string.Format("INSERT INTO " + _discoveredTable.GetFullyQualifiedName() + "({0}) VALUES ({1})",
                string.Join(",", _columns.Select(c => c.GetRuntimeName())),
                string.Join(",", _columns.Select(c => ParameterSymbol + c.GetRuntimeName()))
                );

            var tt = _server.GetQuerySyntaxHelper().TypeTranslater;

            _cmd = _server.GetCommand(sql, connection);
            foreach (var c in _columns)
            {
                var p = _server.AddParameterWithValueToCommand(ParameterSymbol + c.GetRuntimeName(), _cmd, DBNull.Value);
                p.DbType = tt.GetDbTypeForSQLDBType(c.DataType.SQLType);
            }

            _cmd.Prepare();
        }

        public void Dispose()
        {
            _cmd.Dispose();
        }

        public int Upload(DataTable dt)
        {
            int affectedRows = 0;

            var columnsAvailable = new HashSet<string>(dt.Columns.Cast<DataColumn>().Select(c=>c.ColumnName),StringComparer.InvariantCultureIgnoreCase);
            var columnsToPopulate = new HashSet<string>(_columns.Select(c=>c.GetRuntimeName()),StringComparer.InvariantCultureIgnoreCase);

            var missing = columnsAvailable.Except(columnsToPopulate,StringComparer.InvariantCultureIgnoreCase).ToArray();
            var extra = columnsToPopulate.Except(columnsAvailable, StringComparer.InvariantCultureIgnoreCase).ToArray();

            if(missing.Any())
                throw new Exception("The following columns were missing from the destination table (but present in the DataTable you were trying to upload)" + string.Join(",",missing));

            //null out any we don't anticipate populating this time around (bear in mind they might give us multiple calls to Upload with widening DataTables (><)
            foreach (string e in extra)
                _cmd.Parameters[ParameterSymbol + e].Value = DBNull.Value;
            
            foreach (DataRow dataRow in dt.Rows)
            {
                //populate parameters for current row
                foreach (var col in columnsAvailable)
                {
                    var param = _cmd.Parameters[ParameterSymbol + col];

                    //oracle isn't too bright when it comes to these kinds of things, see Test CreateDateColumnFromDataTable
                    if (param.DbType == DbType.DateTime)
                        param.Value = Convert.ToDateTime(dataRow[col]);
                    else
                    {
                        param.Value = dataRow[col];
                    }
                }

                //send query
                affectedRows += _cmd.ExecuteNonQuery();
            }

            return affectedRows;
        }

        public int Timeout { get; set; }
    }
}
