// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapsDirectlyToDatabaseTable.Attributes;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Databases;
using Rdmp.Core.Sharing.Dependency.Gathering;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.Curation.ImportTests
{
    public class GatherAndShareTests:DatabaseTests
    {
        [Test]
        public void Test_SerializeObject_ShareAttribute()
        {
            Dictionary<RelationshipAttribute, Guid> d = new Dictionary<RelationshipAttribute,Guid>();
            
            var json = JsonConvertExtensions.SerializeObject(d,RepositoryLocator);
            var obj = (Dictionary<RelationshipAttribute, Guid>)JsonConvertExtensions.DeserializeObject(json, typeof(Dictionary<RelationshipAttribute, Guid>),RepositoryLocator);

            Assert.AreEqual(0,obj.Count);

            //now add a key
            d.Add(new RelationshipAttribute(typeof(string),RelationshipType.SharedObject,"fff"),Guid.Empty);
                 
            json = JsonConvertExtensions.SerializeObject(d,RepositoryLocator);
            obj = (Dictionary<RelationshipAttribute, Guid>)JsonConvertExtensions.DeserializeObject(json, typeof(Dictionary<RelationshipAttribute, Guid>),RepositoryLocator);

            Assert.AreEqual(1,obj.Count);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GatherAndShare_ANOTable_Test(bool goViaJson)
        {
            var anoserver = new ExternalDatabaseServer(CatalogueRepository, "MyGatherAndShareTestANOServer", new ANOStorePatcher());
            var anoTable = new ANOTable(CatalogueRepository, anoserver, "ANOMagad", "N");

            Assert.AreEqual(anoTable.Server_ID,anoserver.ID);
            
            Gatherer g = new Gatherer(RepositoryLocator);
            Assert.IsTrue(g.CanGatherDependencies(anoTable));

            var gObj = g.GatherDependencies(anoTable);
            
            //root should be the server
            Assert.AreEqual(gObj.Object,anoserver);
            Assert.AreEqual(gObj.Children.Single().Object, anoTable);

            //get the sharing definitions
            var shareManager = new ShareManager(RepositoryLocator);
            ShareDefinition defParent = gObj.ToShareDefinition(shareManager,new List<ShareDefinition>());
            ShareDefinition defChild = gObj.Children.Single().ToShareDefinition(shareManager, new List<ShareDefinition>(new []{defParent}));

            //make it look like we never had it in the first place
            shareManager.GetNewOrExistingExportFor(anoserver).DeleteInDatabase();
            shareManager.GetNewOrExistingExportFor(anoTable).DeleteInDatabase();
            anoTable.DeleteInDatabase();
            anoserver.DeleteInDatabase();

            if(goViaJson)
            {
                var sParent = JsonConvertExtensions.SerializeObject(defParent,RepositoryLocator);
                var sChild = JsonConvertExtensions.SerializeObject(defChild, RepositoryLocator);

                defParent = (ShareDefinition)JsonConvertExtensions.DeserializeObject(sParent, typeof(ShareDefinition),RepositoryLocator);
                defChild = (ShareDefinition)JsonConvertExtensions.DeserializeObject(sChild, typeof(ShareDefinition), RepositoryLocator);
            }

            var anoserverAfter = new ExternalDatabaseServer(shareManager, defParent);
            
            Assert.IsTrue(anoserverAfter.Exists());

            //new instance
            Assert.AreNotEqual(anoserverAfter.ID, anoserver.ID);

            //same properties
            Assert.AreEqual(anoserverAfter.Name, anoserver.Name);
            Assert.AreEqual(anoserverAfter.CreatedByAssembly, anoserver.CreatedByAssembly);
            Assert.AreEqual(anoserverAfter.Database, anoserver.Database);
            Assert.AreEqual(anoserverAfter.DatabaseType, anoserver.DatabaseType);
            Assert.AreEqual(anoserverAfter.Username, anoserver.Username);
            Assert.AreEqual(anoserverAfter.Password, anoserver.Password);

            var anoTableAfter = new ANOTable(shareManager, defChild);

            //new instance
            Assert.AreNotEqual(anoTableAfter.ID, anoTable.ID);
            Assert.AreNotEqual(anoTableAfter.Server_ID, anoTable.Server_ID);

            //same properties
            Assert.AreEqual(anoTableAfter.NumberOfCharactersToUseInAnonymousRepresentation, anoTable.NumberOfCharactersToUseInAnonymousRepresentation);
            Assert.AreEqual(anoTableAfter.Suffix, anoTable.Suffix);

            //change a property and save it
            anoTableAfter.Suffix = "CAMMELS!";
            CatalogueRepository.SaveToDatabase(anoTableAfter);
            //anoTableAfter.SaveToDatabase(); <- this decides to go check the ANOTable exists on the server refernced which is immaginary btw >< thats why we have the above line instead

            //reimport (this time it should be an update, we import the share definitions and it overrdies our database copy (sharing is UPSERT)
            var anoTableAfter2 = new ANOTable(shareManager, defChild);

            Assert.AreEqual(anoTableAfter.ID, anoTableAfter2.ID);
            Assert.AreEqual("N", anoTableAfter2.Suffix);
            
            anoTableAfter.DeleteInDatabase();
            anoserverAfter.DeleteInDatabase();

            foreach (ObjectImport o in RepositoryLocator.CatalogueRepository.GetAllObjects<ObjectImport>())
                o.DeleteInDatabase();
        }

        [Test]
        public void GatherAndShare_Plugin_Test()
        {
            var f1 = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,"Imaginary1"+ PackPluginRunner.PluginPackageSuffix));
            File.WriteAllBytes(f1.FullName,new byte[]{0x1,0x2});
            
            var plugin = new Core.Curation.Data.Plugin(CatalogueRepository,new FileInfo("Imaginary"+ PackPluginRunner.PluginPackageSuffix),new System.Version(1,1,1),new System.Version(1,1,1));
            var lma1 = new LoadModuleAssembly(CatalogueRepository,f1,plugin);

            Assert.AreEqual(lma1.Plugin_ID, plugin.ID);

            Gatherer g = new Gatherer(RepositoryLocator);
            Assert.IsTrue(g.CanGatherDependencies(plugin));

            var gObj = g.GatherDependencies(plugin);

            //root should be the server
            Assert.AreEqual(gObj.Object, plugin);
            Assert.IsTrue(gObj.Children.Any(d=>d.Object.Equals(lma1)));
        }


        [TestCase(true)]
        [TestCase(false)]
        public void GatherAndShare_Catalogue_Test(bool goViaJson)
        {
            //Setup some objects under Catalogue that we can share
            var cata = new Catalogue(CatalogueRepository, "Cata");
            cata.Periodicity = Catalogue.CataloguePeriodicity.BiMonthly;
            cata.SaveToDatabase();

            var catalogueItem1 = new CatalogueItem(CatalogueRepository, cata, "Ci1");
            var catalogueItem2 = new CatalogueItem(CatalogueRepository, cata, "Ci2");

            var tableInfo = new TableInfo(CatalogueRepository, "Myt");
            var colInfo = new ColumnInfo(CatalogueRepository, "[Mt].[C1]", "varchar(10)", tableInfo);

            catalogueItem1.ColumnInfo_ID = colInfo.ID;
            catalogueItem1.SaveToDatabase();

            var ei = new ExtractionInformation(CatalogueRepository, catalogueItem1, colInfo, "UPPER(C1) as Fish");

            //the logging server has a system default so should have been populated
            Assert.IsNotNull(cata.LiveLoggingServer_ID);

            //Catalogue sharing should be allowed
            Gatherer g = new Gatherer(RepositoryLocator);
            Assert.IsTrue(g.CanGatherDependencies(cata));

            //gather the objects depending on Catalogue as a tree
            var gObj = g.GatherDependencies(cata);
            Assert.AreEqual(2, gObj.Children.Count); //both cata items

            var lmd = new LoadMetadata(CatalogueRepository);
            cata.LoadMetadata_ID = lmd.ID;
            cata.SaveToDatabase();

            //get the share definition
            var shareManager = new ShareManager(RepositoryLocator);
            var shareDefinition = gObj.ToShareDefinitionWithChildren(shareManager);


            if (goViaJson)
            {
                var json =
                    shareDefinition.Select(s => JsonConvertExtensions.SerializeObject(s, RepositoryLocator)).ToList();
                shareDefinition =
                    json.Select(
                        j => JsonConvertExtensions.DeserializeObject(j, typeof (ShareDefinition), RepositoryLocator))
                        .Cast<ShareDefinition>()
                        .ToList();
            }

            //make a local change
            cata.Name = "fishfish";
            cata.SubjectNumbers = "123";
            cata.LoadMetadata_ID = null;
            cata.Periodicity = Catalogue.CataloguePeriodicity.Unknown;
            cata.SaveToDatabase();
            
            lmd.DeleteInDatabase();

            //import the saved copy
            shareManager.ImportSharedObject(shareDefinition);

            //revert the memory copy and check it got overwritten with the original saved values
            cata = CatalogueRepository.GetObjectByID<Catalogue>(cata.ID);
            Assert.AreEqual("Cata", cata.Name);

            var exports = CatalogueRepository.GetAllObjects<ObjectExport>();
            Assert.IsTrue(exports.Any());

            //now delete and report
            foreach (var d in exports)
                d.DeleteInDatabase();

            //make a local change including Name
            cata.Name = "fishfish";
            cata.SaveToDatabase();

            //test importing the Catalogue properties only
            shareManager.ImportPropertiesOnly(cata,shareDefinition[0]);
            
            //import the defined properties but not name
            Assert.AreEqual("fishfish",cata.Name);
            Assert.AreEqual(Catalogue.CataloguePeriodicity.BiMonthly,cata.Periodicity); //reset this though
            Assert.IsNull(cata.LoadMetadata_ID);
            cata.SaveToDatabase();

            cata.DeleteInDatabase();

            //none of these should now exist thanks to cascade deletes
            Assert.IsFalse(cata.Exists());
            Assert.IsFalse(catalogueItem1.Exists());
            Assert.IsFalse(catalogueItem2.Exists());

            //import the saved copy
            var newObjects = shareManager.ImportSharedObject(shareDefinition).ToArray();

            Assert.AreEqual("Cata", ((Catalogue) newObjects[0]).Name);
            Assert.AreEqual("Ci1", ((CatalogueItem) newObjects[1]).Name);
            Assert.AreEqual("Ci2", ((CatalogueItem) newObjects[2]).Name);
        }

        [Test]
        public void GatherAndShare_ExtractionFilter_Test()
        {
            //Setup some objects under Catalogue
            var cata = new Catalogue(CatalogueRepository, "Cata");
            cata.Periodicity = Catalogue.CataloguePeriodicity.BiMonthly;
            cata.SaveToDatabase();

            var catalogueItem1 = new CatalogueItem(CatalogueRepository, cata, "Ci1");
            
            var tableInfo = new TableInfo(CatalogueRepository, "Myt");
            var colInfo = new ColumnInfo(CatalogueRepository, "[Mt].[C1]", "varchar(10)", tableInfo);

            catalogueItem1.ColumnInfo_ID = colInfo.ID;
            catalogueItem1.SaveToDatabase();

            //Setup a Filter under this extractable column (the filter is what we will share)
            var ei = new ExtractionInformation(CatalogueRepository, catalogueItem1, colInfo, "UPPER(C1) as Fish");

            var filter = new ExtractionFilter(CatalogueRepository, "My Filter", ei);
            filter.Description = "amagad";
            filter.WhereSQL = "UPPER(C1) = @a";

            //Give the filter a parameter @a just to make things interesting
            var declaration = filter.GetQuerySyntaxHelper().GetParameterDeclaration("@a", new DatabaseTypeRequest(typeof (string), 1));
            var param = filter.GetFilterFactory().CreateNewParameter(filter, declaration);
            
            //Also create a 'known good value' set i.e. recommended value for the parameter to achive some goal (you can have multiple of these - this will not be shared)
            var set = new ExtractionFilterParameterSet(CatalogueRepository, filter, "Fife");
            var val = new ExtractionFilterParameterSetValue(CatalogueRepository, set, (ExtractionFilterParameter) param);
            val.Value = "'FISH'";

            //Gather the dependencies (this is what we are testing)
            var gatherer = new Gatherer(RepositoryLocator);
            
            Assert.IsTrue(gatherer.CanGatherDependencies(filter));
            var gathered = gatherer.GatherDependencies(filter);

            //gatherer should have gathered the filter and the parameter (but not the ExtractionFilterParameterSet sets)
            Assert.AreEqual(1,gathered.Children.Count);
            Assert.AreEqual(param,gathered.Children[0].Object);
             
            //Cleanup
            val.DeleteInDatabase();
            set.DeleteInDatabase();
            cata.DeleteInDatabase();
        }
    }
}
