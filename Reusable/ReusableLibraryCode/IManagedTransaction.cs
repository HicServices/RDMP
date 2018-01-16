using System.Data.Common;
using System.Threading;

namespace ReusableLibraryCode
{
    /// <summary>
    /// Wrapper for DbTransaction that associates it with a specific DbConnection and currently executing Thread.  Helps simplify calls to information 
    /// methods such as DiscoveredTable.GetRowCount etc during the middle of an ongoing database transaction in a Thread/Connection safe way.
    /// </summary>
    public interface IManagedTransaction
    {
        Thread OriginThread { get;}

        DbConnection Connection { get; set; }
        DbTransaction Transaction { get; set; }

        void AbandonAndCloseConnection();
        void CommitAndCloseConnection();
    }
}