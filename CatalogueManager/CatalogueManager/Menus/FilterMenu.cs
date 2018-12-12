using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.Sharing;
using CatalogueManager.DataViewing;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    class FilterMenu : RDMPContextMenuStrip
    {
        public FilterMenu(RDMPContextMenuStripArgs args, IFilter filter): base(args, (DatabaseEntity)filter)
        {
            var cata = filter.GetCatalogue();

            Add(new ExecuteCommandViewFilterMatchData(args.ItemActivator, filter, ViewType.TOP_100));
            Add(new ExecuteCommandViewFilterMatchData(args.ItemActivator, filter, ViewType.Aggregate));

            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandExportObjectsToFileUI(_activator, new[] {filter}));
            Add(new ExecuteCommandImportFilterDescriptionsFromShare(_activator, filter));
           
            if (cata != null)
            { 
                //compatible graphs are those that are not part of a cic (i.e. they are proper aggregate graphs)
                var compatibleGraphs = cata.AggregateConfigurations.Where(a => !a.IsCohortIdentificationAggregate).ToArray();

                if (compatibleGraphs.Any())
                {
                    var graphMenu = new ToolStripMenuItem("View Aggregate Graph of Filter",GetImage(RDMPConcept.AggregateGraph));

                    foreach (AggregateConfiguration graph in compatibleGraphs)
                    {
                        var collection = new FilterGraphObjectCollection(graph, (ConcreteFilter)filter);
                        graphMenu.DropDownItems.Add(graph.Name,GetImage(RDMPConcept.AggregateGraph, OverlayKind.Filter),(s, e) => _activator.ViewFilterGraph(this, collection));
                    }
                    Items.Add(graphMenu);
                }
            }
        }
    }
}