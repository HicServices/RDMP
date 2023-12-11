// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Data.SqlClient;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

internal class HousekeepingTests : DatabaseTests
{
    [Test]
    public void TestCheckUpdateTrigger()
    {
        // set SetUp a test database
        const string tableName = "TestTable";
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);

        var databaseName = db.GetRuntimeName();
        var table = db.CreateTable(tableName, new[] { new DatabaseColumnRequest("Id", "int") });

        var server = db.Server;
        using (var con = server.GetConnection())
        {
            con.Open();
            var cmd = server.GetCommand(
                $"CREATE TRIGGER dbo.[TestTable_OnUpdate] ON [dbo].[{tableName}] AFTER DELETE AS RAISERROR('MESSAGE',16,10)",
                con);

            cmd.ExecuteNonQuery();
        }

        var dbInfo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(databaseName);

        var factory = new TriggerImplementerFactory(dbInfo.Server.DatabaseType);

        var triggerImplementer = factory.Create(table);
        var isEnabled = triggerImplementer.GetTriggerStatus();
        Assert.That(isEnabled, Is.EqualTo(TriggerStatus.Enabled));


        // disable the trigger and test correct reporting
        using (var con = new SqlConnection(dbInfo.Server.Builder.ConnectionString))
        {
            con.Open();
            var cmd =
                new SqlCommand(
                    $"USE [{databaseName}]; DISABLE TRIGGER TestTable_OnUpdate ON [{databaseName}]..[{tableName}]",
                    con);
            cmd.ExecuteNonQuery();
        }

        isEnabled = triggerImplementer.GetTriggerStatus();
        Assert.That(isEnabled, Is.EqualTo(TriggerStatus.Disabled));
    }
}