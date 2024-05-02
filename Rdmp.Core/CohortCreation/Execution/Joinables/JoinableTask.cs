// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


using System;
using System.Data;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CohortCreation.Execution.Joinables;

/// <summary>
///     A single AggregateConfiguration being executed by a CohortCompiler which is defined as a
///     JoinableCohortAggregateConfiguration.  The
///     AggregateConfiguration will be a query like 'select distinct patientId, drugName,prescribedDate from  TableX where
///     ...'.  The  query
///     result table can/will be committed as a CacheCommitJoinableInceptionQuery to  the
///     CachedAggregateConfigurationResultsManager.
/// </summary>
public class JoinableTask : CacheableTask
{
    private readonly CohortIdentificationConfiguration _cohortIdentificationConfiguration;
    private readonly AggregateConfiguration _aggregate;
    private readonly string _catalogueName;

    public JoinableCohortAggregateConfiguration Joinable { get; }


    public JoinableTask(JoinableCohortAggregateConfiguration joinable, CohortCompiler compiler) : base(compiler)
    {
        Joinable = joinable;
        _aggregate = Joinable.AggregateConfiguration;
        _cohortIdentificationConfiguration = _aggregate.GetCohortIdentificationConfigurationIfAny();

        _catalogueName = Joinable.AggregateConfiguration.Catalogue.Name;
        RefreshIsUsedState();
    }

    public override string GetCatalogueName()
    {
        return _catalogueName;
    }

    public override IMapsDirectlyToDatabaseTable Child => Joinable;

    public override IDataAccessPoint[] GetDataAccessPoints()
    {
        return Joinable.AggregateConfiguration.Catalogue.GetTableInfoList(false);
    }

    public override bool IsEnabled()
    {
        return !_aggregate.IsDisabled;
    }

    public override string ToString()
    {
        var name = _aggregate.Name;
        var expectedTrimStart = _cohortIdentificationConfiguration.GetNamingConventionPrefixForConfigurations();
        return name.StartsWith(expectedTrimStart, StringComparison.Ordinal) ? name[expectedTrimStart.Length..] : name;
    }

    public override AggregateConfiguration GetAggregateConfiguration()
    {
        return Joinable.AggregateConfiguration;
    }

    public override CacheCommitArguments
        GetCacheArguments(string sql, DataTable results, DatabaseColumnRequest[] explicitTypes)
    {
        return new CacheCommitJoinableInceptionQuery(Joinable.AggregateConfiguration, sql, results, explicitTypes,
            Timeout);
    }

    public override void ClearYourselfFromCache(CachedAggregateConfigurationResultsManager manager)
    {
        manager.DeleteCacheEntryIfAny(Joinable.AggregateConfiguration, AggregateOperation.JoinableInceptionQuery);
    }

    public override int Order
    {
        get => Joinable.ID;
        set => throw new NotSupportedException();
    }

    public bool IsUnused { get; private set; }

    public void RefreshIsUsedState()
    {
        IsUnused = !Joinable.Users.Any();
    }

    public string GetUnusedWarningText()
    {
        return $"Patient Index Table '{ToString()}' is not used by any of your sets (above).";
    }
}