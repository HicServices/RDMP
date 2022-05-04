// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.Cohort;
using System;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandMoveAggregateIntoContainer : BasicCommandExecution
    {
        private readonly CohortAggregateContainer _targetCohortAggregateContainer;
        private readonly AggregateConfigurationCombineable _sourceAggregateCommand;
        
        public ExecuteCommandMoveAggregateIntoContainer(IBasicActivateItems activator, AggregateConfigurationCombineable sourceAggregateCommand, CohortAggregateContainer targetCohortAggregateContainer) : base(activator)
        {
            _sourceAggregateCommand = sourceAggregateCommand;
            _targetCohortAggregateContainer = targetCohortAggregateContainer;

            var cic = _sourceAggregateCommand.CohortIdentificationConfigurationIfAny;

            if(cic != null && !cic.Equals(_targetCohortAggregateContainer.GetCohortIdentificationConfiguration()))
                SetImpossible("Aggregate belongs to a different CohortIdentificationConfiguration");

            if(_sourceAggregateCommand.ContainerIfAny != null &&  _sourceAggregateCommand.ContainerIfAny.Equals(targetCohortAggregateContainer))
                SetImpossible("Aggregate is already in container");

            if(targetCohortAggregateContainer.ShouldBeReadOnly(out string reason))
                SetImpossible(reason);
        }

        public override void Execute()
        {
            base.Execute();

            //remove it from it's old container
            var oldContainer = _sourceAggregateCommand.ContainerIfAny;
            
            if(oldContainer != null)
                oldContainer.RemoveChild(_sourceAggregateCommand.Aggregate);

            //add  it to the new container
            _targetCohortAggregateContainer.AddChild(_sourceAggregateCommand.Aggregate);
            
            
            //refresh the entire configuration
            Publish(_targetCohortAggregateContainer.GetCohortIdentificationConfiguration());
        }
    }
}