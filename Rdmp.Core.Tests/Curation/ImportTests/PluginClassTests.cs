// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Sharing.Dependency.Gathering;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.ImportTests;

public class PluginClassTests:UnitTests
{
    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        SetupMEF();
    }

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        Repository.Clear();
    }

    [Test]
    public void Catalogue_returns_latest_compatible_plugin()
    {
        var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Blah.zip"));
        File.WriteAllBytes(fi.FullName, new byte[] { 0x1, 0x2 });

        var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        var tripart = new Version(version);

        var lma1 = WhenIHaveA<LoadModuleAssembly>();
        var lma2 = WhenIHaveA<LoadModuleAssembly>();


        lma1.Plugin.Name = "MyPlugin";
        lma1.Plugin.RdmpVersion = new Version(version); //the version of Rdmp.Core targetted
        lma1.Plugin.PluginVersion = new Version(1, 1, 1, 1); //the version of the plugin
        lma1.Plugin.SaveToDatabase();
                       
        lma2.Plugin.Name = "MyPlugin";
        lma2.Plugin.RdmpVersion = new Version(version);//the version of Rdmp.Core targetted (same as above)
        lma2.Plugin.PluginVersion =  new Version(1, 1, 1, 2);//the version of the plugin (higher)
        lma2.SaveToDatabase();

        var plugins = Repository.PluginManager.GetCompatiblePlugins();
        Assert.That(plugins, Has.Length.EqualTo(1));
        Assert.That(plugins[0], Is.EqualTo(lma2.Plugin));
    }

    [Test]
    public void TestPlugin_OrphanImport_Sharing()
    {
        //Setup the load module we want to test (with plugin parent)
        var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,
            $"Blah2.{PackPluginRunner.PluginPackageSuffix}"));
        File.WriteAllBytes(fi.FullName, new byte[] { 0x1, 0x2 });

        var fi2 = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,
            $"Blah2.{PackPluginRunner.PluginPackageSuffix}"));
        File.WriteAllBytes(fi2.FullName, new byte[] { 0x1, 0x2 });
            
        var fi3 = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory,
            $"Blah3.{PackPluginRunner.PluginPackageSuffix}"));
        File.WriteAllBytes(fi3.FullName, new byte[] { 0x3, 0x4 });

        var p = new Core.Curation.Data.Plugin(Repository, fi,new Version(1,1,1),new Version(1,1,1,1));
        var lma = new LoadModuleAssembly(Repository, fi2, p);
        var lma2 = new LoadModuleAssembly(Repository, fi3, p);
            
        //gather dependencies of the plugin (plugin[0] + lma[1])
        var g = new Gatherer(RepositoryLocator);
        var sm = new ShareManager(RepositoryLocator);
        var list = Gatherer.GatherDependencies(p).ToShareDefinitionWithChildren(sm);

        //Delete export definitions
        foreach (var e in Repository.GetAllObjects<ObjectExport>())
            e.DeleteInDatabase();

        //and delete pluing (CASCADE deletes lma too)
        p.DeleteInDatabase();

        //import them
        var created = sm.ImportSharedObject(list).ToArray();

        //There should be 3
        Assert.AreEqual(3, created.Count());

        Assert.AreEqual(3,Repository.GetAllObjects<ObjectImport>().Count());

        lma2 = (LoadModuleAssembly) created[2];

        //now delete lma2 only
        lma2.DeleteInDatabase();
            
        Assert.AreEqual(2, Repository.GetAllObjects<ObjectImport>().Count());

        //import them
        var created2 = sm.ImportSharedObject(list);

        //There should still be 3
        Assert.AreEqual(3, created2.Count());
    }

    [TestCase("Rdmp.1.2.3.nupkg","Rdmp")]
    [TestCase("Rdmp.Dicom.1.2.3.nupkg","Rdmp.Dicom")]
    [TestCase("Rdmp.Dicom.nupkg","Rdmp.Dicom")]
    [TestCase("Rdmp.Dicom","Rdmp.Dicom")]
    public void Test_Plugin_ShortName(string fullname, string expected)
    {
        var p = WhenIHaveA<Rdmp.Core.Curation.Data.Plugin>();
        p.Name = fullname;
        Assert.AreEqual(expected,p.GetShortName());
    }
}