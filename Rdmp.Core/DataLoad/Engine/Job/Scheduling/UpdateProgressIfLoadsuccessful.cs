// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Job.Scheduling;

/// <summary>
///     Data Load Engine disposal step for scheduled data loads (See ScheduledDataLoadJob) in which the LoadProgress head
///     pointer date is updated.  E.g. if the
///     job was to load 5 days then the LoadProgress.DataLoadProgress date would be updated to reflect the loaded date
///     range.  This is non trivial because it might
///     be that although the job was to load 100 days the source data read ended after 10 days so you might only want to
///     update the DataLoadProgress date by 10
///     days on teh assumption that more data will appear later to fill that gap.
/// </summary>
public class UpdateProgressIfLoadsuccessful : IUpdateLoadProgress
{
    protected readonly ScheduledDataLoadJob Job;


    public DateTime DateToSetProgressTo;

    public UpdateProgressIfLoadsuccessful(ScheduledDataLoadJob job)
    {
        Job = job;

        if (Job.DatesToRetrieve == null || !Job.DatesToRetrieve.Any())
            throw new DataLoadProgressUpdateException(
                "Job does not have any DatesToRetrieve! collection was null or empty");

        DateToSetProgressTo = Job.DatesToRetrieve.Max();
    }

    public virtual void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        if (exitCode != ExitCodeType.Success)
            return;

        var progress = Job.LoadProgress;

        if (progress.DataLoadProgress > DateToSetProgressTo)
            throw new DataLoadProgressUpdateException(
                $"Cannot set DataLoadProgress to {DateToSetProgressTo} because it is less than the currently recorded progress:{progress.DataLoadProgress}");

        postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Updating DataLoadProgress of '{progress}' to {DateToSetProgressTo}"));
        progress.DataLoadProgress = DateToSetProgressTo;

        progress.SaveToDatabase();
    }
}