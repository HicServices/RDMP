using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CohortCreation.Execution.Joinables;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.QueryCaching.Aggregation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rdmp.Core.CohortCreation;

/// <summary>
/// Common methods used by Cohort Builder UI implementations.  Eliminates
/// code duplication and makes it possible to add new UI formats later
/// e.g. web/console etc
/// </summary>
public class CohortIdentificationConfigurationUICommon
{
    public CohortIdentificationConfiguration Configuration;

    public ExternalDatabaseServer QueryCachingServer;
    private CohortAggregateContainer _root;
    CancellationTokenSource _cancelGlobalOperations;
    private ISqlParameter[] _globals;
    public CohortCompilerRunner Runner;

    /// <summary>
    /// User interface layer for modal dialogs, showing Exceptions etc
    /// </summary>
    public IBasicActivateItems Activator;

    /// <summary>
    /// Duration in seconds to allow tasks to run for before cancelling
    /// </summary>
    public int Timeout = 3000;

    public CohortCompiler Compiler { get; }
    public CohortIdentificationConfigurationUICommon()
    {
        Compiler = new CohortCompiler(null);
    }
    public object Working_AspectGetter(object rowobject)
    {
        return GetKey(rowobject)?.State;
    }

    public object Time_AspectGetter(object rowobject)
    {
        return GetKey(rowobject)?.ElapsedTime?.ToString(@"hh\:mm\:ss");
    }

    public object CumulativeTotal_AspectGetter(object rowobject)
    {
        return GetKey(rowobject)?.CumulativeRowCount?.ToString("N0");
    }

    public ICompileable GetKey(object rowobject)
    {
        lock (Compiler.Tasks)
        {
            return
                Compiler?.Tasks?.Keys.FirstOrDefault(k =>

                    (rowobject is AggregateConfiguration ac && k.Child is JoinableCohortAggregateConfiguration j
                                                            && j.AggregateConfiguration_ID == ac.ID)
                    || k.Child.Equals(rowobject));
        }
    }

    public object Cached_AspectGetter(object rowobject)
    {
        var key = GetKey(rowobject);

        if (key != null)
            return Configuration.QueryCachingServer_ID == null ? "No Cache" : key.GetCachedQueryUseCount();

        return null;
    }

    public object Count_AspectGetter(object rowobject)
    {
        var key = GetKey(rowobject);

        if (key != null && key.State == CompilationState.Finished)
            return key.FinalRowCount.ToString("N0");

        return null;
    }
    public object Catalogue_AspectGetter(object rowobject)
    {
        return
            rowobject is AggregateConfiguration ac ? ac.Catalogue.Name : null;
    }

    public object ExecuteAspectGetter(object rowObject)
    {
        //don't expose any buttons if global execution is in progress
        if (IsExecutingGlobalOperations())
            return null;

        if (rowObject is AggregateConfiguration || rowObject is CohortAggregateContainer)
        {
            var plannedOp = GetNextOperation(GetState((IMapsDirectlyToDatabaseTable)rowObject));

            if (plannedOp == Operation.None)
                return null;

            return plannedOp;
        }

        return null;
    }

    private CompilationState GetState(IMapsDirectlyToDatabaseTable o)
    {
        lock (Compiler.Tasks)
        {
            var task = GetTaskIfExists(o);

            if (task == null)
                return CompilationState.NotScheduled;

            return task.State;
        }
    }
    public bool IsExecutingGlobalOperations()
    {
        return Runner != null && Runner.ExecutionPhase != CohortCompilerRunner.Phase.None && Runner.ExecutionPhase != CohortCompilerRunner.Phase.Finished;
    }
    private Operation GetNextOperation(CompilationState currentState)
    {
        return currentState switch
        {
            CompilationState.NotScheduled => Operation.Execute,
            CompilationState.Building => Operation.Cancel,
            CompilationState.Scheduled => Operation.None,
            CompilationState.Executing => Operation.Cancel,
            CompilationState.Finished => Operation.Execute,
            CompilationState.Crashed => Operation.Execute,
            _ => throw new ArgumentOutOfRangeException("currentState")
        };
    }

    /// <summary>
    /// Rebuilds the CohortCompiler diagram which shows all the currently configured tasks
    /// </summary>
    /// <param name="cancelTasks"></param>
    public void RecreateAllTasks(bool cancelTasks = true)
    {
        if (cancelTasks)
            Compiler.CancelAllTasks(false);

        Configuration.CreateRootContainerIfNotExists();
        //if there is no root container,create one
        _root = Configuration.RootCohortAggregateContainer;
        _globals = Configuration.GetAllParameters();

        //Could have configured/unconfigured a joinable state
        foreach (var j in Compiler.Tasks.Keys.OfType<JoinableTask>())
            j.RefreshIsUsedState();

    }

    public void SetShowCumulativeTotals(bool show)
    {
        Compiler.IncludeCumulativeTotals = show;
        RecreateAllTasks();
    }

    private void OrderActivity(Operation operation, IMapsDirectlyToDatabaseTable o)
    {
        switch (operation)
        {
            case Operation.Execute:
                StartThisTaskOnly(o);
                break;
            case Operation.Cancel:
                Cancel(o);
                break;
            case Operation.Clear:
                Clear(o);
                break;
            case Operation.None:
                break;
            default:
                throw new ArgumentOutOfRangeException("operation");
        }
    }

