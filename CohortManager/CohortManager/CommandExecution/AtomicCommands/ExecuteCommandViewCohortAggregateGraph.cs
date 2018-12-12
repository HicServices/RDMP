using System.Drawing;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CohortManager.SubComponents.Graphs;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CohortManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewCohortAggregateGraph:BasicUICommandExecution,IAtomicCommand
    {
        private readonly CohortSummaryAggregateGraphObjectCollection _collection;

        public ExecuteCommandViewCohortAggregateGraph(IActivateItems activator, CohortSummaryAggregateGraphObjectCollection collection) : base(activator)
        {
            _collection = collection;
        }

        public override string GetCommandHelp()
        {
            return "Shows a subset of the main graph as it applies to the people in your cohort";
        }

        public override string GetCommandName()
        {
            return _collection.Graph.Name;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.AggregateGraph);
        }

        public override void Execute()
        {
            base.Execute();

            Activator.Activate<CohortSummaryAggregateGraph>(_collection);
        }
    }
}
