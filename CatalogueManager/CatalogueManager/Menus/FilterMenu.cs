using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.Sharing;
using CatalogueManager.DataViewing;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Menus
{
    class FilterMenu : RDMPContextMenuStrip
    {
        public FilterMenu(RDMPContextMenuStripArgs args, IFilter filter): base(args, (DatabaseEntity)filter)
        {
            Add(new ExecuteCommandViewFilterMatchData(args.ItemActivator, filter, ViewType.TOP_100));
            Add(new ExecuteCommandViewFilterMatchData(args.ItemActivator, filter, ViewType.Aggregate));
            Add(new ExecuteCommandViewFilterMatchGraph(_activator, filter));

            Items.Add(new ToolStripSeparator());

            var dis = filter as IDisableable;
            if (dis != null)
                Add(new ExecuteCommandDisableOrEnable(_activator, dis));

            Add(new ExecuteCommandExportObjectsToFileUI(_activator, new[] {filter}));
            Add(new ExecuteCommandImportFilterDescriptionsFromShare(_activator, filter));
        }
    }
}