    private void StartThisTaskOnly(IMapsDirectlyToDatabaseTable configOrContainer)
    {
        var task = Compiler.AddTask(configOrContainer, _globals);
        if (task.State == CompilationState.Crashed)
        {
            Activator.ShowException("Task failed to build", task.CrashMessage);
            return;
        }
        //Cancel the task and remove it from the Compilers task list - so it no longer knows about it
        Compiler.CancelTask(task, true);

        RecreateAllTasks(false);

        task = Compiler.AddTask(configOrContainer, _globals);

        //Task is now in state NotScheduled so we can start it
        Compiler.LaunchSingleTask(task, Timeout, true);
    }

    public void Cancel(IMapsDirectlyToDatabaseTable o)
    {
        var task = Compiler.Tasks.Single(t => t.Key.Child.Equals(o));
        Compiler.CancelTask(task.Key, true);
    }

    public void CancelAll()
    {
        //don't start any more global operations if your midway through
        if (_cancelGlobalOperations != null)
            _cancelGlobalOperations.Cancel();

        Compiler.CancelAllTasks(true);
        RecreateAllTasks();
    }


    public ICompileable GetTaskIfExists(IMapsDirectlyToDatabaseTable o)
    {
        lock (Compiler.Tasks)
        {
            var kvps = Compiler.Tasks.Where(t => t.Key.Child.Equals(o)).ToArray();

            if (kvps.Length == 0)
            {
                return null;
            }

            if (kvps.Length == 1)
            {
                return kvps[0].Key;
            }

            var running = kvps.FirstOrDefault(k => k.Value != null).Key;

            return running ?? kvps[0].Key;
        }
    }

    public void Clear(IMapsDirectlyToDatabaseTable o)
    {
        lock (Compiler.Tasks)
        {
            var task = GetTaskIfExists(o);

            if (task == null)
                return;

            var c = task as CacheableTask;
            if (c != null)
                ClearCacheFor(new ICacheableTask[] { c });

            Compiler.CancelTask(task, true);
        }
    }

    public void ClearAllCaches()
    {
        ClearCacheFor(Compiler.Tasks.Keys.OfType<ICacheableTask>().Where(t => !t.IsCacheableWhenFinished()).ToArray());
    }

    public void ClearCacheFor(ICacheableTask[] tasks)
    {
        var manager = new CachedAggregateConfigurationResultsManager(QueryCachingServer);

        int successes = 0;
        foreach (ICacheableTask t in tasks)
            try
            {
                t.ClearYourselfFromCache(manager);
                Compiler.CancelTask(t, true);
                successes++;
            }
            catch (Exception exception)
            {
                Activator.ShowException("Could not clear cache for task " + t, exception);
            }

        RecreateAllTasks();
    }


    #region Job control
    public enum Operation
    {
        Execute,
        Cancel,
        Clear,
        None
    }

    public Operation PlanGlobalOperation()
    {
        var allTasks = GetAllTasks();

        //if any are still executing or scheduled for execution
        if (allTasks.Any(t => t.State == CompilationState.Executing || t.State == CompilationState.Building || t.State == CompilationState.Scheduled))
            return Operation.Cancel;

        //if all are complete
        return Operation.Execute;
    }
    #endregion

    public ICompileable[] GetAllTasks()
    {
        return Compiler.Tasks.Keys.ToArray();
    }

    /// <summary>
    /// Considers the state of <see cref="Compiler"/> to check for still running
    /// processes.  Returns true to cancel closing (also informs user that closing
    /// cannot happen right now).
    /// </summary>
    /// <returns></returns>
    public bool ConsultAboutClosing()
    {
        if (Compiler != null)
        {
            var aliveCount = Compiler.GetAliveThreadCount();
            if (aliveCount > 0)
            {
                Activator.Show("Confirm Close", "There are " + aliveCount +
                                " Tasks currently executing, you must cancel them before closing");
                
                return true;
            }
            else
            {
                Compiler.CancelAllTasks(true);
            }
        }

        return false;
    }

    /// <summary>
    /// Inspects the state of the object and either starts its execution or
    /// cancels it.  See <see cref="ExecuteAspectGetter(object)"/> to display
    /// the appropriate message to the user
    /// </summary>
    /// <param name="o"></param>
    public void ExecuteOrCancel(object o)
    {
        var aggregate = o as AggregateConfiguration;
        var container = o as CohortAggregateContainer;

        Task.Run(() =>
        {
            if (aggregate != null)
            {
                var joinable = aggregate.JoinableCohortAggregateConfiguration;

                if (joinable != null)
                    OrderActivity(GetNextOperation(GetState(joinable)), joinable);
                else
                    OrderActivity(GetNextOperation(GetState(aggregate)), aggregate);
            }
            if (container != null)
            {
                OrderActivity(GetNextOperation(GetState(container)), container);
            }
        });
    }

    public void StartAll(Action afterDelegate,EventHandler onRunnerPhaseChanged)
    {

        //only allow starting all if we are not mid execution already
        if (IsExecutingGlobalOperations())
            return;

        _cancelGlobalOperations = new CancellationTokenSource();


        Runner = new CohortCompilerRunner(Compiler, Timeout);
        Runner.PhaseChanged += onRunnerPhaseChanged;
        Task.Run(() =>
        {
            try
            {
                Runner.Run(_cancelGlobalOperations.Token);
            }
            catch (Exception e)
            {
                Activator.ShowException("Runer crashed",e);
            }

        }).ContinueWith((s, e) => {
            afterDelegate();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }
}
