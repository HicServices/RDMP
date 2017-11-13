using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.ObjectSharing;
using CatalogueLibraryTests.Integration;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Importing;
using MapsDirectlyToDatabaseTable.ObjectSharing;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Rhino.Mocks;
using Rhino.Mocks.Utilities;
using Tests.Common;

namespace CatalogueLibraryTests.ImportTests
{
    public class TestImportingAnObject : DatabaseTests
    {
        [Test]
        public void ImportACatalogue()
        {
            SharedObjectImporter importer = new SharedObjectImporter(CatalogueRepository);

            var c = new Catalogue(CatalogueRepository, "omg cata");
            Assert.AreEqual(CatalogueRepository.GetAllCatalogues().Count(), 1);

            var c2 = (Catalogue)importer.ImportObject(new MapsDirectlyToDatabaseTableStatelessDefinition(c));

            Assert.AreEqual(c.Name, c2.Name);
            Assert.AreNotEqual(c.ID,c2.ID);

            Assert.AreEqual(CatalogueRepository.GetAllCatalogues().Count(),2);

        }

        [Test]
        public void TestSharingAPluginIgnoringCollisions()
        {
            foreach (var oldP in CatalogueRepository.GetAllObjects<Plugin>())
                oldP.DeleteInDatabase();

            var fi = new FileInfo("CommitAssemblyEmptyAssembly.dll");
            Assert.IsTrue(fi.Exists);

            var p = new Plugin(CatalogueRepository, fi);
            var lma = new LoadModuleAssembly(CatalogueRepository, fi, p);
            
            var n = new SharedPluginImporter(p);
            var importer = new SharedObjectImporter(RepositoryLocator.CatalogueRepository);

            //reject the reuse of an existing one
            var p2 = n.Import(importer, new ThrowImmediatelyCheckNotifier());

            Assert.AreEqual(p.LoadModuleAssemblies.Count(),p2.LoadModuleAssemblies.Count());
            Assert.AreEqual(p.LoadModuleAssemblies.First().Dll,p2.LoadModuleAssemblies.First().Dll);
            Assert.AreNotEqual(p.ID, p2.ID);
        }

        [Test]
        public void TestSharingAPluginReplaceBinary()
        {
            foreach (var oldP in CatalogueRepository.GetAllObjects<Plugin>())
                oldP.DeleteInDatabase();

            var fi = new FileInfo("CommitAssemblyEmptyAssembly.dll");
            Assert.IsTrue(fi.Exists);

            var p = new Plugin(CatalogueRepository, fi);
            var lma = new LoadModuleAssembly(CatalogueRepository, fi, p);

            Assert.IsTrue(lma.Exists());

            Dictionary<string,object> newDll = new Dictionary<string, object>();
            newDll.Add("Name","AmagadNewPluginLma");
            newDll.Add("Dll",new byte[]{0,1,0,1});
            newDll.Add("Pdb",new byte[]{0,1,0,1});
            newDll.Add("Committer","Frankenstine");
            newDll.Add("Description","Stuff");
            newDll.Add("Plugin_ID",999);
            newDll.Add("DllFileVersion","5.0.0.1");
            newDll.Add("UploadDate",new DateTime(2001,1,1));
            newDll.Add("SoftwareVersion", "2.5.0.1");


            var n = new SharedPluginImporter(new MapsDirectlyToDatabaseTableStatelessDefinition(p), new[] { new MapsDirectlyToDatabaseTableStatelessDefinition(typeof(LoadModuleAssembly), newDll) });
            var importer = new SharedObjectImporter(RepositoryLocator.CatalogueRepository);

            //accept that it is an update
            var p2 = n.Import(importer, new AcceptAllCheckNotifier());

            Assert.IsFalse(lma.Exists());
            Assert.AreEqual(p,p2);
            Assert.AreEqual(p.LoadModuleAssemblies.Single().Dll, new byte[] { 0, 1, 0, 1 });
        }
    }
}
