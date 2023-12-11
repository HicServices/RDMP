// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data.Cohort;
using System.Linq;

namespace Rdmp.Core.Tests.CohortCreation;

internal class CohortIdentificationConfigurationMergerTests : CohortIdentificationTests
{
    [Test]
    public void TestSimpleMerge()
    {
        var merger = new CohortIdentificationConfigurationMerger(CatalogueRepository);

        var cic1 = new CohortIdentificationConfiguration(CatalogueRepository, "cic1");
        var cic2 = new CohortIdentificationConfiguration(CatalogueRepository, "cic2");

        cic1.CreateRootContainerIfNotExists();
        var root1 = cic1.RootCohortAggregateContainer;
        root1.Name = "Root1";
        root1.SaveToDatabase();
        root1.AddChild(aggregate1, 1);

        cic2.CreateRootContainerIfNotExists();
        var root2 = cic2.RootCohortAggregateContainer;
        root2.Name = "Root2";
        root2.SaveToDatabase();
        root2.AddChild(aggregate2, 2);

        Assert.Multiple(() =>
        {
            Assert.That(cic1.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively(), Has.Count.EqualTo(1));
            Assert.That(cic2.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively(), Has.Count.EqualTo(1));
        });

        var numberOfCicsBefore = CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().Length;

        var result = merger.Merge(new[] { cic1, cic2 }, SetOperation.UNION);

        Assert.Multiple(() =>
        {
            //original should still be intact
            Assert.That(cic1.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively(), Has.Count.EqualTo(1));
            Assert.That(cic2.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively(), Has.Count.EqualTo(1));

            //the new merged set should contain both
            Assert.That(result.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively(), Has.Count.EqualTo(2));
        });

        Assert.Multiple(() =>
        {
            Assert.That(
                    result.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively()
                        .Any(c => c.Equals(aggregate1)), Is.False,
                    "Expected the merge to include clone aggregates not the originals! (aggregate1)");
            Assert.That(
                result.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively()
                    .Any(c => c.Equals(aggregate2)), Is.False,
                "Expected the merge to include clone aggregates not the originals! (aggregate2)");

            // Now should be a new one
            Assert.That(CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>(), Has.Length.EqualTo(numberOfCicsBefore + 1));
        });

        var newCicId = result.ID;

        // Should have the root containers of the old configs
        var newRoot2 = result.RootCohortAggregateContainer.GetSubContainers().Single(c => c.Name.Equals("Root2"));
        var newRoot1 = result.RootCohortAggregateContainer.GetSubContainers().Single(c => c.Name.Equals("Root1"));
        Assert.Multiple(() =>
        {
            Assert.That(result.RootCohortAggregateContainer.GetSubContainers(), Has.Length.EqualTo(2));

            // And should have
            Assert.That(newRoot2.GetAggregateConfigurations()[0].Name, Is.EqualTo($"cic_{newCicId}_UnitTestAggregate2"));
            Assert.That(newRoot1.GetAggregateConfigurations()[0].Name, Is.EqualTo($"cic_{newCicId}_UnitTestAggregate1"));

            Assert.That(result.Name, Is.EqualTo($"Merged cics (IDs {cic1.ID},{cic2.ID})"));

            Assert.That(cic1.Exists());
            Assert.That(cic2.Exists());
        });
    }

    [Test]
    public void TestSimpleUnMerge()
    {
        var merger = new CohortIdentificationConfigurationMerger(CatalogueRepository);

        var cicInput = new CohortIdentificationConfiguration(CatalogueRepository, "cic99");

        cicInput.CreateRootContainerIfNotExists();
        var root = cicInput.RootCohortAggregateContainer;
        root.Name = "Root";
        root.SaveToDatabase();

        var sub1 = new CohortAggregateContainer(CatalogueRepository, SetOperation.INTERSECT)
        {
            Order = 1
        };
        sub1.SaveToDatabase();

        var sub2 = new CohortAggregateContainer(CatalogueRepository, SetOperation.EXCEPT)
        {
            Order = 2
        };
        sub2.SaveToDatabase();

        root.AddChild(sub1);
        root.AddChild(sub2);

        sub1.AddChild(aggregate1, 0);
        sub2.AddChild(aggregate2, 0);
        sub2.AddChild(aggregate3, 1);

        var numberOfCicsBefore = CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().Length;

        var results = merger.UnMerge(root);

        Assert.Multiple(() =>
        {
            // Now should be two new ones
            Assert.That(CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>(), Has.Length.EqualTo(numberOfCicsBefore + 2));
            Assert.That(results, Has.Length.EqualTo(2));
        });

        Assert.Multiple(() =>
        {
            Assert.That(results[0].RootCohortAggregateContainer.Operation, Is.EqualTo(SetOperation.INTERSECT));
            Assert.That(results[0].RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively(), Has.Count.EqualTo(1));
        });

        Assert.Multiple(() =>
        {
            Assert.That(
                    results[0].RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively()
                        .Intersect(new[] { aggregate1, aggregate2, aggregate3 }).Any(), Is.False, "Expected new aggregates to be new!");

            Assert.That(results[1].RootCohortAggregateContainer.Operation, Is.EqualTo(SetOperation.EXCEPT));
            Assert.That(results[1].RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively(), Has.Count.EqualTo(2));
        });

        Assert.That(
            results[1].RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively()
                .Intersect(new[] { aggregate1, aggregate2, aggregate3 }).Any(), Is.False, "Expected new aggregates to be new!");
    }

    [Test]
    public void TestSimpleImportCic()
    {
        var merger = new CohortIdentificationConfigurationMerger(CatalogueRepository);

        var cic1 = new CohortIdentificationConfiguration(CatalogueRepository, "cic1");
        var cic2 = new CohortIdentificationConfiguration(CatalogueRepository, "cic2");

        cic1.CreateRootContainerIfNotExists();
        var root1 = cic1.RootCohortAggregateContainer;
        root1.Name = "Root1";
        root1.SaveToDatabase();
        root1.AddChild(aggregate1, 1);

        cic2.CreateRootContainerIfNotExists();
        var root2 = cic2.RootCohortAggregateContainer;
        root2.Name = "Root2";
        root2.SaveToDatabase();
        root2.AddChild(aggregate2, 2);

        Assert.Multiple(() =>
        {
            Assert.That(cic1.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively(), Has.Count.EqualTo(1));
            Assert.That(cic2.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively(), Has.Count.EqualTo(1));
        });

        var numberOfCicsBefore = CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().Length;

        //import 2 into 1
        merger.Import(new[] { cic2 }, cic1.RootCohortAggregateContainer);

        Assert.Multiple(() =>
        {
            //no new cics
            Assert.That(CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>(), Has.Length.EqualTo(numberOfCicsBefore));

            // cic 1 should now have both aggregates
            Assert.That(cic1.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively(), Has.Count.EqualTo(2));

            Assert.That(cic1.RootCohortAggregateContainer.Name, Is.EqualTo("Root1"));
            Assert.That(cic1.RootCohortAggregateContainer.GetSubContainers()[0].Name, Is.EqualTo("Root2"));
        });
    }
}