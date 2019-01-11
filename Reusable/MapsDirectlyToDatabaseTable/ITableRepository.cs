using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Security.Permissions;
using FAnsi.Connections;
using FAnsi.Discovery;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// IRepository that uses a database to store IMapsDirectlyToDatabaseTable objects.  Realistically all IRepository are going to be these since the idea
    /// of building a non database IRepository would be virtually impossible.
    /// </summary>
    public interface ITableRepository : IRepository
    {
        IObscureDependencyFinder ObscureDependencyFinder { get; set; }
        string ConnectionString { get; }
        DbConnectionStringBuilder ConnectionStringBuilder { get; }
        DiscoveredServer DiscoveredServer { get; }
        void PopulateUpdateCommandValuesWithCurrentState(DbCommand cmd, IMapsDirectlyToDatabaseTable oTableWrapperObject);
        Dictionary<string, int> GetObjectCountByVersion(Type type);

        IManagedConnection GetConnection();
        IManagedConnection BeginNewTransactedConnection();
        void EndTransactedConnection(bool commit);
        void ClearUpdateCommandCache();
        int? ObjectToNullableInt(object o);
        DateTime? ObjectToNullableDateTime(object o);
        void TestConnection();
        bool SupportsObjectType(Type type);

    }

}