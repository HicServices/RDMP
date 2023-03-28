// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

class TestExecuteCommandNewObject : CommandCliTests
{
    [Test]
    public void Test_NewObjectCommand_NoArguments()
    {
            
        var ex = Assert.Throws<Exception>(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),
            new CommandLineObjectPicker(Array.Empty<string>(), GetActivator())));

        StringAssert.StartsWith("First parameter must be a Type",ex.Message);
    }

    [Test]
    public void Test_NewObjectCommand_NonExistentTypeArgument()
    {
        var ex = Assert.Throws<Exception>(() =>  GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),
            new CommandLineObjectPicker(new[]{"Fissdlkfldfj"}, GetActivator())));

        StringAssert.StartsWith("First parameter must be a Type",ex.Message);
    }

    [Test]
    public void Test_NewObjectCommand_WrongTypeArgument()
    {
        var picker = new CommandLineObjectPicker(new[] {"UnitTests"}, GetActivator());
        Assert.AreEqual(typeof(UnitTests),picker[0].Type);

        var ex = Assert.Throws<Exception>(() =>  GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),picker));

        StringAssert.StartsWith("Type must be derived from DatabaseEntity",ex.Message);
    }

    [Test]
    public void Test_NewObjectCommand_MissingNameArgument()
    {
        var picker = new CommandLineObjectPicker(new[] {"Catalogue"}, GetActivator());
        Assert.AreEqual(typeof(Catalogue),picker[0].Type);

        var ex = Assert.Throws<ArgumentException>(() =>  GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),picker));

        StringAssert.StartsWith("Value needed for parameter 'name' (of type 'System.String')",ex.Message);
    }

    [Test]
    public void Test_NewObjectCommand_Success()
    {
        var picker = new CommandLineObjectPicker(new[] {"Catalogue","lolzeeeyeahyeah"}, GetActivator());
        Assert.AreEqual(typeof(Catalogue),picker[0].Type);

        Assert.DoesNotThrow(() =>  GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),picker));
            
        Assert.Contains("lolzeeeyeahyeah",RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().Select(c=>c.Name).ToArray());
    }
}