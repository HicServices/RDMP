// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using Rdmp.Core.CohortCreation.Execution.Joinables;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CohortCreation.Execution;

/// <summary>
/// Multi threading management class for CohortQueryBuilder.  Supports starting, executing and cancelling multiple cohort builder objects (ICompileable)
/// at once.  Every input object (e.g. CohortAggregateContainer) will be assigned a corresponding ICompileable (e.g. AggregationContainerTask) and a
/// CohortIdentificationTaskExecution.  The ICompileable records how long the query has been running for, how much of the query is cached, whether it
/// has been cancelled / crashed etc.  The CohortIdentificationTaskExecution handles the actual execution of the query on the data set database.
/// 
/// <para>See CohortCompiler.cd</para>
/// </summary>
public class CohortCompiler
{
    private CohortIdentificationConfiguration _cic;

    public CohortIdentificationConfiguration CohortIdentificationConfiguration
    {
        get => _cic;
        set
        {
            _cic = value;
            BuildPluginCohortCompilerList();
        }
    }

    public bool IncludeCumulativeTotals { get; set; }


    /// <summary>
    /// Plugin custom cohort compilers e.g. API calls that return identifier lists
    /// </summary>
    public IReadOnlyCollection<IPluginCohortCompiler> PluginCohortCompilers { get; private set; }

    /// <summary>
    /// Returns the current child provider (creating it if none has been injected yet).
    /// </summary>
    public ICoreChildProvider CoreChildProvider
    {
        get => _coreChildProvider ??= new CatalogueChildProvider(CohortIdentificationConfiguration.CatalogueRepository,
            null, IgnoreAllErrorsCheckNotifier.Instance, null);
        set => _coreChildProvider = value;
    }

    /// <summary>
    /// Tasks currently running in the compiler, Value can be null if the <see cref="ICompileable"/> is still building
    /// and not running yet.
    /// </summary>
    public Dictionary<ICompileable, CohortIdentificationTaskExecution> Tasks = new();

    public List<Thread> Threads = new();
    private ICoreChildProvider _coreChildProvider;

    public CohortCompiler(CohortIdentificationConfiguration cohortIdentificationConfiguration)
    {
        CohortIdentificationConfiguration = cohortIdentificationConfiguration;
    }

    private void BuildPluginCohortCompilerList()
    {
        // we already have them (or crashed out trying to create them
        if (PluginCohortCompilers != null) return;

        // we don't know what we are building yet!
        if (CohortIdentificationConfiguration == null) return;

        try
        {
            PluginCohortCompilers = PluginCohortCompilerFactory.CreateAll();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to build list of IPluginCohortCompilers", ex);
        }
    }

    private void DoTaskAsync(ICompileable task, CohortIdentificationTaskExecution execution, int timeout,
        bool cacheOnCompletion = false)
    {
        try
        {
            task.CancellationToken.ThrowIfCancellationRequested();

            task.Timeout = timeout;
            task.State = CompilationState.Executing;

            execution.GetCohortAsync(timeout);

            task.FinalRowCount = execution.Identifiers.Rows.Count;

            if (execution.CumulativeIdentifiers != null)
                task.CumulativeRowCount = execution.CumulativeIdentifiers.Rows.Count;

            task.State = CompilationState.Finished;
            task.Stopwatch.Stop();

            if (cacheOnCompletion)
                CacheSingleTask(task);
        }
        catch (Exception ex)
        {
            task.Stopwatch.Stop();
            task.State = CompilationState.Crashed;
            task.CrashMessage = ex;
        }
    }

