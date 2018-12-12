using System.Drawing;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandViewFilterMatchGraph : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IFilter _filter;
        private AggregateConfiguration[] _compatibleGraphs;

        public ExecuteCommandViewFilterMatchGraph(IActivateItems activator, IFilter filter):base(activator)
        {
            _filter = filter;
            var cata = filter.GetCatalogue();

            if(cata == null)
            {
                SetImpossible("No Catalogue found for filter");
                return;
            }
            

            //compatible graphs are those that are not part of a cic (i.e. they are proper aggregate graphs)
            var compatibleGraphs = cata.AggregateConfigurations.Where(a => !a.IsCohortIdentificationAggregate).ToArray();

            if (!compatibleGraphs.Any())
            {
                SetImpossible("No graphs defined in Catalogue '" + cata + "'");
                return;
            }

            _compatibleGraphs = compatibleGraphs;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.AggregateGraph, OverlayKind.Filter);
        }

        public override void Execute()
        {
            base.Execute();

            var selected = SelectOne(_compatibleGraphs);
            
            if(selected == null)
                return;

            Activator.ViewFilterGraph(this, new FilterGraphObjectCollection(selected, (ConcreteFilter)_filter));
        }
    }
}