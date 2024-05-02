// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace Rdmp.Core.DataLoad.Engine.Job.Scheduling;

/// <summary>
///     Determines how DateTimes for a Scheduled load are determined.  Scheduled loads are those where the LoadMetadata has
///     one or more LoadProgresses.  We
///     could simply add the next 'batchSize' days to the head date of the LoadProgress.  Alternatively we could inspect
///     the cache to make sure that there
///     are files for those dates and skip any holes.
/// </summary>
public interface IJobDateGenerationStrategy
{
    List<DateTime> GetDates(int batchSize, bool allowLoadingFutureDates);
    int GetTotalNumberOfJobs(int batchSize, bool allowLoadingFutureDates);
}