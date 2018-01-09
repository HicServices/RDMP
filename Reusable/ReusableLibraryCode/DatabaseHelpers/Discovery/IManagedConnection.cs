using System;
using System.Data.Common;
using System.Threading;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Wrapper for DbConnection that might have an ongoing IManagedTransaction running on it.  Transparently handles opening/reusing an current DbConnection when
    /// using the same IManagedConnection instance in the same Thread (allows you to specify using at multiple stack depths without prematurely closing the 
    /// connection).
    /// </summary>
    public interface IManagedConnection : IDisposable
    {
        DbConnection Connection { get; }
        DbTransaction Transaction { get; }
        IManagedTransaction ManagedTransaction { get; }
        Thread OriginThread { get;}
    }
}