// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;

namespace Rdmp.Core.Tests.CommandExecution;

internal class TestExecuteCommandImportTableInfo : CommandCliTests
{
    [Test]
    public void Test_ImportTableInfo_NoArguments()
    {
        var ex = Assert.Throws<Exception>(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandImportTableInfo),
            new CommandLineObjectPicker(Array.Empty<string>(), GetActivator())));

        Assert.That(
            ex.Message, Does.StartWith("Expected parameter at index 0 to be a FAnsi.Discovery.DiscoveredTable (for parameter 'table') but it was Missing"));
    }

    [Test]
    public void Test_ImportTableInfo_MalformedArgument()
    {
        var ex = Assert.Throws<Exception>(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandImportTableInfo),
            new CommandLineObjectPicker(new string[] { "MyTable" }, GetActivator())));

        Assert.That(
            ex.Message, Does.StartWith("Expected parameter at index 0 to be a FAnsi.Discovery.DiscoveredTable (for parameter 'table') but it was MyTable"));
    }

    [Test]
    public void Test_ImportTableInfo_NoTable()
    {
        const string tbl = "Table:MyTable:DatabaseType:MicrosoftSQLServer:Server=myServerAddress;Database=myDataBase;Trusted_Connection=True";

        var ex = Assert.Throws<Exception>(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandImportTableInfo),
            new CommandLineObjectPicker(new string[] { tbl, "true" }, GetActivator())));

        Assert.That(ex.Message, Does.StartWith("Could not reach server myServerAddress"));
    }
}