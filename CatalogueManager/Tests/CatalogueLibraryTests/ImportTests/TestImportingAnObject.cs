using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.ObjectSharing;
using CatalogueLibraryTests.Integration;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.ObjectSharing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Serialization;
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

            var c2 = (Catalogue)importer.ImportObject(new MapsDirectlyToDatabaseTableStatelessDefinition<Catalogue>(c));

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


            var n = new SharedPluginImporter(new MapsDirectlyToDatabaseTableStatelessDefinition<Plugin>(p), new[] { new MapsDirectlyToDatabaseTableStatelessDefinition<LoadModuleAssembly>(newDll) });
            var importer = new SharedObjectImporter(RepositoryLocator.CatalogueRepository);

            //accept that it is an update
            var p2 = n.Import(importer, new AcceptAllCheckNotifier());

            Assert.IsFalse(lma.Exists());
            Assert.AreEqual(p,p2);
            Assert.AreEqual(p.LoadModuleAssemblies.Single().Dll, new byte[] { 0, 1, 0, 1 });
        }

        [Test]
        public void JsonTest()
        {
            foreach (var oldP in CatalogueRepository.GetAllObjects<Plugin>())
                oldP.DeleteInDatabase();

            var fi = new FileInfo("CommitAssemblyEmptyAssembly.dll");
            Assert.IsTrue(fi.Exists);

            var p = new Plugin(CatalogueRepository, fi);
            var lma = new LoadModuleAssembly(CatalogueRepository, fi, p);

            var pStateless = new MapsDirectlyToDatabaseTableStatelessDefinition<Plugin>(p);
            var lmaStatelessArray = new[] {new MapsDirectlyToDatabaseTableStatelessDefinition<LoadModuleAssembly>(lma)};
            
            BinaryFormatter bf = new BinaryFormatter();
            string s;
            string sa;

            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, pStateless);
                s = Convert.ToBase64String(ms.ToArray());
            }

            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, lmaStatelessArray);
                sa = Convert.ToBase64String(ms.ToArray());
            }

            var import = new SharedPluginImporter(s, sa);

            var p2 = import.Import(new SharedObjectImporter(CatalogueRepository), new AcceptAllCheckNotifier());

            Assert.AreEqual(p.LoadModuleAssemblies.Count(),p2.LoadModuleAssemblies.Count());
            Assert.AreEqual(p.LoadModuleAssemblies.First().Dll,p2.LoadModuleAssemblies.First().Dll);
            Assert.AreEqual(p.ID, p2.ID);

        }
    }
}
