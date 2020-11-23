using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Events;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rdmp.Core.CommandLine.Runners
{
    public class PipelineRunner : IPipelineRunner
    {
        public IPipelineUseCase UseCase { get; }
        public IPipeline Pipeline { get; }

        public HashSet<IDataLoadEventListener> AdditionalListeners = new HashSet<IDataLoadEventListener>();

        public event PipelineEngineEventHandler PipelineExecutionFinishedsuccessfully;

        public PipelineRunner(IPipelineUseCase useCase, IPipeline pipeline)
        {
            UseCase = useCase;
            Pipeline = pipeline;
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            // if we have no listener use a throw immediately one (generate exceptions if it went badly)
            if(listener == null)
                listener = new ThrowImmediatelyDataLoadEventListener();
                        
            // whatever happens we want a listener to record the worst result for the return code (even if theres ignore all errors listeners being used)
            var toMemory = new ToMemoryDataLoadEventListener(false);

            // User might have some additional listeners registered
            listener = new ForkDataLoadEventListener(AdditionalListeners.Union(new []{ toMemory, listener}).ToArray());
           
            // build the engine and run it
            var engine = UseCase.GetEngine(Pipeline,listener);            
            engine.ExecutePipeline(token ?? new GracefulCancellationToken());
            
            // return code of -1 if it went badly otherwise 0
            var exitCode = toMemory.GetWorst() >= ProgressEventType.Error ? -1:0;

            if(exitCode ==0)
                PipelineExecutionFinishedsuccessfully(this,new PipelineEngineEventArgs(engine));

            return exitCode;
        }

        public void SetAdditionalProgressListener(IDataLoadEventListener toAdd)
        {
            AdditionalListeners.Add(toAdd);
        }
    }
}
