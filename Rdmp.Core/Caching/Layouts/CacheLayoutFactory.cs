// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Caching.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DataProvider.FromCache;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling.Exceptions;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Layouts;

/// <summary>
///     Creates <see cref="ICacheLayout" /> instances based on the <see cref="ICachedDataProvider" />s declared in the load
///     <see cref="ILoadMetadata" />.  There
///     can be multiple <see cref="ILoadProgress" /> in a load (e.g. Tayside / Fife) so you will also need to provide which
///     <see cref="ILoadProgress" /> you are
///     trying to execute.
/// </summary>
public class CacheLayoutFactory
{
    public static ICacheLayout CreateCacheLayout(ILoadProgress loadProgress, ILoadMetadata metadata)
    {
        AssertThatThereIsACacheDataProvider(metadata, metadata.ProcessTasks.Where(p => !p.IsDisabled));

        var cp = loadProgress.CacheProgress;

        var factory = new CachingPipelineUseCase(cp);
        var destination = factory.CreateDestinationOnly(ThrowImmediatelyDataLoadEventListener.Quiet);

        return destination.CreateCacheLayout();
    }

    private static void AssertThatThereIsACacheDataProvider(ILoadMetadata metadata,
        IEnumerable<IProcessTask> processTasks)
    {
        const string whatWeExpected =
            @"(we expected one that was a MEF class implementing ICachedDataProvider since you are trying to execute a cache based data load)";

        var incompatibleProviders = new List<ProcessTask>();
        var compatibleProviders = new List<ProcessTask>();


        foreach (ProcessTask task in processTasks)
        {
            //it's not a DataProvider
            if (!task.ProcessTaskType.Equals(ProcessTaskType.DataProvider))
                continue;


            var type = MEF.GetType(task.Path);

            if (typeof(ICachedDataProvider).IsAssignableFrom(type))
                compatibleProviders.Add(task);
            else
                incompatibleProviders.Add(task);
        }

        if (!incompatibleProviders.Any() && !compatibleProviders.Any())
            throw new CacheDataProviderFindingException(
                $"LoadMetadata {metadata} does not have ANY process tasks of type ProcessTaskType.DataProvider {whatWeExpected}");

        if (!compatibleProviders.Any())
            throw new CacheDataProviderFindingException(
                $"LoadMetadata {metadata} has some DataProviders tasks but none of them wrap classes that implement ICachedDataProvider {whatWeExpected} FYI the data providers in your load wrap the following classes:{string.Join(",", incompatibleProviders.Select(t => t.Path))}");

        if (compatibleProviders.Count > 1)
            throw new CacheDataProviderFindingException(
                $"LoadMetadata {metadata} has multiple cache DataProviders tasks ({string.Join(",", compatibleProviders.Select(p => p.ToString()))}), you are only allowed 1");
    }
}