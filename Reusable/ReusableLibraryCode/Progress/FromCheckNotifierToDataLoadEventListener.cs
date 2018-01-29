
using System;
using System.Collections.Generic;
using ReusableLibraryCode.Checks;

namespace ReusableLibraryCode.Progress
{
    /// <summary>
    /// Allows you to route IDataLoadEventListener messages to an ICheckNotifier.  All OnNotify events will be translated into a similar OnCheckPerformed event.
    /// For OnProgress events there is a loss of granularity since ICheckNotifiers are not designed to record incremental progress messages.  Therefore the 
    /// ICheckNotifier will only be given one OnCheckPerformed event for each novel OnProgress Tasks seen with the message 'Started progress on x'.
    /// </summary>
    public class FromCheckNotifierToDataLoadEventListener : IDataLoadEventListener
    {
        private readonly ICheckNotifier _checker;

        public FromCheckNotifierToDataLoadEventListener(ICheckNotifier checker)
        {
            _checker = checker;
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            _checker.OnCheckPerformed(e.ToCheckEventArgs());
        }

        private HashSet<string> _progressMessagesReceived = new HashSet<string>();

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            //only tell the user once about each progress message because these can come 100 a second and theres no finished flag so all we can do is tell them it is happening
            if (!_progressMessagesReceived.Contains(e.TaskDescription))
            {
                _progressMessagesReceived.Add(e.TaskDescription);
                _checker.OnCheckPerformed(new CheckEventArgs("Started progress on " + e.TaskDescription,CheckResult.Success));
            }
        }
    }
}