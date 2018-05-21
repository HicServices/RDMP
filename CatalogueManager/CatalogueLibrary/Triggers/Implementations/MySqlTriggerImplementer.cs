using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
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
            throw new NotImplementedException();
        }

        public override string CreateTrigger(ICheckNotifier notifier, int createArchiveIndexTimeout = 30)
        {
            base.CreateTrigger(notifier, createArchiveIndexTimeout);

            /*@"CREATE TRIGGER testref BEFORE INSERT ON test1
  FOR EACH ROW
  BEGIN
    INSERT INTO test2 SET a2 = NEW.a1;
    DELETE FROM test3 WHERE a3 = NEW.a1;
    UPDATE test4 SET b4 = b4 + 1 WHERE a4 = NEW.a1;
  END;"*/
            return null;
        }

        public override TriggerStatus GetTriggerStatus()
        {
            using (var con = _server.GetConnection())
            {
                con.Open();

                var  cmd = _server.GetCommand(string.Format("show triggers like '{0}'", _table.GetRuntimeName()),con);
                var r = cmd.ExecuteReader();

                while (r.Read())
                {
                    if(r["Trigger"].Equals(GetTriggerName()))
                        return TriggerStatus.Enabled;
                }
            }

            return TriggerStatus.Missing;
        }

        private object GetTriggerName()
        {
            return _table.GetRuntimeName() + "_onupdate";
        }

        public override bool CheckUpdateTriggerIsEnabledAndHasExpectedBody()
        {
            throw new NotImplementedException();
        }
    }
}
