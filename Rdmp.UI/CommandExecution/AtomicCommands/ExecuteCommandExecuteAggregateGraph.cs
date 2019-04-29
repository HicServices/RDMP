// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.UI.AggregationUIs;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExecuteAggregateGraph:BasicUICommandExecution,IAtomicCommand
    {
        private readonly AggregateConfiguration _aggregate;

        public ExecuteCommandExecuteAggregateGraph(IActivateItems activator,AggregateConfiguration aggregate) : base(activator)
        {
            _aggregate = aggregate;

            if (aggregate.IsCohortIdentificationAggregate) 
                SetImpossible("AggregateConfiguration is a Cohort aggregate");
            
            SetImpossibleIfFailsChecks(aggregate);

            UseTripleDotSuffix = true;
        }

        public override string GetCommandHelp()
        {
            return "Assembles and runs the graph query and renders the results as a graph";
        }

        public override void Execute()
        {
            base.Execute();

            var graph = Activator.Activate<AggregateGraphUI, AggregateConfiguration>(_aggregate);
            graph.LoadGraphAsync();
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.Graph;
        }
    }
}
