using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.EventHandlers
{
    internal class AutomatedThrowImmediatelyDataLoadEventsListener : IDataLoadEventListener
    {
        private readonly ICatalogueRepository _repository;
        private OnGoingAutomationTask _task;

        protected AutomatedThrowImmediatelyDataLoadEventsListener(ICatalogueRepository repository)
        {
            _repository = repository;
        }

        public AutomatedThrowImmediatelyDataLoadEventsListener(OnGoingAutomationTask task) 
            : this((ICatalogueRepository) task.Repository)
        {
            _task = task;
        }

        public AutomatedThrowImmediatelyDataLoadEventsListener(AutomationServiceSlot automationServiceSlot) 
            : this((ICatalogueRepository) automationServiceSlot.Repository)
        { }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            if(e.ProgressEventType == ProgressEventType.Error)
                if (e.Exception != null)
                {
                    new AutomationServiceException(_repository, e.Exception);
                    throw new Exception(e.Message,e.Exception);
                }
                else
                {
                    var newEx = new Exception(e.Message);
                    new AutomationServiceException(_repository, newEx);
                    throw newEx;
                }

            if (_task != null)
                _task.Job.TickLifeline();
        }

        private DateTime lastTicked = DateTime.MinValue;

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            if(_task != null && DateTime.Now.Subtract(lastTicked).TotalSeconds > 3)
            {
                _task.Job.TickLifeline();
                lastTicked = DateTime.Now;
            }
        }
    }
}