using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using CohortManagerLibrary.Execution.Joinables;
using CohortManagerLibrary.QueryBuilding;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace CohortManagerLibrary.Execution
{
    public delegate void TaskCompletedHandler(object sender, ICompileable completedTask) ;

    public class CohortCompiler
    {
        public CohortIdentificationConfiguration CohortIdentificationConfiguration { get; set; }
        public bool IncludeCumulativeTotals { get; set; }
        
        public Dictionary<ICompileable, CohortIdentificationTaskExecution> Tasks = new Dictionary<ICompileable, CohortIdentificationTaskExecution>();
        
        public List<Thread> Threads = new List<Thread>();

        public event TaskCompletedHandler TaskCompleted;

        public CohortCompiler(CohortIdentificationConfiguration cohortIdentificationConfiguration)
        {
            CohortIdentificationConfiguration = cohortIdentificationConfiguration;
        }

        private void DoTaskAsync(ICompileable task, CohortIdentificationTaskExecution execution, int timeout)
        {
            try
            {
                task.Timeout = timeout;
                task.State = CompilationState.Executing;
                var accessPoints = task.GetDataAccessPoints();
                
                execution.GetCohortAsync(accessPoints, timeout);

                task.FinalRowCount = execution.Identifiers.Rows.Count;

                if (execution.CumulativeIdentifiers != null)
                    task.CumulativeRowCount = execution.CumulativeIdentifiers.Rows.Count;

                task.State = CompilationState.Finished;
                task.Stopwatch.Stop();
            }
            catch (Exception ex)
            {
                task.Stopwatch.Stop();
                task.State= CompilationState.Crashed;
                task.CrashMessage = ex;
            }

            if (TaskCompleted != null)
                TaskCompleted(this, task);
        }

        public ICompileable GetTask(IMapsDirectlyToDatabaseTable c, IEnumerable<ISqlParameter> globals)
        {
            //sync it / add it
            AddTask(c,globals);

            return Tasks.SingleOrDefault(kvp => kvp.Key.Child.Equals(c)).Key;
        }

        public void AddTask(IMapsDirectlyToDatabaseTable c, IEnumerable<ISqlParameter> globals)
        {
            var aggregate = c as AggregateConfiguration;
            var container = c as CohortAggregateContainer;
            var joinable = c as JoinableCohortAggregateConfiguration;

            if (aggregate == null && container == null && joinable == null)
                throw new NotSupportedException(
                    "Expected c to be either AggregateConfiguration or CohortAggregateContainer but it was " +
                    c.GetType().Name);

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
                queryBuilder = new CohortQueryBuilder(aggregate, globals);

                parent = aggregate.GetCohortAggregateContainerIfAny();
            }
            else if (joinable != null)
            {
                task = new JoinableTaskExecution(joinable,this);
                queryBuilder = new CohortQueryBuilder(joinable.AggregateConfiguration,globals,true);
                parent = null;
            }
            else
            {
                task = new AggregationContainerTask(container, this);
                queryBuilder = new CohortQueryBuilder(container, globals);
                parent = container.GetParentContainerIfAny();
            }

            //if there is a parent
            if (parent != null)
            {
                //tell the task what the container is for UI purposes really
                bool isFirstInContainer = parent.GetOrderedContents().First().Equals(c);
                task.SetKnownContainer(parent, isFirstInContainer);

                //but...
                //if the container/aggregate being processed isn't the first component in the container
                if (!isFirstInContainer && IncludeCumulativeTotals) //and we want cumulative totals
                {
                    cumulativeQueryBuilder = new CohortQueryBuilder(parent, globals);
                    cumulativeQueryBuilder.StopContainerWhenYouReach = (IOrderable) c;
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
            catch (QueryBuildingException e)
            {
                //it was not possible to generate valid SQL for the task
                task.CrashMessage = e;
                task.State = CompilationState.Crashed;
            }

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

                    return; //existing task has the same SQL
                }
                else
                {
                    //it is different so cancel the old one
                    existingTask.Value.CancellationTokenSource.Cancel(true);

                    //throw away the old task
                    Tasks.Remove(existingTask.Key);
                }
            
      
            var isResultsForRootContainer = container != null && container.ID == CohortIdentificationConfiguration.RootCohortAggregateContainer_ID;


            var taskExecution = new CohortIdentificationTaskExecution(cacheServer, newsql, cumulativeSql, source,
                queryBuilder.CountOfSubQueries,
                queryBuilder.CountOfCachedSubQueries, 
                isResultsForRootContainer);

            //create a new task 
            Tasks.Add(task, taskExecution);
        }

        public void LaunchSingleTask(ICompileable compileable,int timeout)
        {
            if(!Tasks.ContainsKey(compileable))
                throw new KeyNotFoundException("Cannot launch task because it is not in the list of current Tasks");

            if(compileable.State != CompilationState.NotScheduled)
                throw new ArgumentException("Task must be in state NotScheduled, try clicking Reset");

            KickOff(compileable, Tasks[compileable], timeout);
        }

        public void LaunchScheduledTasksAsync(int timeout)
        {
            foreach (KeyValuePair<ICompileable, CohortIdentificationTaskExecution> kvp in Tasks)
                if (kvp.Key.State == CompilationState.NotScheduled)
                    KickOff(kvp.Key, kvp.Value, timeout);
        }

        private void KickOff(ICompileable task, CohortIdentificationTaskExecution execution, int timeout)
        {
            task.State = CompilationState.Scheduled;
            task.Stopwatch = new Stopwatch();
            task.Stopwatch.Start();

            var t = new Thread(() => DoTaskAsync(task, execution, timeout));
            Threads.Add(t);
            t.Start();    
        }
        
        public int GetRowCount(AggregateConfiguration aggregate)
        {
            return -1;
        }

        public void CancelAllTasks(bool alsoClearTaskList)
        {
            foreach (var v in Tasks.Values)
                if (v.IsExecuting)
                    v.CancellationTokenSource.Cancel();

            if(alsoClearTaskList)
                Tasks.Clear();
        }


        public void CancelTask(ICompileable compileable, bool alsoClearFromTaskList)
        {
            if (Tasks.ContainsKey(compileable))
            {

                if (Tasks[compileable].IsExecuting)
                    Tasks[compileable].CancellationTokenSource.Cancel();

                if(alsoClearFromTaskList)
                    Tasks.Remove(compileable);
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
