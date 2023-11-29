// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.Tests.CommandExecution;

internal class TestExecuteCommandSet : CommandCliTests
{
    [Test]
    public void Test_CatalogueDescription_Normal()
    {
        var cata = new Catalogue(Repository.CatalogueRepository, "Bob");

        GetInvoker().ExecuteCommand(typeof(ExecuteCommandSet), new CommandLineObjectPicker(new[]
        {
            $"Catalogue:{cata.ID}", "Description", "Some long description"
        }, GetActivator()));

        cata.RevertToDatabaseState();
        Assert.That(cata.Description, Is.EqualTo("Some long description"));
    }

    [Test]
    public void Test_CatalogueDescription_Null()
    {
        var cata = new Catalogue(Repository.CatalogueRepository, "Bob")
        {
            Description = "something cool"
        };
        cata.SaveToDatabase();

        GetInvoker().ExecuteCommand(typeof(ExecuteCommandSet), new CommandLineObjectPicker(new[]
        {
            $"Catalogue:{cata.ID}", "Description", "NULL"
        }, GetActivator()));

        cata.RevertToDatabaseState();
        Assert.That(cata.Description, Is.Null);
    }

    [Test]
    public void TestExecuteCommandSet_SetArrayValueFromCLI()
    {
        var pta = WhenIHaveA<ProcessTaskArgument>();
        pta.SetType(typeof(TableInfo[]));
        pta.Name = "TablesToIsolate";
        pta.SaveToDatabase();

        var t1 = WhenIHaveA<TableInfo>();
        var t2 = WhenIHaveA<TableInfo>();
        var t3 = WhenIHaveA<TableInfo>();
        var t4 = WhenIHaveA<TableInfo>();

        var ids = $"{t1.ID},{t2.ID},{t3.ID},{t4.ID}";

        Assert.That(pta.Value, Is.Null);
        Assert.That(pta.GetValueAsSystemType(), Is.Null);

        GetInvoker().ExecuteCommand(typeof(ExecuteCommandSet),
            new CommandLineObjectPicker(new[] { "ProcessTaskArgument:TablesToIsolate", "Value", ids }, GetActivator()));

        Assert.That(pta.Value, Is.EqualTo(ids));

        Assert.That((TableInfo[])pta.GetValueAsSystemType(), Does.Contain(t1));
        Assert.That((TableInfo[])pta.GetValueAsSystemType(), Does.Contain(t2));
        Assert.That((TableInfo[])pta.GetValueAsSystemType(), Does.Contain(t3));
        Assert.That((TableInfo[])pta.GetValueAsSystemType(), Does.Contain(t4));
    }
}