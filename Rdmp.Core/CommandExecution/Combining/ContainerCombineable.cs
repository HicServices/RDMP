// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.Combining;

/// <summary>
/// <see cref="ICombineToMakeCommand"/> for an object of type <see cref="IContainer"/>
/// </summary>
public class ContainerCombineable : ICombineToMakeCommand
{
    public IContainer Container { get; private set; }

    /// <summary>
    /// All the containers that are further down the container hierarchy from this Container.  This includes all children containers, their children and their children and so on.
    /// </summary>
    public List<IContainer> AllSubContainersRecursive { get; private set; }

    /// <summary>
    /// All the containers that are in the current filter tree (includes the Root - which might be us btw and all children).  If Container is the Root then this property
    /// will be the same as AllSubContainersRecursive except that it will also include the Root
    /// </summary>
    public List<IContainer> AllContainersInEntireTreeFromRootDown { get; private set; }

    public ContainerCombineable(IContainer container)
    {
        Container = container;
        AllSubContainersRecursive = Container.GetAllSubContainersRecursively();

        var root = Container.GetRootContainerOrSelf();
        AllContainersInEntireTreeFromRootDown = root.GetAllSubContainersRecursively();
        AllContainersInEntireTreeFromRootDown.Add(root);
    }

    public string GetSqlString() => null;
}