// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.Combining;

/// <summary>
/// <see cref="ICombineToMakeCommand"/> for an object of type <see cref="IFilter"/>
/// </summary>
public class FilterCombineable : ICombineToMakeCommand
{
    public IFilter Filter { get; private set; }

    public IContainer ImmediateContainerIfAny { get; private set; }
    public IContainer RootContainerIfAny { get; private set; }

    public Catalogue SourceCatalogueIfAny { get; private set; }

    /// <summary>
    /// All the containers that are in the current filter tree (includes the Root).
    /// </summary>
    public List<IContainer> AllContainersInEntireTreeFromRootDown { get; private set; }

    public FilterCombineable(IFilter filter)
    {
        Filter = filter;

        FindContainers();

        if (ImmediateContainerIfAny != null)
            SourceCatalogueIfAny = ImmediateContainerIfAny.GetCatalogueIfAny();
    }

    private void FindContainers()
    {
        ImmediateContainerIfAny = Filter.FilterContainer;
        AllContainersInEntireTreeFromRootDown = new List<IContainer>();

        if (ImmediateContainerIfAny != null)
        {
            RootContainerIfAny = ImmediateContainerIfAny.GetRootContainerOrSelf();

            //so we can determine whether we are being dragged into a new hierarchy tree (copy) or just being dragged around inside our own tree (move)
            AllContainersInEntireTreeFromRootDown.Add(RootContainerIfAny);
            AllContainersInEntireTreeFromRootDown.AddRange(RootContainerIfAny.GetAllSubContainersRecursively());
        }
    }


    public string GetSqlString() => Filter.WhereSQL;
}