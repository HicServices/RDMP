// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Alter;

/// <summary>
///     Creates a primary key on the live database table based on one or more columns
/// </summary>
public class ExecuteCommandAlterTableCreatePrimaryKey : AlterTableCommandExecution
{
    private readonly ColumnInfo[] _columnInfos;

    [UseWithCommandLine(ParameterHelpList = "<TableInfo> <ColumnInfo1> <ColumnInfo2> etc",
        ParameterHelpBreakdown = @"TableInfo    The table you want to create a primary key on e.g. TableInfo:*Biochem*
ColumnInfos List of columns that should form the primary key (1 for simple primary key, 2+ for composite primary key)")]
    public ExecuteCommandAlterTableCreatePrimaryKey(IBasicActivateItems activator, CommandLineObjectPicker picker) :
        this(activator, (TableInfo)picker[0].GetValueForParameterOfType(typeof(TableInfo)))
    {
        if (IsImpossible)
            return;

        var pick = new List<ColumnInfo>();
        for (var i = 1; i < picker.Length; i++)
            pick.Add((ColumnInfo)picker[i].GetValueForParameterOfType(typeof(ColumnInfo)));

        _columnInfos = pick.ToArray();
    }

    public ExecuteCommandAlterTableCreatePrimaryKey(IBasicActivateItems activator, TableInfo tableInfo) : base(
        activator, tableInfo)
    {
        if (IsImpossible)
            return;

        if (Table.DiscoverColumns().Any(static c => c.IsPrimaryKey))
            SetImpossible("Table already has a primary key, try synchronizing the TableInfo");
    }

    public override void Execute()
    {
        base.Execute();

        Synchronize();

        var cols = _columnInfos;

        if (cols == null && SelectMany(TableInfo.ColumnInfos, out var selected))
            cols = selected;

        if (cols == null || cols.Length == 0)
            return;

        using var cts = new CancellationTokenSource();

        var task = Task.Run(() =>
            Table.CreatePrimaryKey(cols.Select(c => c.Discover(DataAccessContext.DataLoad)).ToArray()), cts.Token);

        Wait("Creating Primary Key...", task, cts);

        if (task.IsFaulted)
            ShowException("Create Primary Key Failed", task.Exception);

        Synchronize();

        Publish(TableInfo);
    }
}