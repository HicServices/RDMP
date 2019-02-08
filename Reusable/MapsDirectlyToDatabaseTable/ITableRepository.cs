// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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