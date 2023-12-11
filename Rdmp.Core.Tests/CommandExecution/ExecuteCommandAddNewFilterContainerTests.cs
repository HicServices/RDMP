// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Aggregation;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

public class ExecuteCommandAddNewFilterContainerTests : UnitTests
{
    [Test]
    public void TestNormalCase()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();
        var cmd = new ExecuteCommandAddNewFilterContainer(new ThrowImmediatelyActivator(RepositoryLocator), ac);

        Assert.Multiple(() =>
        {
            Assert.That(ac.RootFilterContainer_ID, Is.Null);

            Assert.That(cmd.ReasonCommandImpossible, Is.Null);
            Assert.That(cmd.IsImpossible, Is.False);
        });

        cmd.Execute();

        Assert.That(ac.RootFilterContainer_ID, Is.Not.Null);
    }

    [Test]
    public void Impossible_BecauseAlreadyHasContainer()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();

        ac.CreateRootContainerIfNotExists();
        Assert.That(ac.RootFilterContainer_ID, Is.Not.Null);

        var cmd = new ExecuteCommandAddNewFilterContainer(new ThrowImmediatelyActivator(RepositoryLocator), ac);

        Assert.Multiple(() =>
        {
            Assert.That(cmd.ReasonCommandImpossible, Is.EqualTo("There is already a root filter container on this object"));
            Assert.That(cmd.IsImpossible);
        });
    }

    [Test]
    public void Impossible_BecauseAPI()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();

        var c = ac.Catalogue;
        c.Name = $"{PluginCohortCompiler.ApiPrefix}MyAwesomeAPI";
        c.SaveToDatabase();

        Assert.That(c.IsApiCall());

        var cmd = new ExecuteCommandAddNewFilterContainer(new ThrowImmediatelyActivator(RepositoryLocator), ac);

        Assert.Multiple(() =>
        {
            Assert.That(cmd.ReasonCommandImpossible, Is.EqualTo("Filters cannot be added to API calls"));
            Assert.That(cmd.IsImpossible);
        });
    }
}