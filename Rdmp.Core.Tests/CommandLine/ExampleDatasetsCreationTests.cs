// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandLine;

internal class ExampleDatasetsCreationTests : DatabaseTests
{
    /// <summary>
    /// Tests the creation of example datasets during first installation of RDMP or when running "rdmp.exe install [...] -e" from the CLI
    /// </summary>
    [Test]
    public void Test_ExampleDatasetsCreation()
    {
        Assert.Multiple(() =>
        {
            //Should be empty RDMP metadata database
            Assert.That(CatalogueRepository.GetAllObjects<Catalogue>(), Is.Empty);
            Assert.That(CatalogueRepository.GetAllObjects<AggregateConfiguration>(), Is.Empty);
        });

        //create the pipelines
        var pipes = new CataloguePipelinesAndReferencesCreation(RepositoryLocator, null, null);
        pipes.CreatePipelines(new PlatformDatabaseCreationOptions());

        //create all the stuff
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var creator = new ExampleDatasetsCreation(new ThrowImmediatelyActivator(RepositoryLocator), RepositoryLocator);
        creator.Create(db, ThrowImmediatelyCheckNotifier.Quiet,
            new PlatformDatabaseCreationOptions { Seed = 500, DropDatabases = true });

        //should be at least 2 views (marked as view)
        var views = CatalogueRepository.GetAllObjects<TableInfo>().Count(ti => ti.IsView);
        Assert.Multiple(() =>
        {
            Assert.That(views, Is.GreaterThanOrEqualTo(2));

            //should have at least created some catalogues, graphs etc
            Assert.That(CatalogueRepository.GetAllObjects<Catalogue>(), Has.Length.GreaterThanOrEqualTo(4));
            Assert.That(CatalogueRepository.GetAllObjects<AggregateConfiguration>(), Has.Length.GreaterThanOrEqualTo(4));
        });
    }
}