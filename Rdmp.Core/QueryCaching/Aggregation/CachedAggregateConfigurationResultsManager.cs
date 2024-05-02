// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using FAnsi.Extensions;
using FAnsi.Naming;
using NLog;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.QueryCaching.Aggregation.Arguments;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.QueryCaching.Aggregation;

/// <summary>
///     Handles the caching and versioning of AggregateConfigurations in a QueryCaching database
///     (QueryCaching.Database.csproj).  Query caching is the process
///     of storing the SQL query and resulting DataTable from running an Aggregate Configuration SQL query (Usually built
///     by an AggregateBuilder).
///     <para>
///         Caching is vital for large CohortIdentificationConfigurations which feature many complicated subqueries with
///         WHERE conditions and even Patient Index
///         Tables (See JoinableCohortAggregateConfiguration).  The only way some of these queries can finish in a sensible
///         time frame (i.e. minutes instead of days)
///         is to execute each subquery (AggregateConfiguration) and cache the resulting identifier lists with primary key
///         indexes.  The
///         CohortIdentificationConfiguration can then be built into a query that uses the cached results (See
///         CohortQueryBuilder).
///     </para>
///     <para>
///         In order to ensure the cache is never stale the exact SQL query is stored in a table
///         (CachedAggregateConfigurationResults) so that if the user changes
///         the AggregateConfiguration the cached DataTable is discarded (until the user executes and caches the new
///         version).
///     </para>
///     <para>
///         CachedAggregateConfigurationResultsManager can cache any CacheCommitArguments (includes not just patient
///         identifier lists but also aggregate graphs and
///         patient index tables).
///     </para>
/// </summary>
public partial class CachedAggregateConfigurationResultsManager
{
    private readonly DiscoveredServer _server;
    private readonly DiscoveredDatabase _database;

    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///     The name of the table in the query cache which tracks the SQL executed and the resulting tables generated when
    ///     caching
    /// </summary>
    public const string ResultsManagerTable = "CachedAggregateConfigurationResults";

    public CachedAggregateConfigurationResultsManager(IExternalDatabaseServer server)
    {
        _server = DataAccessPortal.ExpectServer(server, DataAccessContext.InternalDataProcessing);
        _database = DataAccessPortal.ExpectDatabase(server, DataAccessContext.InternalDataProcessing);
    }

    public const string CachingPrefix = "/*Cached:";

    public IHasFullyQualifiedNameToo GetLatestResultsTableUnsafe(AggregateConfiguration configuration,
        AggregateOperation operation)
    {
        return GetLatestResultsTableUnsafe(configuration, operation, out _);
    }

    public IHasFullyQualifiedNameToo GetLatestResultsTableUnsafe(AggregateConfiguration configuration,
        AggregateOperation operation, out string sql)
    {
        var syntax = _database.Server.GetQuerySyntaxHelper();
        var mgrTable = _database.ExpectTable(ResultsManagerTable);

        using (var con = _server.GetConnection())
        {
            con.Open();
            using var cmd = DatabaseCommandHelper.GetCommand(
                $@"Select 
{syntax.EnsureWrapped("TableName")},
{syntax.EnsureWrapped("SqlExecuted")} from {mgrTable.GetFullyQualifiedName()}
WHERE {syntax.EnsureWrapped("AggregateConfiguration_ID")} = {configuration.ID}
AND {syntax.EnsureWrapped("Operation")} = '{operation}'", con);
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                var tableName = r["TableName"].ToString();
                sql = r["SqlExecuted"] as string;
                return _database.ExpectTable(tableName);
            }
        }

        sql = null;
        return null;
    }

