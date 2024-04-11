// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FAnsi.Naming;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Providers;
using Rdmp.Core.QueryBuilding.Parameters;
using Rdmp.Core.QueryCaching.Aggregation;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
/// A single cohort set in a <see cref="CohortIdentificationConfiguration"/> which selects specific patients from the database by their unique <see cref="IColumn.IsExtractionIdentifier"/>.
/// Can include a join to a patient index table.  This class stores the cached (if available) uncached and partially Cached SQL for relevant subsections of the query (and the whole query).
/// So that the decision about whether to use the cache can be delayed till later
/// 
/// </summary>
public class CohortQueryBuilderDependency
{
    private readonly ICoreChildProvider _childProvider;
    private readonly IReadOnlyCollection<IPluginCohortCompiler> _pluginCohortCompilers;

    /// <summary>
    /// The primary table being queried
    /// </summary>
    public AggregateConfiguration CohortSet { get; }

    /// <summary>
    /// The relationship object describing the JOIN relationship between <see cref="CohortSet"/> and another optional table
    /// </summary>
    public JoinableCohortAggregateConfigurationUse PatientIndexTableIfAny { get; }

    /// <summary>
    /// The column in the <see cref="CohortSet"/> that is marked <see cref="IColumn.IsExtractionIdentifier"/>
    /// </summary>
    public AggregateDimension ExtractionIdentifierColumn { get; }

    /// <summary>
    /// The aggregate (query) referenced by <see cref="PatientIndexTableIfAny"/>
    /// </summary>
    public AggregateConfiguration JoinedTo { get; }

    /// <summary>
    /// The raw SQL that can be used to join the <see cref="CohortSet"/> and <see cref="PatientIndexTableIfAny"/> (if there is one).  Null if they exist
    /// on different servers (this is allowed only if the <see cref="CohortSet"/> is on the same server as the cache while the <see cref="PatientIndexTableIfAny"/>
    /// is remote).
    ///
    /// <para>This SQL does not include the parameter declaration SQL since it is designed for nesting e.g. in UNION / INTERSECT / EXCEPT hierarchy</para>
    /// </summary>
    public CohortQueryBuilderDependencySql SqlCacheless { get; private set; }

    /// <summary>
    /// The raw SQL for the <see cref="CohortSet"/> with a join against the cached artifact for the <see cref="PatientIndexTableIfAny"/>
    /// </summary>
    public CohortQueryBuilderDependencySql SqlPartiallyCached { get; private set; }

    /// <summary>
    /// Sql for a single cache fetch  that pulls the cached result of the <see cref="CohortSet"/> joined to <see cref="PatientIndexTableIfAny"/> (if there was any)
    /// </summary>
    public CohortQueryBuilderDependencySql SqlFullyCached { get; private set; }

    public CohortQueryBuilderDependencySql SqlJoinableCacheless { get; private set; }

    public CohortQueryBuilderDependencySql SqlJoinableCached { get; private set; }

    /// <summary>
    /// Locks on aggregate by ID
    /// </summary>
    private static readonly ConcurrentDictionary<int, object> AggregateLocks = new();


    public CohortQueryBuilderDependency(AggregateConfiguration cohortSet,
        JoinableCohortAggregateConfigurationUse patientIndexTableIfAny, ICoreChildProvider childProvider,
        IReadOnlyCollection<IPluginCohortCompiler> pluginCohortCompilers)
    {
        _childProvider = childProvider;
        _pluginCohortCompilers = pluginCohortCompilers;
        CohortSet = cohortSet;
        PatientIndexTableIfAny = patientIndexTableIfAny;

        //record the IsExtractionIdentifier column for the log (helps with debugging count issues)
        var eis = cohortSet?.AggregateDimensions?.Where(d => d.IsExtractionIdentifier).ToArray();

        //Multiple IsExtractionIdentifier columns is a big problem but it's handled elsewhere
        if (eis is { Length: 1 })
            ExtractionIdentifierColumn = eis[0];

        if (PatientIndexTableIfAny != null)
        {
            var join = _childProvider.AllJoinables.Value.SingleOrDefault(j =>
                           j.ID == PatientIndexTableIfAny.JoinableCohortAggregateConfiguration_ID) ??
                       throw new Exception("ICoreChildProvider did not know about the provided patient index table");
            JoinedTo = _childProvider.AllAggregateConfigurations.Value.SingleOrDefault(ac =>
                ac.ID == join.AggregateConfiguration_ID);

            if (JoinedTo == null)
                throw new Exception(
                    "ICoreChildProvider did not know about the provided patient index table AggregateConfiguration");
        }
    }

