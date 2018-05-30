using ReusableLibraryCode.Progress;

namespace HIC.Logging.Listeners
{
    /// <summary>
    /// Acts as wrapper for another <see cref="IDataLoadEventListener"/> but changes all messages that flow through to appear to come from the same
    /// sender (string).  You can use this to help with distinguishing message dispatchers (senders) between discrete tasks / threads.
    /// </summary>
    public class OverrideSenderIDataLoadEventListener : IDataLoadEventListener
    {
        private readonly string _overridingSender;
        private IDataLoadEventListener _child;

        public OverrideSenderIDataLoadEventListener(string overridingSender, IDataLoadEventListener childToPassTo)
        {
            _overridingSender = overridingSender;
            _child = childToPassTo;
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            _child.OnNotify(_overridingSender,e);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            _child.OnProgress(_overridingSender,e);
        }
    }
}