    /// <summary>
    ///     Returns the name of the query cache results table for <paramref name="configuration" /> if the
    ///     <paramref name="currentSql" /> matches
    ///     the SQL run when the cache result was generated.  Returns null if no cache result is found or there are changes in
    ///     the <paramref name="currentSql" />
    ///     since the cache result was generated.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="operation"></param>
    /// <param name="currentSql"></param>
    /// <returns></returns>
    public IHasFullyQualifiedNameToo GetLatestResultsTable(AggregateConfiguration configuration,
        AggregateOperation operation, string currentSql)
    {
        var syntax = _database.Server.GetQuerySyntaxHelper();
        var mgrTable = _database.ExpectTable(ResultsManagerTable);

        using var con = _server.GetConnection();
        con.Open();

        using var cmd = DatabaseCommandHelper.GetCommand(
            $@"Select 
{syntax.EnsureWrapped("TableName")},
{syntax.EnsureWrapped("SqlExecuted")} 
from {mgrTable.GetFullyQualifiedName()} 
WHERE 
{syntax.EnsureWrapped("AggregateConfiguration_ID")} = {configuration.ID} AND
{syntax.EnsureWrapped("Operation")} = '{operation}'", con);
        using var r = cmd.ExecuteReader();
        if (r.Read())
        {
            if (IsMatchOnSqlExecuted(r, currentSql))
            {
                var tableName = r["TableName"].ToString();
                return _database.ExpectTable(tableName);
            }

            return null; //this means that there was outdated SQL, we could show this to user at some point
        }

        return null;
    }

    private bool IsMatchOnSqlExecuted(DbDataReader r, string currentSql)
    {
        //replace all white space with single space
        var standardisedDatabaseSql = Spaces().Replace(r["SqlExecuted"].ToString(), " ");
        var standardisedUsersSql = Spaces().Replace(currentSql, " ");

        var match = standardisedDatabaseSql.ToLower().Trim().Equals(standardisedUsersSql.ToLower().Trim());

        if (match) return true;

        _logger.Info("Cache Miss:");
        _logger.Info("Current Sql:");
        _logger.Info(standardisedUsersSql);
        _logger.Info("Cached Sql:");
        _logger.Info(standardisedDatabaseSql);
        return false;
    }

    public void CommitResults(CacheCommitArguments arguments)
    {
        var configuration = arguments.Configuration;
        var operation = arguments.Operation;

        DeleteCacheEntryIfAny(configuration, operation);

        //Do not change Types of source columns unless there is an explicit override
        arguments.Results.SetDoNotReType(true);

        using var con = _server.GetConnection();
        con.Open();

        var nameWeWillGiveTableInCache = $"{operation}_AggregateConfiguration{configuration.ID}";

        //either it has no name or it already has name we want so its ok
        arguments.Results.TableName = nameWeWillGiveTableInCache;

        //add explicit types
        var tbl = _database.ExpectTable(nameWeWillGiveTableInCache);
        if (tbl.Exists())
            tbl.Drop();

        tbl = _database.CreateTable(nameWeWillGiveTableInCache, arguments.Results, arguments.ExplicitColumns);

        if (!tbl.Exists())
            throw new Exception("Cache table did not exist even after CreateTable completed without error!");

        var mgrTable = _database.ExpectTable(ResultsManagerTable);

        mgrTable.Insert(new Dictionary<string, object>
        {
            { "Committer", Environment.UserName },
            { "AggregateConfiguration_ID", configuration.ID },
            { "SqlExecuted", arguments.SQL.Trim() },
            { "Operation", operation.ToString() },
            { "TableName", tbl.GetRuntimeName() }
        });

        arguments.CommitTableDataCompleted(tbl);
    }

    /// <summary>
    ///     Deletes any cache entries for <paramref name="configuration" /> in its role as <paramref name="operation" />
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="operation"></param>
    /// <returns>True if a cache entry was found and deleted otherwise false</returns>
    /// <exception cref="Exception"></exception>
    public bool DeleteCacheEntryIfAny(AggregateConfiguration configuration, AggregateOperation operation)
    {
        var table = GetLatestResultsTableUnsafe(configuration, operation);
        var mgrTable = _database.ExpectTable(ResultsManagerTable);

        if (table != null)
        {
            using var con = _server.GetConnection();
            con.Open();

            //drop the data
            _database.ExpectTable(table.GetRuntimeName()).Drop();

            //delete the record!
            using var cmd = DatabaseCommandHelper.GetCommand(
                $"DELETE FROM {mgrTable.GetFullyQualifiedName()} WHERE AggregateConfiguration_ID = {configuration.ID} AND Operation = '{operation}'",
                con);
            var deletedRows = cmd.ExecuteNonQuery();
            return deletedRows != 1
                ? throw new Exception(
                    $"Expected exactly 1 record in CachedAggregateConfigurationResults to be deleted when erasing its record of operation {operation} but there were {deletedRows} affected records")
                : true;
        }

        return false;
    }

    [GeneratedRegex("\\s+")]
    private static partial Regex Spaces();
}