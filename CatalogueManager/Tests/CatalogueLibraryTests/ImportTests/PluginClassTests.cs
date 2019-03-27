// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public void Catalogue_returns_latest_compatible_plugin()
        {
            var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Blah.zip"));
            File.WriteAllBytes(fi.FullName, new byte[] { 0x1, 0x2 });

            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            var tripart = new Version(version);

            Plugin p = new Plugin(CatalogueRepository, fi);
            p.PluginVersion = new Version(tripart.Major, tripart.Minor, tripart.Build, 1);
            p.SaveToDatabase();

            Plugin p2 = new Plugin(CatalogueRepository, fi);
            p2.PluginVersion = new Version(tripart.Major, tripart.Minor, tripart.Build, 5);
            p2.SaveToDatabase();

            var plugins = CatalogueRepository.PluginManager.GetCompatiblePlugins();
            Assert.That(plugins, Has.Length.EqualTo(1));
            Assert.That(plugins[0], Is.EqualTo(p2));
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
