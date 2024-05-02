// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.Curation.FilterImporting;

/// <summary>
///     Factory for providing the correct implementation of <see cref="FilterUIOptions" /> based on the Type of the
///     provided <see cref="IFilter" />.
/// </summary>
public class FilterUIOptionsFactory
{
    public static FilterUIOptions Create(IFilter filter)
    {
        return filter switch
        {
            AggregateFilter aggregateFilter => new AggregateFilterUIOptions(aggregateFilter),
            DeployedExtractionFilter deployedExtractionFilter => new DeployedExtractionFilterUIOptions(
                deployedExtractionFilter),
            ExtractionFilter masterCatalogueFilter => new ExtractionFilterUIOptions(masterCatalogueFilter),
            _ => throw new Exception(
                $"Expected IFilter '{filter}' to be either an AggregateFilter, DeployedExtractionFilter or a master ExtractionFilter but it was {filter.GetType().Name}")
        };
    }
}