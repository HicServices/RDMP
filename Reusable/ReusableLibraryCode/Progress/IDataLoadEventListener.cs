namespace ReusableLibraryCode.Progress
{
    /// <summary>
    /// Event handler for progress updates and one off notifications.  This can include errors (ProgressEventType.Error), warnings and information.  Progress
    /// events are incremental messages in which a numerical count increases (possibly to a known maximum) e.g. 'loaded 300 records out of 2000'.
    /// 
    /// It is valid to respond to OnNotify with ProgressEventType.Error (or even Warning) by throwing an Exception.
    /// </summary>
    public interface IDataLoadEventListener
    {
        void OnNotify(object sender, NotifyEventArgs e);
        void OnProgress(object sender, ProgressEventArgs e);
    }
}