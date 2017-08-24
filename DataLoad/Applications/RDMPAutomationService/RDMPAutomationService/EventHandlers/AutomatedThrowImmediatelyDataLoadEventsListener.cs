using System;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.EventHandlers
{
    internal class AutomatedThrowImmediatelyDataLoadEventsListener : IDataLoadEventListener
    {
        private OnGoingAutomationTask _task;

        public AutomatedThrowImmediatelyDataLoadEventsListener(OnGoingAutomationTask task)
        {
            _task = task;
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            if(e.ProgressEventType == ProgressEventType.Error)
                if (e.Exception != null)
                    throw new Exception(e.Message,e.Exception);
                else
                    throw new Exception(e.Message);

            _task.Job.TickLifeline();
        }

        private DateTime lastTicked = DateTime.MinValue;

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            if(DateTime.Now.Subtract(lastTicked).TotalSeconds > 3)
            {
                _task.Job.TickLifeline();
                lastTicked = DateTime.Now;
            }
        }
    }
}