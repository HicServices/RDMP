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

        var cic1 = new CohortIdentificationConfiguration(CatalogueRepository,"cic1");
        var cic2 = new CohortIdentificationConfiguration(CatalogueRepository,"cic2");

        cic1.CreateRootContainerIfNotExists();
        var root1 = cic1.RootCohortAggregateContainer;
        root1.Name = "Root1";
        root1.SaveToDatabase();
        root1.AddChild(aggregate1,1);

        cic2.CreateRootContainerIfNotExists();
        var root2 = cic2.RootCohortAggregateContainer;
        root2.Name = "Root2";
        root2.SaveToDatabase();
        root2.AddChild(aggregate2,2);

        Assert.AreEqual(1,cic1.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);
        Assert.AreEqual(1,cic2.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);
            
        var numberOfCicsBefore = CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().Count();

        var result = merger.Merge(new []{cic1,cic2 },SetOperation.UNION);

        //original should still be intact
        Assert.AreEqual(1,cic1.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);
        Assert.AreEqual(1,cic2.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);

        //the new merged set should contain both
        Assert.AreEqual(2,result.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);

        Assert.IsFalse(result.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Any(c=>c.Equals(aggregate1)),"Expected the merge to include clone aggregates not the originals! (aggregate1)");
        Assert.IsFalse(result.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Any(c=>c.Equals(aggregate2)),"Expected the merge to include clone aggregates not the originals! (aggregate2)");

        // Now should be a new one
        Assert.AreEqual(numberOfCicsBefore + 1,CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().Count());

        var newCicId = result.ID;

        // Should have the root containers of the old configs
        var newRoot2 = result.RootCohortAggregateContainer.GetSubContainers().Single(c => c.Name.Equals("Root2"));
        var newRoot1 = result.RootCohortAggregateContainer.GetSubContainers().Single(c => c.Name.Equals("Root1"));
        Assert.AreEqual(2, result.RootCohortAggregateContainer.GetSubContainers().Length);

        // And should have
        Assert.AreEqual($"cic_{newCicId}_UnitTestAggregate2", newRoot2.GetAggregateConfigurations()[0].Name);
        Assert.AreEqual($"cic_{newCicId}_UnitTestAggregate1",newRoot1.GetAggregateConfigurations()[0].Name);

        Assert.AreEqual($"Merged cics (IDs {cic1.ID},{cic2.ID})",result.Name);

        Assert.IsTrue(cic1.Exists());
        Assert.IsTrue(cic2.Exists());

    }

    [Test]
    public void TestSimpleUnMerge()
    {
        var merger = new CohortIdentificationConfigurationMerger(CatalogueRepository);

        var cicInput = new CohortIdentificationConfiguration(CatalogueRepository,"cic99");

        cicInput.CreateRootContainerIfNotExists();
        var root = cicInput.RootCohortAggregateContainer;
        root.Name = "Root";
        root.SaveToDatabase();

        var sub1 = new CohortAggregateContainer(CatalogueRepository,SetOperation.INTERSECT)
        {
            Order = 1
        };
        sub1.SaveToDatabase();

        var sub2 = new CohortAggregateContainer(CatalogueRepository,SetOperation.EXCEPT)
        {
            Order = 2
        };
        sub2.SaveToDatabase();

        root.AddChild(sub1);
        root.AddChild(sub2);

        sub1.AddChild(aggregate1,0);
        sub2.AddChild(aggregate2,0);
        sub2.AddChild(aggregate3,1);
            
        var numberOfCicsBefore = CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().Count();

        var results = merger.UnMerge(root);

        // Now should be two new ones
        Assert.AreEqual(numberOfCicsBefore + 2,CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().Count());
        Assert.AreEqual(2,results.Length);

        Assert.AreEqual(SetOperation.INTERSECT,results[0].RootCohortAggregateContainer.Operation);
        Assert.AreEqual(1,results[0].RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);
            
        Assert.IsFalse(results[0].RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Intersect(new []{ aggregate1,aggregate2,aggregate3}).Any(),"Expected new aggregates to be new!");

        Assert.AreEqual(SetOperation.EXCEPT,results[1].RootCohortAggregateContainer.Operation);
        Assert.AreEqual(2,results[1].RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);

        Assert.IsFalse(results[1].RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Intersect(new []{ aggregate1,aggregate2,aggregate3}).Any(),"Expected new aggregates to be new!");

    }

    [Test]
    public void TestSimpleImportCic()
    {
        var merger = new CohortIdentificationConfigurationMerger(CatalogueRepository);

        var cic1 = new CohortIdentificationConfiguration(CatalogueRepository,"cic1");
        var cic2 = new CohortIdentificationConfiguration(CatalogueRepository,"cic2");

        cic1.CreateRootContainerIfNotExists();
        var root1 = cic1.RootCohortAggregateContainer;
        root1.Name = "Root1";
        root1.SaveToDatabase();
        root1.AddChild(aggregate1,1);

        cic2.CreateRootContainerIfNotExists();
        var root2 = cic2.RootCohortAggregateContainer;
        root2.Name = "Root2";
        root2.SaveToDatabase();
        root2.AddChild(aggregate2,2);

        Assert.AreEqual(1,cic1.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);
        Assert.AreEqual(1,cic2.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);
            
        var numberOfCicsBefore = CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().Count();

        //import 2 into 1
        merger.Import(new []{cic2 },cic1.RootCohortAggregateContainer);

        //no new cics
        Assert.AreEqual(numberOfCicsBefore,CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().Count());

        // cic 1 should now have both aggregates
        Assert.AreEqual(2,cic1.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().Count);
            
        Assert.AreEqual("Root1",cic1.RootCohortAggregateContainer.Name);
        Assert.AreEqual("Root2",cic1.RootCohortAggregateContainer.GetSubContainers()[0].Name);

    }
}