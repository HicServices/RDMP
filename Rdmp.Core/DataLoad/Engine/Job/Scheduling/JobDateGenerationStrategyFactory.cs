// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.LoadProcess.Scheduling.Strategy;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Job.Scheduling;

/// <summary>
/// Decides the date generation strategy (e.g. pick next X days from the head of the LoadProgress or base dates loaded on next files available in cache)
/// </summary>
public class JobDateGenerationStrategyFactory
{
    private readonly Type _typeToCreate;

    /// <summary>
    /// Always respects the LoadProgress dates and crashes if there aren't any load progresses associated with the given load metadata
    /// Uses SingleScheduleCacheDateTrackingStrategy if there is a cache associated with any of the load progresses otherwise uses SingleScheduleConsecutiveDateStrategy (meaning for example each day for the next 5 days)
    /// </summary>
    /// <param name="strategy"></param>
    public JobDateGenerationStrategyFactory(ILoadProgressSelectionStrategy strategy)

    {
        _typeToCreate =
            strategy.GetAllLoadProgresses()
                .Any(p => p.CacheProgress !=
                          null) //if any of the strategies you plan to use (without locking btw) have a cache progress
                ? typeof(SingleScheduleCacheDateTrackingStrategy) //then we should use a cache progress based strategy
                : typeof(SingleScheduleConsecutiveDateStrategy); //otherwise we should probably use consecutive days strategy;
    }

    public IJobDateGenerationStrategy Create(ILoadProgress loadProgress, IDataLoadEventListener listener)
    {
        if (_typeToCreate == typeof(SingleScheduleConsecutiveDateStrategy))
            return new SingleScheduleConsecutiveDateStrategy(loadProgress);

        var loadMetadata = loadProgress.LoadMetadata;

        return _typeToCreate == typeof(SingleScheduleCacheDateTrackingStrategy)
            ? (IJobDateGenerationStrategy)new SingleScheduleCacheDateTrackingStrategy(
                CacheLayoutFactory.CreateCacheLayout(loadProgress, loadMetadata), loadProgress, listener)
            : throw new Exception("Factory has been configured to supply an unknown type");
    }
}