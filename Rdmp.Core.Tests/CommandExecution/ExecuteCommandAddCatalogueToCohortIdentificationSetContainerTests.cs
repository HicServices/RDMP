// Copyright (c) The University of Dundee 2026-2026
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

public class ExecuteCommandAddCatalogueToCohortIdentificationSetContainerTests : UnitTests
{
    [Test]
    public void TheChosenIdentifierColumnIsUsedWhenTheCatalogueHasTwo()
    {
        var cic = WhenIHaveA<CohortIdentificationConfiguration>();
        cic.CreateRootContainerIfNotExists();
        var catalogue = new Catalogue(Repository, "Biochemistry");
        var table = WhenIHaveA<TableInfo>();
        MakeIdentifier(Repository, catalogue, table, "chi");
        var chosen = MakeIdentifier(Repository, catalogue, table, "chi_at_test");

        Assert.That(chosen.CatalogueItem_ID, Is.Not.EqualTo(catalogue.ID),
            "The catalogue item id must differ from the catalogue id, or the test cannot show the check is right.");

        var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(
            new ThrowImmediatelyActivator(RepositoryLocator), cic.RootCohortAggregateContainer, catalogue, chosen);

        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);

        cmd.Execute();

        Assert.That(cmd.AggregateCreatedIfAny.AggregateDimensions.Single().ExtractionInformation_ID,
            Is.EqualTo(chosen.ID), "The new set should use the identifier column that was chosen.");
    }

    [Test]
    public void AColumnOfAnotherCatalogueIsRefusedWhenItsItemIdEqualsTheCatalogueId()
    {
        var repository = new IdChoosingRepository();
        var cic = new CohortIdentificationConfiguration(repository, "My cic");
        cic.CreateRootContainerIfNotExists();
        var table = WhenIHaveA<TableInfo>(repository);

        var other = new Catalogue(repository, "Haematology");
        var foreign = MakeIdentifier(repository, other, table, "chi");
        var target = repository.WithNextId(foreign.CatalogueItem_ID, () => new Catalogue(repository, "Biochemistry"));
        MakeIdentifier(repository, target, table, "patient");

        Assert.Multiple(() =>
        {
            Assert.That(foreign.CatalogueItem_ID, Is.EqualTo(target.ID),
                "The column's catalogue item id must equal the target catalogue's id.");
            Assert.That(foreign.CatalogueItem.Catalogue_ID, Is.Not.EqualTo(target.ID),
                "The column must belong to another catalogue.");
        });

        var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(
            new ThrowImmediatelyActivator(new RepositoryProvider(repository)), cic.RootCohortAggregateContainer,
            target, foreign);

        Assert.That(cmd.IsImpossible, "A column of another catalogue should be refused.");
        Assert.That(cmd.ReasonCommandImpossible, Does.Contain("is not from the same Catalogue"));
    }

    private static ExtractionInformation MakeIdentifier(MemoryDataExportRepository repository, Catalogue catalogue,
        TableInfo table, string name)
    {
        var column = new ColumnInfo(repository, name, "varchar(10)", table);
        var item = new CatalogueItem(repository, catalogue, name);
        var extractionInformation = new ExtractionInformation(repository, item, column, name)
        {
            IsExtractionIdentifier = true
        };
        extractionInformation.SaveToDatabase();
        return extractionInformation;
    }

    /// <summary>
    /// Gives one new object a chosen ID. The memory repository numbers all objects from one counter, so a catalogue
    /// never shares its ID with a catalogue item unless the test arranges it.
    /// </summary>
    private class IdChoosingRepository : MemoryDataExportRepository
    {
        public T WithNextId<T>(int id, Func<T> create)
        {
            var next = NextObjectId;
            NextObjectId = id - 1;
            try
            {
                return create();
            }
            finally
            {
                NextObjectId = next;
            }
        }
    }
}
