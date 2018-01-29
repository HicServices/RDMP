using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CohortManager.SubComponents.Graphs;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CohortManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewCohortAggregateGraph:BasicUICommandExecution,IAtomicCommand
    {
        private readonly CohortSummaryAggregateGraphObjectCollection _collection;

        public ExecuteCommandViewCohortAggregateGraph(IActivateItems activator, CohortSummaryAggregateGraphObjectCollection collection) : base(activator)
        {
            _collection = collection;
            
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
