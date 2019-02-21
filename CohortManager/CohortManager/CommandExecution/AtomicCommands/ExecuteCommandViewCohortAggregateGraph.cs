// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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

            if(collection.CohortIfAny != null && collection.CohortIfAny.IsJoinablePatientIndexTable())
                SetImpossible("Graphs cannot be generated for Patient Index tables");
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
