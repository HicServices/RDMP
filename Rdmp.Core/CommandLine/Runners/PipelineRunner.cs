// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Events;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandLine.Runners;

/// <summary>
/// <see cref="IRunner"/> that runs an <see cref="IPipeline"/> for a given <see cref="IPipelineUseCase"/> and reports events to a flexible set of listeners
/// </summary>
public class PipelineRunner : IPipelineRunner
{
    public IPipelineUseCase UseCase { get; }
    public IPipeline Pipeline { get; }

    public HashSet<IDataLoadEventListener> AdditionalListeners = new();

    public event PipelineEngineEventHandler PipelineExecutionFinishedsuccessfully;

    public PipelineRunner(IPipelineUseCase useCase, IPipeline pipeline)
    {
        UseCase = useCase;
        Pipeline = pipeline;
    }

    public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,
        ICheckNotifier checkNotifier, GracefulCancellationToken token, int? dataLoadId = null)
    {
        // if we have no listener use a throw immediately one (generate exceptions if it went badly)
        listener ??= ThrowImmediatelyDataLoadEventListener.Quiet;

        // whatever happens we want a listener to record the worst result for the return code (even if theres ignore all errors listeners being used)
        var toMemory = new ToMemoryDataLoadEventListener(false);

        // User might have some additional listeners registered
        listener = new ForkDataLoadEventListener(AdditionalListeners.Union(new[] { toMemory, listener }).ToArray());

        // build the engine and run it
        var engine = UseCase.GetEngine(Pipeline, listener);
        engine.ExecutePipeline(token ?? new GracefulCancellationToken());

        // return code of -1 if it went badly otherwise 0
        var exitCode = toMemory.GetWorst() >= ProgressEventType.Error ? -1 : 0;

        if (exitCode == 0)
            PipelineExecutionFinishedsuccessfully?.Invoke(this, new PipelineEngineEventArgs(engine));

        return exitCode;
    }

    public void SetAdditionalProgressListener(IDataLoadEventListener toAdd)
    {
        AdditionalListeners.Add(toAdd);
    }
}