    public override string ToString() =>
        JoinedTo != null
            ? $"{CohortSet.Name}{PatientIndexTableIfAny.JoinType} JOIN {JoinedTo.Name}"
            : CohortSet.Name;

    public void Build(CohortQueryBuilderResult parent, ISqlParameter[] globals, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isSolitaryPatientIndexTable = CohortSet.IsJoinablePatientIndexTable();

        // if it is a plugin aggregate we only want to ever serve up the cached SQL
        var pluginCohortCompiler = _pluginCohortCompilers.FirstOrDefault(c => c.ShouldRun(CohortSet));
        var joinedToPluginCohortCompiler =
            JoinedTo == null ? null : _pluginCohortCompilers.FirstOrDefault(c => c.ShouldRun(JoinedTo));

        if (pluginCohortCompiler != null)
        {
            if (joinedToPluginCohortCompiler != null)
                throw new Exception($"APIs cannot be joined together ('{CohortSet}' and '{JoinedTo}')");

            if (parent.CacheManager == null)
                throw new Exception(
                    $"Aggregate '{CohortSet}' is a plugin aggregate (According to '{pluginCohortCompiler}') but no cache is configured on {CohortSet.GetCohortIdentificationConfigurationIfAny()}.  You must enable result caching to use plugin aggregates.");

            // It's a plugin aggregate so only ever run the cached SQL
            SqlFullyCached = GetCacheFetchSqlIfPossible(parent, CohortSet, SqlCacheless, isSolitaryPatientIndexTable,
                pluginCohortCompiler, cancellationToken);

            if (SqlFullyCached == null)
                throw new Exception(
                    $"Aggregate '{CohortSet}' is a plugin aggregate (According to '{pluginCohortCompiler}') but no cached results were found after running.");
            return;
        }

        //Includes the parameter declaration and no rename operations (i.e. couldn't be used for building the tree but can be used for cache hit testing).
        if (JoinedTo != null)
        {
            if (joinedToPluginCohortCompiler == null)
            {
                SqlJoinableCacheless = CohortQueryBuilderHelper.GetSQLForAggregate(JoinedTo,
                    new QueryBuilderArgs(
                        new QueryBuilderCustomArgs(), //don't respect customizations in the inception bit!
                        globals));
                SqlJoinableCached = GetCacheFetchSqlIfPossible(parent, JoinedTo, SqlJoinableCacheless, true, null,
                    cancellationToken);
            }
            else
            {
                // It is not possible to do a cacheless query because an API is involved
                SqlJoinableCached = GetCacheFetchSqlIfPossible(parent, JoinedTo, SqlJoinableCacheless, true,
                    joinedToPluginCohortCompiler, cancellationToken);

                if (SqlJoinableCached == null)
                    throw new Exception(
                        $"Unable to build query for '{CohortSet}' because it joins to API cohort '{JoinedTo}' that did not exist in the cache");

                // Since the only way to query the dataset is using the cache we can pretend that it is the cacheless way
                // of querying it too.
                SqlJoinableCacheless = SqlJoinableCached;
            }
        }

        if (isSolitaryPatientIndexTable)
        {
            //explicit execution of a patient index table on its own
            //the full uncached SQL for the query
            SqlCacheless =
                CohortQueryBuilderHelper.GetSQLForAggregate(CohortSet, new QueryBuilderArgs(parent.Customise, globals));

            if (SqlJoinableCached != null)
                throw new QueryBuildingException("Patient index tables can't use other patient index tables!");
        }
        else
        {
            //the full uncached SQL for the query
            SqlCacheless = CohortQueryBuilderHelper.GetSQLForAggregate(CohortSet,
                new QueryBuilderArgs(PatientIndexTableIfAny, JoinedTo,
                    SqlJoinableCacheless, parent.Customise, globals));


            //if the joined to table is cached we can generate a partial too with full sql for the outer sql block and a cache fetch join
            if (SqlJoinableCached != null)
                SqlPartiallyCached = CohortQueryBuilderHelper.GetSQLForAggregate(CohortSet,
                    new QueryBuilderArgs(PatientIndexTableIfAny, JoinedTo,
                        SqlJoinableCached, parent.Customise, globals));
        }

        //We would prefer a cache hit on the exact uncached SQL
        SqlFullyCached = GetCacheFetchSqlIfPossible(parent, CohortSet, SqlCacheless, isSolitaryPatientIndexTable,
            pluginCohortCompiler, cancellationToken);

        // if we have a patient index table where the Sql is not fully cached then we should invalidate anyone using it
        if (isSolitaryPatientIndexTable && SqlFullyCached == null && parent.CacheManager != null)
            ClearCacheForUsersOfPatientIndexTable(parent.CacheManager, CohortSet);

        //but if that misses we would take a cache hit of an execution of the SqlPartiallyCached
        if (SqlFullyCached == null && SqlPartiallyCached != null)
            SqlFullyCached = GetCacheFetchSqlIfPossible(parent, CohortSet, SqlPartiallyCached,
                isSolitaryPatientIndexTable, pluginCohortCompiler, cancellationToken);
    }

