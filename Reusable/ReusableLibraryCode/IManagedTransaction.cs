using System.Data.Common;
using System.Threading;

namespace ReusableLibraryCode
{
    public interface IManagedTransaction
    {
        Thread OriginThread { get;}

        DbConnection Connection { get; set; }
        DbTransaction Transaction { get; set; }

        void AbandonAndCloseConnection();
        void CommitAndCloseConnection();
    }
}