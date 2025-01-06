// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.QueryBuilding.Parameters;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
/// Builds a subset of a <see cref="CohortIdentificationConfiguration"/> e.g. a single <see cref="CohortAggregateContainer"/> (UNION / INTERSECT / EXCEPT) or a
/// cohort set.  This includes identifying all <see cref="Dependencies"/> and resolving the dependencies servers / the <see cref="CacheServer"/> to determine
/// whether a valid query can be assembled from the sub-components and deciding where it can be run (e.g. should the query run on the cache server or the data server
/// or are they both the same server so query sections can be mixed depending on cache hit/miss for each bit).
/// </summary>
public class CohortQueryBuilderResult
{
    public ExternalDatabaseServer CacheServer { get; }
    public CachedAggregateConfigurationResultsManager CacheManager { get; }

    public bool IsForContainer { get; private set; }
    public ICoreChildProvider ChildProvider { get; }
    public CohortQueryBuilderHelper Helper { get; }
    public QueryBuilderCustomArgs Customise { get; }
    public CancellationToken CancellationToken { get; }

    private readonly StringBuilder _log = new();

    /// <summary>
    /// Log of all activities undertaken while building
    /// </summary>
    public string Log => _log.ToString();

    /// <summary>
    /// The allowable caching state based on the <see cref="Dependencies"/>, whether there is a
    /// <see cref="CacheServer"/> and if they are on the same or separate servers from one another
    /// </summary>
    public CacheUsage CacheUsageDecision { get; private set; }

    private List<CohortQueryBuilderDependency> _dependencies = new();
    private bool _alreadyBuilt;

    public IReadOnlyCollection<CohortQueryBuilderDependency> Dependencies => _dependencies;

    /// <summary>
    /// Only Populated after Building.  If all <see cref="Dependencies"/> are on the same server as one another
    /// then this will contain all tables that must be queried otherwise it will be null
    /// </summary>
    public DataAccessPointCollection DependenciesSingleServer { get; private set; }

    /// <summary>
    /// The final SQL that should be executed on the <see cref="TargetServer"/>
    /// </summary>
    public string Sql { get; private set; }

    /// <summary>
    /// The location at which the <see cref="Sql"/> should be run (may be a data server or a cache server or they may be one and the same!)
    /// </summary>
    public DiscoveredServer TargetServer { get; set; }

    public IOrderable StopContainerWhenYouReach { get; set; }
    public int CountOfSubQueries => Dependencies.Count;
    public int CountOfCachedSubQueries { get; private set; }

    public IReadOnlyCollection<IPluginCohortCompiler> PluginCohortCompilers { get; } =
        Array.Empty<PluginCohortCompiler>().ToList().AsReadOnly();

    /// <summary>
    /// Creates a new result for a single <see cref="AggregateConfiguration"/> or <see cref="CohortAggregateContainer"/>
    /// </summary>
    /// <param name="cacheServer"></param>
    /// <param name="childProvider"></param>
    /// <param name="helper"></param>
    /// <param name="customise"></param>
    /// <param name="cancellationToken"></param>
    public CohortQueryBuilderResult(ExternalDatabaseServer cacheServer, ICoreChildProvider childProvider,
        CohortQueryBuilderHelper helper, QueryBuilderCustomArgs customise, CancellationToken cancellationToken)
    {
        CacheServer = cacheServer;
        ChildProvider = childProvider;
        Helper = helper;
        Customise = customise;
        CancellationToken = cancellationToken;

        if (cacheServer != null)
        {
            CacheManager = new CachedAggregateConfigurationResultsManager(CacheServer);

            try
            {
                PluginCohortCompilers = PluginCohortCompilerFactory.CreateAll();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to build list of IPluginCohortCompilers", ex);
            }
        }
    }


    public void BuildFor(CohortAggregateContainer container, ParameterManager parameterManager)
    {
        ThrowIfAlreadyBuilt();
        IsForContainer = true;

        _log.AppendLine($"Starting Build for {container}");
        //gather dependencies
        foreach (var cohortSet in ChildProvider.GetAllChildrenRecursively(container).OfType<AggregateConfiguration>()
                     .Where(IsEnabled).OrderBy(ac => ac.Order))
            AddDependency(cohortSet);

        if (!Dependencies.Any())
            throw new QueryBuildingException(
                $"There are no AggregateConfigurations under the SET container '{container}'");

        LogDependencies();

        BuildDependenciesSql(parameterManager.ParametersFoundSoFarInQueryGeneration[ParameterLevel.Global].ToArray());

        MakeCacheDecision();

        Sql = BuildSql(container, parameterManager);
    }

