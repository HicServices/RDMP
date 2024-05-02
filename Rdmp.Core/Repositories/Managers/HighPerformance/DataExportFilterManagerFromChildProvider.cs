// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;

namespace Rdmp.Core.Repositories.Managers.HighPerformance;

/// <summary>
///     Provides a memory based efficient (in terms of the number of database queries sent) way of finding all containers
///     and subcontainers and filters in the entire DataExportManager
///     database at once rather than using the methods on IContainer and IFilter which send individual database queries for
///     relevant subcontainers etc.
/// </summary>
internal class DataExportFilterManagerFromChildProvider : DataExportFilterManager
{
    private readonly Dictionary<int, List<FilterContainer>> _subContainers = new();

    private readonly Dictionary<int, List<DeployedExtractionFilter>> _containersToFilters;

    /// <summary>
    ///     Fetches all containers and filters out of the <paramref name="repository" /> and sets the class up to provide
    ///     fast access to them.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="childProvider"></param>
    public DataExportFilterManagerFromChildProvider(DataExportRepository repository,
        DataExportChildProvider childProvider) : base(repository)
    {
        _containersToFilters = childProvider.AllDeployedExtractionFilters
            .Where(static f => f.FilterContainer_ID.HasValue).GroupBy(static f => f.FilterContainer_ID.Value)
            .ToDictionary(static gdc => gdc.Key, static gdc => gdc.ToList());

        var server = repository.DiscoveredServer;
        using var con = repository.GetConnection();
        using var r = server.GetCommand("SELECT *  FROM FilterContainerSubcontainers", con).ExecuteReader();
        while (r.Read())
        {
            var parentId = Convert.ToInt32(r["FilterContainer_ParentID"]);
            var subcontainerId = Convert.ToInt32(r["FilterContainerChildID"]);

            if (!_subContainers.ContainsKey(parentId))
                _subContainers.Add(parentId, new List<FilterContainer>());

            _subContainers[parentId].Add(childProvider.AllContainers[subcontainerId]);
        }

        r.Close();
    }

    /// <summary>
    ///     Returns all subcontainers found in the <paramref name="parent" /> (results are returned from the cache created
    ///     during class construction)
    /// </summary>
    public override IContainer[] GetSubContainers(IContainer parent)
    {
        return _subContainers.TryGetValue(parent.ID, out var result)
            ? result.ToArray()
            : Array.Empty<IContainer>();
    }

    public override IFilter[] GetFilters(IContainer container)
    {
        return _containersToFilters.TryGetValue(container.ID, out var filters)
            ? filters.ToArray()
            : Array.Empty<IFilter>();
    }
}