// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandReOrderAggregateContainer : BasicUICommandExecution 
    {
        private readonly CohortAggregateContainerCommand _sourceCohortAggregateContainerCommand;

        private CohortAggregateContainer _targetParent;
        private IOrderable _targetOrderable;
        private readonly InsertOption _insertOption;

        public ExecuteCommandReOrderAggregateContainer(IActivateItems activator, CohortAggregateContainerCommand sourceCohortAggregateContainerCommand, CohortAggregateContainer targetCohortAggregateContainer, InsertOption insertOption):this(activator,targetCohortAggregateContainer,insertOption)
        {
            _sourceCohortAggregateContainerCommand = sourceCohortAggregateContainerCommand;
            
            _targetParent = targetCohortAggregateContainer.GetParentContainerIfAny();
            
            //reorder is only possible within a container
            if (!_sourceCohortAggregateContainerCommand.ParentContainerIfAny.Equals(_targetParent))
                SetImpossible("First move containers to share the same parent container");

            if(_insertOption == InsertOption.Default)
                SetImpossible("Insert must be above/below");
        }

        public ExecuteCommandReOrderAggregateContainer(IActivateItems activator, CohortAggregateContainerCommand sourceCohortAggregateContainerCommand, AggregateConfiguration targetAggregate, InsertOption insertOption):this(activator,targetAggregate,insertOption)
        {
            _sourceCohortAggregateContainerCommand = sourceCohortAggregateContainerCommand;
            _targetParent = targetAggregate.GetCohortAggregateContainerIfAny();

            //if they do not share the same parent container
            if(!_sourceCohortAggregateContainerCommand.ParentContainerIfAny.Equals(_targetParent))
                SetImpossible("First move objects into the same parent container");
        }

        private ExecuteCommandReOrderAggregateContainer(IActivateItems activator,IOrderable orderable, InsertOption insertOption) : base(activator)
        {
            _targetOrderable = orderable;
            _insertOption = insertOption;

        }

        public override void Execute()
        {
            base.Execute();

            var source = _sourceCohortAggregateContainerCommand.AggregateContainer;

            int order = _targetOrderable.Order;
            
            _targetParent.CreateInsertionPointAtOrder(source,order,_insertOption == InsertOption.InsertAbove);
            source.Order = order;
            source.SaveToDatabase();

            //refresh the parent container
            Publish(_sourceCohortAggregateContainerCommand.ParentContainerIfAny);
        }
    }
}