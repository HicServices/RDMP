// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core.CommandExecution.Combining;

/// <summary>
/// <see cref="ICombineToMakeCommand"/> for an object of type <see cref="CohortAggregateContainer"/>
/// </summary>
public class CohortAggregateContainerCombineable : ICombineToMakeCommand
{
    public CohortAggregateContainer ParentContainerIfAny { get; private set; }
    public CohortAggregateContainer AggregateContainer { get; private set; }
    public List<CohortAggregateContainer> AllSubContainersRecursively { get; private set; }

    public CohortAggregateContainerCombineable(CohortAggregateContainer aggregateContainer)
    {
        AggregateContainer = aggregateContainer;
        AllSubContainersRecursively = AggregateContainer.GetAllSubContainersRecursively();

        ParentContainerIfAny = AggregateContainer.GetParentContainerIfAny();
    }



    public string GetSqlString() => null;
}