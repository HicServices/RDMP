// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Migration;
using Rdmp.Core.DataLoad.Engine.Migration.QueryBuilding;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.DataLoad.Modules.Mutilators.QueryBuilders;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

/// <summary>
/// Deletes records in STAGING which are 'older' versions of records that currently exist in LIVE.  Normally RMDP supports a 'newer is better' policy in which
/// all records loaded in a DLE run automatically replace/add to the LIVE table based on primary key (i.e. a newly loaded record with pk X will result in an
/// UPDATE of the values for that record to the new values in STAGING that are being loaded).
/// 
/// <para>This component is designed to support loading periods of old data into a LIVE data table that has moved on (i.e. to backfill a dataset) without
/// overwriting newer versions of a record (with primary key x) with old.  For example it is 2011 and you have found a year of data you forgot to load back
/// in 2009 but you expect that since 2009 there have been historical record updates for records originally generated in 2009 (you want to load all 2009 records
/// from the historical batch except where there has been an update since).</para>
/// 
/// <para>This is done by selecting a 'TimePeriodicity' field that identifies the 'dataset time' of the record (as opposed to the load time) e.g. 'date blood sample
///  taken'.  STAGING records will be deleted where there are records in LIVE which  have the same primary key but a newer TimePeriodicity date.</para>
/// </summary>
public class StagingBackfillMutilator : IPluginMutilateDataTables
{
    private DiscoveredDatabase _dbInfo;
    private TableInfo _tiWithTimeColumn;
    private BackfillSqlHelper _sqlHelper;
    private MigrationConfiguration _migrationConfiguration;

    // Only a test runner can set this
    public bool TestContext { get; set; }

    [DemandsInitialization("Time periodicity field", Mandatory = true)]
    public ColumnInfo TimePeriodicityField { get; set; }

    // Currently hardcode this as the TableNamingScheme when not in a test context: it is hardcoded in the load and ITableNamingScheme is not a supported process argument.
    //[DemandsInitialization("The class name of the ITableNamingScheme used for this load. This should really come from the load itself, as we don't want a different class from the rest of the load process being chosen here but that will require some design changes.")]
    public INameDatabasesAndTablesDuringLoads TableNamingScheme { get; set; }

    public ExitCodeType Mutilate(IDataLoadJob listener)
    {
        if (TimePeriodicityField == null)
            throw new InvalidOperationException("TimePeriodicityField has not been set.");

        var liveDatabaseInfo = GetLiveDatabaseInfo();

        if (TestContext)
        {
            // If we are operating inside a test, the client is responsible for providing a TableNamingScheme
            if (TableNamingScheme == null)
                throw new InvalidOperationException(
                    "Executing within test context but no TableNamingScheme has been provided");
        }
        else
        // If we are not operating inside a Test, hardwire the TableNamingScheme
        {
            TableNamingScheme = new FixedStagingDatabaseNamer(liveDatabaseInfo.GetRuntimeName());
        }

        // create invariant helpers
        _sqlHelper = new BackfillSqlHelper(TimePeriodicityField, _dbInfo, liveDatabaseInfo);
        _migrationConfiguration =
            new MigrationConfiguration(liveDatabaseInfo, LoadBubble.Live, LoadBubble.Staging, TableNamingScheme);

        // starting with the TimePeriodicity table, we descend the join relationships to the leaf tables then ascend back up to the TimePeriodicity table
        // at each step we determine the effective date of the record by joining back to the TimePeriodicity table
        // this allows us to remove updates that are older than the corresponding record in live
        // - however we don't remove rows that still have children, hence the recursion from leaves upwards
        // -- a record may be an 'old update', but have a child for insertion (i.e. the child is not in live), in this case the parent must remain in staging despite it being 'old'
        // - 'old updates' that are not deleted (because they have new descendants) must have their own data updated to reflect what is in live if there is a difference between the two, otherwise we may overwrite live with stale data
        //
        _tiWithTimeColumn = TimePeriodicityField.TableInfo;
        ProcessOldUpdatesInTable(_tiWithTimeColumn, new List<JoinInfo>());

        // Having processed all descendants of the TimePeriodicity table, we now recursively ascend back up through its predecessors to the top of the join tree
        // Doing effectively the same thing, removing items that are older than the corresponding live items that do not also have new descendants and updating staging rows with live data where required.
        ProcessPredecessors(_tiWithTimeColumn, new List<JoinInfo>());

        return ExitCodeType.Success;
    }

    /// <summary>
    /// Get the database credentials for the Live server, accessing them via the TimePeriodicityField ColumnInfo
    /// </summary>
    /// <returns></returns>
    private DiscoveredDatabase GetLiveDatabaseInfo()
    {
        var timePeriodicityTable = TimePeriodicityField.TableInfo;
        return DataAccessPortal.ExpectDatabase(timePeriodicityTable, DataAccessContext.DataLoad);
    }

