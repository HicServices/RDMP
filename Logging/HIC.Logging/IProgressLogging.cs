namespace HIC.Logging
{
    public interface IProgressLogging
    {
        void LogProgress(IDataLoadInfo dataLoadInfo, ProgressLogging.ProgressEventType pevent, string Source, string Description);
    }
}