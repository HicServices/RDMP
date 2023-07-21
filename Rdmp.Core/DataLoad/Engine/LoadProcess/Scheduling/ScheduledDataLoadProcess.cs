// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.DataLoad;
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
///     Loads data according to a data-based schedule, e.g. Biochemistry.
///     Needs to know: how to generate dates for the job, how to select a load schedule
/// </summary>
public abstract class ScheduledDataLoadProcess : DataLoadProcess
{
    protected readonly JobDateGenerationStrategyFactory JobDateGenerationStrategyFactory;
    protected readonly ILoadProgressSelectionStrategy LoadProgressSelectionStrategy;
    protected readonly int? OverrideNumberOfDaysToLoad;

    protected ScheduledDataLoadProcess(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        ILoadMetadata loadMetadata, ICheckable preExecutionChecker, IDataLoadExecution loadExecution,
        JobDateGenerationStrategyFactory jobDateGenerationStrategyFactory,
        ILoadProgressSelectionStrategy loadProgressSelectionStrategy, int? overrideNumberOfDaysToLoad,
        ILogManager logManager, IDataLoadEventListener dataLoadEventListener, HICDatabaseConfiguration configuration)
        : base(repositoryLocator, loadMetadata, preExecutionChecker, logManager, dataLoadEventListener, loadExecution,
            configuration)
    {
        JobDateGenerationStrategyFactory = jobDateGenerationStrategyFactory;
        LoadProgressSelectionStrategy = loadProgressSelectionStrategy;
        OverrideNumberOfDaysToLoad = overrideNumberOfDaysToLoad;
    }
}