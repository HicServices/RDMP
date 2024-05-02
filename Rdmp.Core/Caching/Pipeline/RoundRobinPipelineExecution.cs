// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Pipeline;

/// <summary>
///     Strategy for executing several IDataFlowPipelineEngines one chunk at a time in serial.  The difference between this
///     and SerialPipelineExecution
///     is that only one ChunkPeriod is read at once from each engine.  If there is more data available to fetch in any
///     engine they are all run again until
///     all caches are up to date or the cancellation token is set
/// </summary>
public class RoundRobinPipelineExecution : IMultiPipelineEngineExecutionStrategy
{
    public void Execute(IEnumerable<IDataFlowPipelineEngine> engines, GracefulCancellationToken cancellationToken,
        IDataLoadEventListener listener)
    {
        // Execute one pass through a pipeline before moving to the next. Continue until completion.
        var engineList = engines.ToList();
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Round robin executor has {engineList.Count} pipeline(s) to run."));

        var allComplete = false;
        while (!allComplete)
        {
            allComplete = true;
            foreach (var engine in engineList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // assigned to temporary variable here to make the logic a bit more explicit
                var hasMoreData = engine.ExecuteSinglePass(cancellationToken);
                allComplete = !hasMoreData && allComplete;
            }
        }

        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information,
                "Round robin executor is finished, all pipelines have run to completion."));
    }
}