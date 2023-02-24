// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandMakePatientIndexTableIntoRegularCohortIdentificationSetAgain : BasicCommandExecution
{
    private readonly AggregateConfigurationCombineable _sourceAggregateCommand;
    private readonly CohortAggregateContainer _targetCohortAggregateContainer;

    [UseWithObjectConstructor]
    public ExecuteCommandMakePatientIndexTableIntoRegularCohortIdentificationSetAgain(IBasicActivateItems activator, AggregateConfiguration aggregate, CohortAggregateContainer targetCohortAggregateContainer) 
        : this (activator,new AggregateConfigurationCombineable(aggregate), targetCohortAggregateContainer)
    {

    }

    public ExecuteCommandMakePatientIndexTableIntoRegularCohortIdentificationSetAgain(IBasicActivateItems activator, AggregateConfigurationCombineable sourceAggregateCommand, CohortAggregateContainer targetCohortAggregateContainer) : base(activator)
    {
        _sourceAggregateCommand = sourceAggregateCommand;
        _targetCohortAggregateContainer = targetCohortAggregateContainer;

        if (!_sourceAggregateCommand.CohortIdentificationConfigurationIfAny.Equals(_targetCohortAggregateContainer.GetCohortIdentificationConfiguration()))
            SetImpossible("Aggregate belongs to a different CohortIdentificationConfiguration");
            
        if(_sourceAggregateCommand.JoinableUsersIfAny.Any())
            SetImpossible(
                $"The following Cohort Set(s) use this PatientIndex table:{string.Join(",", _sourceAggregateCommand.JoinableUsersIfAny.Select(j => j.ToString()))}");

        if(_targetCohortAggregateContainer.ShouldBeReadOnly(out var reason))
            SetImpossible(reason);
    }

    public override void Execute()
    {
        base.Execute();

        //remove it from its old container (really shouldn't be in any!) 
        if(_sourceAggregateCommand.ContainerIfAny != null)
            _sourceAggregateCommand.ContainerIfAny.RemoveChild(_sourceAggregateCommand.Aggregate);

        //remove any non IsExtractionIdentifier columns
        foreach (var dimension in _sourceAggregateCommand.Aggregate.AggregateDimensions)
            if (!dimension.IsExtractionIdentifier)
                if (YesNo(
                        $"Changing to a CohortSet means deleting AggregateDimension '{dimension}'.  Ok?",
                        "Delete Aggregate Dimension"))
                    dimension.DeleteInDatabase();
                else
                    return;
            
        //make it is no longer a joinable
        _sourceAggregateCommand.JoinableDeclarationIfAny.DeleteInDatabase();

        //add  it to the new container
        _targetCohortAggregateContainer.AddChild(_sourceAggregateCommand.Aggregate, 0);

        //refresh the entire configuration
        Publish(_sourceAggregateCommand.CohortIdentificationConfigurationIfAny);
    }
}