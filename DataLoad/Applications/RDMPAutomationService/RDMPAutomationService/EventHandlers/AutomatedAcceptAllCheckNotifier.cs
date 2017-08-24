using CatalogueLibrary.Data.Automation;
using ReusableLibraryCode.Checks;

namespace RDMPAutomationService.EventHandlers
{
    internal class AutomatedAcceptAllCheckNotifier : AcceptAllCheckNotifier
    {
        private OnGoingAutomationTask _task;

        public AutomatedAcceptAllCheckNotifier(OnGoingAutomationTask task)
        {
            _task = task;
        }

        override public bool OnCheckPerformed(CheckEventArgs args)
        {
            _task.Job.TickLifeline();

            //elevate it to running
            if (_task.Job.LastKnownStatus == AutomationJobStatus.NotYetStarted)
                _task.Job.SetLastKnownStatus(AutomationJobStatus.Running);
            
            return base.OnCheckPerformed(args);
        }
    }
}