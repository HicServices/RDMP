using System.Linq;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Triggers.Implementations
{
    public abstract class TriggerImplementer:ITriggerImplementer
    {
        protected readonly bool _createDataLoadRunIdAlso;
        
        protected readonly DiscoveredServer _server;
        protected readonly DiscoveredTable _table;
        protected readonly DiscoveredTable _archiveTable;
        protected readonly DiscoveredColumn[] _columns;
        protected readonly DiscoveredColumn[] _primaryKeys;

        public TriggerImplementer(DiscoveredTable table, bool createDataLoadRunIDAlso = true)
        {
            _server = table.Database.Server;
            _table = table;
            _archiveTable = _table.Database.ExpectTable(table.GetRuntimeName() + "_Archive");
            _columns = table.DiscoverColumns();
            _primaryKeys = _columns.Where(c => c.IsPrimaryKey).ToArray();
            
            _createDataLoadRunIdAlso = createDataLoadRunIDAlso;
        }

        public abstract void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger);
        public abstract void CreateTrigger(ICheckNotifier notifier, int createArchiveIndexTimeout = 30);
        public abstract TriggerStatus GetTriggerStatus();
        public abstract bool CheckUpdateTriggerIsEnabledAndHasExpectedBody();
    }
}
