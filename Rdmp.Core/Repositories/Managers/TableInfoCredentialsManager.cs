// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Repositories.Managers;

/// <summary>
/// Allows you to create and destroy usage relationships between TableInfos and DataAccessCredentials (under context X).  For example you might have a DataAccessCredentials
/// called 'RoutineLoaderAccount' and give tables A,B and C permission to use it under DataAccessContext.DataLoad then have a separate DataAccessCredentials called
/// 'ReadonlyUserAccount' and give tables A,B,C and D permission to use it under DataAccessContext.Any
/// 
/// <para></para>
/// </summary>
internal class TableInfoCredentialsManager : ITableInfoCredentialsManager
{
    private readonly CatalogueRepository _repository;

    //returns of querying these links are either
    //          Dictionary<DataAccessContext,DataAccessCredentials> for all links where there is only one access point 1 - M (1 point many credentials)
    //OR        Dictionary<DataAccessContext, List<TableInfo>>      for all links where the query originates with a credentials M-M (credential is used by many users under many different contexts including potentially used by the same user under two+ different contexts)

    //Cannot query Find all links between [collection of access points] and [collection of credentials] yet (probably never need to do this)

    /// <summary>
    /// Creates a new helper class instance for writing/deleting credential usages for <see cref="TableInfo"/> objects in the <paramref name="repository"/>
    /// </summary>
    /// <param name="repository"></param>
    public TableInfoCredentialsManager(CatalogueRepository repository)
    {
        _repository = repository;
    }


