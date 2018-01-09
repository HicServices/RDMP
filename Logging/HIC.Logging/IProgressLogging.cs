namespace HIC.Logging
{
    /// <summary>
    /// See ProgressLogging
    /// </summary>
    public interface IProgressLogging
    {
        void LogProgress(IDataLoadInfo dataLoadInfo, ProgressLogging.ProgressEventType pevent, string Source, string Description);
    }
}