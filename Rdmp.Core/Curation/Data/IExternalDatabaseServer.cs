// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// <para>Records information about a server.  This can be an RDMP platform database e.g. a Logging database or an ANOStore or it could be a generic
/// database you use to hold data (e.g. lookups).  These are usually database servers but don't have to be (e.g. you could create a reference to an FTP server).</para>
/// 
/// <para>IMPORTANT: do not add an ExternalDatabaseServer just because you store data on it, instead you should import pointers to the data you hold as TableInfo
/// objects which themselves store Server/Database which allows for minimal disruption when you decide to move a table to a different server (it also allows
/// for accessing the data under different accounts based on what is being done - loading vs extraction : see DataAccessCredentials_TableInfo).</para>
/// 
/// <para>ExternalDatabaseServer are really only for fixed global entities such as logging/identifier dumps etc.</para>
/// 
/// <para>Servers can but do not have to have usernames/passwords in which case integrated security (windows account) is used when opening connections.  Password
/// is encrypted in the same fashion as in the DataAccessCredentials table.</para>
/// </summary>
public interface IExternalDatabaseServer : IDataAccessPoint, INamed
{
    /// <summary>
    /// Determines whether the given database was created by the specified patcher e.g. <see cref="CataloguePatcher"/>.  If it is then the
    /// schema will be under version control (by the patchers assembly) and <see cref="DatabaseEntity"/> objects may be retrievable from it.
    /// </summary>
    /// <returns></returns>
    bool WasCreatedBy(IPatcher patcher);

    /// <summary>
    /// Provides a live object for interacting directly with the server referenced by this <see cref="IExternalDatabaseServer"/>.  This will work
    /// even if the server is unreachable (See <see cref="IMightNotExist"/>)
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    DiscoveredDatabase Discover(DataAccessContext context);
}