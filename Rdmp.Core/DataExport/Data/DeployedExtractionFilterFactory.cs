// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     Constructs IFilters etc for data extraction via SelectedDataSets (See IFilterFactory).  Each SelectedDataSets in an
///     ExtractionConfiguration has (optionally)
///     its own root container IFilters, subcontainers etc.
/// </summary>
public class DeployedExtractionFilterFactory : IFilterFactory
{
    private readonly IDataExportRepository _repository;

    /// <summary>
    ///     Prepares to create extraction filters for project datasets int eh provided <paramref name="repository" />
    /// </summary>
    /// <param name="repository"></param>
    public DeployedExtractionFilterFactory(IDataExportRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public IFilter CreateNewFilter(string name)
    {
        return new DeployedExtractionFilter(_repository, name, null);
    }

    /// <inheritdoc />
    public ISqlParameter CreateNewParameter(IFilter filter, string parameterSQL)
    {
        return new DeployedExtractionFilterParameter(_repository, parameterSQL, filter);
    }

    /// <inheritdoc />
    public Type GetRootOwnerType()
    {
        return typeof(SelectedDataSets);
    }

    /// <inheritdoc />
    public Type GetIContainerTypeIfAny()
    {
        return typeof(FilterContainer);
    }

    public IContainer CreateNewContainer()
    {
        return new FilterContainer(_repository);
    }
}