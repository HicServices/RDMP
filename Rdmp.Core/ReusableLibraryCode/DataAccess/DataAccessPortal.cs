// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.ReusableLibraryCode.Exceptions;

namespace Rdmp.Core.ReusableLibraryCode.DataAccess;

/// <summary>
///     Translation class for converting IDataAccessPoints into DiscoveredServer / DiscoveredDatabase / ConnectionStrings
///     etc.  IDataAccessPoints are named
///     servers/databases which might have usernames/passwords associated with them (or might use Integrated Security).
///     Each IDataAccessPoint can have multiple
///     credentials that can be used with it depending on the DataAccessContext.  Therefore when using the DataAccessPortal
///     you always have to specify the
///     Context of the activity you are doing e.g. DataAccessContext.DataLoad.
/// </summary>
public static class DataAccessPortal
{
    public static DiscoveredServer ExpectServer(IDataAccessPoint dataAccessPoint, DataAccessContext context,
        bool setInitialDatabase = true)
    {
        return GetServer(dataAccessPoint, context, setInitialDatabase);
    }

    public static DiscoveredDatabase ExpectDatabase(IDataAccessPoint dataAccessPoint, DataAccessContext context)
    {
        return GetServer(dataAccessPoint, context, true).GetCurrentDatabase();
    }

    public static DiscoveredServer ExpectDistinctServer(IDataAccessPoint[] collection, DataAccessContext context,
        bool setInitialDatabase)
    {
        return GetServer(GetDistinct(collection, context, setInitialDatabase), context, setInitialDatabase);
    }

    private static DiscoveredServer GetServer(IDataAccessPoint dataAccessPoint, DataAccessContext context,
        bool setInitialDatabase)
    {
        var credentials = dataAccessPoint.GetCredentialsIfExists(context);

        if (string.IsNullOrWhiteSpace(dataAccessPoint.Server))
            throw new NullReferenceException(
                $"Could not get connection string because Server was null on dataAccessPoint '{dataAccessPoint}'");

        string dbName = null;

        if (setInitialDatabase)
            if (!string.IsNullOrWhiteSpace(dataAccessPoint.Database))
                dbName = dataAccessPoint.GetQuerySyntaxHelper().GetRuntimeName(dataAccessPoint.Database);
            else
                throw new Exception(
                    $"Could not get server with setInitialDatabase=true because no Database was set on IDataAccessPoint {dataAccessPoint}");

        var server = new DiscoveredServer(dataAccessPoint.Server, dbName, dataAccessPoint.DatabaseType,
            credentials?.Username, credentials?.GetDecryptedPassword());

        return server;
    }

    private static IDataAccessPoint GetDistinct(IDataAccessPoint[] collection, DataAccessContext context,
        bool setInitialDatabase)
    {
        ///////////////////////Exception handling///////////////////////////////////////////////
        if (!collection.Any())
            throw new Exception("No IDataAccessPoints were passed, so no connection string builder can be created");

        var first = collection.First();

        //There can be only one - server
        foreach (var accessPoint in collection)
        {
            if (first.Server == null)
                throw new Exception($"Server is null for {first}");

            if (!first.Server.Equals(accessPoint.Server, StringComparison.CurrentCultureIgnoreCase))
                throw new ExpectedIdenticalStringsException(
                    $"There was a mismatch in server names for data access points {first} and {accessPoint} server names must match exactly",
                    first.Server, accessPoint.Server);

            if (first.DatabaseType != accessPoint.DatabaseType)
                throw new ExpectedIdenticalStringsException(
                    $"There was a mismatch on DatabaseType for data access points {first} and {accessPoint}",
                    first.DatabaseType.ToString(), accessPoint.DatabaseType.ToString());

            if (setInitialDatabase)
            {
                if (string.IsNullOrWhiteSpace(first.Database))
                    throw new Exception($"DataAccessPoint '{first}' does not have a Database specified on it");

                var querySyntaxHelper = accessPoint.GetQuerySyntaxHelper();

                var firstDbName = querySyntaxHelper.GetRuntimeName(first.Database);
                var currentDbName = querySyntaxHelper.GetRuntimeName(accessPoint.Database);

                if (!firstDbName.Equals(currentDbName))
                    throw new ExpectedIdenticalStringsException(
                        $"All data access points must be into the same database, access points '{first}' and '{accessPoint}' are into different databases",
                        firstDbName, currentDbName);
            }
        }

        //There can be only one - credentials (but there might not be any)
        var credentials = collection.Select(t => t.GetCredentialsIfExists(context)).ToArray();

        //if there are credentials
        if (credentials.Any(c => c != null))
            if (credentials.Any(c => c == null)) //all objects in collection must have a credentials if any of them do
                throw new Exception(
                    $"IDataAccessPoint collection could not agree whether to use Credentials or not {Environment.NewLine}Objects wanting to use Credentials{string.Join(",", collection.Where(c => c.GetCredentialsIfExists(context) != null).Select(s => s.ToString()))}{Environment.NewLine}Objects not wanting to use Credentials{string.Join(",", collection.Where(c => c.GetCredentialsIfExists(context) == null).Select(s => s.ToString()))}{Environment.NewLine}"
                );
            else
                //There can be only one - Username
            if (credentials.Select(c => c.Username).Distinct().Count() != 1)
                throw new Exception(
                    $"IDataAccessPoint collection could not agree on a single Username to use to access the data under context {context} (Servers were {string.Join($",{Environment.NewLine}", collection.Select(c => $"{c} = {c.Database} - {c.DatabaseType}"))})");
            else
                //There can be only one - Password
            if (credentials.Select(c => c.GetDecryptedPassword()).Distinct().Count() != 1)
                throw new Exception(
                    $"IDataAccessPoint collection could not agree on a single Password to use to access the data under context {context} (Servers were {string.Join($",{Environment.NewLine}", collection.Select(c => $"{c} = {c.Database} - {c.DatabaseType}"))})");


        ///////////////////////////////////////////////////////////////////////////////

        //the bit that actually matters:
        return first;
    }
}