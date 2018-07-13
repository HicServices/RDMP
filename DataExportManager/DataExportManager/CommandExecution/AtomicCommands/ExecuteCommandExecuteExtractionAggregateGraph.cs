using System.Drawing;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportManager.ProjectUI.Graphs;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExecuteExtractionAggregateGraph : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ExtractionAggregateGraphObjectCollection _collection;

        public ExecuteCommandExecuteExtractionAggregateGraph(IActivateItems activator,ExtractionAggregateGraphObjectCollection collection) : base(activator)
        {
            _collection = collection;
        }

        public override string GetCommandHelp()
        {
            return "Shows a subset of the main graph as it applies to the records that will be extracted";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.AggregateGraph);
        }

        public override void Execute()
        {
            base.Execute();

            Activator.Activate<ExtractionAggregateGraph>(_collection);
        }
    }
}
