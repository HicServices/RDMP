// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Mutilators;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

internal class PrematureLoadEnderTests : DatabaseTests
{
    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    public void TestEndLoadBecause_NoTables(DatabaseType type)
    {
        var database = GetCleanedServer(type);

        Assert.AreEqual(0, database.DiscoverTables(false).Length);

        var ender = new PrematureLoadEnder
        {
            ConditionsToTerminateUnder = PrematureLoadEndCondition.NoRecordsInAnyTablesInDatabase,
            ExitCodeToReturnIfConditionMet = ExitCodeType.OperationNotRequired
        };

        ender.Initialize(database, LoadStage.AdjustRaw);

        Assert.AreEqual(ExitCodeType.OperationNotRequired, ender.Mutilate(new ThrowImmediatelyDataLoadJob()));
    }

    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    public void TestEndLoadBecause_NoRows(DatabaseType type)
    {
        var database = GetCleanedServer(type);

        var dt = new DataTable();
        dt.Columns.Add("Fish");

        database.CreateTable("MyTable", dt);
        var ender = new PrematureLoadEnder
        {
            ConditionsToTerminateUnder = PrematureLoadEndCondition.NoRecordsInAnyTablesInDatabase,
            ExitCodeToReturnIfConditionMet = ExitCodeType.OperationNotRequired
        };

        ender.Initialize(database, LoadStage.AdjustRaw);

        Assert.AreEqual(ExitCodeType.OperationNotRequired, ender.Mutilate(new ThrowImmediatelyDataLoadJob()));
    }

    [TestCase(DatabaseType.MySql)]
    [TestCase(DatabaseType.MicrosoftSQLServer)]
    public void TestNoEnd_BecauseRows(DatabaseType type)
    {
        var database = GetCleanedServer(type);

        var dt = new DataTable();
        dt.Columns.Add("Fish");
        dt.Rows.Add("myval");

        database.CreateTable("MyTable", dt);
        var ender = new PrematureLoadEnder
        {
            ConditionsToTerminateUnder = PrematureLoadEndCondition.NoRecordsInAnyTablesInDatabase,
            ExitCodeToReturnIfConditionMet = ExitCodeType.OperationNotRequired
        };

        ender.Initialize(database, LoadStage.AdjustRaw);

        Assert.AreEqual(ExitCodeType.Success, ender.Mutilate(new ThrowImmediatelyDataLoadJob()));
    }
}