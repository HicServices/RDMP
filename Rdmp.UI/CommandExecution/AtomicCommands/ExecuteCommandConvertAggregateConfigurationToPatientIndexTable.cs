// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandConvertAggregateConfigurationToPatientIndexTable : BasicUICommandExecution
    {
        private readonly AggregateConfigurationCombineable _sourceAggregateConfigurationCommand;
        private readonly CohortIdentificationConfiguration _cohortIdentificationConfiguration;
        
        public ExecuteCommandConvertAggregateConfigurationToPatientIndexTable(IActivateItems activator, AggregateConfigurationCombineable sourceAggregateConfigurationCommand,CohortIdentificationConfiguration cohortIdentificationConfiguration) : base(activator)
        {
            _sourceAggregateConfigurationCommand = sourceAggregateConfigurationCommand;
            _cohortIdentificationConfiguration = cohortIdentificationConfiguration;

            if(sourceAggregateConfigurationCommand.JoinableDeclarationIfAny != null)
                SetImpossible("Aggregate is already a Patient Index Table");

            if(_sourceAggregateConfigurationCommand.CohortIdentificationConfigurationIfAny != null &&
                _sourceAggregateConfigurationCommand.CohortIdentificationConfigurationIfAny.ID != _cohortIdentificationConfiguration.ID)
                SetImpossible("Aggregate '" + _sourceAggregateConfigurationCommand.Aggregate + "'  belongs to a different Cohort Identification Configuration");

        }

        public override void Execute()
        {
            base.Execute();

            var sourceAggregate = _sourceAggregateConfigurationCommand.Aggregate;
            
            //make sure it is not part of any folders
            var parent = sourceAggregate.GetCohortAggregateContainerIfAny();
            if (parent != null)
                parent.RemoveChild(sourceAggregate);

            //create a new patient index table usage allowance for this aggregate
            new JoinableCohortAggregateConfiguration(Activator.RepositoryLocator.CatalogueRepository, _cohortIdentificationConfiguration, sourceAggregate);
            
            Publish(_cohortIdentificationConfiguration);
            Emphasise(sourceAggregate);
        }
    }
}