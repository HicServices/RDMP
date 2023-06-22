// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands.Alter;
using Rdmp.Core.CommandLine.Interactive;
using System;
using System.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandAlterTableMakeDistinctTests : DatabaseTests
{
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.PostgreSql)]
    public void Test(DatabaseType dbType)
    {
        var db = GetCleanedServer(dbType);

        var dt = new DataTable();
        dt.Columns.Add("fff");
        dt.Rows.Add("1");
        dt.Rows.Add("1");
        dt.Rows.Add("2");
        dt.Rows.Add("2");
        dt.Rows.Add("2");

        var tbl = db.CreateTable("MyTable", dt);

        Import(tbl, out var tblInfo, out _);

        Assert.AreEqual(5, tbl.GetRowCount());

        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet()) { DisallowInput = true };

        var cmd = new ExecuteCommandAlterTableMakeDistinct(activator, tblInfo, 700, true);

        Assert.IsFalse(cmd.IsImpossible, cmd.ReasonCommandImpossible);

        cmd.Execute();

        Assert.AreEqual(2, tbl.GetRowCount());

        tbl.CreatePrimaryKey(tbl.DiscoverColumn("fff"));

        cmd = new ExecuteCommandAlterTableMakeDistinct(activator, tblInfo, 700, true);

        var ex = Assert.Throws<Exception>(() => cmd.Execute());

        Assert.AreEqual("Table 'MyTable' has primary key columns so cannot contain duplication", ex.Message);
    }
}