    /// <summary>
    /// Adds all subqueries and containers that are below the current CohortIdentificationConfiguration as tasks to the compiler
    /// </summary>
    /// <param name="addSubcontainerTasks">The root container is always added to the task list but you could skip subcontainer totals if all you care about is the final total for the cohort
    /// and you don't have a dependant UI etc.  Passing false will add all joinables, subqueries etc and the root container (final answer for who is in cohort) only.</param>
    /// <returns></returns>
    public List<ICompileable> AddAllTasks(bool addSubcontainerTasks = true)
    {
        var globals = CohortIdentificationConfiguration.GetAllParameters().ToArray();
        CohortIdentificationConfiguration.CreateRootContainerIfNotExists();

        var toReturn = CohortIdentificationConfiguration.GetAllJoinables()
            .Select(joinable => AddTask(joinable, globals)).ToList();

        toReturn.AddRange(AddTasksRecursively(globals.ToArray(), CohortIdentificationConfiguration.RootCohortAggregateContainer,
            addSubcontainerTasks));

        return toReturn;
    }

    public List<ICompileable> AddTasksRecursively(ISqlParameter[] globals, CohortAggregateContainer container,
        bool addSubcontainerTasks = true)
    {
        var tasks = AddTasksRecursivelyAsync(globals, container, addSubcontainerTasks);

        Task.WaitAll(tasks.ToArray());

        return tasks.Select(t => t.Result).ToList();
    }

    /// <summary>
    /// Adds all AggregateConfigurations and CohortAggregateContainers in the specified container or subcontainers. Passing addSubcontainerTasks false will still process the subcontainers
    /// but will only add AggregateConfigurations to the task list.
    /// 
    /// <para>Does not add disabled objects</para>
    /// </summary>
    /// <param name="globals"></param>
    /// <param name="container"></param>
    /// <param name="addSubcontainerTasks">The root container is always added to the task list but you could skip subcontainer totals if all you care about is the final total for the cohort
    /// and you don't have a dependant UI etc.  Passing false will add all joinables, subqueries etc and the root container (final answer for who is in cohort) only.</param>
    /// <returns></returns>
    public List<Task<ICompileable>> AddTasksRecursivelyAsync(ISqlParameter[] globals,
        CohortAggregateContainer container, bool addSubcontainerTasks = true)
    {
        var toReturn = new List<Task<ICompileable>>();

        if (CohortIdentificationConfiguration.RootCohortAggregateContainer_ID == null)
            throw new QueryBuildingException(
                $"CohortIdentificationConfiguration '{CohortIdentificationConfiguration}' had not root SET container (UNION / INERSECT / EXCEPT)");

        //if it is the root container or we are adding tasks for all containers including subcontainers
        if (CohortIdentificationConfiguration.RootCohortAggregateContainer_ID == container.ID || addSubcontainerTasks)
            if (!container.IsDisabled)
                toReturn.Add(Task.Run(() => AddTask(container, globals)));


        foreach (var c in container.GetOrderedContents())
            switch (c)
            {
                case CohortAggregateContainer { IsDisabled: false } aggregateContainer:
                    toReturn.AddRange(AddTasksRecursivelyAsync(globals, aggregateContainer, addSubcontainerTasks));
                    break;
                case AggregateConfiguration { IsDisabled: false } aggregate:
                    toReturn.Add(Task.Run(() => AddTask(aggregate, globals)));
                    break;
            }

        return toReturn;
    }

