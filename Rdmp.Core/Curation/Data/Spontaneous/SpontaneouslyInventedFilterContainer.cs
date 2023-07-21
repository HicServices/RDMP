// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.Spontaneous;

/// <summary>
///     Spontaneous (memory only) implementation of IContainer.
///     <para>
///         IContainers are collections of subcontainers and WHERE statements e.g.
///         (
///         --age is above 5
///         Age > 5
///         AND
///         --name is bob
///         Name like 'Bob%'
///         )
///     </para>
///     <para>
///         Most IContainers come from the DataCatalogue/DataExport Database and are a hierarchical list of filters the
///         user wants to use to create a query.  But sometimes IN CODE,
///         we want to create an impromptu container and ram some additional filters we have either also invented or have
///         pulled out of the Catalogue into the container.  This
///         Class lets you do that, it creates a 'memory only' container which cannot be saved/deleted etc but can be used
///         in query building by ISqlQueryBuilders.
///     </para>
///     <para>See also SpontaneouslyInventedFilter</para>
/// </summary>
public class SpontaneouslyInventedFilterContainer : ConcreteContainer, IContainer
{
    public SpontaneouslyInventedFilterContainer(MemoryCatalogueRepository repo, IContainer[] subContainersIfAny,
        IFilter[] filtersIfAny, FilterContainerOperation operation) : base(repo)
    {
        repo.InsertAndHydrate(this, new Dictionary<string, object>());

        if (subContainersIfAny != null)
            foreach (var container in subContainersIfAny)
                AddChild(container);


        if (filtersIfAny != null)
            foreach (var filter in filtersIfAny)
                AddChild(filter is SpontaneouslyInventedFilter
                    ? filter
                    : new SpontaneouslyInventedFilter(repo, this, filter.WhereSQL, filter.Name, filter.Description,
                        filter.GetAllParameters()));

        Operation = operation;
    }


    public override Catalogue GetCatalogueIfAny()
    {
        return null;
    }

    public override bool ShouldBeReadOnly(out string reason)
    {
        reason = null;
        return false;
    }

    public override IContainer DeepCloneEntireTreeRecursivelyIncludingFilters()
    {
        throw new NotSupportedException("Spontaneously invented filter containers cannot be cloned");
    }

    public override IFilterFactory GetFilterFactory()
    {
        throw new NotSupportedException("Spontaneously invented filters do not have a corresponding IFilterFactory");
    }
}