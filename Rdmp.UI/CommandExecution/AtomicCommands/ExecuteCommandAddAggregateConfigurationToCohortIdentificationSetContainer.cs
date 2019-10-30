// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer :BasicUICommandExecution
    {
        private readonly AggregateConfigurationCommand _aggregateConfigurationCommand;
        private readonly CohortAggregateContainer _targetCohortAggregateContainer;

        public AggregateConfiguration AggregateCreatedIfAny { get; private set; }

        /// <summary>
        /// True if the <see cref="AggregateConfigurationCommand"/> passed to the constructor was a newly created one and does
        /// not need cloning.
        /// </summary>
        public bool DoNotClone { get; set; }
        
        public ExecuteCommandAddAggregateConfigurationToCohortIdentificationSetContainer(IActivateItems activator,AggregateConfigurationCommand aggregateConfigurationCommand, CohortAggregateContainer targetCohortAggregateContainer) : base(activator)
        {
            _aggregateConfigurationCommand = aggregateConfigurationCommand;
            _targetCohortAggregateContainer = targetCohortAggregateContainer;
        }

        public override void Execute()
        {
            base.Execute();

            var cic = _targetCohortAggregateContainer.GetCohortIdentificationConfiguration();

            AggregateConfiguration child = DoNotClone ? _aggregateConfigurationCommand.Aggregate :
                cic.ImportAggregateConfigurationAsIdentifierList(_aggregateConfigurationCommand.Aggregate,CohortCommandHelper.PickOneExtractionIdentifier);

            //current contents
            var contents = _targetCohortAggregateContainer.GetOrderedContents().ToArray();

            //insert it at the begining of the contents
            int minimumOrder = 0;
            if(contents.Any())
                minimumOrder = contents.Min(o => o.Order);

            //bump everyone down to make room
            _targetCohortAggregateContainer.CreateInsertionPointAtOrder(child,minimumOrder,true);
            _targetCohortAggregateContainer.AddChild(child,minimumOrder);
            Publish(_targetCohortAggregateContainer);

            AggregateCreatedIfAny = child;
        }
    }
}