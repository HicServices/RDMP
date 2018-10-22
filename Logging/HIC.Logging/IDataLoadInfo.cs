using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace HIC.Logging
{
    /// <summary>
    /// See DataLoadInfo
    /// </summary>
    public interface IDataLoadInfo
    {
        ITableLoadInfo CreateTableLoadInfo(string suggestedRollbackCommand, string destinationTable, DataSource[] sources, int expectedInserts);
        void LogFatalError(string errorSource, string errorDescription);
        void LogProgress(DataLoadInfo.ProgressEventType pevent, string Source, string Description);

        void CloseAndMarkComplete();
        int ID { get; }
        DiscoveredServer DatabaseSettings { get; }
        
        bool IsClosed { get; }
    }
}