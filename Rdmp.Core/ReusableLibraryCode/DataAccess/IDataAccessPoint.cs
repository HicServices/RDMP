// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using FAnsi.Discovery.QuerySyntax;

namespace Rdmp.Core.ReusableLibraryCode.DataAccess;

/// <summary>
///     Stores the location and credentials to access a database.  There can be multiple credentials available for use with
///     a given IDataAccessPoint depending
///     on the usage context e.g. DataAccessContext.DataLoad might know credentials for a user account with write
///     permission while its Credentials for use
///     under DataAccessContext.DataExport are for a readonly user account.
///     <para>
///         You can translate an IDataAccessPoint into a connection string / DiscoveredServer by using
///         DataAccessPortal.GetInstance().ExpectDatabase(...)
///     </para>
///     <para>
///         IDataAccessCredentials can include Encrypted passwords which the current user may or may not have access to
///         decrypt.  Where no credentials are
///         available it is assumed that the connection should be made using Integrated Security (Windows Security).
///     </para>
/// </summary>
public interface IDataAccessPoint : IHasQuerySyntaxHelper
{
    /// <summary>
    ///     The name of the server e.g. localhost\sqlexpress
    /// </summary>
    string Server { get; set; }

    /// <summary>
    ///     The name of the database to connect to e.g. master, tempdb, MyCoolDb etc
    /// </summary>
    string Database { get; set; }

    /// <summary>
    ///     The DBMS type of the server e.g. Sql Server / MySql / Oracle
    /// </summary>
    DatabaseType DatabaseType { get; set; }

    /// <summary>
    ///     The username/password to use when connecting to the server (otherwise integrated security is used)
    /// </summary>
    /// <param name="context">
    ///     What you intend to do after you have connected (may determine which credentials to use e.g.
    ///     readonly vs readwrite)
    /// </param>
    /// <returns></returns>
    IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context);


    /// <summary>
    ///     Attempts to connect to the server using the provided <paramref name="context" />.  If the object is not properly
    ///     setup for a valid reference e.g.
    ///     <see cref="Server" /> is missing or the referenced database/server could not be connected to then the method
    ///     returns false
    /// </summary>
    /// <param name="context"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    bool DiscoverExistence(DataAccessContext context, out string reason);
}