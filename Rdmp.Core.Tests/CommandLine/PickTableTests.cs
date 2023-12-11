// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandLine;

internal class PickTableTests : UnitTests
{
    [Test]
    public void TestPickTable()
    {
        var pick = new PickTable();
        var result =
            pick.Parse(
                @"Table:v_cool:Schema:dbo:IsView:True:DatabaseType:MicrosoftSQLServer:Name:MyDb:Server=localhost\sqlexpress;Trusted_Connection=True;",
                0);

        Assert.That(result.Table, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Table.TableType, Is.EqualTo(TableType.View));
            Assert.That(result.Table.Schema, Is.EqualTo("dbo"));

            Assert.That(result.Table.GetRuntimeName(), Is.EqualTo("v_cool"));
            Assert.That(result.Table.Database.GetRuntimeName(), Is.EqualTo("MyDb"));
            Assert.That(result.Table.Database.Server.Name, Is.EqualTo("localhost\\sqlexpress"));
            Assert.That(result.Table.Database.Server.DatabaseType, Is.EqualTo(DatabaseType.MicrosoftSQLServer));
            Assert.That(result.Table.Database.Server.ExplicitPasswordIfAny, Is.Null);
            Assert.That(result.Table.Database.Server.ExplicitUsernameIfAny, Is.Null);
        });
    }
}