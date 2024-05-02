// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.FilterImporting.Construction;

/// <summary>
///     Constructs IFilters etc for main Catalogue database filter (ExtractionFilter).  These are the master filters which
///     are copied out as needed for cohort identification,
///     extraction etc and therefore do not have any IContainer type (AND/OR).
/// </summary>
public class ExtractionFilterFactory : IFilterFactory
{
    private readonly ICatalogueRepository _repository;
    private readonly ExtractionInformation _extractionInformation;

    /// <summary>
    ///     Prepares to create master extraction filters at <see cref="Catalogue" /> level which can be reused in cohort
    ///     generation, project extractions etc.  Filters created
    ///     will be stored under the specific <paramref name="extractionInformation" /> (extractable column) provided.
    /// </summary>
    /// <param name="extractionInformation"></param>
    public ExtractionFilterFactory(ExtractionInformation extractionInformation)
    {
        _repository = (ICatalogueRepository)extractionInformation.Repository;
        _extractionInformation = extractionInformation;
    }

    /// <inheritdoc />
    public IFilter CreateNewFilter(string name)
    {
        return new ExtractionFilter(_repository, name, _extractionInformation);
    }

    /// <inheritdoc />
    public ISqlParameter CreateNewParameter(IFilter filter, string parameterSQL)
    {
        return new ExtractionFilterParameter(_repository, parameterSQL, (ExtractionFilter)filter);
    }

    /// <inheritdoc />
    public Type GetRootOwnerType()
    {
        return typeof(ExtractionInformation);
    }

    /// <inheritdoc />
    public Type GetIContainerTypeIfAny()
    {
        return null;
    }

    public IContainer CreateNewContainer()
    {
        throw new NotSupportedException("Catalogue level master filters do not support IContainers");
    }
}