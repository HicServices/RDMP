// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;

namespace Rdmp.Core.DataLoad.Engine.DataProvider.FromCache;

/// <summary>
///     Simple implementation of abstract CachedFileRetriever which unzips/copies data out of the cache into the ForLoading
///     directory according to
///     the current IDataLoadJob coverage dates (workload).
/// </summary>
public class BasicCacheDataProvider : CachedFileRetriever
{
    public override void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
    }

    public override ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        var scheduledJob = ConvertToScheduledJob(job);

        var workload = GetDataLoadWorkload(scheduledJob);
        ExtractJobs(scheduledJob);

        job.PushForDisposal(new DeleteCachedFilesOperation(scheduledJob, workload));
        return ExitCodeType.Success;
    }
}