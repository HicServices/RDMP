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

class PickTableTests : UnitTests
{

    [Test]
    public void TestPickTable()
    {
        var pick = new PickTable();
        var result = pick.Parse(@"Table:v_cool:Schema:dbo:IsView:True:DatabaseType:MicrosoftSQLServer:Name:MyDb:Server=localhost\sqlexpress;Trusted_Connection=True;",0);

        Assert.IsNotNull(result.Table);
            
        Assert.AreEqual(TableType.View,result.Table.TableType);
        Assert.AreEqual("dbo",result.Table.Schema);

        Assert.AreEqual("v_cool",result.Table.GetRuntimeName());
        Assert.AreEqual("MyDb",result.Table.Database.GetRuntimeName());
        Assert.AreEqual("localhost\\sqlexpress",result.Table.Database.Server.Name);
        Assert.AreEqual(DatabaseType.MicrosoftSQLServer,result.Table.Database.Server.DatabaseType);
        Assert.IsNull(result.Table.Database.Server.ExplicitPasswordIfAny);
        Assert.IsNull(result.Table.Database.Server.ExplicitUsernameIfAny);
    }
}