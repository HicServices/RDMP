// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Normally to open a connection to an IDataAccessPoint (location of server/database) you also need an optional IDataAccessCredentials (username and encrypted password).  These
/// These are usually two separate objects e.g. TableInfo and DataAccessCredentials (optional - if ommmited connections use integrated/windows security).
/// 
/// <para>Instead of doing that however, you can use this class to store all the bits in one object that implements both interfaces.  It can then be used with a
/// DataAccessPortal.</para>
/// </summary>
public class SelfCertifyingDataAccessPoint : EncryptedPasswordHost, IDataAccessCredentials, IDataAccessPoint
{
    /// <summary>
    /// Blank constructor for use with serializers
    /// </summary>
    public SelfCertifyingDataAccessPoint()
    {
    }

    /// <inheritdoc cref="SelfCertifyingDataAccessPoint"/>
    public SelfCertifyingDataAccessPoint(ICatalogueRepository repository, DatabaseType databaseType) : base(repository)
    {
        DatabaseType = databaseType;
    }

    public SelfCertifyingDataAccessPoint(DiscoveredServer server)
    {
        Server = server.Name;
        Database = server.GetCurrentDatabase()?.GetRuntimeName();
        DatabaseType = server.DatabaseType;

        Username = server.ExplicitUsernameIfAny;
        Password = server.ExplicitPasswordIfAny;
    }

    /// <inheritdoc/>
    public string Server { get; set; }

    /// <inheritdoc/>
    public string Database { get; set; }

    /// <inheritdoc/>
    public string Username { get; set; }

    /// <inheritdoc/>
    [NoMappingToDatabase]
    public DatabaseType DatabaseType { get; set; }

    /// <inheritdoc/>
    public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
    {
        //this class is not configured with a username so pretend like we don't have any credentials
        if (string.IsNullOrWhiteSpace(Username))
            return null;

        //this class is its own credentials
        return this;
    }

    /// <inheritdoc/>
    public IQuerySyntaxHelper GetQuerySyntaxHelper() => QuerySyntaxHelperFactory.Create(DatabaseType);

    public bool DiscoverExistence(DataAccessContext context, out string reason)
    {
        DiscoveredDatabase db;
        try
        {
            db = Discover(context);
        }
        catch (Exception ex)
        {
            reason = ex.Message;
            return false;
        }


        if (!db.Exists())
        {
            reason = "database did not exist";
            return false;
        }

        reason = null;
        return true;
    }

    internal DiscoveredDatabase Discover(DataAccessContext context) => DataAccessPortal.ExpectDatabase(this, context);
}