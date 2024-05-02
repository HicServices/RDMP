// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.DataLoad.Triggers.Exceptions;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataLoad.Triggers;

/// <summary>
///     Checks that the specified table has a backup trigger <see cref="ITriggerImplementer" /> on it.  Also inspects the
///     _Archive table schema and compares it
///     to the live schema to make sure they are compatible.
/// </summary>
public class TriggerChecks : ICheckable
{
    private readonly DiscoveredTable _table;
    private readonly DiscoveredTable _archiveTable;
    private readonly DiscoveredServer _server;

    public bool TriggerCreated { get; private set; }

    public TriggerChecks(DiscoveredTable table)
    {
        _server = table.Database.Server;
        _table = table;
        _archiveTable = table.Database.ExpectTable($"{_table.GetRuntimeName()}_Archive", _table.Schema);
    }

    /// <inheritdoc />
    public void Check(ICheckNotifier notifier)
    {
        if (_table.Exists() && _archiveTable.Exists())
        {
            var liveCols = _table.DiscoverColumns().Select(c => c.GetRuntimeName()).ToArray();
            var archiveCols = _archiveTable.DiscoverColumns().Select(c => c.GetRuntimeName()).ToArray();

            var passed = CheckColumnOrderInTablesAndArchiveMatch(liveCols, archiveCols, notifier);

            if (!passed)
                return;
        }

        var factory = new TriggerImplementerFactory(_server.DatabaseType);
        var implementer = factory.Create(_table);

        bool present;

        var primaryKeys = _table.DiscoverColumns().Where(c => c.IsPrimaryKey).ToArray();

        //we don't know the primary keys
        if (!primaryKeys.Any())
            try
            {
                //see if it exists
                present = implementer.GetTriggerStatus() == TriggerStatus.Enabled;
            }
            catch (TriggerMissingException)
            {
                //clearly it doesnt exist
                present = false;
            }
        else
            try
            {
                //we do know the primary keys
                present = implementer.CheckUpdateTriggerIsEnabledAndHasExpectedBody();
            }
            catch (IrreconcilableColumnDifferencesInArchiveException e)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Archive table for table {_table} is corrupt, see inner Exception for specific errors",
                        CheckResult.Fail, e));
                return;
            }
            catch (Exception e)
            {
                NotifyFail(e, notifier, implementer);
                return;
            }

        if (present)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Trigger presence/intactness for table {_table} matched expected presence", CheckResult.Success));
        else
            NotifyFail(null, notifier, implementer); //try creating it
    }


    private bool CheckColumnOrderInTablesAndArchiveMatch(string[] liveCols, string[] archiveCols,
        ICheckNotifier notifier)
    {
        var passed = true;

        foreach (var requiredArchiveColumns in new[] { "hic_validTo", "hic_userID", "hic_status" })
            if (!archiveCols.Any(c => c.Equals(requiredArchiveColumns, StringComparison.CurrentCultureIgnoreCase)))
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Column {requiredArchiveColumns} was not found in {_archiveTable}",
                        CheckResult.Fail));
                passed = false;
            }


        foreach (var missingColumn in liveCols.Except(archiveCols))
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Column {missingColumn} in table {_table} was not found in the  _Archive table",
                CheckResult.Fail));
            passed = false;
        }

        return passed;
    }

    private void NotifyFail(Exception e, ICheckNotifier notifier, ITriggerImplementer microsoftSQLTrigger)
    {
        var shouldCreate = notifier.OnCheckPerformed(new CheckEventArgs(
            $"Trigger error encountered when checking integrity of table {_table}",
            CheckResult.Warning, e, $"Drop and then Re-Create Trigger on table {_table}"));

        if (!shouldCreate) return;

        microsoftSQLTrigger.DropTrigger(out var problemsDroppingTrigger, out var thingsThatWorkedDroppingTrigger);

        if (!string.IsNullOrWhiteSpace(thingsThatWorkedDroppingTrigger))
            notifier.OnCheckPerformed(new CheckEventArgs(thingsThatWorkedDroppingTrigger, CheckResult.Success));

        if (!string.IsNullOrWhiteSpace(problemsDroppingTrigger))
            notifier.OnCheckPerformed(new CheckEventArgs(problemsDroppingTrigger, CheckResult.Warning));

        microsoftSQLTrigger.CreateTrigger(notifier);
        TriggerCreated = true;
    }
}