// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataLoad.Engine.Checks.Checkers;

/// <summary>
/// Checks DLE databases (RAW, STAGING, LIVE) are in a correct state ahead of running a data load (See LoadMetadata).
/// </summary>
public class PreExecutionChecker : ICheckable
{
    private readonly ILoadMetadata _loadMetadata;
    private readonly IList<ICatalogue> _cataloguesToLoad;
    private readonly HICDatabaseConfiguration _databaseConfiguration;

    /// <summary>
    /// True if when running <see cref="Check"/> there was a catastrophic problem e.g. unable to reach tables which means you shouldn't bother
    /// running any other kinds of checks
    /// </summary>
    public bool HardFail { get; private set; }

    public PreExecutionChecker(ILoadMetadata loadMetadata, HICDatabaseConfiguration overrideDatabaseConfiguration)
    {
        _loadMetadata = loadMetadata;
        _databaseConfiguration = overrideDatabaseConfiguration ?? new HICDatabaseConfiguration(loadMetadata);
        _cataloguesToLoad = _loadMetadata.GetAllCatalogues().ToList();
    }

    private void PreExecutionStagingDatabaseCheck(bool skipLookups)
    {
        var allTableInfos = _cataloguesToLoad.SelectMany(catalogue => catalogue.GetTableInfoList(!skipLookups))
            .Distinct().ToList();
        CheckDatabaseExistsForStage(LoadBubble.Staging, "STAGING database found", "STAGING database not found");

        if (_databaseConfiguration.RequiresStagingTableCreation)
        {
            CheckTablesDoNotExistOnStaging(allTableInfos);
        }
        else
        {
            CheckTablesAreEmptyInDatabaseOnServer();
            CheckColumnInfosMatchWithWhatIsInDatabaseAtStage(allTableInfos, LoadBubble.Staging);
        }
    }

    private void CheckDatabaseExistsForStage(LoadBubble deploymentStage, string successMessage, string failureMessage)
    {
        var dbInfo = _databaseConfiguration.DeployInfo[deploymentStage];

        if (!dbInfo.Exists())
        {
            var createDatabase = _notifier.OnCheckPerformed(new CheckEventArgs($"{failureMessage}: {dbInfo}",
                CheckResult.Fail, null,
                $"Create {dbInfo.GetRuntimeName()} on {dbInfo.Server.Name}"));


            if (createDatabase)
                dbInfo.Server.CreateDatabase(dbInfo.GetRuntimeName());
        }
        else
        {
            _notifier.OnCheckPerformed(new CheckEventArgs($"{successMessage}: {dbInfo}", CheckResult.Success, null));
        }
    }

    private void CheckTablesDoNotExistOnStaging(IEnumerable<ITableInfo> allTableInfos)
    {
        var stagingDbInfo = _databaseConfiguration.DeployInfo[LoadBubble.Staging];

        var tableNames = allTableInfos.Select(info =>
            info.GetRuntimeName(LoadBubble.Staging, _databaseConfiguration.DatabaseNamer));
        var alreadyExistingTableInfosThatShouldntBeThere =
            tableNames.Where(tableName => stagingDbInfo.ExpectTable(tableName).Exists()).ToList();

        if (alreadyExistingTableInfosThatShouldntBeThere.Any())
        {
            var nukeTables = _notifier.OnCheckPerformed(new CheckEventArgs(
                $"The following tables: '{string.Join(',', alreadyExistingTableInfosThatShouldntBeThere)}' exists in the Staging database ({stagingDbInfo.GetRuntimeName()}) but the database load configuration requires that tables are created during the load process",
                CheckResult.Fail, null, "Drop the tables"));

            if (nukeTables)
                RemoveTablesFromDatabase(alreadyExistingTableInfosThatShouldntBeThere, stagingDbInfo);
        }
        else
        {
            _notifier.OnCheckPerformed(new CheckEventArgs("Staging table is clear", CheckResult.Success, null));
        }
    }

