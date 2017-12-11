namespace CachingEngine.DataRetrievers
{
    /// <summary>
    /// The results of an attempt to start a Caching action.
    /// </summary>
    public enum RetrievalResult
    {
        Complete,
        Aborted,
        Stopped,
        NotPermitted,
        Error
    }
}