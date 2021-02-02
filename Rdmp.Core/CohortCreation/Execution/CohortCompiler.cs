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
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.CohortCreation.Execution.Joinables;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Providers;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CohortCreation.Execution
{
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
        public CohortIdentificationConfiguration CohortIdentificationConfiguration { get; set; }
        public bool IncludeCumulativeTotals { get; set; }

        /// <summary>
        /// Returns the current child provider (creating it if none has been injected yet).
        /// </summary>
        public ICoreChildProvider CoreChildProvider
        {
            get => _coreChildProvider = _coreChildProvider ?? new CatalogueChildProvider(CohortIdentificationConfiguration.CatalogueRepository,null,new IgnoreAllErrorsCheckNotifier(),null);
            set => _coreChildProvider = value;
        }

        public Dictionary<ICompileable, CohortIdentificationTaskExecution> Tasks = new Dictionary<ICompileable, CohortIdentificationTaskExecution>();
        
        public List<Thread> Threads = new List<Thread>();
        private ICoreChildProvider _coreChildProvider;

        public CohortCompiler(CohortIdentificationConfiguration cohortIdentificationConfiguration)
        {
            CohortIdentificationConfiguration = cohortIdentificationConfiguration;
        }

        private void DoTaskAsync(ICompileable task, CohortIdentificationTaskExecution execution, int timeout,bool cacheOnCompletion = false)
        {
            try
            {
                task.Timeout = timeout;
                task.State = CompilationState.Executing;
                
                execution.GetCohortAsync( timeout);

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
                task.State= CompilationState.Crashed;
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
            var toReturn = new List<ICompileable>();
            var globals = CohortIdentificationConfiguration.GetAllParameters();
            CohortIdentificationConfiguration.CreateRootContainerIfNotExists();
            
            foreach (var joinable in CohortIdentificationConfiguration.GetAllJoinables())
                toReturn.Add(AddTask(joinable, globals));

            toReturn.AddRange( AddTasksRecursively(globals,CohortIdentificationConfiguration.RootCohortAggregateContainer,addSubcontainerTasks));
            
            return toReturn;
        }

        /// <summary>
        /// Adds all AggregateConfigurations and CohortAggregateContainers in the specified container or subcontainers. Passing addSubcontainerTasks false will still process the subcontainers
        /// but will only add AggregateConfigurations to the task list
        /// </summary>
        /// <param name="globals"></param>
        /// <param name="container"></param>
        /// <param name="addSubcontainerTasks">The root container is always added to the task list but you could skip subcontainer totals if all you care about is the final total for the cohort
        /// and you don't have a dependant UI etc.  Passing false will add all joinables, subqueries etc and the root container (final answer for who is in cohort) only.</param>
        /// <returns></returns>
        public List<ICompileable> AddTasksRecursively(ISqlParameter[] globals, CohortAggregateContainer container, bool addSubcontainerTasks = true)
        {
            var toReturn = new List<ICompileable>();

            if(CohortIdentificationConfiguration.RootCohortAggregateContainer_ID == null)
                throw new QueryBuildingException($"CohortIdentificationConfiguration '{CohortIdentificationConfiguration}' had not root SET container (UNION / INERSECT / EXCEPT)");

            //if it is the root container or we are adding tasks for all containers including subcontainers
            if (CohortIdentificationConfiguration.RootCohortAggregateContainer_ID == container.ID || addSubcontainerTasks)
                toReturn.Add(AddTask(container,globals));

            foreach (IOrderable c in container.GetOrderedContents())
            {
                var aggregateContainer = c as CohortAggregateContainer;
                var aggregate = c as AggregateConfiguration;
                
                if (aggregateContainer != null)
                    toReturn.AddRange(AddTasksRecursively(globals, aggregateContainer, addSubcontainerTasks));

                if(aggregate != null)
                    toReturn.Add(AddTask(aggregate, globals));
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
        public ICompileable AddTask(IMapsDirectlyToDatabaseTable runnable, IEnumerable<ISqlParameter> globals)
        {
            var aggregate = runnable as AggregateConfiguration;
            var container = runnable as CohortAggregateContainer;
            var joinable = runnable as JoinableCohortAggregateConfiguration;


            if (aggregate == null && container == null && joinable == null)
                throw new NotSupportedException(
                    "Expected c to be either AggregateConfiguration or CohortAggregateContainer but it was " +
                    runnable.GetType().Name);

            CancellationTokenSource source = new CancellationTokenSource();
            ICompileable task;

            //thing that will produce the SQL
            CohortQueryBuilder queryBuilder;
            CohortQueryBuilder cumulativeQueryBuilder = null;
            CohortAggregateContainer parent;

            //if it is an aggregate
            if (aggregate != null)
            {
                //which has a parent
                task = new AggregationTask(aggregate, this);
                queryBuilder = new CohortQueryBuilder(aggregate, globals,CoreChildProvider);

                parent = aggregate.GetCohortAggregateContainerIfAny();
            }
            else if (joinable != null)
            {
                task = new JoinableTask(joinable,this);
                queryBuilder = new CohortQueryBuilder(joinable.AggregateConfiguration,globals,CoreChildProvider);
                parent = null;
            }
            else
            {
                task = new AggregationContainerTask(container, this);
                queryBuilder = new CohortQueryBuilder(container, globals,CoreChildProvider);
                parent = container.GetParentContainerIfAny();
            }

            //if there is a parent
            if (parent != null)
            {
                //tell the task what the container is for UI purposes really
                bool isFirstInContainer = parent.GetOrderedContents().First().Equals(runnable);
                task.SetKnownContainer(parent, isFirstInContainer);

                //but...
                //if the container/aggregate being processed isn't the first component in the container
                if (!isFirstInContainer && IncludeCumulativeTotals) //and we want cumulative totals
                {
                    cumulativeQueryBuilder = new CohortQueryBuilder(parent, globals,CoreChildProvider);
                    cumulativeQueryBuilder.StopContainerWhenYouReach = (IOrderable) runnable;
                }
                
            }
            ExternalDatabaseServer cacheServer = null;
            //if the overall owner has a cache configured
            if (CohortIdentificationConfiguration.QueryCachingServer_ID != null)
            {
                
                cacheServer = CohortIdentificationConfiguration.QueryCachingServer;
                queryBuilder.CacheServer = cacheServer;

                if (cumulativeQueryBuilder != null)
                    cumulativeQueryBuilder.CacheServer = cacheServer;
            }

            //setup cancellation 
            task.CancellationToken = source.Token;
            string newsql = "";
            string cumulativeSql = "";

            try
            {
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

            //we have seen this entity before (by ID & entity type)
            KeyValuePair<ICompileable, CohortIdentificationTaskExecution> existingTask;

            if (joinable != null)
                existingTask = Tasks.SingleOrDefault((kvp => kvp.Key.Child.Equals(joinable)));
            else
            if (aggregate != null)
                existingTask = Tasks.SingleOrDefault(kvp => kvp.Key.Child.Equals(aggregate));
            else
                existingTask = Tasks.SingleOrDefault(kvp => kvp.Key.Child.Equals(container));


            //job already exists (this is the same as saying existingTask!=null)
            if (!existingTask.Equals(default(KeyValuePair<ICompileable, CohortIdentificationTaskExecution>)))
                if (existingTask.Value.CountSQL.Equals(newsql))
                {
                    //The SQl is the same but the order or cached 
                    if (existingTask.Key.Order != task.Order)//do not delete this if statement, it prevents rewrites to the database where Order asignment has side affects
                        existingTask.Key.Order = task.Order;

                    return existingTask.Key; //existing task has the same SQL
                }
                else
                {
                    //it is different so cancel the old one
                    existingTask.Value.Cancel();

                    //throw away the old task
                    Tasks.Remove(existingTask.Key);

                    //dispose of any resources it's holding onto
                    existingTask.Value.Dispose();
                }
            
      
            var isResultsForRootContainer = container != null && container.ID == CohortIdentificationConfiguration.RootCohortAggregateContainer_ID;


            var taskExecution = new CohortIdentificationTaskExecution(cacheServer, newsql, cumulativeSql, source,
                queryBuilder?.Results?.CountOfSubQueries ?? -1,
                queryBuilder?.Results?.CountOfCachedSubQueries ??-1,
                isResultsForRootContainer,
                queryBuilder?.Results?.TargetServer);

            //create a new task 
            Tasks.Add(task, taskExecution);

            return task;
        }

        public void LaunchSingleTask(ICompileable compileable,int timeout,bool cacheOnCompletion)
        {
            if(!Tasks.ContainsKey(compileable))
                throw new KeyNotFoundException("Cannot launch task because it is not in the list of current Tasks");

            if(compileable.State != CompilationState.NotScheduled)
                throw new ArgumentException($"Task must be in state NotScheduled but was {compileable.State}");

            KickOff(compileable, Tasks[compileable], timeout, cacheOnCompletion);
        }

        public void LaunchScheduledTasksAsync(int timeout,bool cacheOnCompletion)
        {
            foreach (KeyValuePair<ICompileable, CohortIdentificationTaskExecution> kvp in Tasks)
                if (kvp.Key.State == CompilationState.NotScheduled)
                    KickOff(kvp.Key, kvp.Value, timeout, cacheOnCompletion);
        }

        private void KickOff(ICompileable task, CohortIdentificationTaskExecution execution, int timeout,bool cacheOnCompletion)
        {
            task.State = CompilationState.Scheduled;
            task.Stopwatch = new Stopwatch();
            task.Stopwatch.Start();

            var t = new Thread(() => DoTaskAsync(task, execution, timeout, cacheOnCompletion));
            Threads.Add(t);
            t.Start();
        }

        private void CacheSingleTask(ICompileable completedtask)
        {
            if (CohortIdentificationConfiguration.QueryCachingServer == null)
                return;

            var cacheable = completedtask as ICacheableTask;
            if (cacheable != null && cacheable.IsCacheableWhenFinished())
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

                AggregateConfiguration configuration = cacheableTask.GetAggregateConfiguration();
                try
                {
                    //the identifier column that we read from
                    var identifiers = configuration.AggregateDimensions.Where(c => c.IsExtractionIdentifier).ToArray();

                    if (identifiers.Length != 1)
                        throw new Exception(string.Format(
                            "There were {0} columns in the configuration marked IsExtractionIdentifier:{1}",
                            identifiers.Length, string.Join(",", identifiers.Select(i => i.GetRuntimeName()))));

                    var identifierDimension = identifiers[0];
                    ColumnInfo identifierColumnInfo = identifierDimension.ColumnInfo;
                    var destinationDataType = GetDestinationType(identifierColumnInfo.Data_type,cacheableTask,queryCachingServer);
                    
                    explicitTypes.Add(new DatabaseColumnRequest(identifierDimension.GetRuntimeName(), destinationDataType));

                    //make other non transform Types have explicit values
                    foreach(AggregateDimension d in configuration.AggregateDimensions)
                    {
                        if(d != identifierDimension)
                        {
                            //if the user has not changed the SelectSQL and the SelectSQL of the original column is not a transform
                            if(d.ExtractionInformation.SelectSQL.Equals(d.SelectSQL) && !d.ExtractionInformation.IsProperTransform())
                            {
                                //then use the origin datatype
                                explicitTypes.Add(new DatabaseColumnRequest(d.GetRuntimeName(),GetDestinationType(d.ExtractionInformation.ColumnInfo.Data_type, cacheableTask, queryCachingServer)));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Error occurred trying to find the data type of the identifier column when attempting to submit the result data table to the cache", e);
                }

                CacheCommitArguments args = cacheableTask.GetCacheArguments(sql, Tasks[cacheableTask].Identifiers, explicitTypes.ToArray());

                manager.CommitResults(args);
            }
            catch (Exception e)
            {
                cacheableTask.State = CompilationState.Crashed;
                cacheableTask.CrashMessage = new Exception("Failed to cache results",e);
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
        private string GetDestinationType(string data_type, ICacheableTask cacheableTask, ExternalDatabaseServer queryCachingServer)
        {
            var accessPoints = cacheableTask.GetDataAccessPoints();

            var server = DataAccessPortal.GetInstance().ExpectDistinctServer(accessPoints, DataAccessContext.DataExport, false);
            
            var sourceSyntax = server.GetQuerySyntaxHelper();
            var destinationSyntax = queryCachingServer.GetQuerySyntaxHelper();
            
            //if we have a change in syntax e.g. read from Oracle write to Sql Server
            if (sourceSyntax.DatabaseType != destinationSyntax.DatabaseType)
            {
                return sourceSyntax.TypeTranslater.TranslateSQLDBType(data_type, destinationSyntax.TypeTranslater);
            }

            return data_type;
        }

        /// <summary>
        /// Stops the execution of all currently executing ICompileable CohortIdentificationTaskExecutions. If it is executing an SQL query this should cancel the ongoing query.  If the
        /// ICompileable is not executing (it has crashed or finished etc) then nothing will happen.  alsoClearFromTaskList is always respected
        /// </summary>
        /// <param name="alsoClearTaskList">True to also remove all ICompileables, False to leave the Tasks intact (allows you to rerun them or clear etc)</param>
        public void CancelAllTasks(bool alsoClearTaskList)
        {
            foreach (var v in Tasks.Values)
                if (v.IsExecuting)
                    v.Cancel();

            if(alsoClearTaskList)
            {
                foreach (var v in Tasks.Values)
                    v.Dispose();

                Tasks.Clear();
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
            if (Tasks.ContainsKey(compileable))
            {

                if (Tasks[compileable].IsExecuting)
                    Tasks[compileable].Cancel();

                if(alsoClearFromTaskList)
                {
                    Tasks[compileable].Dispose();
                    Tasks.Remove(compileable);
                }
            }
        }

        public int GetAliveThreadCount()
        {
            return Threads.Count(t => t.IsAlive);
        }

        public string GetCachedQueryUseCount(ICompileable task)
        {
            if (!Tasks.ContainsKey(task))
                return "Unknown";

            var execution = Tasks[task];
            return execution.SubqueriesCached + "/" + execution.SubQueries;
        }

        public bool AreaAllQueriesCached(ICompileable task )
        {
            if (!Tasks.ContainsKey(task))
                return false;

            var execution = Tasks[task];
            return execution.SubqueriesCached == execution.SubQueries && execution.SubQueries >=1;
        }
    }
}
