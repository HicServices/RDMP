using System;
using System.Collections.Generic;
using System.Data;
using System.Linq; 
using Oracle.ManagedDataAccess.Client;


namespace ReusableLibraryCode.DatabaseHelpers.Discovery.Oracle
{
    class OracleBulkCopy : BulkCopy
    {
        private readonly DiscoveredServer _server;
        
        private const char ParameterSymbol = ':';

        public OracleBulkCopy(DiscoveredTable targetTable, IManagedConnection connection): base(targetTable, connection)
        {
            _server = targetTable.Database.Server;
        }
        
        public override int Upload(DataTable dt)
        {
            int affectedRows = 0;
            
            var mapping = GetMapping(dt.Columns.Cast<DataColumn>());

            var dateColumns = new HashSet<DataColumn>();

            var sql = string.Format("INSERT INTO " + TargetTable.GetFullyQualifiedName() + "({0}) VALUES ({1})",
                string.Join(",", mapping.Values.Select(c=>c.GetRuntimeName())),
                string.Join(",", mapping.Keys.Select(c => ParameterSymbol + c.ColumnName))
                );

            var tt = _server.GetQuerySyntaxHelper().TypeTranslater;

            using(OracleCommand cmd = (OracleCommand) _server.GetCommand(sql, Connection))
            {
                //send all the data at once
                cmd.ArrayBindCount = dt.Rows.Count;

                foreach (var kvp in mapping)
                {
                    var p = _server.AddParameterWithValueToCommand(ParameterSymbol + kvp.Key.ColumnName, cmd, DBNull.Value);
                    p.DbType = tt.GetDbTypeForSQLDBType(kvp.Value.DataType.SQLType);

                    if (p.DbType == DbType.DateTime)
                        dateColumns.Add(kvp.Key);
                }
                
                var values = new Dictionary<DataColumn, List<object>>();

                foreach (DataColumn c in mapping.Keys)
                    values.Add(c, new List<object>());


                foreach (DataRow dataRow in dt.Rows)
                {
                    //populate parameters for current row
                    foreach (var col in mapping.Keys)
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

                foreach (DataColumn col in mapping.Keys)
                {
                    var param = cmd.Parameters[ParameterSymbol + col.ColumnName];
                    param.Value = values[col].ToArray();
                }

                //send query
                affectedRows += cmd.ExecuteNonQuery();
            }
            return affectedRows;
        }
    }
}
