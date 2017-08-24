namespace ReusableLibraryCode.Progress
{
    public class ForkDataLoadEventListener:IDataLoadEventListener
    {
        private IDataLoadEventListener[] _listeners;

        public ForkDataLoadEventListener(params IDataLoadEventListener[] listeners)
        {
            _listeners = listeners;
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            foreach (IDataLoadEventListener listener in _listeners)
                listener.OnNotify(sender, e);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            foreach (IDataLoadEventListener listener in _listeners)
                listener.OnProgress(sender, e);
        }
    }
}