    private void CheckTablesAreEmptyInDatabaseOnServer()
    {
        var stagingDbInfo = _databaseConfiguration.DeployInfo[LoadBubble.Staging];

        foreach (var table in stagingDbInfo.DiscoverTables(false))
            if (!table.IsEmpty())
                _notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Table {table} in staging database '{stagingDbInfo.GetRuntimeName()}' is not empty on {stagingDbInfo.Server.Name}",
                    CheckResult.Fail));
            else
                _notifier.OnCheckPerformed(new CheckEventArgs($"Staging database is empty ({stagingDbInfo})",
                    CheckResult.Success, null));
    }

    // Check that the column infos from the catalogue match up with what is actually in the staging databases
    private void CheckColumnInfosMatchWithWhatIsInDatabaseAtStage(IEnumerable<ITableInfo> allTableInfos,
        LoadBubble deploymentStage)
    {
        var dbInfo = _databaseConfiguration.DeployInfo[deploymentStage];
        foreach (var tableInfo in allTableInfos)
        {
            var columnNames = tableInfo.ColumnInfos.Select(info => info.GetRuntimeName()).ToList();

            if (!columnNames.Any())
                _notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Table '{tableInfo.GetRuntimeName()}' has no ColumnInfos", CheckResult.Fail, null));

            var tableName = tableInfo.GetRuntimeName(deploymentStage, _databaseConfiguration.DatabaseNamer);
            var table = dbInfo.ExpectTable(tableName);

            if (!table.Exists())
                throw new Exception(
                    $"PreExecutionChecker spotted that table does not exist:{table} it was about to check whether the TableInfo matched the columns or not");
        }
    }

    private void PreExecutionDatabaseCheck()
    {
        var allNonLookups = _cataloguesToLoad.SelectMany(catalogue => catalogue.GetTableInfoList(false)).Distinct()
            .ToList();
        CheckDatabaseExistsForStage(LoadBubble.Live, "LIVE database found", "LIVE database not found");
        CheckColumnInfosMatchWithWhatIsInDatabaseAtStage(allNonLookups, LoadBubble.Live);

        if (!_loadMetadata.IgnoreTrigger)
            CheckUpdateTriggers(allNonLookups);

        CheckRAWDatabaseIsNotPresent();
    }

    private void CheckRAWDatabaseIsNotPresent()
    {
        var persistentRaw = LoadMetadata.UsesPersistentRaw(_loadMetadata);
        var rawDbInfo = _databaseConfiguration.DeployInfo[LoadBubble.Raw];

        // Check that the raw database is not present
        if (!rawDbInfo.Exists())
        {
            // RAW db does not exist that's usually good unless...
            if (persistentRaw)
            {
                var shouldCreate = _notifier.OnCheckPerformed(new CheckEventArgs(
                    $"RAW database '{rawDbInfo}' does not exist but load is persistentRaw", CheckResult.Fail, null,
                    "Create RAW database?"));
                if (shouldCreate) rawDbInfo.Create();
            }

            return;
        }


        var shouldDrop = _notifier.OnCheckPerformed(new CheckEventArgs($"RAW database '{rawDbInfo}' exists",
            CheckResult.Fail, null,
            $"Drop {(persistentRaw ? "table(s)" : "database")} {rawDbInfo}"));

        if (!rawDbInfo.GetRuntimeName().EndsWith("_RAW", StringComparison.CurrentCultureIgnoreCase))
            throw new Exception(
                $"rawDbInfo database name did not end with _RAW! It was:{rawDbInfo.GetRuntimeName()} (Why is the system trying to drop this database?)");
        if (shouldDrop)
        {
            foreach (var t in rawDbInfo.DiscoverTables(true))
            {
                _notifier.OnCheckPerformed(new CheckEventArgs($"Dropping table {t.GetFullyQualifiedName()}...",
                    CheckResult.Success));
                t.Drop();
            }

            if (!persistentRaw)
            {
                _notifier.OnCheckPerformed(new CheckEventArgs($"Finally dropping database{rawDbInfo}...",
                    CheckResult.Success));
                rawDbInfo.Drop();
            }
        }
    }

    private void CheckUpdateTriggers(IEnumerable<ITableInfo> allTableInfos)
    {
        // Check that the update triggers are present/enabled
        foreach (var tableInfo in allTableInfos)
        {
            var checker = new TriggerChecks(_databaseConfiguration.DeployInfo[LoadBubble.Live]
                .ExpectTable(tableInfo.GetRuntimeName(), tableInfo.Schema));
            checker.Check(_notifier);
        }
    }


    private ICheckNotifier _notifier;


    public void Check(ICheckNotifier notifier)
    {
        _notifier = notifier;

        //For each table in load can we reach it and is it a valid table type
        foreach (var ti in _loadMetadata.GetAllCatalogues().SelectMany(c => c.GetTableInfoList(true)).Distinct())
        {
            DiscoveredTable tbl;
            try
            {
                tbl = ti.Discover(DataAccessContext.DataLoad);
            }
            catch (Exception e)
            {
                HardFail = true;
                notifier.OnCheckPerformed(new CheckEventArgs($"Could not reach table in load '{ti.Name}'",
                    CheckResult.Fail, e));
                return;
            }

            if (!tbl.Exists())
            {
                HardFail = true;
                notifier.OnCheckPerformed(new CheckEventArgs($"Table '{ti.Name}' does not exist", CheckResult.Fail));
            }

            if (tbl.TableType != TableType.Table)
            {
                HardFail = true;
                notifier.OnCheckPerformed(new CheckEventArgs($"Table '{ti}' is a {tbl.TableType}", CheckResult.Fail));
            }
        }

        if (HardFail)
            return;

        AtLeastOneTaskCheck();

        PreExecutionStagingDatabaseCheck(false);
        PreExecutionDatabaseCheck();
    }


    private void AtLeastOneTaskCheck()
    {
        if (_loadMetadata.ProcessTasks.All(p => p.IsDisabled))
            _notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"There are no ProcessTasks defined for '{_loadMetadata}'",
                    CheckResult.Fail));
    }

    public void RemoveTablesFromDatabase(IEnumerable<string> tableNames, DiscoveredDatabase dbInfo)
    {
        if (!IsNukable(dbInfo))
            throw new Exception(
                "This method loops through every table in a database and nukes it! for obvious reasons this is only allowed on databases with a suffix _STAGING/_RAW");

        foreach (var tableName in tableNames)
            dbInfo.ExpectTable(tableName).Drop();
    }

    private static bool IsNukable(DiscoveredDatabase dbInfo) =>
        dbInfo.GetRuntimeName().EndsWith("_STAGING", StringComparison.CurrentCultureIgnoreCase) ||
        dbInfo.GetRuntimeName().EndsWith("_RAW", StringComparison.CurrentCultureIgnoreCase);
}