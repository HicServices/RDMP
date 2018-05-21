using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

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

        public override void CreateTrigger(ICheckNotifier notifier, int createArchiveIndexTimeout = 30)
        {
            throw new NotImplementedException();
        }

        public override TriggerStatus GetTriggerStatus()
        {
            throw new NotImplementedException();
        }

        public override bool CheckUpdateTriggerIsEnabledAndHasExpectedBody()
        {
            throw new NotImplementedException();
        }
    }
}
