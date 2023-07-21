// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling.Exceptions;

namespace Rdmp.Core.DataLoad.Engine.Job.Scheduling;

/// <summary>
///     Returns dates consecutively with reference to a LoadProgress, starting from the last successful load
///     (DataLoadProgress) or OriginDate if no load has yet
///     been successfully completed
/// </summary>
public class SingleScheduleConsecutiveDateStrategy : IJobDateGenerationStrategy
{
    private DateTime _lastAssignedLoadDate;

    public SingleScheduleConsecutiveDateStrategy(ILoadProgress loadProgress)
    {
        if (loadProgress.DataLoadProgress == null)
        {
            if (loadProgress.OriginDate == null)
                throw new LoadOrCacheProgressUnclearException(
                    "Don't know when to start the data load, both DataLoadProgress and OriginDate are null");

            _lastAssignedLoadDate = loadProgress.OriginDate.Value;
        }
        else
        {
            _lastAssignedLoadDate = loadProgress.DataLoadProgress.Value;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="batchSize"></param>
    /// <param name="allowLoadingFutureDates">
    ///     if true then dates can be loaded that are in the future otherwise the returned
    ///     list of dates will only contain dates in teh past
    /// </param>
    /// <returns></returns>
    public List<DateTime> GetDates(int batchSize, bool allowLoadingFutureDates)
    {
        var dates = new List<DateTime>();

        for (var i = 0; i < batchSize; i++)
        {
            _lastAssignedLoadDate = _lastAssignedLoadDate.Add(new TimeSpan(1, 0, 0, 0));

            //if we are about to order the loading of today or any later day
            if (_lastAssignedLoadDate.Date >= DateTime.Now.Date)
                if (!allowLoadingFutureDates) //if user does not want to load future dates
                    return dates; //he is done

            //it is not a future date or user wants to allow loading of future dates
            dates.Add(_lastAssignedLoadDate);
        }

        return dates;
    }

    public int GetTotalNumberOfJobs(int batchSize, bool allowLoadingFutureDates)
    {
        if (allowLoadingFutureDates)
            throw new NotImplementedException();

        var numberOfDays = DateTime.Now.Date.Subtract(_lastAssignedLoadDate.Date).Days;
        var totalNumberOfJobs = numberOfDays / batchSize;
        if (numberOfDays % batchSize > 0) ++totalNumberOfJobs;
        return totalNumberOfJobs;
    }
}