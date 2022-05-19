// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandConvertAggregateConfigurationToPatientIndexTable : BasicCommandExecution
    {
        private readonly AggregateConfigurationCombineable _sourceAggregateConfigurationCombineable;
        private readonly CohortIdentificationConfiguration _cohortIdentificationConfiguration;

        [UseWithObjectConstructor]
        public ExecuteCommandConvertAggregateConfigurationToPatientIndexTable(IBasicActivateItems activator, AggregateConfiguration aggregate, CohortIdentificationConfiguration cic) 
            : this(activator,new AggregateConfigurationCombineable(aggregate),cic)
        {

        }

        public ExecuteCommandConvertAggregateConfigurationToPatientIndexTable(IBasicActivateItems activator, AggregateConfigurationCombineable sourceAggregateConfigurationCommand,CohortIdentificationConfiguration cohortIdentificationConfiguration) : base(activator)
        {
            _sourceAggregateConfigurationCombineable = sourceAggregateConfigurationCommand;
            _cohortIdentificationConfiguration = cohortIdentificationConfiguration;

            if(sourceAggregateConfigurationCommand.JoinableDeclarationIfAny != null)
                SetImpossible("Aggregate is already a Patient Index Table");

            var cic = _sourceAggregateConfigurationCombineable.CohortIdentificationConfigurationIfAny;

            if( cic != null && cic.ID != _cohortIdentificationConfiguration.ID)
                SetImpossible("Aggregate '" + _sourceAggregateConfigurationCombineable.Aggregate + "'  belongs to a different Cohort Identification Configuration");
            
            if(cic != null && cic.ShouldBeReadOnly(out string reason))
                SetImpossible(reason);
        }

        public override void Execute()
        {
            base.Execute();

            var sourceAggregate = _sourceAggregateConfigurationCombineable.Aggregate;
            
            //make sure it is not part of any folders
            var parent = sourceAggregate.GetCohortAggregateContainerIfAny();
            if (parent != null)
                parent.RemoveChild(sourceAggregate);

            //create a new patient index table usage allowance for this aggregate
            new JoinableCohortAggregateConfiguration(BasicActivator.RepositoryLocator.CatalogueRepository, _cohortIdentificationConfiguration, sourceAggregate);
            
            Publish(_cohortIdentificationConfiguration);
            Emphasise(sourceAggregate);
        }
    }
}