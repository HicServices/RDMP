using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportManager.ProjectUI.Graphs;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExecuteExtractionAggregateGraph : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ExtractionAggregateGraphObjectCollection _collection;

        public ExecuteCommandExecuteExtractionAggregateGraph(IActivateItems activator,ExtractionAggregateGraphObjectCollection collection) : base(activator)
        {
            _collection = collection;
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
