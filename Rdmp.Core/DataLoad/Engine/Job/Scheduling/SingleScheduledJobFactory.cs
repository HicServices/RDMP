// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Job.Scheduling;

/// <summary>
///     Return a ScheduledDataLoadJob hydrated with appropriate dates for the LoadProgress supplied (e.g. load the next 5
///     days of Load Progress 'Tayside Biochem
///     Loading').
/// </summary>
public class SingleScheduledJobFactory : ScheduledJobFactory
{
    private readonly ILoadProgress _loadProgress;
    private readonly IJobDateGenerationStrategy _jobDateGenerationStrategy;

    public SingleScheduledJobFactory(ILoadProgress loadProgress, IJobDateGenerationStrategy jobDateGenerationStrategy,
        int overrideNumberOfDaysToLoad, ILoadMetadata loadMetadata, ILogManager logManager) : base(
        overrideNumberOfDaysToLoad, loadMetadata, logManager)
    {
        _loadProgress = loadProgress;
        _jobDateGenerationStrategy = jobDateGenerationStrategy;
    }

    public override bool HasJobs()
    {
        return _jobDateGenerationStrategy.GetTotalNumberOfJobs(
            OverrideNumberOfDaysToLoad ?? _loadProgress.DefaultNumberOfDaysToLoadEachTime, false) > 0;
    }

    protected override ScheduledDataLoadJob CreateImpl(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        IDataLoadEventListener listener, HICDatabaseConfiguration configuration)
    {
        var LoadDirectory = new LoadDirectory(LoadMetadata.LocationOfFlatFiles);
        return new ScheduledDataLoadJob(repositoryLocator, JobDescription, LogManager, LoadMetadata, LoadDirectory,
            listener, configuration)
        {
            LoadProgress = _loadProgress,
            DatesToRetrieve =
                _jobDateGenerationStrategy.GetDates(
                    OverrideNumberOfDaysToLoad ?? _loadProgress.DefaultNumberOfDaysToLoadEachTime, false)
        };
    }
}