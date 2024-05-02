// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.DataLoad.Engine.LoadExecution;
using Rdmp.Core.DataLoad.Engine.LoadProcess.Scheduling.Strategy;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadProcess.Scheduling;

/// <summary>
///     DataLoadProcess for LoadMetadata's which have one or more LoadProgresses (See ScheduledDataLoadProcess).  This
///     version of ScheduledDataLoadProcess will
///     run loads one after another until all LoadProgresses are exhausted (they have loaded all data up to today / when
///     available data stops).  For example
///     if you have a load 'Load biochemistry records' with a LoadProgress which 'loads 5 days at a time' and is currently
///     at LoadProgress.DataLoadProgress of
///     2001-01-01 it will keep running data loads iteratively until the IDataLoadExecution returns OperationNotRequired
///     (i.e. the load is up-to-date).
/// </summary>
public class IterativeScheduledDataLoadProcess : ScheduledDataLoadProcess
{
    // todo: refactor to cut down on ctor params
    public IterativeScheduledDataLoadProcess(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ILoadMetadata loadMetadata, ICheckable preExecutionChecker, IDataLoadExecution loadExecution,
        JobDateGenerationStrategyFactory jobDateGenerationStrategyFactory,
        ILoadProgressSelectionStrategy loadProgressSelectionStrategy, int? overrideNumberOfDaysToLoad,
        ILogManager logManager, IDataLoadEventListener dataLoadEventsreceiver, HICDatabaseConfiguration configuration)
        : base(repositoryLocator, loadMetadata, preExecutionChecker, loadExecution, jobDateGenerationStrategyFactory,
            loadProgressSelectionStrategy, overrideNumberOfDaysToLoad, logManager, dataLoadEventsreceiver,
            configuration)
    {
    }

    public override ExitCodeType Run(GracefulCancellationToken loadCancellationToken, object payload = null)
    {
        // grab all the load schedules we can and lock them
        var loadProgresses = LoadProgressSelectionStrategy.GetAllLoadProgresses();
        if (!loadProgresses.Any())
            return ExitCodeType.OperationNotRequired;

        // create job factory
        var progresses = loadProgresses.ToDictionary(loadProgress => loadProgress,
            loadProgress => JobDateGenerationStrategyFactory.Create(loadProgress, DataLoadEventListener));
        var jobProvider =
            new MultipleScheduleJobFactory(progresses, OverrideNumberOfDaysToLoad, LoadMetadata, LogManager);

        // check if the factory will produce any jobs, if not we can stop here
        if (!jobProvider.HasJobs())
            return ExitCodeType.OperationNotRequired;

        // Run the data load process
        JobProvider = jobProvider;

        //Do a data load
        ExitCodeType result;
        while ((result = base.Run(loadCancellationToken, payload)) ==
               ExitCodeType.Success) //stop if it said not required
        {
            //or if between executions the token is set
            if (loadCancellationToken.IsAbortRequested)
                return ExitCodeType.Abort;

            if (loadCancellationToken.IsCancellationRequested)
                return ExitCodeType.Success;
        }

        //should be Operation Not Required or Error since the token inside handles stopping
        return result;
    }
}