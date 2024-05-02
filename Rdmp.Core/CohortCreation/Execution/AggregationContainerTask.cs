// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CohortCreation.Execution;

/// <summary>
///     The runtime/compile time wrapper for CohortAggregateContainer. UNION,EXCEPT,INTERSECT containers with 0 or more
///     AggregateConfigurations within
///     them - also optionally with other sub containers.
/// </summary>
public class AggregationContainerTask : Compileable, IOrderable
{
    private readonly CohortAggregateContainer[] _parentContainers;
    public CohortAggregateContainer Container { get; set; }

    public CohortAggregateContainer[] SubContainers { get; set; }
    public AggregateConfiguration[] ContainedConfigurations { get; set; }

    public AggregationContainerTask(CohortAggregateContainer container, CohortCompiler compiler) : base(compiler)
    {
        Container = container;

        SubContainers = compiler.CoreChildProvider.GetChildren(Container).OfType<CohortAggregateContainer>().ToArray();
        ContainedConfigurations =
            compiler.CoreChildProvider.GetChildren(Container).OfType<AggregateConfiguration>().ToArray();

        var d = compiler.CoreChildProvider.GetDescendancyListIfAnyFor(Container);
        _parentContainers = d?.Parents?.OfType<CohortAggregateContainer>()?.ToArray() ??
                            Array.Empty<CohortAggregateContainer>();
    }

    public override string GetCatalogueName()
    {
        return "";
    }

    public override IMapsDirectlyToDatabaseTable Child => Container;

    public override IDataAccessPoint[] GetDataAccessPoints()
    {
        var cataIDs = Container.GetAggregateConfigurations().Select(c => c.Catalogue_ID).Distinct().ToList();

        //if this container does not have any configurations
        if (!cataIDs.Any()) //try looking at the subcontainers
        {
            var subcontainers = Container.GetSubContainers()
                .FirstOrDefault(subcontainer => subcontainer.GetAggregateConfigurations().Any());
            if (subcontainers != null)
                cataIDs = subcontainers.GetAggregateConfigurations().Select(c => c.Catalogue_ID).Distinct().ToList();
        }

        //none of the subcontainers have any catalogues either!
        if (!cataIDs.Any())
            throw new Exception(
                $"Aggregate Container {Container.ID} does not have any datasets in it and neither does an of its direct subcontainers have any, how far down the tree do you expect me to look!");

        var catas = Container.Repository.GetAllObjectsInIDList<Catalogue>(cataIDs);

        return catas.SelectMany(c => c.GetTableInfoList(false)).ToArray();
    }

    public override bool IsEnabled()
    {
        return !Container.IsDisabled && !_parentContainers.Any(c => c.IsDisabled);
    }

    public string DescribeOperation()
    {
        return ((CohortAggregateContainer)Child).Operation switch
        {
            SetOperation.UNION =>
                @"Includes ALL patients which appear in any of the sets in this container.  If there are subcontainers
(i.e. other UNION/INTERSECT/EXCEPT blocks under this one) then the UNION operation will be applied to the
result of the subcontainer.",
            SetOperation.INTERSECT =>
                @"Only includes patients which appear in EVERY set in this container.  This means that for a patient to
returned by this operation they must be in all the sets under this (including the results of any subcontainers)",
            SetOperation.EXCEPT =>
                @"Includes ALL patients in the FIRST set (or subcontainer) under this container but ONLY if they DO NOT
APPEAR in any of the sets that come after the FIRST.  This means that you get everyone in the first set
EXCEPT anyone appearing in any of the other sets that follow the FIRST.",
            _ => throw new ArgumentOutOfRangeException(
                $"Did not know what tool tip to return for set operation {ToString()}")
        };
    }
}