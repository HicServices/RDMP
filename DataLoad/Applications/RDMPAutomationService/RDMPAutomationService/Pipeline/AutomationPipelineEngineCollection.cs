using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using HIC.Logging;
using HIC.Logging.Listeners;
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

        internal Dictionary<DataFlowPipelineEngine<OnGoingAutomationTask>, PipelineRunStatus> PipeStatuses { get; set; }

        public AutomationPipelineEngineCollection(IRDMPPlatformRepositoryServiceLocator repositoryLocator, AutomationServiceSlot slot, AutomationDestination fixedDestination)
        {
            _slot = slot;
            
            _listener = new ForkDataLoadEventListener(
                new ToFileDataLoadEventListener(this),
                new ThrowImmediatelyDataLoadEventListener()
            );
              
            PipeStatuses = new Dictionary<DataFlowPipelineEngine<OnGoingAutomationTask>, PipelineRunStatus>();

            DLEPipe = new DataFlowPipelineEngine<OnGoingAutomationTask>(AutomationPipelineContext.Context, new DLEAutomationSource(), fixedDestination, _listener);
            DLEPipe.Initialize(slot, repositoryLocator);
            PipeStatuses.Add(DLEPipe, new PipelineRunStatus());

            DQEPipe = new DataFlowPipelineEngine<OnGoingAutomationTask>(AutomationPipelineContext.Context, new DQEAutomationSource(), fixedDestination, _listener);
            DQEPipe.Initialize(slot, repositoryLocator);
            PipeStatuses.Add(DQEPipe, new PipelineRunStatus());

            CachePipe = new DataFlowPipelineEngine<OnGoingAutomationTask>(AutomationPipelineContext.Context, new CacheAutomationSource(), fixedDestination, _listener);
            CachePipe.Initialize(slot, repositoryLocator);
            PipeStatuses.Add(CachePipe, new PipelineRunStatus());

            UserSpecificPipelines = new List<DataFlowPipelineEngine<OnGoingAutomationTask>>();
            foreach (AutomateablePipeline automateablePipeline in slot.AutomateablePipelines)
            {
                var factory = new DataFlowPipelineEngineFactory<OnGoingAutomationTask>(((CatalogueRepository)slot.Repository).MEF,AutomationPipelineContext.Context);
                factory.ExplicitDestination = fixedDestination;
                var pipe = (DataFlowPipelineEngine<OnGoingAutomationTask>)factory.Create(automateablePipeline.Pipeline, _listener);
                pipe.Initialize(slot, repositoryLocator);

                UserSpecificPipelines.Add(pipe);
                PipeStatuses.Add(pipe, new PipelineRunStatus());
            }
        }

        /// <summary>
        /// Runs all automation pipelines once.  This will identify all new automation jobs and dispatch them to the IAutomationDestination but will not wait
        /// for the jobs to complete before returning (IAutomationDestination is multi threaded).
        /// </summary>
        /// <param name="minimumLengthOfTimeToWaitWhileDoingThis"></param>
        public void ExecuteAll(int minimumLengthOfTimeToWaitWhileDoingThis)
        {
            var tasks = new List<Task>();
            if (ShouldRun(DLEPipe))
                tasks.Add(GetPipelineTask(DLEPipe));
            if (ShouldRun(DQEPipe))
                tasks.Add(GetPipelineTask(DQEPipe));
            if (ShouldRun(CachePipe))
                tasks.Add(GetPipelineTask(CachePipe));
            
            foreach (var pipeline in UserSpecificPipelines)
            {
                if (ShouldRun(pipeline))
                    tasks.Add(GetPipelineTask(pipeline));
            }

            _slot.RevertToDatabaseState();
            foreach (var task in tasks)
            {
                task.Start();
            } 
            
            var delay = Task.Delay(minimumLengthOfTimeToWaitWhileDoingThis);//yes we really do mean MINIMUM - this method should always take this long or longer

            tasks.Add(delay);

            Task.WaitAll(tasks.ToArray());
        }

        private bool ShouldRun(DataFlowPipelineEngine<OnGoingAutomationTask> pipeline)
        {
            if (!PipeStatuses.ContainsKey(pipeline))
                return false;

            var status = PipeStatuses[pipeline];

            // if the last error is more than a day ago
            if (status.LastError < DateTime.UtcNow.AddDays(-1))
            {
                status.NumErrors = 0;
                return true;
            }

            if (status.NumErrors < 5)
                return true;

            return false;
        }

        private Task GetPipelineTask(DataFlowPipelineEngine<OnGoingAutomationTask> pipeline)
        {
            return new Task(() =>
            {
                try
                {
                    pipeline.ExecutePipeline(new GracefulCancellationToken());
                }
                catch (Exception ex)
                {
                    PipeStatuses[pipeline].NumErrors++;
                    PipeStatuses[pipeline].LastError = DateTime.UtcNow;
                    throw ex;
                }
            });
        }
    }
}
