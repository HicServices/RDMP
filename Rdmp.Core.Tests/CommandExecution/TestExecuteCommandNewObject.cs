// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

internal class TestExecuteCommandNewObject : CommandCliTests
{
    [Test]
    public void Test_NewObjectCommand_NoArguments()
    {
        var ex = Assert.Throws<Exception>(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),
            new CommandLineObjectPicker(Array.Empty<string>(), GetActivator())));

        Assert.That(ex.Message, Does.StartWith("First parameter must be a Type"));
    }

    [Test]
    public void Test_NewObjectCommand_NonExistentTypeArgument()
    {
        var ex = Assert.Throws<Exception>(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),
            new CommandLineObjectPicker(new[] { "Fissdlkfldfj" }, GetActivator())));

        Assert.That(ex.Message, Does.StartWith("First parameter must be a Type"));
    }

    [Test]
    public void Test_NewObjectCommand_WrongTypeArgument()
    {
        var picker = new CommandLineObjectPicker(new[] { "UnitTests" }, GetActivator());
        Assert.That(picker[0].Type, Is.EqualTo(typeof(UnitTests)));

        var ex = Assert.Throws<Exception>(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject), picker));

        Assert.That(ex.Message, Does.StartWith("Type must be derived from DatabaseEntity"));
    }

    [Test]
    public void Test_NewObjectCommand_MissingNameArgument()
    {
        var picker = new CommandLineObjectPicker(new[] { "Catalogue" }, GetActivator());
        Assert.That(picker[0].Type, Is.EqualTo(typeof(Catalogue)));

        var ex = Assert.Throws<ArgumentException>(() =>
            GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject), picker));

        Assert.That(ex.Message, Does.StartWith("Value needed for parameter 'name' (of type 'System.String')"));
    }

    [Test]
    public void Test_NewObjectCommand_Success()
    {
        var picker = new CommandLineObjectPicker(new[] { "Catalogue", "lolzeeeyeahyeah" }, GetActivator());
        Assert.That(picker[0].Type, Is.EqualTo(typeof(Catalogue)));

        Assert.DoesNotThrow(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject), picker));

        Assert.That(RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().Select(c => c.Name).ToArray(), Does.Contain("lolzeeeyeahyeah"));
    }
}