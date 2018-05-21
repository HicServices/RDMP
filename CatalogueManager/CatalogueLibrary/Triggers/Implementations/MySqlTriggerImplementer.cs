using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Exceptions;
using Xceed.Words.NET;

namespace CatalogueLibrary.Triggers.Implementations
{
    internal class MySqlTriggerImplementer:TriggerImplementer
    {
        public MySqlTriggerImplementer(DiscoveredTable table, bool createDataLoadRunIDAlso = true) : base(table, createDataLoadRunIDAlso)
        {
        }

        public override void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger)
        {
            problemsDroppingTrigger = "";
            thingsThatWorkedDroppingTrigger = "";

            try
            {
                using (var con = _server.GetConnection())
                {
                    con.Open();

                    var cmd = _server.GetCommand("DROP TRIGGER " + GetTriggerName(), con);
                    cmd.ExecuteNonQuery();

                    thingsThatWorkedDroppingTrigger = "Droppped trigger " + GetTriggerName();
                }
            }
            catch (Exception exception)
            {
                //this is not a problem really since it is likely that DLE chose to recreate the trigger because it was FUBARed or missing, this is just belt and braces try and drop anything that is lingering, whether or not it is there
                problemsDroppingTrigger += "Failed to drop Trigger:" + exception.Message + Environment.NewLine; ;
            }
        }

        public override string CreateTrigger(ICheckNotifier notifier, int createArchiveIndexTimeout = 30)
        {
            string creationSql = base.CreateTrigger(notifier, createArchiveIndexTimeout);

            var sql = string.Format(@"CREATE TRIGGER {0} BEFORE UPDATE ON {1} FOR EACH ROW
{2};", 
       GetTriggerName(),
       _table,
       CreateTriggerBody());

            using (var con = _server.GetConnection())
            {
                con.Open();

                var cmd = _server.GetCommand(sql, con);
                cmd.ExecuteNonQuery();
            }

            return creationSql;
        }

        private string CreateTriggerBody()
        {
            return string.Format(@"BEGIN
    INSERT INTO {0} SET {1},hic_validTo=now(),hic_userID=CURRENT_USER(),hic_status='U';
  END", _archiveTable,
                string.Join(",", _columns.Select(c => c.GetRuntimeName() + "=OLD." + c.GetRuntimeName())));
        }

        public override TriggerStatus GetTriggerStatus()
        {
            return string.IsNullOrWhiteSpace(GetTriggerBody())? TriggerStatus.Missing : TriggerStatus.Enabled;
        }

        private string GetTriggerBody()
        {
            using (var con = _server.GetConnection())
            {
                con.Open();

                var cmd = _server.GetCommand(string.Format("show triggers like '{0}'", _table.GetRuntimeName()), con);
                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    if (r["Trigger"].Equals(GetTriggerName()))
                        return (string) r["Statement"];
                }
            }

            return null;
        }

        private object GetTriggerName()
        {
            return _table.GetRuntimeName() + "_onupdate";
        }

        public override bool CheckUpdateTriggerIsEnabledAndHasExpectedBody()
        {
            if (!base.CheckUpdateTriggerIsEnabledAndHasExpectedBody())
                return false;

            var sqlThen = GetTriggerBody();
            var sqlNow = CreateTriggerBody();

            if(!sqlNow.Equals(sqlThen))
                throw new ExpectedIdenticalStringsException("Sql body for trigger doesn't match expcted sql",sqlNow,sqlThen);
            
            return true;
        }
    }
}
