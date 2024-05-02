// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Pipeline;

/// <summary>
///     Strategy for executing several IDataFlowPipelineEngines one after the other in serial.  This will fully exhaust
///     each IDataFlowPipelineEngine one
///     after the other.
/// </summary>
public class SerialPipelineExecution : IMultiPipelineEngineExecutionStrategy
{
    public void Execute(IEnumerable<IDataFlowPipelineEngine> engines, GracefulCancellationToken cancellationToken,
        IDataLoadEventListener listener)
    {
        // Execute each pipeline to completion before starting the next
        foreach (var engine in engines)
        {
            engine.ExecutePipeline(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                break;
        }
    }
}