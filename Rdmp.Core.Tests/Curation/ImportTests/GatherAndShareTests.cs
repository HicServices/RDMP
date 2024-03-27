// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Databases;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Sharing.Dependency.Gathering;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.Curation.ImportTests;

public class GatherAndShareTests : DatabaseTests
{
    [Test]
    public void Test_SerializeObject_ShareAttribute()
    {
        var d = new Dictionary<RelationshipAttribute, Guid>();

        var json = JsonConvertExtensions.SerializeObject(d, RepositoryLocator);
        var obj = (Dictionary<RelationshipAttribute, Guid>)JsonConvertExtensions.DeserializeObject(json,
            typeof(Dictionary<RelationshipAttribute, Guid>), RepositoryLocator);

        Assert.That(obj, Is.Empty);

        //now add a key
        d.Add(new RelationshipAttribute(typeof(string), RelationshipType.SharedObject, "fff"), Guid.Empty);

        json = JsonConvertExtensions.SerializeObject(d, RepositoryLocator);
        obj = (Dictionary<RelationshipAttribute, Guid>)JsonConvertExtensions.DeserializeObject(json,
            typeof(Dictionary<RelationshipAttribute, Guid>), RepositoryLocator);

        Assert.That(obj, Has.Count.EqualTo(1));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GatherAndShare_ANOTable_Test(bool goViaJson)
    {
        var anoserver =
            new ExternalDatabaseServer(CatalogueRepository, "MyGatherAndShareTestANOServer", new ANOStorePatcher());
        var anoTable = new ANOTable(CatalogueRepository, anoserver, "ANOMagad", "N");

        Assert.That(anoserver.ID, Is.EqualTo(anoTable.Server_ID));

        var g = new Gatherer(RepositoryLocator);
        Assert.That(g.CanGatherDependencies(anoTable));

        var gObj = Gatherer.GatherDependencies(anoTable);

        Assert.Multiple(() =>
        {
            //root should be the server
            Assert.That(anoserver, Is.EqualTo(gObj.Object));
            Assert.That(anoTable, Is.EqualTo(gObj.Children.Single().Object));
        });

        //get the sharing definitions
        var shareManager = new ShareManager(RepositoryLocator);
        var defParent = gObj.ToShareDefinition(shareManager, new List<ShareDefinition>());
        var defChild = gObj.Children.Single()
            .ToShareDefinition(shareManager, new List<ShareDefinition>(new[] { defParent }));

        //make it look like we never had it in the first place
        shareManager.GetNewOrExistingExportFor(anoserver).DeleteInDatabase();
        shareManager.GetNewOrExistingExportFor(anoTable).DeleteInDatabase();
        anoTable.DeleteInDatabase();
        anoserver.DeleteInDatabase();

        if (goViaJson)
        {
            var sParent = JsonConvertExtensions.SerializeObject(defParent, RepositoryLocator);
            var sChild = JsonConvertExtensions.SerializeObject(defChild, RepositoryLocator);

            defParent = (ShareDefinition)JsonConvertExtensions.DeserializeObject(sParent, typeof(ShareDefinition),
                RepositoryLocator);
            defChild = (ShareDefinition)JsonConvertExtensions.DeserializeObject(sChild, typeof(ShareDefinition),
                RepositoryLocator);
        }

        var anoserverAfter = new ExternalDatabaseServer(shareManager, defParent);

        Assert.Multiple(() =>
        {
            Assert.That(anoserverAfter.Exists());

            //new instance
            Assert.That(anoserver.ID, Is.Not.EqualTo(anoserverAfter.ID));

            //same properties
            Assert.That(anoserver.Name, Is.EqualTo(anoserverAfter.Name));
            Assert.That(anoserver.CreatedByAssembly, Is.EqualTo(anoserverAfter.CreatedByAssembly));
            Assert.That(anoserver.Database, Is.EqualTo(anoserverAfter.Database));
            Assert.That(anoserver.DatabaseType, Is.EqualTo(anoserverAfter.DatabaseType));
            Assert.That(anoserver.Username, Is.EqualTo(anoserverAfter.Username));
            Assert.That(anoserver.Password, Is.EqualTo(anoserverAfter.Password));
        });

        var anoTableAfter = new ANOTable(shareManager, defChild);

        Assert.Multiple(() =>
        {
            //new instance
            Assert.That(anoTable.ID, Is.Not.EqualTo(anoTableAfter.ID));
            Assert.That(anoTable.Server_ID, Is.Not.EqualTo(anoTableAfter.Server_ID));

            //same properties
            Assert.That(anoTable.NumberOfCharactersToUseInAnonymousRepresentation, Is.EqualTo(anoTableAfter.NumberOfCharactersToUseInAnonymousRepresentation));
            Assert.That(anoTable.Suffix, Is.EqualTo(anoTableAfter.Suffix));
        });

        //change a property and save it
        anoTableAfter.Suffix = "CAMMELS!";
        CatalogueRepository.SaveToDatabase(anoTableAfter);
        //anoTableAfter.SaveToDatabase(); <- this decides to go check the ANOTable exists on the server referenced which is imaginary btw >< that's why we have the above line instead

        //reimport (this time it should be an update, we import the share definitions and it overrides our database copy (sharing is UPSERT)
        var anoTableAfter2 = new ANOTable(shareManager, defChild);

        Assert.Multiple(() =>
        {
            Assert.That(anoTableAfter2.ID, Is.EqualTo(anoTableAfter.ID));
            Assert.That(anoTableAfter2.Suffix, Is.EqualTo("N"));
        });

        anoTableAfter.DeleteInDatabase();
        anoserverAfter.DeleteInDatabase();

        foreach (var o in RepositoryLocator.CatalogueRepository.GetAllObjects<ObjectImport>())
            o.DeleteInDatabase();
    }


    [TestCase(true)]
    [TestCase(false)]
    public void GatherAndShare_Catalogue_Test(bool goViaJson)
    {
        //Setup some objects under Catalogue that we can share
        var cata = new Catalogue(CatalogueRepository, "Cata")
        {
            Periodicity = Catalogue.CataloguePeriodicity.BiMonthly
        };
        cata.SaveToDatabase();

        var catalogueItem1 = new CatalogueItem(CatalogueRepository, cata, "Ci1");
        var catalogueItem2 = new CatalogueItem(CatalogueRepository, cata, "Ci2");

        var tableInfo = new TableInfo(CatalogueRepository, "Myt");
        var colInfo = new ColumnInfo(CatalogueRepository, "[Mt].[C1]", "varchar(10)", tableInfo);

        catalogueItem1.ColumnInfo_ID = colInfo.ID;
        catalogueItem1.SaveToDatabase();

        var ei = new ExtractionInformation(CatalogueRepository, catalogueItem1, colInfo, "UPPER(C1) as Fish");

        //the logging server has a system default so should have been populated
        Assert.That(cata.LiveLoggingServer_ID, Is.Not.Null);

        //Catalogue sharing should be allowed
        var g = new Gatherer(RepositoryLocator);
        Assert.That(g.CanGatherDependencies(cata));

        //gather the objects depending on Catalogue as a tree
        var gObj = Gatherer.GatherDependencies(cata);
        Assert.That(gObj.Children, Has.Count.EqualTo(2)); //both cata items

        var lmd = new LoadMetadata(CatalogueRepository);
        cata.SaveToDatabase();
        var linkage = new LoadMetadataCatalogueLinkage(CatalogueRepository, lmd, cata);
        linkage.SaveToDatabase();
        //get the share definition
        var shareManager = new ShareManager(RepositoryLocator);
        var shareDefinition = gObj.ToShareDefinitionWithChildren(shareManager);


        if (goViaJson)
        {
            var json =
                shareDefinition.Select(s => JsonConvertExtensions.SerializeObject(s, RepositoryLocator)).ToList();
            shareDefinition =
                json.Select(
                        j => JsonConvertExtensions.DeserializeObject(j, typeof(ShareDefinition), RepositoryLocator))
                    .Cast<ShareDefinition>()
                    .ToList();
        }

        //make a local change
        cata.Name = "fishfish";
        cata.SubjectNumbers = "123";
        cata.Periodicity = Catalogue.CataloguePeriodicity.Unknown;
        cata.SaveToDatabase();
        lmd.UnlinkFromCatalogue(cata);
        lmd.DeleteInDatabase();

        //import the saved copy
        shareManager.ImportSharedObject(shareDefinition);

        //revert the memory copy and check it got overwritten with the original saved values
        cata = CatalogueRepository.GetObjectByID<Catalogue>(cata.ID);
        Assert.That(cata.Name, Is.EqualTo("Cata"));

        var exports = CatalogueRepository.GetAllObjects<ObjectExport>();
        Assert.That(exports.Any());

        //now delete and report
        foreach (var d in exports)
            d.DeleteInDatabase();

        //make a local change including Name
        cata.Name = "fishfish";
        cata.SaveToDatabase();

        //test importing the Catalogue properties only
        ShareManager.ImportPropertiesOnly(cata, shareDefinition[0]);

        Assert.Multiple(() =>
        {
            //import the defined properties but not name
            Assert.That(cata.Name, Is.EqualTo("fishfish"));
            Assert.That(cata.Periodicity, Is.EqualTo(Catalogue.CataloguePeriodicity.BiMonthly)); //reset this though
            Assert.That(cata.LoadMetadatas(), Is.Empty);
        });
        cata.SaveToDatabase();

        cata.DeleteInDatabase();

        Assert.Multiple(() =>
        {
            //none of these should now exist thanks to cascade deletes
            Assert.That(cata.Exists(), Is.False);
            Assert.That(catalogueItem1.Exists(), Is.False);
            Assert.That(catalogueItem2.Exists(), Is.False);
        });

        //import the saved copy
        var newObjects = shareManager.ImportSharedObject(shareDefinition).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(((Catalogue)newObjects[0]).Name, Is.EqualTo("Cata"));
            Assert.That(((CatalogueItem)newObjects[1]).Name, Is.EqualTo("Ci1"));
            Assert.That(((CatalogueItem)newObjects[2]).Name, Is.EqualTo("Ci2"));
        });
    }

    [Test]
    public void GatherAndShare_ExtractionFilter_Test()
    {
        //Setup some objects under Catalogue
        var cata = new Catalogue(CatalogueRepository, "Cata")
        {
            Periodicity = Catalogue.CataloguePeriodicity.BiMonthly
        };
        cata.SaveToDatabase();

        var catalogueItem1 = new CatalogueItem(CatalogueRepository, cata, "Ci1");

        var tableInfo = new TableInfo(CatalogueRepository, "Myt");
        var colInfo = new ColumnInfo(CatalogueRepository, "[Mt].[C1]", "varchar(10)", tableInfo);

        catalogueItem1.ColumnInfo_ID = colInfo.ID;
        catalogueItem1.SaveToDatabase();

        //Setup a Filter under this extractable column (the filter is what we will share)
        var ei = new ExtractionInformation(CatalogueRepository, catalogueItem1, colInfo, "UPPER(C1) as Fish");

        var filter = new ExtractionFilter(CatalogueRepository, "My Filter", ei)
        {
            Description = "amagad",
            WhereSQL = "UPPER(C1) = @a"
        };

        //Give the filter a parameter @a just to make things interesting
        var declaration = filter.GetQuerySyntaxHelper()
            .GetParameterDeclaration("@a", new DatabaseTypeRequest(typeof(string), 1));
        var param = filter.GetFilterFactory().CreateNewParameter(filter, declaration);

        //Also create a 'known good value' set i.e. recommended value for the parameter to achive some goal (you can have multiple of these - this will not be shared)
        var set = new ExtractionFilterParameterSet(CatalogueRepository, filter, "Fife");
        var val = new ExtractionFilterParameterSetValue(CatalogueRepository, set, (ExtractionFilterParameter)param)
        {
            Value = "'FISH'"
        };

        //Gather the dependencies (this is what we are testing)
        var gatherer = new Gatherer(RepositoryLocator);

        Assert.That(gatherer.CanGatherDependencies(filter));
        var gathered = Gatherer.GatherDependencies(filter);

        //gatherer should have gathered the filter and the parameter (but not the ExtractionFilterParameterSet sets)
        Assert.That(gathered.Children, Has.Count.EqualTo(1));
        Assert.That(gathered.Children[0].Object, Is.EqualTo(param));

        //Cleanup
        val.DeleteInDatabase();
        set.DeleteInDatabase();
        cata.DeleteInDatabase();
    }
}