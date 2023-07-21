// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandMoveCohortAggregateContainerIntoSubContainer : BasicCommandExecution
{
    private readonly CohortAggregateContainerCombineable _sourceCohortAggregateContainer;
    private readonly CohortAggregateContainer _targetCohortAggregateContainer;
        
    [UseWithObjectConstructor]
    public ExecuteCommandMoveCohortAggregateContainerIntoSubContainer(IBasicActivateItems activator, CohortAggregateContainer toMove, CohortAggregateContainer into) 
        : this(activator,new CohortAggregateContainerCombineable(toMove),into)
    {

    }
    public ExecuteCommandMoveCohortAggregateContainerIntoSubContainer(IBasicActivateItems activator, CohortAggregateContainerCombineable sourceCohortAggregateContainer, CohortAggregateContainer targetCohortAggregateContainer) : base(activator)
    {    
        _sourceCohortAggregateContainer = sourceCohortAggregateContainer;
        _targetCohortAggregateContainer = targetCohortAggregateContainer;

        if(_sourceCohortAggregateContainer.AllSubContainersRecursively.Contains(_targetCohortAggregateContainer))
            SetImpossible("Cannot move a container into one of its own subcontainers");

        if(_sourceCohortAggregateContainer.AggregateContainer.Equals(_targetCohortAggregateContainer))
            SetImpossible("Cannot move a container into itself");
            
        if(_targetCohortAggregateContainer.ShouldBeReadOnly(out var reason))
            SetImpossible(reason);
    }

    public override void Execute()
    {
        base.Execute();

        var cic = _sourceCohortAggregateContainer.AggregateContainer.GetCohortIdentificationConfiguration();
        var srcContainer = _sourceCohortAggregateContainer.AggregateContainer;
        srcContainer.MakeIntoAnOrphan();
        _targetCohortAggregateContainer.AddChild(srcContainer);
        Publish(cic);
    }
}