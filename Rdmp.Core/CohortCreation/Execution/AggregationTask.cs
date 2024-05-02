// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CohortCreation.Execution;

/// <summary>
///     A single AggregateConfiguration being executed by a CohortCompiler.  The AggregateConfiguration will be a query
///     like 'select distinct patientId from
///     TableX where ...'.  The  query result table can/will be committed as a CacheCommitIdentifierList to  the
///     CachedAggregateConfigurationResultsManager.
/// </summary>
public class AggregationTask : CacheableTask
{
    public AggregateConfiguration Aggregate { get; }

    private readonly string _catalogueName;
    private readonly CohortIdentificationConfiguration _cohortIdentificationConfiguration;
    private readonly List<CohortAggregateContainer> _allParentContainers;

    public AggregationTask(AggregateConfiguration aggregate, CohortCompiler compiler) : base(compiler)
    {
        Aggregate = aggregate;
        _catalogueName = aggregate.Catalogue.Name;
        _cohortIdentificationConfiguration = compiler.CohortIdentificationConfiguration;

        var container = aggregate.GetCohortAggregateContainerIfAny();

        if (container != null)
        {
            _allParentContainers = container.GetAllParentContainers().ToList();
            _allParentContainers.Add(container);
        }
    }


    public override string GetCatalogueName()
    {
        return _catalogueName;
    }

    public override IMapsDirectlyToDatabaseTable Child => Aggregate;


    public override string ToString()
    {
        var name = Aggregate.ToString();

        var expectedTrimStart = _cohortIdentificationConfiguration.GetNamingConventionPrefixForConfigurations();

        return name.StartsWith(expectedTrimStart) ? name[expectedTrimStart.Length..] : name;
    }

    public override IDataAccessPoint[] GetDataAccessPoints()
    {
        return Aggregate.Catalogue.GetTableInfoList(false);
    }

    public override bool IsEnabled()
    {
        //aggregate is not disabled and none of the parent containers are disabled either
        return !Aggregate.IsDisabled && !_allParentContainers.Any(c => c.IsDisabled);
    }

    public override AggregateConfiguration GetAggregateConfiguration()
    {
        return Aggregate;
    }

    public override CacheCommitArguments GetCacheArguments(string sql, DataTable results,
        DatabaseColumnRequest[] explicitTypes)
    {
        return new CacheCommitIdentifierList(Aggregate, sql, results, explicitTypes.Single(), Timeout);
    }

    public override void ClearYourselfFromCache(CachedAggregateConfigurationResultsManager manager)
    {
        manager.DeleteCacheEntryIfAny(Aggregate, AggregateOperation.IndexedExtractionIdentifierList);
    }
}