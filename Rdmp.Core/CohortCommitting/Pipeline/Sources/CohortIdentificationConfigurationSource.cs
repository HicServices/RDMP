// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using System.Threading;
using Rdmp.Core.CohortCreation;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.QueryCaching.Aggregation;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.CohortCommitting.Pipeline.Sources;

/// <summary>
///     Executes a Cohort Identification Configuration query and releases the identifiers read into the pipeline as a
///     single column DataTable.
/// </summary>
public class CohortIdentificationConfigurationSource : IPluginDataFlowSource<DataTable>,
    IPipelineRequirement<CohortIdentificationConfiguration>
{
    private CohortIdentificationConfiguration _cohortIdentificationConfiguration;

    [DemandsInitialization(
        "The length of time (in seconds) to wait before timing out the SQL command to execute the CohortIdentificationConfiguration, if you find it is taking exceptionally long for a CohortIdentificationConfiguration to execute then consider caching some of the subqueries",
        DemandType.Unspecified, 10000)]
    public int Timeout { get; set; }

    [DemandsInitialization(
        "If ticked, will Freeze the CohortIdentificationConfiguration if the import pipeline terminates successfully")]
    public bool FreezeAfterSuccessfulImport { get; set; }

    private bool haveSentData;
    private readonly CancellationTokenSource _cancelGlobalOperations = new();

    /// <summary>
    ///     If you are refreshing a cohort or running a cic which was run and cached a long time ago you might want to clear
    ///     out the cache.  This will mean that
    ///     when run you will get a view of the live tables (which might be recached as part of building the cic) rather than
    ///     the (potentially stale) current cache
    /// </summary>
    public bool ClearCohortIdentificationConfigurationCacheBeforeRunning { get; set; }

    public DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        if (haveSentData)
            return null;

        haveSentData = true;

        return GetDataTable(listener);
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        // Freeze if requested, if it didn't crash
        if (pipelineFailureExceptionIfAny != null || !FreezeAfterSuccessfulImport) return;

        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information, "Freezing CohortIdentificationConfiguration"));
        _cohortIdentificationConfiguration.Freeze();
    }


    public void Abort(IDataLoadEventListener listener)
    {
        _cancelGlobalOperations.Cancel();
    }

    public DataTable TryGetPreview()
    {
        return GetDataTable(ThrowImmediatelyDataLoadEventListener.Quiet);
    }

    private DataTable GetDataTable(IDataLoadEventListener listener)
    {
        listener ??= ThrowImmediatelyDataLoadEventListener.Quiet;

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"About to lookup which server to interrogate for CohortIdentificationConfiguration {_cohortIdentificationConfiguration}"));

        if (_cohortIdentificationConfiguration.RootCohortAggregateContainer_ID == null)
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                $"CohortIdentificationConfiguration '{_cohortIdentificationConfiguration}' has no RootCohortAggregateContainer_ID, is it empty?"));

        var cohortCompiler = new CohortCompiler(_cohortIdentificationConfiguration);

        var rootContainerTask =
            //no caching set up so no point in running CohortCompilerRunner
            _cohortIdentificationConfiguration.QueryCachingServer_ID == null
                ? RunRootContainerOnlyNoCaching(cohortCompiler)
                : RunAllTasksWithRunner(cohortCompiler, listener);

        if (rootContainerTask.State == CompilationState.Executing)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning,
                    "Root container task was unexpectedly still executing... let's give it a little longer to run"));

            var countdown = Math.Max(5000, Timeout * 1000);
            while (rootContainerTask.State == CompilationState.Executing && countdown > 0)
            {
                Thread.Sleep(100);
                countdown -= 100;
            }
        }

        if (rootContainerTask.State != CompilationState.Finished)
            throw new Exception($"CohortIdentificationCriteria execution resulted in state '{rootContainerTask.State}'",
                rootContainerTask.CrashMessage);

        if (rootContainerTask == null)
            throw new Exception("Root container task was null, was the execution cancelled? / crashed");

        var execution = cohortCompiler.Tasks[rootContainerTask];

        if (execution.Identifiers == null || execution.Identifiers.Rows.Count == 0)
            throw new Exception(
                "CohortIdentificationCriteria execution resulted in an empty dataset (there were no cohorts matched by the query?)");

        var dt = execution.Identifiers;
        foreach (DataColumn column in dt.Columns)
            column.ReadOnly = false;

        return dt;
    }


    private ICompileable RunRootContainerOnlyNoCaching(CohortCompiler cohortCompiler)
    {
        //add root container task
        var task = cohortCompiler.AddTask(_cohortIdentificationConfiguration.RootCohortAggregateContainer,
            _cohortIdentificationConfiguration.GetAllParameters());

        cohortCompiler.LaunchSingleTask(task, Timeout, false);

        //timeout is in seconds
        var countDown = Math.Max(5000, Timeout * 1000);

        while (
            //hasn't timed out
            countDown > 0 &&
            task.State is CompilationState.Executing or CompilationState.NotScheduled or CompilationState.Scheduled
        )
        {
            Thread.Sleep(100);
            countDown -= 100;
        }


        if (countDown <= 0)
            try
            {
                throw new Exception(
                    $"Cohort failed to reach a final state (Finished/Crashed) after {Timeout} seconds. Current state is {task.State}.  The task will be cancelled");
            }
            finally
            {
                cohortCompiler.CancelAllTasks(true);
            }

        return task;
    }

    private ICompileable RunAllTasksWithRunner(CohortCompiler cohortCompiler, IDataLoadEventListener listener)
    {
        if (ClearCohortIdentificationConfigurationCacheBeforeRunning)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information, "Clearing Cohort Identifier Cache"));

            var cacheManager =
                new CachedAggregateConfigurationResultsManager(_cohortIdentificationConfiguration.QueryCachingServer);

            cohortCompiler.AddAllTasks(false);
            foreach (var cacheable in cohortCompiler.Tasks.Keys.OfType<ICacheableTask>())
                cacheable.ClearYourselfFromCache(cacheManager);
        }

        var runner = new CohortCompilerRunner(cohortCompiler, Timeout)
        {
            RunSubcontainers = false
        };
        runner.PhaseChanged += (s, e) => listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"CohortCompilerRunner entered Phase '{runner.ExecutionPhase}'"));
        return runner.Run(_cancelGlobalOperations.Token);
    }

    public void Check(ICheckNotifier notifier)
    {
        var container = _cohortIdentificationConfiguration.RootCohortAggregateContainer;
        if (container != null)
        {
            if (container.IsDisabled)
                notifier.OnCheckPerformed(new CheckEventArgs("Root container is disabled", CheckResult.Fail));

            foreach (var sub in container.GetAllSubContainersRecursively())
            {
                if (sub.IsDisabled)
                    notifier.OnCheckPerformed(new CheckEventArgs($"Query includes disabled container '{sub}'",
                        CheckResult.Warning));

                foreach (var configuration in sub.GetAggregateConfigurations())
                    if (configuration.IsDisabled)
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            $"Query includes disabled aggregate '{configuration}'", CheckResult.Warning));
            }
        }

        try
        {
            if (_cohortIdentificationConfiguration.Frozen)
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"CohortIdentificationConfiguration {_cohortIdentificationConfiguration} is Frozen (By {_cohortIdentificationConfiguration.FrozenBy} on {_cohortIdentificationConfiguration.FrozenDate}).  It might have already been imported once before.",
                        CheckResult.Warning));

            if (!UserSettings.SkipCohortBuilderValidationOnCommit)
            {
                var result = TryGetPreview();

                if (result.Rows.Count == 0)
                    throw new Exception("No Identifiers were returned by the cohort query");
            }
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Could not build extraction SQL for {_cohortIdentificationConfiguration}", CheckResult.Fail, e));
        }
    }


    public void PreInitialize(CohortIdentificationConfiguration value, IDataLoadEventListener listener)
    {
        _cohortIdentificationConfiguration = value;
    }
}