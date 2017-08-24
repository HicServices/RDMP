namespace ReusableLibraryCode.Progress
{
    public interface IDataLoadEventListener 
    {
        void OnNotify(object sender, NotifyEventArgs e);
        void OnProgress(object sender, ProgressEventArgs e);
    }
}