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
        [Test]
        public void TestSettingPdbNull()
        {
            //Setup the load module we want to test (with plugin parent)
            var fi = new FileInfo("Blah.zip");
            File.WriteAllBytes(fi.FullName,new byte[]{0x1,0x2});

            var fi2 = new FileInfo("Blah.dll");
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
    }   
}
