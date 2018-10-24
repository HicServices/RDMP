using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Repositories;
using NUnit.Framework;
using Sharing.Dependency.Gathering;
using Tests.Common;

namespace CatalogueLibraryTests.ImportTests
{

    public class PluginClassTests:DatabaseTests
    {
        [SetUp]
        public void ClearImportExportDefinitions()
        {
            RunBlitzDatabases(RepositoryLocator);
        }

        [Test]
        public void TestPlugin_PdbNull_Sharing()
        {
            //Setup the load module we want to test (with plugin parent)
            var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"Blah.zip"));
            File.WriteAllBytes(fi.FullName,new byte[]{0x1,0x2});

            var fi2 = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"Blah.dll"));
            File.WriteAllBytes(fi2.FullName, new byte[] { 0x1, 0x2 });

            Plugin p = new Plugin(CatalogueRepository,fi);
            var lma = new LoadModuleAssembly(CatalogueRepository, fi2, p);
            
            //Give it some pdb bytes
            lma.Pdb = new byte[]{0x1};
            lma.SaveToDatabase();

            //gather dependencies of the plugin (plugin[0] + lma[1])
            Gatherer g = new Gatherer(RepositoryLocator);
            ShareManager sm = new ShareManager(RepositoryLocator);
            var list = g.GatherDependencies(p).ToShareDefinitionWithChildren(sm);

            //Delete export definitions
            foreach (var e in CatalogueRepository.GetAllObjects<ObjectExport>())
                e.DeleteInDatabase();

            //and delete pluing (CASCADE deletes lma too)
            p.DeleteInDatabase();

            //import it again
            p = new Plugin(sm, list[0]);
            lma = new LoadModuleAssembly(sm, list[1]);
            
            Assert.AreEqual(1,lma.Pdb.Length); //1 byte in pdb

            //now make it like there are no pdb bytes at all
            lma.Pdb = null;
            lma.SaveToDatabase();
            
            //get a new share definition for the new state
            list = g.GatherDependencies(p).ToShareDefinitionWithChildren(sm);
            
            //Delete export definitions
            foreach (var e in CatalogueRepository.GetAllObjects<ObjectExport>())
                e.DeleteInDatabase();

            //and delete pluing (CASCADE deletes lma too)
            p.DeleteInDatabase();

            p = new Plugin(sm, list[0]);
            lma = new LoadModuleAssembly(sm, list[1]);

            Assert.IsNull(lma.Pdb);

        }

        [Test]
        public void TestPlugin_OrphanImport_Sharing()
        {
            //Setup the load module we want to test (with plugin parent)
            var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"Blah2.zip"));
            File.WriteAllBytes(fi.FullName, new byte[] { 0x1, 0x2 });

            var fi2 = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"Blah2.dll"));
            File.WriteAllBytes(fi2.FullName, new byte[] { 0x1, 0x2 });
            
            var fi3 = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"Blah3.dll"));
            File.WriteAllBytes(fi3.FullName, new byte[] { 0x3, 0x4 });

            Plugin p = new Plugin(CatalogueRepository, fi);
            var lma = new LoadModuleAssembly(CatalogueRepository, fi2, p);
            var lma2 = new LoadModuleAssembly(CatalogueRepository, fi3, p);
            
            //gather dependencies of the plugin (plugin[0] + lma[1])
            Gatherer g = new Gatherer(RepositoryLocator);
            ShareManager sm = new ShareManager(RepositoryLocator);
            var list = g.GatherDependencies(p).ToShareDefinitionWithChildren(sm);

            //Delete export definitions
            foreach (var e in CatalogueRepository.GetAllObjects<ObjectExport>())
                e.DeleteInDatabase();

            //and delete pluing (CASCADE deletes lma too)
            p.DeleteInDatabase();

            //import them
            var created = sm.ImportSharedObject(list).ToArray();

            //There should be 3
            Assert.AreEqual(3, created.Count());

            Assert.AreEqual(3,CatalogueRepository.GetAllObjects<ObjectImport>().Count());

            lma2 = (LoadModuleAssembly) created[2];

            //now delete lma2 only
            lma2.DeleteInDatabase();
            
            Assert.AreEqual(2, CatalogueRepository.GetAllObjects<ObjectImport>().Count());

            //import them
            var created2 = sm.ImportSharedObject(list);

            //There should still be 3
            Assert.AreEqual(3, created2.Count());
        }
    }   
}
