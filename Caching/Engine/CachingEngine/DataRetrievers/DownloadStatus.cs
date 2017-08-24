namespace CachingEngine.DataRetrievers
{
    public enum DownloadStatus
    {
        InProgress,
        Aborted,
        Completed, // i.e. lead time has been reached
        NotInPermittedCachingPeriod,
        Failed
    }
}