    /// <summary>
    /// Adds the given AggregateConfiguration, CohortAggregateContainer or JoinableCohortAggregateConfiguration to the compiler Task list or returns the existing
    /// ICompileable if it is already part of the Compilation list.  This will not start the task, you will have to call Launch... to start the ICompileable executing
    /// </summary>
    /// <param name="runnable">An AggregateConfiguration, CohortAggregateContainer or JoinableCohortAggregateConfiguration you want to schedule for execution</param>
    /// <param name="globals"></param>
    /// <returns></returns>
    public ICompileable AddTask(IMapsDirectlyToDatabaseTable runnable, ISqlParameter[] globals)
    {
        var aggregate = runnable as AggregateConfiguration;
        var container = runnable as CohortAggregateContainer;
        var joinable = runnable as JoinableCohortAggregateConfiguration;
        var obj = (aggregate ?? container ?? (IMapsDirectlyToDatabaseTable)joinable) ?? throw new NotSupportedException(
            $"Expected c to be either AggregateConfiguration or CohortAggregateContainer but it was {runnable.GetType().Name}");
        var source = new CancellationTokenSource();
        ICompileable task;

        //thing that will produce the SQL
        CohortQueryBuilder queryBuilder;
        CohortQueryBuilder cumulativeQueryBuilder = null;
        CohortAggregateContainer parent;

        //if it is an aggregate
        if (aggregate != null)
        {
            // is this a custom aggregate type that gets handled differently e.g. by queriying an API?
            var plugin = PluginCohortCompilers.FirstOrDefault(c => c.ShouldRun(aggregate));

            task = plugin != null
                ?
                // yes
                new PluginCohortCompilerTask(aggregate, this, plugin)
                // no
                : new AggregationTask(aggregate, this);

            queryBuilder = new CohortQueryBuilder(aggregate, globals, CoreChildProvider);

            //which has a parent
            parent = aggregate.GetCohortAggregateContainerIfAny();
        }
        else if (joinable != null)
        {
            task = new JoinableTask(joinable, this);
            queryBuilder = new CohortQueryBuilder(joinable.AggregateConfiguration, globals, CoreChildProvider);
            parent = null;
        }
        else
        {
            task = new AggregationContainerTask(container, this);
            queryBuilder = new CohortQueryBuilder(container, globals, CoreChildProvider);
            parent = container.GetParentContainerIfAny();
        }

        //if there is a parent
        if (parent != null)
        {
            //tell the task what the container is for UI purposes really
            var isFirstInContainer = parent.GetOrderedContents().First().Equals(runnable);
            task.SetKnownContainer(parent, isFirstInContainer);

            //but...
            //if the container/aggregate being processed isn't the first component in the container
            if (!isFirstInContainer && IncludeCumulativeTotals) //and we want cumulative totals
                cumulativeQueryBuilder = new CohortQueryBuilder(parent, globals, CoreChildProvider)
                {
                    StopContainerWhenYouReach = (IOrderable)runnable
                };
        }

        //if the overall owner has a cache configured
        if (CohortIdentificationConfiguration.QueryCachingServer_ID != null)
        {
            var cacheServer = CohortIdentificationConfiguration.QueryCachingServer;
            queryBuilder.CacheServer = cacheServer;

            if (cumulativeQueryBuilder != null)
                cumulativeQueryBuilder.CacheServer = cacheServer;
        }

        //setup cancellation
        task.CancellationToken = source.Token;
        task.CancellationTokenSource = source;
        task.State = CompilationState.Building;

        lock (Tasks)
        {
            //we have seen this entity before (by ID & entity type)
            foreach (var c in Tasks.Keys.Where(k => k.Child.Equals(obj) && k != task).ToArray())
            {
                // the task is already setup ready to go somehow
                if (c.CancellationTokenSource == source)
                    // it's already added, no worries just return the already existing one
                    return c;

                CancelTask(c, true);
            }

            Tasks.Add(task, null);
        }

        var newsql = "";
        var cumulativeSql = "";

        try
        {
            // build the SQL but respect the cancellation token
            queryBuilder.RegenerateSQL(source.Token);

            //get the count(*) SQL
            newsql = queryBuilder.SQL;

            if (cumulativeQueryBuilder != null)
                cumulativeSql = cumulativeQueryBuilder.SQL;
        }
        catch (Exception e)
        {
            //it was not possible to generate valid SQL for the task
            task.CrashMessage = e;
            task.State = CompilationState.Crashed;
        }

        task.Log = queryBuilder?.Results?.Log;


        var isResultsForRootContainer = container != null &&
                                        container.ID == CohortIdentificationConfiguration
                                            .RootCohortAggregateContainer_ID;


        var taskExecution = new CohortIdentificationTaskExecution(newsql, cumulativeSql, source,
            queryBuilder?.Results?.CountOfSubQueries ?? -1,
            queryBuilder?.Results?.CountOfCachedSubQueries ?? -1,
            isResultsForRootContainer,
            queryBuilder?.Results?.TargetServer);

        // task is now built but not yet
        if (task.State != CompilationState.Crashed) task.State = CompilationState.NotScheduled;

        lock (Tasks)
        {
            //assign the execution
            Tasks[task] = taskExecution;
        }

        return task;
    }

