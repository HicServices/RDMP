using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAnsi.Discovery;
using Oracle.ManagedDataAccess.Client;
using Rdmp.Core.DataLoad.Triggers.Exceptions;
using ReusableLibraryCode.Exceptions;

namespace Rdmp.Core.DataLoad.Triggers.Implementations
{
    internal class OracleTriggerImplementer:MySqlTriggerImplementer
    {
        public OracleTriggerImplementer(DiscoveredTable table, bool createDataLoadRunIDAlso = true) : base(table, createDataLoadRunIDAlso)
        {
        }

        protected override string GetTriggerBody()
        {
            using (var con = _server.GetConnection())
            {
                con.Open();

                var cmd = _server.GetCommand(string.Format("select trigger_body from all_triggers where trigger_name = UPPER('{0}')", GetTriggerName()), con);
                ((OracleCommand)cmd).InitialLONGFetchSize = -1;
                
                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    return (string) r["trigger_body"];
                }
            }

            return null;
        }
        protected override string CreateTriggerBody()
        {
            return string.Format(@"BEGIN
    INSERT INTO {0} ({1},hic_validTo,hic_userID,hic_status) VALUES ({2},CURRENT_DATE,USER,'U');
  END", _archiveTable.GetFullyQualifiedName(),
                string.Join(",", _columns.Select(c => c.GetRuntimeName())),
                string.Join(",", _columns.Select(c => ":old." + c.GetRuntimeName())));
        }

        protected override void AssertTriggerBodiesAreEqual(string sqlThen, string sqlNow)
        {
            sqlNow = sqlNow??"";
            sqlThen = sqlThen??"";

            if(!sqlNow.Trim(';',' ','\t').Equals(sqlThen.Trim(';',' ','\t')))
                throw new ExpectedIdenticalStringsException("Sql body for trigger doesn't match expcted sql",sqlThen,sqlNow);
        }
    }
}
