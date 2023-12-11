// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using System.Linq;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandSetExtendedPropertyTests : CommandCliTests
{
    [Test]
    public void CommandImpossible_BecausePropertyDoesNotExist()
    {
        var c1 = WhenIHaveA<Catalogue>();

        var cmd = new ExecuteCommandSetExtendedProperty(GetMockActivator(), new[] { c1 }, "blarg", "fff");

        Assert.Multiple(() =>
        {
            Assert.That(cmd.IsImpossible);
            Assert.That(cmd.ReasonCommandImpossible, Does.StartWith("blarg is not a known property.  Known properties are:"));
        });
    }

    [Test]
    public void SetIsTemplate_OnMultipleObjects()
    {
        var ac1 = WhenIHaveA<AggregateConfiguration>();
        var ac2 = WhenIHaveA<AggregateConfiguration>();

        Assert.Multiple(() =>
        {
            Assert.That(
                Repository.CatalogueRepository.GetExtendedProperties(ac1), Is.Empty);
            Assert.That(
                Repository.CatalogueRepository.GetExtendedProperties(ac2), Is.Empty);
        });

        var cmd = new ExecuteCommandSetExtendedProperty(GetMockActivator(), new[] { ac1, ac2 },
            ExtendedProperty.IsTemplate, "true");

        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);

        cmd.Execute();

        var declaration1 = Repository.CatalogueRepository.GetExtendedProperties(ac1).Single();
        var declaration2 = Repository.CatalogueRepository.GetExtendedProperties(ac2).Single();

        foreach (var dec in new[] { declaration1, declaration2 })
        {
            Assert.Multiple(() =>
            {
                Assert.That(dec.Name, Is.EqualTo("IsTemplate"));
                Assert.That(dec.Value, Is.EqualTo("true"));
            });
        }

        // now clear that status

        cmd = new ExecuteCommandSetExtendedProperty(GetMockActivator(), new[] { ac1, ac2 },
            ExtendedProperty.IsTemplate, null);

        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);

        cmd.Execute();

        Assert.Multiple(() =>
        {
            // should now be back where we started
            Assert.That(
                Repository.CatalogueRepository.GetExtendedProperties(ac1), Is.Empty);
            Assert.That(
                Repository.CatalogueRepository.GetExtendedProperties(ac2), Is.Empty);
        });
    }
}