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
        
        /// <summary>
        /// The root container is always added to the task list but you could skip subcontainer totals if all you care about is the final total for the cohort
        /// and you don't have a dependant UI etc.  Setting false will add all joinables, subqueries etc and the root container (final answer for who is in cohort) 
        /// but not the other subcontainers (if there were any in the first place!).  Defaults to true.
        /// </summary>
        public bool RunSubcontainers { get; set; }
        
        /// <summary>
        /// Creates a new runner for the given <paramref name="compiler"/> which will facilitate running it's Tasks in a sensible order using result caching if possible
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="timeout">CommandTimeout for each individual command in seconds</param>
        public CohortCompilerRunner(CohortCompiler compiler, int timeout)
        {
            _timeout = timeout;
            Compiler = compiler;
            
            if(Compiler.CohortIdentificationConfiguration == null)
                throw new ArgumentException("CohortCompiler must have a CohortIdentificationConfiguration");

            _cic = Compiler.CohortIdentificationConfiguration;

            if (_cic.QueryCachingServer_ID != null)
                _queryCachingServer = _cic.QueryCachingServer;

            RunSubcontainers = true;
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
        
        public ICompileable Run(CancellationToken token)
        {
            try
            {
                var globals = _cic.GetAllParameters();

                //clear compiler list
                Compiler.CancelAllTasks(true);

                SetPhase(Phase.RunningJoinableTasks);

                foreach (var j in _cic.GetAllJoinables())
                    Compiler.AddTask(j, globals);

                Compiler.CancelAllTasks(false);

                RunAsync(Compiler.Tasks.Keys.Where(c => c is JoinableTask && c.State == CompilationState.NotScheduled),token);

                SetPhase(Phase.CachingJoinableTasks);

                CacheAsync(Compiler.Tasks.Keys.OfType<JoinableTask>().Where(c => c.State == CompilationState.Finished && c.IsCacheableWhenFinished()),token);

                SetPhase(Phase.RunningAggregateTasks);

                Compiler.AddTasksRecursively(globals, _cic.RootCohortAggregateContainer, false);

                Compiler.CancelAllTasks(false);

                RunAsync(Compiler.Tasks.Keys.Where(c => c is AggregationTask && c.State == CompilationState.NotScheduled), token);

                SetPhase(Phase.CachingAggregateTasks);

                CacheAsync(Compiler.Tasks.Keys.OfType<AggregationTask>().Where(c => c.State == CompilationState.Finished && c.IsCacheableWhenFinished()), token);

                SetPhase(Phase.RunningFinalTotals);

                var toReturn = Compiler.AddTask(_cic.RootCohortAggregateContainer, globals);

                if(RunSubcontainers)
                    foreach (var a in _cic.RootCohortAggregateContainer.GetAllSubContainersRecursively())
                        Compiler.AddTask(a, globals);

                Compiler.CancelAllTasks(false);

                RunAsync(Compiler.Tasks.Keys.Where(c => c.State == CompilationState.NotScheduled), token);

                SetPhase(Phase.Finished);
            
                return toReturn;
            }
            catch (OperationCanceledException)
            {
                SetPhase(Phase.None);
                return null;
            }
        }

        private void RunAsync(IEnumerable<ICompileable> toRun, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var tasks = toRun.ToArray();

            foreach (var r in tasks)
                Compiler.LaunchSingleTask(r, _timeout,false);

            //while there are executing tasks
            while (tasks.Any(t => t.State == CompilationState.Scheduled || t.State == CompilationState.Executing))
                Thread.Sleep(1000);
        }

        private void CacheAsync(IEnumerable<ICacheableTask> toCache, CancellationToken token)
        {
            if (_queryCachingServer == null)
                return;

            token.ThrowIfCancellationRequested();

            foreach (var c in toCache) 
                Compiler.CacheSingleTask(c, _queryCachingServer);
        }

        private void SetPhase(Phase p)
        {
            ExecutionPhase = p;

            var h = PhaseChanged;
            if (h != null)
                h(this,new EventArgs());
        }
    }
}
