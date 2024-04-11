// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Providers;

namespace Rdmp.Core.Repositories.Managers.HighPerformance;

/// <summary>
/// Provides a memory based efficient (in terms of the number of database queries sent) way of finding all Catalogue filters and parameters as well as those used in
/// AggregateConfigurations
/// 
/// </summary>
internal class FilterManagerFromChildProvider : AggregateFilterManager
{
    /// <summary>
    /// Where ID key is the ID of the parent and the Value List is all the subcontainers.  If there is no key there are no subcontainers.
    /// </summary>
    private readonly Dictionary<int, List<AggregateFilterContainer>> _subcontainers = new();

    private Dictionary<int, List<AggregateFilter>> _containersToFilters;

    public FilterManagerFromChildProvider(CatalogueRepository repository, ICoreChildProvider childProvider) :
        base(repository)
    {
        _containersToFilters =
            childProvider.AllAggregateFilters.Value.Where(f => f.FilterContainer_ID.HasValue)
                .GroupBy(f => f.FilterContainer_ID.Value)
                .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

        var server = repository.DiscoveredServer;
        using var con = repository.GetConnection();
        var r = server
            .GetCommand(
                "SELECT [AggregateFilterContainer_ParentID],[AggregateFilterContainer_ChildID]  FROM [AggregateFilterSubContainer]",
                con).ExecuteReader();
        while (r.Read())
        {
            var parentId = Convert.ToInt32(r["AggregateFilterContainer_ParentID"]);
            var subcontainerId = Convert.ToInt32(r["AggregateFilterContainer_ChildID"]);

            if (!_subcontainers.ContainsKey(parentId))
                _subcontainers.Add(parentId, new List<AggregateFilterContainer>());

            _subcontainers[parentId].Add(childProvider.AllAggregateContainersDictionary.Value[subcontainerId]);
        }
        r.Close();
    }

    public override IContainer[] GetSubContainers(IContainer container)
    {
        return _subcontainers.TryGetValue(container.ID, out var result) ? result.ToArray() :
            Array.Empty<AggregateFilterContainer>();
    }

    public override IFilter[] GetFilters(IContainer container) =>
        _containersToFilters.TryGetValue(container.ID, out var result) ? result.ToArray() : Array.Empty<IFilter>();
}