// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Job.Scheduling;

/// <summary>
/// Return a ScheduledDataLoadJob hydrated with appropriate dates for the LoadProgress supplied.  This class differs from SingleScheduledJobFactory only
/// in that it lets you pass multiple ILoadProgress instead of only one, the class will decide which is the next most sensible one to run.  For example
/// you might have 'Load Biochem For Tayside' and 'Load Biochem For Fife' as two LoadProgress in the LoadMetadata 'Loading Biochemistry', this class would
/// pick which one to execute next.
/// </summary>
public class MultipleScheduleJobFactory : ScheduledJobFactory
{
    private readonly Dictionary<ILoadProgress, IJobDateGenerationStrategy> _availableSchedules;
    private readonly List<ILoadProgress> _scheduleList;
    private int _lastScheduleId;

    public MultipleScheduleJobFactory(Dictionary<ILoadProgress, IJobDateGenerationStrategy> availableSchedules,
        int? overrideNumberOfDaysToLoad, ILoadMetadata loadMetadata, ILogManager logManager)
        : base(overrideNumberOfDaysToLoad, loadMetadata, logManager)
    {
        _availableSchedules = availableSchedules;

        _scheduleList = _availableSchedules.Keys.ToList();

        _lastScheduleId = 0;
    }

    /// <summary>
    /// Returns false only if no schedule has any jobs associated with it
    /// </summary>
    /// <returns></returns>
    public override bool HasJobs()
    {
        return _scheduleList.Any(loadProgress =>
            _availableSchedules[loadProgress]
                .GetTotalNumberOfJobs(OverrideNumberOfDaysToLoad ?? loadProgress.DefaultNumberOfDaysToLoadEachTime,
                    false) > 0);
    }

    protected override ScheduledDataLoadJob CreateImpl(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        IDataLoadEventListener listener, HICDatabaseConfiguration configuration)
    {
        var loadProgress = _scheduleList[_lastScheduleId];
        var datesToRetrieve = _availableSchedules[loadProgress]
            .GetDates(OverrideNumberOfDaysToLoad ?? _scheduleList[_lastScheduleId].DefaultNumberOfDaysToLoadEachTime,
                false);
        if (!datesToRetrieve.Any())
            return null;

        var LoadDirectory = new LoadDirectory(LoadMetadata.LocationOfForLoadingDirectory);
        var job = new ScheduledDataLoadJob(repositoryLocator, JobDescription, LogManager, LoadMetadata, LoadDirectory,
            listener, configuration)
        {
            LoadProgress = loadProgress,
            DatesToRetrieve = datesToRetrieve
        };

        // move our circular pointer for the round-robin assignment
        _lastScheduleId = (_lastScheduleId + 1) % _scheduleList.Count;

        return job;
    }
}