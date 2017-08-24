
using System;
using System.Collections.Generic;
using ReusableLibraryCode.Checks;

namespace ReusableLibraryCode.Progress
{
    public class FromCheckNotifierToDataLoadEventListener : IDataLoadEventListener
    {
        private readonly ICheckNotifier _checker;

        public FromCheckNotifierToDataLoadEventListener(ICheckNotifier checker)
        {
            _checker = checker;
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            CheckResult result;

            switch (e.ProgressEventType)
            {
                case ProgressEventType.Information:
                    result = CheckResult.Success;
                    break;
                case ProgressEventType.Warning:
                    result = CheckResult.Warning;
                    break;
                case ProgressEventType.Error:
                    result = CheckResult.Fail;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _checker.OnCheckPerformed(new CheckEventArgs(e.Message, result, e.Exception));
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