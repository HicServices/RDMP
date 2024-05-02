// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Alter;

/// <summary>
///     Removes exact duplication from the given table
/// </summary>
public class ExecuteCommandAlterTableMakeDistinct : AlterTableCommandExecution
{
    public int Timeout { get; }
    public bool NoWarn { get; }

    public ExecuteCommandAlterTableMakeDistinct(IBasicActivateItems activator,
        [DemandsInitialization("Table to remove exact duplicates from")]
        ITableInfo tableInfo,
        [DemandsInitialization(
            "The number of seconds to allow for the make distinct command to run.  Defaults to 6000s",
            DefaultValue = 6000)]
        int timeout = 6000,
        [DemandsInitialization("True to carry out the command without warning the user first")]
        bool noWarn = false) : base(activator, tableInfo)
    {
        Timeout = timeout;
        NoWarn = noWarn;

        if (TableInfo.ColumnInfos.Any(c => c.IsPrimaryKey))
            SetImpossible("Table has primary key so cannot have exact duplication");
    }

    public override void Execute()
    {
        base.Execute();

        if (Table.DiscoverColumns().Any(c => c.IsPrimaryKey))
            throw new Exception($"Table '{Table}' has primary key columns so cannot contain duplication");

        if (!NoWarn)
            if (!BasicActivator.YesNo(
                    "Make Distinct requires re-creating the table data which may affect indexes or fail if user permissions are missing (e.g. create table).  Do you want to continue?",
                    $"Make distinct '{Table}'"))
                return;

        using var cts = new CancellationTokenSource();

        var task = Task.Run(() =>
            Table.MakeDistinct(Timeout), cts.Token);

        Wait("Making Distinct...", task, cts);

        if (task.IsFaulted)
            ShowException("Make Distinct Failed", task.Exception);
    }
}