    public void BuildFor(AggregateConfiguration configuration, ParameterManager parameterManager)
    {
        ThrowIfAlreadyBuilt();
        IsForContainer = false;

        _log.AppendLine($"Starting Build for {configuration}");
        var d = AddDependency(configuration);

        LogDependencies();

        BuildDependenciesSql(parameterManager.ParametersFoundSoFarInQueryGeneration[ParameterLevel.Global].ToArray());

        MakeCacheDecision();


        Sql = BuildSql(d, parameterManager);
    }

    private string BuildSql(CohortAggregateContainer container, ParameterManager parameterManager)
    {
        Dictionary<CohortQueryBuilderDependency, string> sqlDictionary;

        //if we are fully cached on everything
        if (Dependencies.All(d => d.SqlFullyCached != null))
        {
            SetTargetServer(GetCacheServer(), "all dependencies are fully cached"); //run on the cache server

            //all are cached
            CountOfCachedSubQueries = CountOfSubQueries;

            sqlDictionary =
                Dependencies.ToDictionary(k => k,
                    v => v.SqlFullyCached.Use(parameterManager)); //run the fully cached sql
        }
        else
        {
            var uncached =
                $"CacheUsageDecision is {CacheUsageDecision} and the following were not cached:{string.Join(Environment.NewLine, Dependencies.Where(d => d.SqlFullyCached == null))}";

            switch (CacheUsageDecision)
            {
                case CacheUsage.MustUse:
                    throw new QueryBuildingException(
                        $"Could not build final SQL because some queries are not fully cached and {uncached}");

                case CacheUsage.Opportunistic:

                    //The cache and dataset are on the same server so run it
                    SetTargetServer(DependenciesSingleServer.GetDistinctServer(),
                        $"not all dependencies are cached while {uncached}");

                    CountOfCachedSubQueries = Dependencies.Count(d => d.SqlFullyCached != null);

                    sqlDictionary =
                        Dependencies.ToDictionary(k => k,
                            v => v.SqlFullyCached?.Use(parameterManager) ??
                                 v.SqlPartiallyCached?.Use(parameterManager) ??
                                 v.SqlCacheless.Use(parameterManager)); //run the fully cached sql
                    break;

                case CacheUsage.AllOrNothing:

                    //It's not fully cached so we have to run it entirely uncached
                    SetTargetServer(DependenciesSingleServer.GetDistinctServer(),
                        $"not all dependencies are cached while {uncached}");

                    //cannot use any of the caches because it's all or nothing
                    CountOfCachedSubQueries = 0;
                    sqlDictionary =
                        Dependencies.ToDictionary(k => k,
                            v => v.SqlCacheless.Use(parameterManager)); //run the fully uncached sql
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return WriteContainers(container, TargetServer.GetQuerySyntaxHelper(), sqlDictionary, 0);
    }

    private void SetTargetServer(DiscoveredServer target, string reason)
    {
        if (TargetServer != null)
            throw new InvalidOperationException("You are only supposed to pick a target server once");

        TargetServer = target;
        _log.AppendLine($"Picked TargetServer as {target} because {reason}");
    }

    private string WriteContainers(CohortAggregateContainer container, IQuerySyntaxHelper syntaxHelper,
        Dictionary<CohortQueryBuilderDependency, string> sqlDictionary, int tabs)
    {
        var sql = "";

        //Things we need to output
        var toWriteOut = container.GetOrderedContents().Where(IsEnabled).ToArray();

        if (toWriteOut.Any())
            sql += Environment.NewLine + TabIn("(", tabs) + Environment.NewLine;
        else
            throw new QueryBuildingException($"Container '{container}' is empty, Disable it if you don't want it run'");

        var firstEntityWritten = false;
        foreach (var toWrite in toWriteOut)
        {
            if (firstEntityWritten)
                sql += Environment.NewLine +
                       TabIn(
                           GetSetOperationSql(container.Operation, syntaxHelper.DatabaseType) + Environment.NewLine +
                           Environment.NewLine, tabs);

            if (toWrite is AggregateConfiguration)
                sql += TabIn(sqlDictionary.Single(kvp => Equals(kvp.Key.CohortSet, toWrite)).Value, tabs);

            if (toWrite is CohortAggregateContainer sub)
                sql += WriteContainers(sub, syntaxHelper, sqlDictionary, tabs + 1);

            //we have now written the first thing at this level of recursion - all others will need to be separated by the OPERATION e.g. UNION
            firstEntityWritten = true;

            if (StopContainerWhenYouReach != null && StopContainerWhenYouReach.Equals(toWrite))
                if (tabs != 0)
                    throw new NotSupportedException(
                        "Stopping prematurely only works when the aggregate to stop at is in the top level container");
                else
                    break;
        }

        //if we outputted anything
        if (toWriteOut.Any())
            sql += Environment.NewLine + TabIn(")", tabs) + Environment.NewLine;

        return sql;
    }

    private bool IsEnabled(IOrderable arg) => IsEnabled(arg, ChildProvider);

    /// <summary>
    /// Objects are enabled if they do not support disabling (<see cref="IDisableable"/>) or are <see cref="IDisableable.IsDisabled"/> = false
    /// </summary>
    /// <returns></returns>
    public static bool IsEnabled(IOrderable arg, ICoreChildProvider childProvider)
    {
        var parentDisabled = childProvider.GetDescendancyListIfAnyFor(arg)?.Parents.Any(p => p is IDisableable
        {
            IsDisabled: true
        });

        //if a parent is disabled
        if (parentDisabled.HasValue && parentDisabled.Value)
            return false;

        // skip empty containers unless strict validation is enabled
        if (arg is CohortAggregateContainer container &&
            !UserSettings.StrictValidationForCohortBuilderContainers)
            if (!container.GetOrderedContents().Any())
                return false;

        //or you yourself are disabled
        return arg is not IDisableable { IsDisabled: true };
    }

    /// <summary>
    /// Returns the SQL keyword for the <paramref name="currentContainerOperation"/>
    /// </summary>
    /// <param name="currentContainerOperation"></param>
    /// <param name="dbType"></param>
    /// <returns></returns>
    protected virtual string GetSetOperationSql(SetOperation currentContainerOperation, DatabaseType dbType)
    {
        return currentContainerOperation switch
        {
            SetOperation.UNION => "UNION",
            SetOperation.INTERSECT => "INTERSECT",
            SetOperation.EXCEPT => dbType == DatabaseType.Oracle ? "MINUS" : "EXCEPT",
            _ => throw new ArgumentOutOfRangeException(nameof(currentContainerOperation), currentContainerOperation,
                null)
        };
    }

    private string BuildSql(CohortQueryBuilderDependency dependency, ParameterManager parameterManager)
    {
        //if we are fully cached on everything
        if (dependency.SqlFullyCached != null)
        {
            SetTargetServer(GetCacheServer(), " dependency is cached"); //run on the cache server
            CountOfCachedSubQueries++; //it is cached
            return dependency.SqlFullyCached.Use(parameterManager); //run the fully cached sql
        }

        switch (CacheUsageDecision)
        {
            case CacheUsage.MustUse:
                throw new QueryBuildingException(
                    $"Could not build final SQL because {dependency} is not fully cached and CacheUsageDecision is {CacheUsageDecision}");

            case CacheUsage.Opportunistic:

                //The cache and dataset are on the same server so run it
                SetTargetServer(DependenciesSingleServer.GetDistinctServer(), "data and cache are on the same server");
                return dependency.SqlPartiallyCached?.Use(parameterManager) ??
                       dependency.SqlCacheless.Use(parameterManager);
            case CacheUsage.AllOrNothing:

                //It's not fully cached so we have to run it entirely uncached
                SetTargetServer(DependenciesSingleServer.GetDistinctServer(),
                    "cache and data are on separate servers / access credentials and not all datasets are in the cache");
                return dependency.SqlCacheless.Use(parameterManager);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private DiscoveredServer GetCacheServer() => CacheServer.Discover(DataAccessContext.InternalDataProcessing).Server;

    private void BuildDependenciesSql(ISqlParameter[] globals)
    {
        foreach (var d in Dependencies)
            d.Build(this, globals, CancellationToken);
    }


    private CohortQueryBuilderDependency AddDependency(AggregateConfiguration cohortSet)
    {
        if (cohortSet.Catalogue.IsApiCall())
        {
            if (CacheManager == null) throw new Exception($"Caching must be enabled to execute API call '{cohortSet}'");

            if (!PluginCohortCompilers.Any(c => c.ShouldRun(cohortSet)))
                throw new Exception(
                    $"No PluginCohortCompilers claimed to support '{cohortSet}' in their ShouldRun method");
        }

        var join = ChildProvider.AllJoinUses.Where(j => j.AggregateConfiguration_ID == cohortSet.ID).ToArray();

        if (join.Length > 1)
            throw new NotSupportedException(
                $"There are {join.Length} joins configured to AggregateConfiguration {cohortSet}");

        var d = new CohortQueryBuilderDependency(cohortSet, join.SingleOrDefault(), ChildProvider,
            PluginCohortCompilers);
        _dependencies.Add(d);

        return d;
    }

    private void MakeCacheDecision()
    {
        if (CacheServer == null)
        {
            SetCacheUsage(CacheUsage.AllOrNothing, "there is no cache server");
        }
        else
        {
            _log.AppendLine($"Cache Server:{CacheServer.Server} (DatabaseType:{CacheServer.DatabaseType})");
            SetCacheUsage(CacheUsage.Opportunistic, "there is a cache server (so starting with Opportunistic)");
        }

        DependenciesSingleServer = new DataAccessPointCollection(true);

        foreach (var dependency in Dependencies)
        {
            _log.AppendLine($"Evaluating '{dependency.CohortSet}'");
            foreach (var dependantTable in dependency.CohortSet.Catalogue.GetTableInfoList(false))
                HandleDependency(dependency, false, dependantTable);

            if (dependency.JoinedTo != null)
            {
                _log.AppendLine($"Evaluating '{dependency.JoinedTo}'");
                foreach (var dependantTable in dependency.JoinedTo.Catalogue.GetTableInfoList(false))
                    HandleDependency(dependency, true, dependantTable);
            }
        }
    }

    private void HandleDependency(CohortQueryBuilderDependency dependency, bool isPatientIndexTable,
        ITableInfo dependantTable)
    {
        _log.AppendLine(
            $"Found dependant table '{dependantTable}' (Server:{dependantTable.Server} DatabaseType:{dependantTable.DatabaseType})");

        //if dependencies are on different servers / access credentials
        if (DependenciesSingleServer != null)
            if (!DependenciesSingleServer.TryAdd(dependantTable))
            {
                //we can no longer establish a consistent connection to all the dependencies
                _log.AppendLine($"Found problematic dependant table '{dependantTable}'");

                //if there's no cache server that's a problem!
                if (CacheServer == null)
                    throw new QueryBuildingException(
                        $"Table {dependantTable} is on a different server (or uses different access credentials) from previously seen dependencies and no QueryCache is configured");

                //there is a cache server, perhaps we can dodge 'dependantTable' by going to cache instead
                var canUseCacheForDependantTable =
                    (isPatientIndexTable ? dependency.SqlJoinableCached : dependency.SqlFullyCached)
                    != null;

                //can we go to the cache server instead?
                if (canUseCacheForDependantTable && DependenciesSingleServer.TryAdd(CacheServer))
                {
                    _log.AppendLine($"Avoided problematic dependant table '{dependantTable}' by using the cache");
                }
                else
                {
                    DependenciesSingleServer = null;

                    //there IS a cache so we now Must use it
                    if (CacheUsageDecision != CacheUsage.MustUse)
                        SetCacheUsage(CacheUsage.MustUse,
                            $"Table {dependantTable} is on a different server (or uses different access credentials) from previously seen dependencies.  Therefore the QueryCache MUST be used for all dependencies");
                }
            }

        if (DependenciesSingleServer != null &&
            CacheServer != null &&
            CacheUsageDecision == CacheUsage.Opportunistic)
        {
            //We can only do opportunistic joins if the Cache and Data server are on the same server
            var canCombine = DependenciesSingleServer.AddWouldBePossible(CacheServer);

            if (!canCombine)
                SetCacheUsage(CacheUsage.AllOrNothing,
                    "All datasets are on one server/access credentials while Cache is on a separate one");
        }
    }

    private void LogDependencies()
    {
        foreach (var d in Dependencies)
        {
            _log.AppendLine($"Dependency '{d}' is {d.DescribeCachedState()}");
            _log.AppendLine(
                $"Dependency '{d}' IsExtractionIdentifier column is {d.ExtractionIdentifierColumn?.GetRuntimeName() ?? "NULL"}");
        }
    }


    private void SetCacheUsage(CacheUsage value, string thereIsNoCacheServer)
    {
        CacheUsageDecision = value;
        _log.AppendLine($"Setting {nameof(CacheUsageDecision)} to {value} because {thereIsNoCacheServer}");
    }

    private void ThrowIfAlreadyBuilt()
    {
        if (_alreadyBuilt)
            throw new InvalidOperationException("Dependencies have already been built");

        _alreadyBuilt = true;
    }

    public static string TabIn(string str, int numberOfTabs)
    {
        if (string.IsNullOrWhiteSpace(str))
            return str;

        var tabs = new string('\t', numberOfTabs);
        return tabs + str.Replace(Environment.NewLine, Environment.NewLine + tabs);
    }
}