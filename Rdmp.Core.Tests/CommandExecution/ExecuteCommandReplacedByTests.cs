// Copyright (c) The University of Dundee 2018-2021
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using System.Linq;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandReplacedByTests : CommandCliTests
{
        [Test]
        public void CommandImpossible_BecauseNotDeprecated()
        {
                var c1 = WhenIHaveA<Catalogue>();
                var c2 = WhenIHaveA<Catalogue>();

                var cmd = new ExecuteCommandReplacedBy(GetMockActivator(), c1, c2);

        Assert.That(cmd.IsImpossible);
        Assert.That(cmd.ReasonCommandImpossible, Does.Contain("is not marked IsDeprecated"));
        }

        [Test]
        public void CommandImpossible_BecauseDifferentTypes()
        {
                var c1 = WhenIHaveA<Catalogue>();
                var ci1 = WhenIHaveA<CatalogueItem>();

                c1.IsDeprecated = true;
                c1.SaveToDatabase();

                var cmd = new ExecuteCommandReplacedBy(GetMockActivator(), c1, ci1);

        Assert.That(cmd.IsImpossible);
        Assert.That(cmd.ReasonCommandImpossible, Does.Contain("because it is a different object Type"));
        }

        [Test]
        public void CommandImpossible_Allowed()
        {
                var c1 = WhenIHaveA<Catalogue>();
                var c2 = WhenIHaveA<Catalogue>();

                c1.IsDeprecated = true;
                c1.SaveToDatabase();

                var cmd = new ExecuteCommandReplacedBy(GetMockActivator(), c1, c2);
        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);

                cmd.Execute();

                var replacement = RepositoryLocator.CatalogueRepository
                    .GetAllObjectsWhere<ExtendedProperty>("Name", ExtendedProperty.ReplacedBy)
                    .Single(r => r.IsReferenceTo(c1));

        Assert.That(replacement.IsReferenceTo(c1));
        Assert.That(replacement.Value, Is.EqualTo(c2.ID.ToString()));

                // running command multiple times shouldn't result in duplicate objects
                cmd.Execute();
                cmd.Execute();
                cmd.Execute();
                cmd.Execute();

        Assert.That(RepositoryLocator.CatalogueRepository
                    .GetAllObjectsWhere<ExtendedProperty>("Name", ExtendedProperty.ReplacedBy)
                    .Count(r => r.IsReferenceTo(c1)), Is.EqualTo(1));

                cmd = new ExecuteCommandReplacedBy(GetMockActivator(), c1, null);
                cmd.Execute();

        Assert.That(RepositoryLocator.CatalogueRepository
                    .GetAllObjectsWhere<ExtendedProperty>("Name", ExtendedProperty.ReplacedBy)
                    .Where(r => r.IsReferenceTo(c1)), Is.Empty);
        }
}