    /// <summary>
    /// Ascends join tree from the TimePeriodicity table, processing tables at each step
    /// </summary>
    /// <param name="tiCurrent"></param>
    /// <param name="joinPathToTimeTable"></param>
    private void ProcessPredecessors(TableInfo tiCurrent, List<JoinInfo> joinPathToTimeTable)
    {
        var repository = tiCurrent.Repository;

        // Find all parents of this table
        var allJoinInfos = repository.GetAllObjects<JoinInfo>();
        var joinsWithThisTableAsChild =
            allJoinInfos.Where(info => info.ForeignKey.TableInfo_ID == tiCurrent.ID).ToList();

        // Infinite recursion check
        var seenBefore = joinPathToTimeTable.Intersect(joinsWithThisTableAsChild).ToList();
        if (seenBefore.Any())
            throw new InvalidOperationException(
                $"Join loop: I've seen join(s) {string.Join(",", seenBefore.Select(j => $"{j.PrimaryKey} -> {j.ForeignKey}"))} before so we must have hit a loop (and will never complete the recursion).");

        // Process this table and its children (we need info about the children in order to join and detect childless rows)
        var joinsWithThisTableAsParent =
            allJoinInfos.Where(info => info.PrimaryKey.TableInfo_ID == tiCurrent.ID).ToList();
        ProcessTable(tiCurrent, joinPathToTimeTable, joinsWithThisTableAsParent);

        // Ascend into parent tables once this table has been processed
        foreach (var join in joinsWithThisTableAsChild)
        {
            var tiParent = join.PrimaryKey.TableInfo;

            // We may have a stale ID, some other pass may have deleted the table through a different path of JoinInfos
            if (tiParent == null)
                continue;

            ProcessPredecessors(tiParent, new List<JoinInfo>(joinPathToTimeTable) { join });
        }
    }

    /// <summary>
    /// Descends to leaves of join tree, then processes tables on way back up
    /// </summary>
    /// <param name="tiCurrent"></param>
    /// <param name="joinPathToTimeTable"></param>
    private void ProcessOldUpdatesInTable(TableInfo tiCurrent, List<JoinInfo> joinPathToTimeTable)
    {
        var repository = tiCurrent.Repository;

        // Process old updates in children first
        // Does toCurrent have any children?
        var allJoinInfos = repository.GetAllObjects<JoinInfo>();
        var joinsToProcess = allJoinInfos.Where(info => info.PrimaryKey.TableInfo_ID == tiCurrent.ID).ToList();
        foreach (var join in joinsToProcess)
        {
            var tiChild = join.ForeignKey.TableInfo;
            ProcessOldUpdatesInTable(tiChild, new List<JoinInfo>(joinPathToTimeTable) { join });
        }

        ProcessTable(tiCurrent, joinPathToTimeTable, joinsToProcess);
    }

    /// <summary>
    /// Deletes any rows in tiCurrent that are out-of-date (with respect to live) and childless, then updates remaining out-of-date rows with the values from staging.
    /// Out-of-date remaining rows will only be present if they have children which are to be inserted. Any other children will have been deleted in an earlier pass through the recursion (since it starts at the leaves and works upwards).
    /// </summary>
    /// <param name="tiCurrent"></param>
    /// <param name="joinPathToTimeTable">Chain of JoinInfos back to the TimePeriodicity table so we can join to it and recover the effective date of a particular row</param>
    /// <param name="childJoins"></param>
    private void ProcessTable(ITableInfo tiCurrent, List<JoinInfo> joinPathToTimeTable, List<JoinInfo> childJoins)
    {
        var columnSetsToMigrate =
            _migrationConfiguration.CreateMigrationColumnSetFromTableInfos(new[] { tiCurrent }.ToList(), null,
                new BackfillMigrationFieldProcessor());
        var columnSet = columnSetsToMigrate.Single();
        var queryHelper = new ReverseMigrationQueryHelper(columnSet);
        var mcsQueryHelper = new MigrationColumnSetQueryHelper(columnSet);

        // Any DELETEs needed?
        DeleteEntriesHavingNoChildren(tiCurrent, joinPathToTimeTable, childJoins, mcsQueryHelper);

        // Update any out-of-date rows that have survived the delete, so they don't overwrite live with stale data. They will only survive the delete if they have children due for insertion into live.
        UpdateOldParentsThatHaveNewChildren(tiCurrent, joinPathToTimeTable, queryHelper, mcsQueryHelper);
    }