    /// <inheritdoc/>
    public void CreateLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo, DataAccessContext context)
    {
        using (var con = _repository.GetConnection())
        {
            using var cmd = DatabaseCommandHelper.GetCommand(
                "INSERT INTO DataAccessCredentials_TableInfo(DataAccessCredentials_ID,TableInfo_ID,Context) VALUES (@cid,@tid,@context)",
                con.Connection, con.Transaction);
            cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@cid", cmd));
            cmd.Parameters["@cid"].Value = credentials.ID;

            cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@tid", cmd));
            cmd.Parameters["@tid"].Value = tableInfo.ID;

            cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@context", cmd));
            cmd.Parameters["@context"].Value = context;
            cmd.ExecuteNonQuery();
        }

        tableInfo.ClearAllInjections();
    }

    /// <inheritdoc/>
    public void BreakLinkBetween(DataAccessCredentials credentials, ITableInfo tableInfo, DataAccessContext context)
    {
        _repository.Delete(
            "DELETE FROM DataAccessCredentials_TableInfo WHERE DataAccessCredentials_ID = @cid AND TableInfo_ID = @tid and Context =@context",
            new Dictionary<string, object>
            {
                { "cid", credentials.ID },
                { "tid", tableInfo.ID },
                { "context", context }
            });

        tableInfo.ClearAllInjections();
    }

    /// <inheritdoc/>
    public void BreakAllLinksBetween(DataAccessCredentials credentials, ITableInfo tableInfo)
    {
        _repository.Delete(
            "DELETE FROM DataAccessCredentials_TableInfo WHERE DataAccessCredentials_ID = @cid AND TableInfo_ID = @tid",
            new Dictionary<string, object>
            {
                { "cid", credentials.ID },
                { "tid", tableInfo.ID }
            }, false);
    }

    /// <inheritdoc/>
    public DataAccessCredentials GetCredentialsIfExistsFor(ITableInfo tableInfo, DataAccessContext context)
    {
        var toReturn = -1;

        using (var con = _repository.GetConnection())
        {
            using var cmd = DatabaseCommandHelper.GetCommand(
                $"SELECT DataAccessCredentials_ID,Context FROM DataAccessCredentials_TableInfo WHERE TableInfo_ID = @tid and (Context =@context OR Context={(int)DataAccessContext.Any}) ",
                con.Connection, con.Transaction);
            cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@tid", cmd));
            cmd.Parameters["@tid"].Value = tableInfo.ID;
            cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@context", cmd));
            cmd.Parameters["@context"].Value = context;

            using var r = cmd.ExecuteReader();
            //gets the first licenced usage
            if (r.Read())
            {
                //there is one
                //get it by its id
                toReturn = Convert.ToInt32(r["DataAccessCredentials_ID"]);

                //if the first record is licenced for Any
                if (Convert.ToInt32(r["Context"]) == (int)DataAccessContext.Any)
                    //see if there is a more specific second record (e.g. DataLoad)
                    if (r.Read())
                        toReturn = Convert.ToInt32(r["DataAccessCredentials_ID"]);
            }
        }

        return toReturn != -1 ? _repository.GetObjectByID<DataAccessCredentials>(toReturn) : null;
    }


    /// <inheritdoc/>
    public Dictionary<DataAccessContext, DataAccessCredentials> GetCredentialsIfExistsFor(ITableInfo tableInfo)
    {
        Dictionary<DataAccessContext, int> toReturn;

        using (var con = _repository.GetConnection())
        {
            using var cmd = DatabaseCommandHelper.GetCommand(
                "SELECT DataAccessCredentials_ID,Context FROM DataAccessCredentials_TableInfo WHERE TableInfo_ID = @tid",
                con.Connection, con.Transaction);
            cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@tid", cmd));
            cmd.Parameters["@tid"].Value = tableInfo.ID;

            using var r = cmd.ExecuteReader();
            toReturn = GetLinksFromReader(r);
        }

        return toReturn.ToDictionary(k => k.Key, v => _repository.GetObjectByID<DataAccessCredentials>(v.Value));
    }

    /// <inheritdoc/>
    public Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>> GetAllCredentialUsagesBy(
        DataAccessCredentials[] allCredentials, ITableInfo[] allTableInfos)
    {
        var allCredentialsDictionary = allCredentials.ToDictionary(k => k.ID, v => v);
        var allTablesDictionary = allTableInfos.ToDictionary(k => k.ID, v => v);

        var toReturn = new Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>>();

        using var con = _repository.GetConnection();
        using var cmd = DatabaseCommandHelper.GetCommand(
            "SELECT DataAccessCredentials_ID,TableInfo_ID,Context FROM DataAccessCredentials_TableInfo",
            con.Connection, con.Transaction);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            //get the context
            var context = GetContext(r);

            var tid = Convert.ToInt32(r["TableInfo_ID"]);
            var cid = Convert.ToInt32(r["DataAccessCredentials_ID"]);

            //async error? someone created a new credential usage between the allCredentials array being fetched and us reaching this methods execution?
            if (!allTablesDictionary.ContainsKey(tid) || !allCredentialsDictionary.ContainsKey(cid))
                continue; //should be super rare never gonna happen

            var t = allTablesDictionary[tid];
            var c = allCredentialsDictionary[cid];

            if (!toReturn.ContainsKey(t))
                toReturn.Add(t, new List<DataAccessCredentialUsageNode>());

            toReturn[t].Add(new DataAccessCredentialUsageNode(c, t, context));
        }

        return toReturn;
    }

    /// <inheritdoc/>
    public Dictionary<DataAccessContext, List<ITableInfo>> GetAllTablesUsingCredentials(
        DataAccessCredentials credentials)
    {
        var toReturn = new Dictionary<DataAccessContext, List<int>>
        {
            { DataAccessContext.Any, new List<int>() },
            { DataAccessContext.DataExport, new List<int>() },
            { DataAccessContext.DataLoad, new List<int>() },
            { DataAccessContext.InternalDataProcessing, new List<int>() },
            { DataAccessContext.Logging, new List<int>() }
        };

        using (var con = _repository.GetConnection())
        {
            using var cmd = DatabaseCommandHelper.GetCommand(
                "SELECT TableInfo_ID,Context FROM DataAccessCredentials_TableInfo WHERE DataAccessCredentials_ID = @cid",
                con.Connection, con.Transaction);
            cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@cid", cmd));
            cmd.Parameters["@cid"].Value = credentials.ID;

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                //get the context
                var context = GetContext(r);

                //add the TableInfo under that context
                toReturn[context].Add((int)r["TableInfo_ID"]);
            }
        }

        return toReturn.ToDictionary(k => k.Key,
            v => _repository.GetAllObjectsInIDList<TableInfo>(v.Value).Cast<ITableInfo>().ToList());
    }

    /// <summary>
    /// Helper that returns 1-M results (where there is only one originating TableInfo, if there are more than 1 table info in your SQL query you will get key collisions)
    /// </summary>
    /// <param name="r"></param>
    /// <returns></returns>
    private static Dictionary<DataAccessContext, int> GetLinksFromReader(DbDataReader r)
    {
        var toReturn = new Dictionary<DataAccessContext, int>();
        //gets the first licenced usage
        while (r.Read())
        {
            //get the context
            //if it's not a valid context something has gone very wrong
            if (!Enum.TryParse((string)r["Context"], out DataAccessContext context))
                throw new Exception($"Invalid DataAccessContext {r["Context"]}");

            //there is only one credential per context per table info so don't worry about key collisions they should be impossible
            toReturn.Add(context, Convert.ToInt32(r["DataAccessCredentials_ID"]));
        }

        return toReturn;
    }

    /// <inheritdoc/>
    public DataAccessCredentials GetCredentialByUsernameAndPasswordIfExists(string username, string password)
    {
        //see if we already have a record of this user
        var existingCredentials = _repository.GetAllObjects<DataAccessCredentials>()
            .Where(c => c.Username.Equals(username)).ToArray();

        //found an existing credential that matched on username
        if (existingCredentials.Any())
        {
            //there is one or more existing credential with this username
            var matchingOnPassword = existingCredentials.Where(c => c.PasswordIs(password)).ToArray();

            if (matchingOnPassword.Length == 1)
                return matchingOnPassword.Single();

            if (matchingOnPassword.Length > 1)
                throw new Exception(
                    $"Found {matchingOnPassword.Length} DataAccessCredentials that matched the supplied username/password - does your database have massive duplication in it?");

            //there are 0 that match on password
            return null;
        }

        //did not find an existing credential that matched on username
        return null;
    }

    private static DataAccessContext GetContext(DbDataReader r) =>
        //if it's not a valid context something has gone very wrong
        !Enum.TryParse((string)r["Context"], out DataAccessContext context)
            ? throw new Exception($"Invalid DataAccessContext {r["Context"]}")
            : context;
}