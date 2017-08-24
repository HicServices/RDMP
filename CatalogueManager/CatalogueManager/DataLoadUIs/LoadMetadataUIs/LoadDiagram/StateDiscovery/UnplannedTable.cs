using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery
{
    public class UnplannedTable:IHasLoadDiagramState
    {
        private readonly DiscoveredTable _table;
        public readonly DiscoveredColumn[] Columns;
        public LoadDiagramState State { get{return LoadDiagramState.New;}}

        public UnplannedTable(DiscoveredTable table)
        {
            _table = table;
            Columns = table.DiscoverColumns();
        }

        public override string ToString()
        {
            return _table.GetRuntimeName();
        }
    }
}
