using System;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using RDMPAutomationService.Interfaces;
using RDMPAutomationService.Logic.DQE;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Pipeline.Sources
{
    /// <summary>
    /// Identifies when a Catalogue Data Quality Engine Evaluation can be executed (according to DQERunFinder), packages it as an AutomatedDQERun and releases it
    /// into the automation pipeline for scheduling/execution.
    /// </summary>
    public class DQEAutomationSource : IAutomationSource
    {
        public DQEAutomationSource()
        {
            
        }

        private AutomationServiceSlot _slottedService;
        private DQERunFinder _finder;
        private IDataLoadEventListener _listener;

        public void PreInitialize(AutomationServiceSlot value, IDataLoadEventListener listener)
        {
            _slottedService = value;
            _listener = listener;
            _finder = new DQERunFinder((CatalogueRepository) _slottedService.Repository, _slottedService.DQESelectionStrategy, _slottedService.DQEDaysBetweenEvaluations, _listener);
        }

        public OnGoingAutomationTask GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (_slottedService.IsAcceptingNewJobs(AutomationJobType.DQE))
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Slot is accepting new DQE jobs... let's find one."));
                var catalogueWeCanRun = _finder.SuggestRun();

                //if a run was suggested, set it up
                if (catalogueWeCanRun != null)
                    return new AutomatedDQERun(_slottedService, catalogueWeCanRun).GetTask();
                else
                    return null;
            }
            else
            {
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Slot is not accepting any new DQE jobs..."));
                return null;
            }
        }


        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            
        }

        public void Abort(IDataLoadEventListener listener)
        {
            
        }

        public OnGoingAutomationTask TryGetPreview()
        {
            throw new NotSupportedException();
        }

    }
}
