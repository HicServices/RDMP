namespace ReusableLibraryCode.Progress
{
    /// <summary>
    /// IDataLoadEventListener which routes events to multiple other IDataLoadEventListeners.  Listeners are called in the order they appear in the array
    /// therefore if you get an error (e.g. OnNotify event with ProgressEventType.Error) and the first listener decides to respond by raising it as an 
    /// Exception then the second listener will not get called (since at this point you will have entered Exception handling).
    /// </summary>
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