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

internal class TestExecuteCommandAssociateCatalogueWithLoadMetadata : CommandCliTests
{
    [Test]
    public void TestExecuteCommandAssociateCatalogueWithLoadMetadata_Simple()
    {
        var cata1 = new Catalogue(RepositoryLocator.CatalogueRepository, "fff");
        var cata2 = new Catalogue(RepositoryLocator.CatalogueRepository, "bbb");

        Assert.Multiple(() =>
        {
            Assert.That(cata1.LoadMetadatas(), Is.Empty);
            Assert.That(cata2.LoadMetadatas(), Is.Empty);
        });

        var lmd = new LoadMetadata(RepositoryLocator.CatalogueRepository, "mylmd");

        GetInvoker().ExecuteCommand(typeof(ExecuteCommandAssociateCatalogueWithLoadMetadata),
            new CommandLineObjectPicker(new[] { $"LoadMetadata:{lmd.ID}", "Catalogue:fff" }, GetActivator()));

        cata1.RevertToDatabaseState();
        cata2.RevertToDatabaseState();

        Assert.Multiple(() =>
        {
            Assert.That(cata1.LoadMetadatas()[0].ID, Is.EqualTo(lmd.ID));
            Assert.That(cata2.LoadMetadatas(), Is.Empty);
        });
    }
}