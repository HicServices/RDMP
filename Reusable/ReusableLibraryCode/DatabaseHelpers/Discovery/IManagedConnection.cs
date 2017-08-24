using System;
using System.Data.Common;
using System.Threading;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    public interface IManagedConnection : IDisposable
    {
        DbConnection Connection { get; }
        DbTransaction Transaction { get; }
        IManagedTransaction ManagedTransaction { get; }
        Thread OriginThread { get;}
    }
}