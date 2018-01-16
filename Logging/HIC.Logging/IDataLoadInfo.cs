using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace HIC.Logging
{
    /// <summary>
    /// See DataLoadInfo
    /// </summary>
    public interface IDataLoadInfo
    {
        ITableLoadInfo CreateTableLoadInfo(string suggestedRollbackCommand, string destinationTable, DataSource[] sources, int expectedInserts);
        void CloseAndMarkComplete();
        int ID { get; }
        DiscoveredServer DatabaseSettings { get; }
        
        bool IsClosed { get; }
    }
}