    internal void LaunchSingleTask([NotNull] ICompileable compileable, int timeout, bool cacheOnCompletion)
    {
        if (!Tasks.TryGetValue(compileable, out var taskExecution))
            throw new KeyNotFoundException("Cannot launch task because it is not in the list of current Tasks");

        if (compileable.State != CompilationState.NotScheduled)
            throw new ArgumentException(
                $"Task must be in state NotScheduled but was {compileable.State}.  Crash message is:{compileable.CrashMessage?.ToString() ?? "null"}");

        KickOff(compileable, taskExecution, timeout, cacheOnCompletion);
    }

    private void KickOff(ICompileable task, CohortIdentificationTaskExecution execution, int timeout,
        bool cacheOnCompletion)
    {
        task.State = CompilationState.Scheduled;
        task.Stopwatch = Stopwatch.StartNew();

        var t = new Thread(() => DoTaskAsync(task, execution, timeout, cacheOnCompletion));
        Threads.Add(t);
        t.Start();
    }

    private void CacheSingleTask(ICompileable completedtask)
    {
        if (CohortIdentificationConfiguration.QueryCachingServer == null)
            return;

        if (completedtask is ICacheableTask cacheable && cacheable.IsCacheableWhenFinished())
            CacheSingleTask(cacheable, CohortIdentificationConfiguration.QueryCachingServer);
    }

