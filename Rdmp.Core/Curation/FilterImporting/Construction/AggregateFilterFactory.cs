// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.FilterImporting.Construction;

/// <summary>
///     Constructs IFilters etc for AggregateConfigurations (See IFilterFactory)
/// </summary>
public class AggregateFilterFactory : IFilterFactory
{
    private readonly ICatalogueRepository _repository;

    /// <summary>
    ///     Sets class up to create <see cref="AggregateFilter" /> objects in the provided <paramref name="repository" />
    /// </summary>
    /// <param name="repository"></param>
    public AggregateFilterFactory(ICatalogueRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public IFilter CreateNewFilter(string name)
    {
        return new AggregateFilter(_repository, name);
    }

    /// <inheritdoc />
    public ISqlParameter CreateNewParameter(IFilter filter, string parameterSQL)
    {
        return new AggregateFilterParameter(_repository, parameterSQL, (AggregateFilter)filter);
    }

    /// <inheritdoc />
    public Type GetRootOwnerType()
    {
        return typeof(AggregateConfiguration);
    }

    /// <inheritdoc />
    public Type GetIContainerTypeIfAny()
    {
        return typeof(AggregateFilterContainer);
    }

    public IContainer CreateNewContainer()
    {
        return new AggregateFilterContainer(_repository, FilterContainerOperation.AND);
    }
}