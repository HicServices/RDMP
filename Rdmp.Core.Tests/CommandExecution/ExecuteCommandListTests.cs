// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Moq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using System.Text.RegularExpressions;

namespace Rdmp.Core.Tests.CommandExecution;

internal class TestsExecuteCommandList : CommandCliTests
{
    [Test]
    public void Test_ExecuteCommandList_NoCataloguesParsing()
    {
        foreach(var cat in RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>())
            cat.DeleteInDatabase();

        Assert.IsEmpty(RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>(),"Failed to clear CatalogueRepository");

        GetInvoker().ExecuteCommand(typeof(ExecuteCommandList),
            new CommandLineObjectPicker(new string[]{ "Catalogue"}, GetActivator()));
    }
        
    [Test]
    public void Test_ExecuteCommandList_OneCatalogueParsing()
    {
        var c = WhenIHaveA<Catalogue>();

        GetInvoker().ExecuteCommand(typeof(ExecuteCommandList),
            new CommandLineObjectPicker(new string[]{ "Catalogue"}, GetActivator()));
            
        c.DeleteInDatabase();
    }
    [Test]
    public void Test_ExecuteCommandList_OneCatalogue()
    {
        var c = WhenIHaveA<Catalogue>();
        c.Name = "fff";
        c.SaveToDatabase();

        var mock = GetMockActivator();

        var cmd = new ExecuteCommandList(mock.Object,new []{c});
        Assert.IsFalse(cmd.IsImpossible,cmd.ReasonCommandImpossible);

        cmd.Execute();

        var contents = Regex.Escape($"{c.ID}:fff");

        // Called once
        mock.Verify(m => m.Show(It.IsRegex(contents)), Times.Once());

        c.DeleteInDatabase();
    }
}