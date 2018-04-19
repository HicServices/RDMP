using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.DataFlowPipeline;
using RDMPAutomationService;
using RDMPAutomationService.Interfaces;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationServiceTests.AutomationLoopTests.FictionalAutomationPlugin
{
    public class TestAutomationPluginSource : IPluginAutomationSource
    {
        TestSkullWriter _skullWriter;

        List<DateTime> timesProcessed = new List<DateTime>();
        private AutomationServiceSlot _serviceSlot;
        
        [DemandsInitialization("The number of seconds between outputting images of 2brains the skull, images will be called yy_MM_dd_HHmmss_skull.png must be greater than 0")]
        public int NumberOfSecondsToWaitBetweenCreatingSkullImages { get; set; }

        [DemandsInitialization("The maximum number of skull outputting jobs to allow to flow to the destination before stopping sending new ones (since the pipelines should complete within 1s this should only come up if they are crashing or being cancelled or someone modifies this to generate a task every millisecond or something)")]
        public int MaxNumberOfSimultaneousSkullImageJobs { get; set; }

        public OnGoingAutomationTask GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (NumberOfSecondsToWaitBetweenCreatingSkullImages <= 0)
                throw new NotSupportedException("NumberOfSecondsToWaitBetweenCreatingSkullImages cannot be 0 or less");

            if (MaxNumberOfSimultaneousSkullImageJobs <= 0)
                throw new NotSupportedException("MaxNumberOfSimultaneousSkullImageJobs cannot be 0 or less");

            //if it is over x seconds since we last outputted a skull image
            if (DateTime.Now.Subtract(timesProcessed.Last()).TotalSeconds >= NumberOfSecondsToWaitBetweenCreatingSkullImages)
                //and we haven't got 5 or more skull writing jobs already
                if (_serviceSlot.AutomationJobs.Count(j => j.AutomationJobType == AutomationJobType.UserCustomPipeline && j.Description.Contains("skull")) < MaxNumberOfSimultaneousSkullImageJobs)
                {
                    var dt = DateTime.Now;
                    timesProcessed.Add(dt);
                    return new TestSkullWriter(_serviceSlot,dt ,listener).GetTask();
                }


            //1 minute has not elapsed or there are already 5 ongoing/crashed jobs at the desitination
            return null;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            throw new NotSupportedException();
        }

        public OnGoingAutomationTask TryGetPreview()
        {
            return _skullWriter.GetTask();
        }

        public void PreInitialize(AutomationServiceSlot value, IDataLoadEventListener listener)
        {
            _serviceSlot = value;
            timesProcessed.Add(DateTime.MinValue);
        }
    }
}
