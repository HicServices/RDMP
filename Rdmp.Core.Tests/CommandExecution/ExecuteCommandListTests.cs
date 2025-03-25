// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Tests.CommandExecution;

internal class TestsExecuteCommandList : CommandCliTests
{
    [Test]
    public void Test_ExecuteCommandList_NoCataloguesParsing()
    {
        foreach (var cat in RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>())
            cat.DeleteInDatabase();

        Assert.That(RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>(), Is.Empty,
            "Failed to clear CatalogueRepository");

        GetInvoker().ExecuteCommand(typeof(ExecuteCommandList),
            new CommandLineObjectPicker(new[] { "Catalogue" }, GetActivator()));
    }

    [Test]
    public void Test_ExecuteCommandList_OneCatalogueParsing()
    {
        var c = WhenIHaveA<Catalogue>();

        GetInvoker().ExecuteCommand(typeof(ExecuteCommandList),
            new CommandLineObjectPicker(new[] { "Catalogue" }, GetActivator()));

        c.DeleteInDatabase();
    }

    [Test]
    public void Test_ExecuteCommandList_OneCatalogue()
    {
        var c = WhenIHaveA<Catalogue>();
        c.Name = "fff";
        c.SaveToDatabase();

        var mock = GetMockActivator();

        var cmd = new ExecuteCommandList(mock, new[] { c });
        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);

        cmd.Execute();

        Regex.Escape($"{c.ID}:fff");

        // Called once
        mock.Received(1).Show(Arg.Is<string>(i => i.Contains($"{c.ID}:fff")));

        c.DeleteInDatabase();
    }
}