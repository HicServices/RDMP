using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.AggregationUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExecuteAggregateGraph:BasicUICommandExecution,IAtomicCommand
    {
        private readonly AggregateConfiguration _aggregate;

        public ExecuteCommandExecuteAggregateGraph(IActivateItems activator,AggregateConfiguration aggregate) : base(activator)
        {
            _aggregate = aggregate;

            if (aggregate.IsCohortIdentificationAggregate) 
                SetImpossible("AggregateConfiguration is a Cohort aggregate");
        }


        public override void Execute()
        {
            base.Execute();

            var graph = Activator.Activate<AggregateGraph, AggregateConfiguration>(_aggregate);
            graph.LoadGraphAsync();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.Graph;
        }
    }
}
