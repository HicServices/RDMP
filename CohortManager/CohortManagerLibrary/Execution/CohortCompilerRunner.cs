using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CohortManagerLibrary.Execution.Joinables;
using QueryCaching.Aggregation;
using QueryCaching.Aggregation.Arguments;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CohortManagerLibrary.Execution
{
    /// <summary>
    /// Manages the adding and executing of tasks in a CohortCompiler to execute a CohortIdentificationConfiguration in the most optimised way when a cache server is present.  For example it
    /// will run all the individual 'Patient Index Tables' first and cache them, then run the individual 'cohort queries' and cache those before finally running the top level set containers
    /// (which will now execute from the cached lists).
    /// </summary>
    public class CohortCompilerRunner
    {
        private readonly int _timeout;
        public CohortCompiler Compiler { get; set; }
        public Phase ExecutionPhase = Phase.None;

        public event EventHandler PhaseChanged;

        private CohortIdentificationConfiguration _cic;
        private ExternalDatabaseServer _queryCachingServer;


        public CohortCompilerRunner(CohortCompiler compiler, int timeout)
        {
            _timeout = timeout;
            Compiler = compiler;

            if(Compiler.CohortIdentificationConfiguration == null)
                throw new ArgumentException("CohortCompiler must have a CohortIdentificationConfiguration");

            _cic = Compiler.CohortIdentificationConfiguration;

            if (_cic.QueryCachingServer_ID != null)
                _queryCachingServer = _cic.QueryCachingServer;
        }

        public enum Phase
        {
            None,
            RunningJoinableTasks,
            CachingJoinableTasks,
            RunningAggregateTasks,
            CachingAggregateTasks,
            RunningFinalTotals,
            Finished
        }

        public void Run()
        {
            var globals = _cic.GetAllParameters();

            //clear compiler list
            Compiler.CancelAllTasks(true);

            SetPhase(Phase.RunningJoinableTasks);

            foreach (var j in _cic.GetAllJoinables())
                Compiler.AddTask(j, globals);

            Compiler.CancelAllTasks(false);

            RunAsync(Compiler.Tasks.Keys.Where(c => c is JoinableTaskExecution && c.State == CompilationState.NotScheduled));

            SetPhase(Phase.CachingJoinableTasks);

            CacheAsync(Compiler.Tasks.Keys.OfType<JoinableTaskExecution>().Where(c => c.State == CompilationState.Finished && c.IsCacheableWhenFinished()));

            SetPhase(Phase.RunningAggregateTasks);

            foreach (var a in _cic.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively())
                Compiler.AddTask(a, globals);

            Compiler.CancelAllTasks(false);

            RunAsync(Compiler.Tasks.Keys.Where(c => c is AggregationTask && c.State == CompilationState.NotScheduled));

            SetPhase(Phase.CachingAggregateTasks);

            CacheAsync(Compiler.Tasks.Keys.OfType<AggregationTask>().Where(c => c.State == CompilationState.Finished && c.IsCacheableWhenFinished()));

            SetPhase(Phase.RunningFinalTotals);

            Compiler.AddTask(_cic.RootCohortAggregateContainer, globals);

            foreach (var a in _cic.RootCohortAggregateContainer.GetAllSubContainersRecursively())
                Compiler.AddTask(a, globals);

            Compiler.CancelAllTasks(false);

            RunAsync(Compiler.Tasks.Keys.Where(c => c.State == CompilationState.NotScheduled));

            SetPhase(Phase.Finished);
        }

        private void RunAsync(IEnumerable<ICompileable> toRun)
        {
            var tasks = toRun.ToArray();

            foreach (var r in tasks)
                Compiler.LaunchSingleTask(r, _timeout);

            //while there are executing tasks
            while (tasks.Any(t => t.State == CompilationState.Scheduled || t.State == CompilationState.Executing))
                Thread.Sleep(1000);
        }

        private void CacheAsync(IEnumerable<ICacheableTask> toCache)
        {
            if (_queryCachingServer == null)
                return;

            foreach (var c in toCache)
                SaveToCache(c);
        }
        
        private void SaveToCache(ICacheableTask cacheable)
        {
            CachedAggregateConfigurationResultsManager manager = new CachedAggregateConfigurationResultsManager(_queryCachingServer);

            var explicitTypes = new List<DatabaseColumnRequest>();

            AggregateConfiguration configuration = cacheable.GetAggregateConfiguration();
            try
            {
                ColumnInfo identifierColumnInfo = configuration.AggregateDimensions.Single(c => c.IsExtractionIdentifier).ColumnInfo;
                explicitTypes.Add(new DatabaseColumnRequest(identifierColumnInfo.GetRuntimeName(), identifierColumnInfo.Data_type));
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred trying to find the data type of the identifier column when attempting to submit the result data table to the cache", e);
            }

            CacheCommitArguments args = cacheable.GetCacheArguments(Compiler.Tasks[cacheable].CountSQL, Compiler.Tasks[cacheable].Identifiers, explicitTypes.ToArray());

            manager.CommitResults(args);
        }

        private void SetPhase(Phase p)
        {
            ExecutionPhase = p;
            if (PhaseChanged != null)
                PhaseChanged(this,new EventArgs());
        }
    }
}
