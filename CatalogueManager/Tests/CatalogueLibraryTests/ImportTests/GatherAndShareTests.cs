using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANOStore;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.Repositories;
using Newtonsoft.Json;
using NUnit.Framework;
using Sharing.Dependency.Gathering;
using Tests.Common;

namespace CatalogueLibraryTests.ImportTests
{
    public class GatherAndShareTests:DatabaseTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void GatherAndShare_ANOTable_Test(bool goViaJson)
        {
            var anoserver = new ExternalDatabaseServer(CatalogueRepository, "MyGatherAndShareTestANOServer", typeof (Class1).Assembly);
            var anoTable = new ANOTable(CatalogueRepository, anoserver, "ANOMagad", "_N");

            Assert.AreEqual(anoTable.Server_ID,anoserver.ID);
            
            Gatherer g = new Gatherer(RepositoryLocator);
            Assert.IsTrue(g.CanGatherDependencies(anoTable));

            var gObj = g.GatherDependencies(anoTable);
            
            //root should be the server
            Assert.AreEqual(gObj.Object,anoserver);
            Assert.AreEqual(gObj.Dependencies.Single().Object, anoTable);
            Assert.AreEqual(gObj.Dependencies.Single().ForeignKeyPropertyIfAny, "Server_ID");

            //get the sharing definitions
            var shareManager = new ShareManager(RepositoryLocator);
            ShareDefinition defParent = gObj.ToShareDefinition(shareManager,new List<ShareDefinition>());
            ShareDefinition defChild = gObj.Dependencies.Single().ToShareDefinition(shareManager, new List<ShareDefinition>(new []{defParent}));

            //make it look like we never had it in the first place
            shareManager.GetExportFor(anoserver).DeleteInDatabase();
            shareManager.GetExportFor(anoTable).DeleteInDatabase();
            anoTable.DeleteInDatabase();
            anoserver.DeleteInDatabase();

            if(goViaJson)
            {
                var sParent = JsonConvertExtensions.SerializeObject(defParent,RepositoryLocator);
                var sChild = JsonConvertExtensions.SerializeObject(defChild, RepositoryLocator);

                defParent = (ShareDefinition)JsonConvertExtensions.DeserializeObject(sParent, typeof(ShareDefinition),RepositoryLocator);
                defChild = (ShareDefinition)JsonConvertExtensions.DeserializeObject(sChild, typeof(ShareDefinition), RepositoryLocator);
            }

            var anoserverAfter = new ExternalDatabaseServer(CatalogueRepository, defParent);

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
        }

        [Test]
        public void GatherAndShare_Plugin_Test()
        {
            var f1 = new FileInfo("Imaginary1.dll");
            File.WriteAllBytes(f1.FullName,new byte[]{0x1,0x2});

            var f2 = new FileInfo("Imaginary1.dll");
            File.WriteAllBytes(f2.FullName, new byte[] { 0x3, 0x3 });

            var plugin = new Plugin(CatalogueRepository,new FileInfo("Imaginary.zip"));
            var lma1 = new LoadModuleAssembly(CatalogueRepository,f1,plugin);
            var lma2 = new LoadModuleAssembly(CatalogueRepository, f2, plugin);

            Assert.AreEqual(lma1.Plugin_ID, plugin.ID);
            Assert.AreEqual(lma2.Plugin_ID, plugin.ID);

            Gatherer g = new Gatherer(RepositoryLocator);
            Assert.IsTrue(g.CanGatherDependencies(plugin));

            var gObj = g.GatherDependencies(plugin);

            //root should be the server
            Assert.AreEqual(gObj.Object, plugin);
            Assert.IsTrue(gObj.Dependencies.Any(d=>d.Object.Equals(lma1)));
            Assert.IsTrue(gObj.Dependencies.Any(d => d.Object.Equals(lma2)));
            Assert.IsTrue(gObj.Dependencies.All(d=>d.ForeignKeyPropertyIfAny == "Plugin_ID"));
        }
    }
}
