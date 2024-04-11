// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Providers;

namespace Rdmp.Core.Repositories.Managers.HighPerformance;

/// <summary>
/// Performance class that builds the hierarchy of CohortIdentificationConfiguration children.  This includes containers (CohortAggregateContainer) and subcontainers
/// and thier contained cohort sets ( AggregateConfiguration).  This is done in memory by fetching all the relevant relationship records with two queries and then
/// sorting out the already fetched objects in CatalogueChildProvider into the relevant hierarchy.
/// 
/// <para>This allows you to use GetSubContainers and GetAggregateConfigurations in bulk without having to use the method on IContainer directly (which goes back to the database).</para>
/// </summary>
internal class CohortContainerManagerFromChildProvider : CohortContainerManager
{
    private readonly Dictionary<int, List<IOrderable>> _contents = new();

    public CohortContainerManagerFromChildProvider(CatalogueRepository repository, CatalogueChildProvider childProvider)
        : base(repository)
    {
        FetchAllRelationships(childProvider);
    }

    /// <summary>
    /// Returns cached answers without running database queries
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    public override IOrderable[] GetChildren(CohortAggregateContainer parent) =>
        _contents.TryGetValue(parent.ID, out var content) ? content.ToArray() : Array.Empty<IOrderable>();

    public void FetchAllRelationships(ICoreChildProvider childProvider)
    {
        using var con = CatalogueRepository.GetConnection();
        //find all the cohort SET operation subcontainers e.g. UNION Ag1,Ag2,(Agg3 INTERSECT Agg4) would have 2 CohortAggregateContainers (the UNION and the INTERSECT) in which the INTERSECT was the child container of the UNION
        var r = CatalogueRepository.DiscoveredServer
            .GetCommand(
                "SELECT [CohortAggregateContainer_ParentID],[CohortAggregateContainer_ChildID] FROM [CohortAggregateSubContainer] ORDER BY CohortAggregateContainer_ParentID",
                con).ExecuteReader();

        while (r.Read())
        {
            var currentParentId = Convert.ToInt32(r["CohortAggregateContainer_ParentID"]);
            var currentChildId = Convert.ToInt32(r["CohortAggregateContainer_ChildID"]);

            if (!_contents.ContainsKey(currentParentId))
                _contents.Add(currentParentId, new List<IOrderable>());

            _contents[currentParentId]
                .Add(childProvider.AllCohortAggregateContainers.Value.Single(c => c.ID == currentChildId));
        }

        r.Close();

        //now find all the Agg configurations within the containers too, (in the above example we will find Agg1 in the UNION container at order 1 and Agg2 at order 2 and then we find Agg3 and Agg4 in the INTERSECT container)
        r = CatalogueRepository.DiscoveredServer
            .GetCommand(
                @"SELECT [CohortAggregateContainer_ID], [AggregateConfiguration_ID],[Order] FROM [CohortAggregateContainer_AggregateConfiguration] ORDER BY CohortAggregateContainer_ID",
                con).ExecuteReader();

        while (r.Read())
        {
            var currentParentId = Convert.ToInt32(r["CohortAggregateContainer_ID"]);
            var currentChildId = Convert.ToInt32(r["AggregateConfiguration_ID"]);
            var currentOrder = Convert.ToInt32(r["Order"]);

            if (!_contents.ContainsKey(currentParentId))
                _contents.Add(currentParentId, new List<IOrderable>());

            AggregateConfiguration config;

            try
            {
                config = childProvider.AllAggregateConfigurations.Value.Single(a => a.ID == currentChildId);
            }
            catch (Exception)
            {
                throw new Exception(
                    $"Error occurred trying to find AggregateConfiguration with ID {currentChildId} which is allegedly a child of CohortAggregateContainer {currentParentId}");
            }

            config.SetKnownOrder(currentOrder);

                _contents[currentParentId].Add(config);
        }

        r.Close();
    }
}