    private void ClearCacheForUsersOfPatientIndexTable(CachedAggregateConfigurationResultsManager cacheManager,
        AggregateConfiguration cohortSet)
    {
        if (cacheManager == null)
            return;

        var join = CohortSet.JoinableCohortAggregateConfiguration ?? throw new Exception(
            $"{nameof(AggregateConfiguration.JoinableCohortAggregateConfiguration)} was null for CohortSet {cohortSet} so we were unable to clear the joinable cache users");

        // get each Aggregate Configuration that joins using this patient index table
        foreach (var user in join.Users.Select(j => j.AggregateConfiguration))
            cacheManager.DeleteCacheEntryIfAny(user, AggregateOperation.IndexedExtractionIdentifierList);
    }

    private CohortQueryBuilderDependencySql GetCacheFetchSqlIfPossible(CohortQueryBuilderResult parent,
        AggregateConfiguration aggregate,
        CohortQueryBuilderDependencySql sql, bool isPatientIndexTable, IPluginCohortCompiler pluginCohortCompiler,
        CancellationToken cancellationToken)
    {
        if (parent.CacheManager == null)
            return null;

        var aggregateType = isPatientIndexTable
            ? AggregateOperation.JoinableInceptionQuery
            : AggregateOperation.IndexedExtractionIdentifierList;
        IHasFullyQualifiedNameToo existingTable;

        // since we might make a plugin API run call we had better lock this to
        var oLock = AggregateLocks.GetOrAdd(aggregate.ID, i => new object());
        lock (oLock)
        {
            // unless it is a plugin driven aggregate we need to assemble the SQL to check if the cache is stale
            if (pluginCohortCompiler == null)
            {
                var parameterSql =
                    QueryBuilder.GetParameterDeclarationSQL(sql.ParametersUsed.Clone()
                        .GetFinalResolvedParametersList());
                var hitTestSql = parameterSql + sql.Sql;
                existingTable = parent.CacheManager.GetLatestResultsTable(aggregate, aggregateType, hitTestSql);
            }
            else
            {
                existingTable =
                    parent.CacheManager.GetLatestResultsTableUnsafe(aggregate, aggregateType, out var oldDescription);

                if (pluginCohortCompiler.IsStale(aggregate, oldDescription)) existingTable = null;
            }

            // if there are no cached results in the destination (and it's a plugin cohort) then we need to run the plugin API call
            if (existingTable == null && pluginCohortCompiler != null)
            {
                // no cached results were there so run the plugin
                pluginCohortCompiler.Run(CohortSet, parent.CacheManager, cancellationToken);

                // try again now
                existingTable = parent.CacheManager.GetLatestResultsTableUnsafe(aggregate, aggregateType);

                if (existingTable == null)
                    throw new Exception(
                        $"Run method on {pluginCohortCompiler} failed to populate the Query Result Cache for {CohortSet}");
            }
        }

        //if there is a cached entry matching the cacheless SQL then we can just do a select from it (in theory)
        if (existingTable != null)
        {
            var sqlCachFetch =
                $@"{CachedAggregateConfigurationResultsManager.CachingPrefix}{aggregate.Name}*/{Environment.NewLine}select * from {existingTable.GetFullyQualifiedName()}{Environment.NewLine}";

            //Cache fetch does not require any parameters
            return new CohortQueryBuilderDependencySql(sqlCachFetch, new ParameterManager());
        }


        return null;
    }

    public string DescribeCachedState()
    {
        if (SqlFullyCached != null)
            return "Fully Cached";

        if (SqlPartiallyCached != null)
            return "Partially Cached";

        return SqlCacheless != null ? "Not Cached" : "Not Built";
    }
}