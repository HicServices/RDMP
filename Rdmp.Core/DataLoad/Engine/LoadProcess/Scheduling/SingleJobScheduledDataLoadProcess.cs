// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
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
///     run a single execution of a LoadProgress.  For example if you have a load 'Load biochemistry records' with a
///     LoadProgress which 'loads 5 days at a time'
///     and is currently at LoadProgress.DataLoadProgress of 2001-01-01 it will run a single load (See
///     ScheduledDataLoadJob) for the next 5 days and then stop.
/// </summary>
public class SingleJobScheduledDataLoadProcess : ScheduledDataLoadProcess
{
    private SingleScheduledJobFactory _scheduledJobFactory;

    // todo: refactor to cut down on ctor params
    public SingleJobScheduledDataLoadProcess(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ILoadMetadata loadMetadata, ICheckable preExecutionChecker, IDataLoadExecution loadExecution,
        JobDateGenerationStrategyFactory jobDateGenerationStrategyFactory,
        ILoadProgressSelectionStrategy loadProgressSelectionStrategy, int? overrideNumberOfDaysToLoad,
        ILogManager logManager, IDataLoadEventListener dataLoadEventListener, HICDatabaseConfiguration configuration) :
        base(repositoryLocator, loadMetadata, preExecutionChecker, loadExecution, jobDateGenerationStrategyFactory,
            loadProgressSelectionStrategy, overrideNumberOfDaysToLoad, logManager, dataLoadEventListener, configuration)
    {
    }

    public override ExitCodeType Run(GracefulCancellationToken loadCancellationToken, object payload = null)
    {
        // single job, so grab the first available LoadProgress and lock it
        var loadProgresses = LoadProgressSelectionStrategy.GetAllLoadProgresses();
        if (!loadProgresses.Any())
            return ExitCodeType.OperationNotRequired;

        var loadProgress = loadProgresses.First();

        // we don't need any other schedules the strategy may have given us
        loadProgresses.Remove(loadProgress);

        // Create the job factory
        if (_scheduledJobFactory != null)
            throw new Exception("Job factory should only be created once");

        _scheduledJobFactory = new SingleScheduledJobFactory(loadProgress,
            JobDateGenerationStrategyFactory.Create(loadProgress, DataLoadEventListener),
            OverrideNumberOfDaysToLoad ?? loadProgress.DefaultNumberOfDaysToLoadEachTime, LoadMetadata, LogManager);

        // If the job factory won't produce any jobs we can bail out here
        if (!_scheduledJobFactory.HasJobs())
            return ExitCodeType.OperationNotRequired;

        // Run the data load
        JobProvider = _scheduledJobFactory;

        return base.Run(loadCancellationToken, payload);
    }
}