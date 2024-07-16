// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.DataLoad.Engine.Migration;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataLoad.Engine.Checks.Checkers;

internal class CatalogueLoadChecks : ICheckable
{
    private readonly ILoadMetadata _loadMetadata;
    private readonly HICLoadConfigurationFlags _loadConfigurationFlags;
    private readonly HICDatabaseConfiguration _databaseConfiguration;

    public CatalogueLoadChecks(ILoadMetadata loadMetadata, HICLoadConfigurationFlags loadConfigurationFlags,
        HICDatabaseConfiguration databaseConfiguration)
    {
        _loadMetadata = loadMetadata;
        _loadConfigurationFlags = loadConfigurationFlags;
        _databaseConfiguration = databaseConfiguration;
    }


    private bool ValidateFilePath(string directoryPath)
    {
        return Path.Exists(directoryPath);
    }

    public void Check(ICheckNotifier notifier)
    {
        Catalogue[] catalogueMetadatas;

        try
        {
            //check there are catalogues and we can retrieve them
            catalogueMetadatas = _loadMetadata.GetAllCatalogues().Cast<Catalogue>().ToArray();
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Crashed trying to fetch Catalogues for metadata",
                CheckResult.Fail, e));
            return;
        }

        //there no catalogues
        if (catalogueMetadatas.Length == 0)
            notifier.OnCheckPerformed(new CheckEventArgs("There are no Catalogues associated with this metadata",
                CheckResult.Fail, null));

        var tablesFound = new List<ITableInfo>();

        //check each catalogue is sufficiently configured to perform a migration
        foreach (var catalogue in catalogueMetadatas)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Found Catalogue:{catalogue}", CheckResult.Success, null));

            var tableInfos = catalogue.GetTableInfoList(true).Distinct().ToArray();

            if (tableInfos.Length == 0)
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Catalogue {catalogue.Name} does not have any TableInfos", CheckResult.Fail, null));

            tablesFound.AddRange(tableInfos.Where(tableInfo => !tablesFound.Contains(tableInfo)));
            if(_loadMetadata.LocationOfForLoadingDirectory != null && !ValidateFilePath(_loadMetadata.LocationOfForLoadingDirectory))
                 notifier.OnCheckPerformed(new CheckEventArgs($"The ForLoading directory for this load ({_loadMetadata.LocationOfForLoadingDirectory}) does not exist.",
                CheckResult.Fail, null));
            if (_loadMetadata.LocationOfForArchivingDirectory!= null && !ValidateFilePath(_loadMetadata.LocationOfForArchivingDirectory))
                notifier.OnCheckPerformed(new CheckEventArgs($"The ForArchiving directory for this load ({_loadMetadata.LocationOfForArchivingDirectory}) does not exist.",
               CheckResult.Fail, null));
            if (_loadMetadata.LocationOfCacheDirectory != null && _loadMetadata.LocationOfForLoadingDirectory != null && !ValidateFilePath(_loadMetadata.LocationOfCacheDirectory))
                notifier.OnCheckPerformed(new CheckEventArgs($"The Cache directory for this load ({_loadMetadata.LocationOfCacheDirectory}) does not exist.",
               CheckResult.Fail, null));
            if (_loadMetadata.LocationOfExecutablesDirectory != null && !ValidateFilePath(_loadMetadata.LocationOfExecutablesDirectory))
                notifier.OnCheckPerformed(new CheckEventArgs($"The Executables directory for this load ({_loadMetadata.LocationOfExecutablesDirectory}) does not exist.",
               CheckResult.Fail, null));
        }


        //check regular tables
        foreach (TableInfo regularTable in tablesFound)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"About To check configuration of TableInfo:{regularTable}",
                CheckResult.Success, null));
            CheckTableInfo(regularTable, notifier);

            //check anonymisation
            var anonymisationChecks = new AnonymisationChecks(regularTable);
            anonymisationChecks.Check(notifier);
        }
    }


    #region These methods check TableInfos (Lookup including lookups)

    private void CheckTableInfo(TableInfo tableInfo, ICheckNotifier notifier)
    {
        //get all columns

        //check whether the live database and staging databases have appropriate columns and triggers etc on them
        var staging = _databaseConfiguration.DeployInfo[LoadBubble.Staging];
        var live = DataAccessPortal.ExpectDatabase(tableInfo, DataAccessContext.DataLoad);

        var liveTable =
            live.ExpectTable(tableInfo.GetRuntimeName(LoadBubble.Live, _databaseConfiguration.DatabaseNamer),
                tableInfo.Schema);
        var liveCols = liveTable.DiscoverColumns();

        CheckTableInfoSynchronization(tableInfo, notifier);

        CheckTableHasColumnInfosAndPrimaryKeys(live, tableInfo, out _, out _, notifier);

        //check for trying to ignore primary keys
        foreach (var col in tableInfo.ColumnInfos)
            if (col.IgnoreInLoads && col.IsPrimaryKey)
                notifier.OnCheckPerformed(
                    new CheckEventArgs($"ColumnInfo {col} is marked both IgnoreInLoads and IsPrimaryKey",
                        CheckResult.Fail));

        try
        {
            if (!_loadMetadata.IgnoreTrigger)
            {
                //if trigger is created as part of this check then it is likely to have resulted in changes to the underlying table (e.g. added hic_validFrom field) in which case we should resynch the TableInfo to pickup these new columns
                CheckTriggerIntact(liveTable, notifier, out var runSynchronizationAgain);

                if (runSynchronizationAgain)
                    CheckTableInfoSynchronization(tableInfo, notifier);
            }

            if (!_databaseConfiguration.RequiresStagingTableCreation)
            {
                //Important:
                //Regarding this if block: None of the current loading systems in RDMP have RequiresStagingTableCreation as false but in theory you could build some kind of multi threaded horror
                //which had multiple simultaneous loads all populating a single STAGING bubble therefore this code is left in the codebase, it probably works ok but you will need a
                //fair bit of work if you want to realise such a monstrosity (for one cleanup on each run probably cleans up STAGING on exit)

                //live can have additional hic_ columns which do not appear in staging (lookups cannot)
                var stagingTable =
                    staging.ExpectTable(tableInfo.GetRuntimeName(LoadBubble.Staging,
                        _databaseConfiguration.DatabaseNamer));

                if (!stagingTable.Exists())
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "RequiresStagingTableCreation is false but staging does not exist, this flag should indicate that you anticipate STAGING to be already setup before you kick off the load",
                        CheckResult.Fail));

                var stagingCols = stagingTable.DiscoverColumns();

                ConfirmStagingAndLiveHaveSameColumns(tableInfo.GetRuntimeName(), stagingCols, liveCols, false,
                    notifier);

                CheckStagingToLiveMigrationForTable(stagingTable, stagingCols, liveTable, liveCols, notifier);
            }
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Load Checks Crashed {tableInfo}", CheckResult.Fail, e));
        }
    }

    private static void CheckTableInfoSynchronization(TableInfo tableInfo, ICheckNotifier notifier)
    {
        //live is the current data load's (possibly overridden server/database)
        var tableInfoSynchronizer = new TableInfoSynchronizer(tableInfo);

        var problemList = "";

        //synchronize but refuse to apply all fixes, problems are instead added to problemList
        if (tableInfoSynchronizer.Synchronize(notifier))
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"TableInfo {tableInfo} passed Synchronization check",
                CheckResult.Success, null)); //passed synchronization
        }
        else
        {
            var launchSyncFixer = notifier.OnCheckPerformed(new CheckEventArgs(
                $"TableInfo {tableInfo} failed Synchronization check with following problems:{problemList}",
                CheckResult.Fail,
                null, "Launch Synchronization Fixing")); //failed synchronization

            if (launchSyncFixer)
            {
                var userFixed =
                    //if silent running accept all changes
                    tableInfoSynchronizer.Synchronize(notifier);

                if (!userFixed)
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        $"TableInfo {tableInfo} still failed Synchronization check", CheckResult.Fail,
                        null)); //passed synchronization
            }
        }
    }

    private static void CheckTableHasColumnInfosAndPrimaryKeys(DiscoveredDatabase live, TableInfo tableInfo,
        out ColumnInfo[] columnInfos, out ColumnInfo[] columnInfosWhichArePrimaryKeys, ICheckNotifier notifier)
    {
        columnInfos = tableInfo.ColumnInfos.ToArray();
        columnInfosWhichArePrimaryKeys = columnInfos.Where(col => col.IsPrimaryKey).ToArray();


        //confirm there are at least 1
        if (!columnInfos.Any())
            notifier.OnCheckPerformed(new CheckEventArgs($"TableInfo {tableInfo.Name} has no columninfos",
                CheckResult.Fail, null));


        if (!columnInfosWhichArePrimaryKeys.Any())
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"TableInfo {tableInfo.Name} has no IsPrimaryKey columns", CheckResult.Fail, null));

        var primaryKeys = live.ExpectTable(tableInfo.GetRuntimeName(), tableInfo.Schema).DiscoverColumns()
            .Where(c => c.IsPrimaryKey).ToArray();

        if (primaryKeys.Any(k => k.IsAutoIncrement))
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"AutoIncrement columns {string.Join(",", primaryKeys.Where(k => k.IsAutoIncrement))} are not allowed as Primary Keys for your table because there is no way to differentiate new data being loaded from duplicate old data being loaded (the entire purpose of the RDMP DLE)",
                CheckResult.Fail));

        //confirm primary keys match underlying table
        //sort pks alphabetically and confirm they match the underlying live system table
        var actualPks = primaryKeys.Select(c => c.GetRuntimeName()).OrderBy(s => s).ToArray();
        var pksWeExpect = columnInfosWhichArePrimaryKeys.Select(c => c.GetRuntimeName()).OrderBy(s => s).ToArray();


        if (actualPks.Length != pksWeExpect.Length)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Primary keys in Catalogue for database table {tableInfo.GetRuntimeName()} does not match Catalogue entry (difference in number of keys)",
                CheckResult.Fail, null));
        else
            for (var i = 0; i < pksWeExpect.Length; i++)
                if (!pksWeExpect[i].Equals(actualPks[i]))
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        $"Mismatch between primary key defined in Catalogue {pksWeExpect[i]} and one found in live table {tableInfo.GetRuntimeName()}",
                        CheckResult.Fail, null));
                else
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        $"Found primary key {pksWeExpect[i]} in LIVE table {tableInfo.GetRuntimeName()}",
                        CheckResult.Success, null));
    }

    private static void CheckTriggerIntact(DiscoveredTable table, ICheckNotifier notifier,
        out bool runSynchronizationAgain)
    {
        var checker = new TriggerChecks(table);
        checker.Check(notifier);

        runSynchronizationAgain = checker.TriggerCreated;
    }


    private static void ConfirmStagingAndLiveHaveSameColumns(string tableName, DiscoveredColumn[] stagingCols,
        DiscoveredColumn[] liveCols, bool requireSameNumberAndOrder, ICheckNotifier notifier)
    {
        //in LIVE but not STAGING
        foreach (var missingColumn in liveCols.Select(c => c.GetRuntimeName())
                     .Except(stagingCols.Select(c => c.GetRuntimeName())))
            //column is in live but not in staging, but it is hic_
            if (SpecialFieldNames.IsHicPrefixed(missingColumn)) //this is permitted
                continue;
            else
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Column {missingColumn} is missing from STAGING", CheckResult.Fail, null));

        //in STAGING but not LIVE
        foreach (var missingColumn in stagingCols.Except(liveCols))
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Column {missingColumn} is in STAGING but not LIVE", CheckResult.Fail, null));


        if (requireSameNumberAndOrder)
        {
            var passedColumnOrderCheck = true;

            if (stagingCols.Length != liveCols.Length)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Column count mismatch between staging and live in table {tableName}", CheckResult.Fail, null));
                passedColumnOrderCheck = false;
            }
            else
            //check they are in the same order
            {
                for (var i = 0; i < stagingCols.Length; i++)
                    if (!stagingCols[i].Equals(liveCols[i]))
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            $"Column name/order mismatch between staging and live in table {tableName}, column {i} is {stagingCols[i]} in staging but is {liveCols[i]} in live.",
                            CheckResult.Fail, null));
                        passedColumnOrderCheck = false;
                        break;
                    }
            }

            if (passedColumnOrderCheck)
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Column order match confirmed between staging and live on table {tableName}", CheckResult.Success,
                    null));
        }
    }

    private void CheckStagingToLiveMigrationForTable(DiscoveredTable stagingTable, DiscoveredColumn[] stagingCols,
        DiscoveredTable liveTable, DiscoveredColumn[] liveCols, ICheckNotifier notifier)
    {
        try
        {
            new MigrationColumnSet(stagingTable, liveTable, new StagingToLiveMigrationFieldProcessor
            {
                NoBackupTrigger = _loadMetadata.IgnoreTrigger
            });
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"TableInfo {liveTable} passed {nameof(MigrationColumnSet)} check ", CheckResult.Success, null));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"{nameof(MigrationColumnSet)} reports a problem with the configuration of columns on STAGING/LIVE or in the ColumnInfos for TableInfo {liveTable}",
                CheckResult.Fail, e));
        }

        //live columns
        foreach (var col in liveCols)
            if (!SpecialFieldNames.IsHicPrefixed(col) && col.IsAutoIncrement) //must start hic_ if they are identities
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Column {col} is an identity column in the LIVE database but does not start with hic_",
                    CheckResult.Fail, null)); //this one does not

        //staging columns
        foreach (var col in stagingCols) //staging columns
            if (col.IsAutoIncrement) //if there are any auto increments
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Column {col} is an identity column and is in STAGING, the identity flag must be removed from the STAGING table",
                    CheckResult.Fail,
                    null)); //complain since don't want a mismatch between IDs in staging and live or complaints about identity insert from SQL server

        //staging must allow null dataloadrunids and validfroms
        ConfirmNullability(stagingTable.DiscoverColumn(SpecialFieldNames.DataLoadRunID), true, notifier);
        ConfirmNullability(stagingTable.DiscoverColumn(SpecialFieldNames.ValidFrom), true, notifier);

        //live must allow nulls in validFrom
        ConfirmNullability(liveTable.DiscoverColumn(SpecialFieldNames.ValidFrom), true, notifier);
    }

    private static void ConfirmNullability(DiscoveredColumn column, bool expectedNullability, ICheckNotifier notifier)
    {
        var nullability = column.AllowNulls;
        notifier.OnCheckPerformed(new CheckEventArgs(
            $"Nullability of {column} is AllowNulls={nullability}, (expected {expectedNullability})",
            nullability == expectedNullability ? CheckResult.Success : CheckResult.Fail, null));
    }

    #endregion
}