    public void CacheSingleTask(ICacheableTask cacheableTask, ExternalDatabaseServer queryCachingServer)
    {
        try
        {
            //if it is already cached don't inception cache
            var sql = Tasks[cacheableTask].CountSQL;

            if (sql.Trim().StartsWith(CachedAggregateConfigurationResultsManager.CachingPrefix))
                return;

            var manager = new CachedAggregateConfigurationResultsManager(queryCachingServer);

            var explicitTypes = new List<DatabaseColumnRequest>();

            var configuration = cacheableTask.GetAggregateConfiguration();
            try
            {
                //the identifier column that we read from
                var identifiers = configuration.AggregateDimensions.Where(c => c.IsExtractionIdentifier).ToArray();

                if (identifiers.Length != 1)
                    throw new Exception(
                        $"There were {identifiers.Length} columns in the configuration marked IsExtractionIdentifier:{string.Join(",", identifiers.Select(i => i.GetRuntimeName()))}");

                var identifierDimension = identifiers[0];
                var identifierColumnInfo = identifierDimension.ColumnInfo;
                var destinationDataType =
                    GetDestinationType(identifierColumnInfo.Data_type, cacheableTask, queryCachingServer);

                explicitTypes.Add(new DatabaseColumnRequest(identifierDimension.GetRuntimeName(), destinationDataType));

                //make other non transform Types have explicit values
                explicitTypes.AddRange(configuration.AggregateDimensions.Where(d => d != identifierDimension)
                    .Where(d => d.ExtractionInformation.SelectSQL.Equals(d.SelectSQL) &&
                                !d.ExtractionInformation.IsProperTransform())
                    .Select(d => new DatabaseColumnRequest(d.GetRuntimeName(),
                        GetDestinationType(d.ExtractionInformation.ColumnInfo.Data_type, cacheableTask,
                            queryCachingServer))));
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Error occurred trying to find the data type of the identifier column when attempting to submit the result data table to the cache",
                    e);
            }

            var args = cacheableTask.GetCacheArguments(sql, Tasks[cacheableTask].Identifiers, explicitTypes.ToArray());

            manager.CommitResults(args);
        }
        catch (Exception e)
        {
            cacheableTask.State = CompilationState.Crashed;
            cacheableTask.CrashMessage = new Exception("Failed to cache results", e);
        }
    }

    /// <summary>
    /// Translates the <paramref name="data_type"/> which was read from <paramref name="cacheableTask"/> into an appropriate type
    /// that can be written into the tables referenced by <paramref name="queryCachingServer"/>.
    /// </summary>
    /// <param name="data_type">The datatype you want translated e.g. varchar2(10) (oracle syntax)</param>
    /// <param name="cacheableTask">Where the datatype was read from e.g. Oracle</param>
    /// <param name="queryCachingServer">Where the datatype is going to be stored e.g. Sql Server</param>
    /// <returns></returns>
    private static string GetDestinationType(string data_type, ICacheableTask cacheableTask,
        ExternalDatabaseServer queryCachingServer)
    {
        var accessPoints = cacheableTask.GetDataAccessPoints();

        var server = DataAccessPortal.ExpectDistinctServer(accessPoints, DataAccessContext.DataExport, false);

        var sourceSyntax = server.GetQuerySyntaxHelper();
        var destinationSyntax = queryCachingServer.GetQuerySyntaxHelper();

        //if we have a change in syntax e.g. read from Oracle write to Sql Server
        return sourceSyntax.DatabaseType != destinationSyntax.DatabaseType
            ? sourceSyntax.TypeTranslater.TranslateSQLDBType(data_type, destinationSyntax.TypeTranslater)
            : data_type;
    }

    /// <summary>
    /// Stops the execution of all currently executing ICompileable CohortIdentificationTaskExecutions. If it is executing an SQL query this should cancel the ongoing query.  If the
    /// ICompileable is not executing (it has crashed or finished etc) then nothing will happen.  alsoClearFromTaskList is always respected
    /// </summary>
    /// <param name="alsoClearTaskList">True to also remove all ICompileables, False to leave the Tasks intact (allows you to rerun them or clear etc)</param>
    public void CancelAllTasks(bool alsoClearTaskList)
    {
        lock (Tasks)
        {
            foreach (var k in Tasks.Keys) CancelTask(k, alsoClearTaskList);

            if (alsoClearTaskList) Tasks.Clear();
        }
    }

    /// <summary>
    /// Stops execution of the specified ICompileable CohortIdentificationTaskExecutions.  If it is executing an SQL query this should cancel the ongoing query.  If the
    /// ICompileable is not executing (it has crashed or finished etc) then nothing will happen.  alsoClearFromTaskList is always respected
    /// </summary>
    /// <param name="compileable"></param>
    /// <param name="alsoClearFromTaskList">True to remove the ICompileable from the tasks list, False to leave the Tasks intact (allows you to rerun it or clear etc) </param>
    public void CancelTask(ICompileable compileable, bool alsoClearFromTaskList)
    {
        lock (Tasks)
        {
            if (Tasks.TryGetValue(compileable, out var execution))
            {
                if (execution is { IsExecuting: true }) execution.Cancel();

                // cancel the source
                if (
                    compileable.State is CompilationState.Building or CompilationState.Executing)
                    compileable.CancellationTokenSource.Cancel();


                if (alsoClearFromTaskList)
                {
                    execution?.Dispose();
                    Tasks.Remove(compileable);
                }
            }
        }
    }

    public int GetAliveThreadCount()
    {
        return Threads.Count(static t => t.IsAlive);
    }

    public string GetCachedQueryUseCount(ICompileable task)
    {
        if (!Tasks.TryGetValue(task, out var execution) || execution == null)
            return "Unknown";

        return $"{execution.SubqueriesCached}/{execution.SubQueries}";
    }

    public bool AreaAllQueriesCached(ICompileable task)
    {
        if (!Tasks.TryGetValue(task, out var execution) || execution == null)
            return false;

        return execution.SubqueriesCached == execution.SubQueries && execution.SubQueries >= 1;
    }
}