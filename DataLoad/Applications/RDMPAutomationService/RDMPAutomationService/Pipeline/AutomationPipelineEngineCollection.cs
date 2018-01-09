using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using RDMPAutomationService.EventHandlers;
using RDMPAutomationService.Pipeline.Sources;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Pipeline
{
    /// <summary>
    /// Hosts and runs all the AutomationPipelines running in the currently running Automation Service executable.  Each pipeline has an IAutomationSource which
    /// is polled for new jobs, all jobs are passed to the same instance of IAutomationDestination which manages async execution of them.
    /// </summary>
    public class AutomationPipelineEngineCollection
    {
        private readonly AutomationServiceSlot _slot;
        private IDataLoadEventListener _listener;

        public DataFlowPipelineEngine<OnGoingAutomationTask> DLEPipe { get; private set; }
        public DataFlowPipelineEngine<OnGoingAutomationTask> DQEPipe { get; private set; }
        public DataFlowPipelineEngine<OnGoingAutomationTask> CachePipe { get; private set; }

        public List<DataFlowPipelineEngine<OnGoingAutomationTask>> UserSpecificPipelines { get; private set; }

        public AutomationPipelineEngineCollection(IRDMPPlatformRepositoryServiceLocator repositoryLocator,AutomationServiceSlot slot, AutomationDestination fixedDestination)
        {
            _slot = slot;

            _listener = new FromCheckNotifierToDataLoadEventListener(new ThrowImmediatelyCheckNotifier(){WriteToConsole = false});
            
            DLEPipe = new DataFlowPipelineEngine<OnGoingAutomationTask>(AutomationPipelineContext.Context, new DLEAutomationSource(), fixedDestination,_listener);
            DLEPipe.Initialize(slot, repositoryLocator);

            DQEPipe = new DataFlowPipelineEngine<OnGoingAutomationTask>(AutomationPipelineContext.Context, new DQEAutomationSource(), fixedDestination, _listener);
            DQEPipe.Initialize(slot, repositoryLocator);

            CachePipe = new DataFlowPipelineEngine<OnGoingAutomationTask>(AutomationPipelineContext.Context, new CacheAutomationSource(), fixedDestination, _listener);
            CachePipe.Initialize(slot,repositoryLocator);

            UserSpecificPipelines = new List<DataFlowPipelineEngine<OnGoingAutomationTask>>();
            foreach (AutomateablePipeline automateablePipeline in slot.AutomateablePipelines)
            {
                var factory = new DataFlowPipelineEngineFactory<OnGoingAutomationTask>(((CatalogueRepository)slot.Repository).MEF,AutomationPipelineContext.Context);
                factory.ExplicitDestination = fixedDestination;
                var pipe = factory.Create(automateablePipeline.Pipeline, _listener);
                pipe.Initialize(slot, repositoryLocator);

                UserSpecificPipelines.Add((DataFlowPipelineEngine<OnGoingAutomationTask>) pipe);
            }
        }

        /// <summary>
        /// Runs all automation pipelines once.  This will identify all new automation jobs and dispatch them to the IAutomationDestination but will not wait
        /// for the jobs to complete before returning (IAutomationDestination is multi threaded).
        /// </summary>
        /// <param name="minimumLengthOfTimeToWaitWhileDoingThis"></param>
        public void ExecuteAll(int minimumLengthOfTimeToWaitWhileDoingThis)
        {
            var delay = Task.Delay(minimumLengthOfTimeToWaitWhileDoingThis);//yes we really do mean MINIMUM - this method should always take this long or longer
            Task task = new Task(() =>
            {
                //we are not trying to stop so look for new tasks
                _slot.RevertToDatabaseState();

                DLEPipe.ExecutePipeline(new GracefulCancellationToken());
                DQEPipe.ExecutePipeline(new GracefulCancellationToken());
                CachePipe.ExecutePipeline(new GracefulCancellationToken());

                foreach (var pipeline in UserSpecificPipelines)
                    pipeline.ExecutePipeline(new GracefulCancellationToken());
            });
            
            task.Start();

            var tasks = new[] {delay, task};

            Task.WaitAll(tasks);

        }
    }
}
