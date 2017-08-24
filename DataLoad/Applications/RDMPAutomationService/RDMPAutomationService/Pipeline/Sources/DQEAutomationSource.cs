using System;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using RDMPAutomationService.Interfaces;
using RDMPAutomationService.Logic.DQE;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.Pipeline.Sources
{
    public class DQEAutomationSource : IAutomationSource
    {
        public DQEAutomationSource()
        {
            
        }

        private AutomationServiceSlot _slottedService;
        private DQERunFinder _finder;

        public void PreInitialize(AutomationServiceSlot value, IDataLoadEventListener listener)
        {
            _slottedService = value;
            _finder = new DQERunFinder((CatalogueRepository) _slottedService.Repository,_slottedService.DQESelectionStrategy,_slottedService.DQEDaysBetweenEvaluations);
        }

        public OnGoingAutomationTask GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (_slottedService.IsAcceptingNewJobs(AutomationJobType.DQE))
            {
                var catalogueWeCanRun = _finder.SuggestRun();

                //if a run was suggested, set it up
                if (catalogueWeCanRun != null)
                    return new AutomatedDQERun(_slottedService, catalogueWeCanRun).GetTask();
            }

            return null;
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
