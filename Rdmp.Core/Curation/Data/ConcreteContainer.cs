// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Managers;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Common abstract base class for IContainer (AND/OR Where logic) classes that are persisted in the database as a
///     <see cref="DatabaseEntity" />
/// </summary>
public abstract class ConcreteContainer : DatabaseEntity, IContainer
{
    private IFilterManager _manager;
    private FilterContainerOperation _operation;

    /// <inheritdoc />
    public FilterContainerOperation Operation
    {
        get => _operation;
        set => SetField(ref _operation, value);
    }

    public ConcreteContainer()
    {
    }

    public void SetManager(IFilterManager manager)
    {
        _manager = manager;
    }

    protected ConcreteContainer(IFilterManager manager, IRepository repository, DbDataReader r) : base(repository, r)
    {
        _manager = manager;
        Operation = (FilterContainerOperation)Enum.Parse(typeof(FilterContainerOperation), r["Operation"].ToString());
    }

    protected ConcreteContainer(IFilterManager manager)
    {
        _manager = manager;
    }

    /// <inheritdoc />
    public IContainer GetParentContainerIfAny()
    {
        return _manager.GetParentContainerIfAny(this);
    }

    /// <inheritdoc />
    public IContainer[] GetSubContainers()
    {
        return _manager.GetSubContainers(this);
    }

    /// <inheritdoc />
    public IFilter[] GetFilters()
    {
        return _manager.GetFilters(this);
    }

    /// <inheritdoc />
    public void AddChild(IContainer child)
    {
        _manager.AddSubContainer(this, child);
    }

    /// <inheritdoc />
    public void AddChild(IFilter filter)
    {
        if (filter.FilterContainer_ID.HasValue)
            if (filter.FilterContainer_ID == ID)
                return; //It's already a child of us
            else
                throw new NotSupportedException(
                    $"Filter {filter} is already a child of nother container (ID={filter.FilterContainer_ID})");

        _manager.AddChild(this, filter);
    }

    /// <inheritdoc />
    public void MakeIntoAnOrphan()
    {
        _manager.MakeIntoAnOrphan(this);
    }

    /// <inheritdoc />
    public override void DeleteInDatabase()
    {
        var children = GetAllFiltersIncludingInSubContainersRecursively();

        //then delete any children it has itself
        foreach (var subContainer in GetAllSubContainersRecursively().Where(subContainer => subContainer.Exists()))
            subContainer.DeleteInDatabase();

        //clean up the orphans that will be created by killing ourselves
        foreach (var filter in children.Where(filter => filter.Exists()))
            filter.DeleteInDatabase();

        // then delete the actual component
        base.DeleteInDatabase();

        // remove any lingering references
        MakeIntoAnOrphan();
    }

    /// <inheritdoc />
    public IContainer GetRootContainerOrSelf()
    {
        return GetRootContainerOrSelf(this);
    }

    private static IContainer GetRootContainerOrSelf(IContainer container)
    {
        while (true)
        {
            var parent = container.GetParentContainerIfAny();
            if (parent == null) return container;
            container = parent;
        }
    }

    /// <inheritdoc />
    public List<IFilter> GetAllFiltersIncludingInSubContainersRecursively()
    {
        return GetAllFiltersIncludingInSubContainersRecursively(this);
    }

    private static List<IFilter> GetAllFiltersIncludingInSubContainersRecursively(IContainer container)
    {
        var toReturn = new List<IFilter>();

        toReturn.AddRange(container.GetFilters());

        var subs = container.GetSubContainers();

        if (subs != null)
            foreach (var sub in subs)
                toReturn.AddRange(GetAllFiltersIncludingInSubContainersRecursively(sub));

        return toReturn;
    }

    public abstract Catalogue GetCatalogueIfAny();

    /// <inheritdoc />
    public List<IContainer> GetAllSubContainersRecursively()
    {
        return GetAllSubContainersRecursively(this);
    }

    private List<IContainer> GetAllSubContainersRecursively(IContainer current)
    {
        var toReturn = new List<IContainer>();

        var currentSubs = current.GetSubContainers();
        toReturn.AddRange(currentSubs);

        foreach (var sub in currentSubs)
            toReturn.AddRange(GetAllSubContainersRecursively(sub));

        return toReturn;
    }

    /// <inheritdoc />
    public abstract bool ShouldBeReadOnly(out string reason);

    public abstract IContainer DeepCloneEntireTreeRecursivelyIncludingFilters();

    public abstract IFilterFactory GetFilterFactory();
}