    private void UpdateOldParentsThatHaveNewChildren(ITableInfo tiCurrent, List<JoinInfo> joinPathToTimeTable,
        ReverseMigrationQueryHelper queryHelper, MigrationColumnSetQueryHelper mcsQueryHelper)
    {
        var update = $@"WITH 
{GetLiveDataToUpdateStaging(tiCurrent, joinPathToTimeTable)}
UPDATE CurrentTable
SET {queryHelper.BuildUpdateClauseForRow("LiveDataForUpdating", "CurrentTable")}
FROM 
LiveDataForUpdating LEFT JOIN {$"[{_dbInfo.GetRuntimeName()}]..[{tiCurrent.GetRuntimeName()}]"} AS CurrentTable {mcsQueryHelper.BuildJoinClause("LiveDataForUpdating", "CurrentTable")}";

        using var connection = (SqlConnection)_dbInfo.Server.GetConnection();
        connection.Open();
        var cmd = new SqlCommand(update, connection);
        cmd.ExecuteNonQuery();
    }

    private void DeleteEntriesHavingNoChildren(ITableInfo tiCurrent, List<JoinInfo> joinPathToTimeTable,
        List<JoinInfo> joinsToProcess, MigrationColumnSetQueryHelper mcsQueryHelper)
    {
        // If there are no joins then we should delete any old updates at this level
        string deleteSql;
        if (!joinsToProcess.Any())
        {
            deleteSql =
                $"WITH {GetCurrentOldEntriesSQL(tiCurrent, joinPathToTimeTable)}, EntriesToDelete AS (SELECT * FROM CurrentOldEntries)";
        }
        else
        {
            // Join on children so we can detect childless rows and delete them
            var joins = new List<string>();
            var wheres = new List<string>();

            // create sql for child joins
            foreach (var childJoin in joinsToProcess)
            {
                var childTable = childJoin.ForeignKey.TableInfo;
                joins.Add(string.Format("LEFT JOIN {0} {1} ON CurrentOldEntries.{2} = {1}.{3}",
                    $"[{_dbInfo.GetRuntimeName()}]..[{childTable.GetRuntimeName()}]",
                    childTable.GetRuntimeName(),
                    childJoin.PrimaryKey.GetRuntimeName(),
                    childJoin.ForeignKey.GetRuntimeName()
                ));

                wheres.Add($"{childTable.GetRuntimeName()}.{childJoin.ForeignKey.GetRuntimeName()} IS NULL");
            }

            deleteSql =
                $"WITH {GetCurrentOldEntriesSQL(tiCurrent, joinPathToTimeTable)}, EntriesToDelete AS (SELECT DISTINCT CurrentOldEntries.* FROM CurrentOldEntries {string.Join(" ", joins)} WHERE {string.Join(" AND ", wheres)})";
        }

        deleteSql += $@"
DELETE CurrentTable
FROM {$"[{_dbInfo.GetRuntimeName()}]..[{tiCurrent.GetRuntimeName()}]"} CurrentTable
RIGHT JOIN EntriesToDelete {mcsQueryHelper.BuildJoinClause("EntriesToDelete", "CurrentTable")}";

        using var connection = (SqlConnection)_dbInfo.Server.GetConnection();
        connection.Open();
        var cmd = new SqlCommand(deleteSql, connection);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// This and GetLiveDataToUpdateStaging are ugly in that they just reflect modifications to the comparison CTE. Leaving for now as a more thorough refactoring may be required once the full test suite is available.
    /// </summary>
    /// <param name="tiCurrent"></param>
    /// <param name="joinPathToTimeTable"></param>
    /// <returns></returns>
    private string GetCurrentOldEntriesSQL(ITableInfo tiCurrent, List<JoinInfo> joinPathToTimeTable) =>
        $@"
CurrentOldEntries AS (
SELECT ToLoadWithTime.* FROM 

{_sqlHelper.GetSQLComparingStagingAndLiveTables(tiCurrent, joinPathToTimeTable)} 
";

    /// <summary>
    /// This and GetCurrentOldEntriesSQL are ugly in that they just reflect modifications to the comparison CTE. Leaving for now as a more thorough refactoring may be required once the full test suite is available.
    /// </summary>
    /// <param name="tiCurrent"></param>
    /// <param name="joinPathToTimeTable"></param>
    /// <returns></returns>
    private string GetLiveDataToUpdateStaging(ITableInfo tiCurrent, List<JoinInfo> joinPathToTimeTable) =>
        $@"
LiveDataForUpdating AS (
SELECT LoadedWithTime.* FROM

{_sqlHelper.GetSQLComparingStagingAndLiveTables(tiCurrent, joinPathToTimeTable)}";

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        _dbInfo = dbInfo;
    }

    public void Check(ICheckNotifier notifier)
    {
        // if we're not executing in a test context, fail the whole component: it doesn't yet have sufficient test coverage
        if (!TestContext)
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "Don't use the StagingBackfillMutilator component for now! Does not yet have sufficient test coverage.",
                    CheckResult.Fail));
    }


    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }
}