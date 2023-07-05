// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation;

internal class YamlRepositoryTests
{
    [Test]
    public void BlankConstructorsForEveryone()
    {
        var sb = new StringBuilder();

        foreach(var t in new YamlRepository(GetUniqueDirectory()).GetCompatibleTypes())
        {
            var blankConstructor = t.GetConstructor(Type.EmptyTypes);

            if (blankConstructor == null)
                sb.AppendLine(t.Name);
        }

        if(sb.Length > 0)
        {
            Assert.Fail($"All data classes must have a blank constructor.  The following did not:{Environment.NewLine}{sb}");
        }
    }

    [Test]
    public void PersistDefaults()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);
        var eds = new ExternalDatabaseServer(repo1,"myServer",null);
        repo1.SetDefault(PermissableDefaults.LiveLoggingServer_ID, eds);

        var repo2 = new YamlRepository(dir);
        Assert.AreEqual(eds, repo2.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
    }

    [Test]
    public void PersistPackageContents()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var ds = UnitTests.WhenIHaveA<ExtractableDataSet>(repo1);

        var package = UnitTests.WhenIHaveA<ExtractableDataSetPackage>(repo1);

        Assert.IsEmpty(repo1.GetPackageContentsDictionary());
        repo1.PackageManager.AddDataSetToPackage(package, ds);
        Assert.IsNotEmpty(repo1.GetPackageContentsDictionary());

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        Assert.IsNotEmpty(repo2.GetPackageContentsDictionary());
    }

    [Test]
    public void PersistDataExportPropertyManagerValues()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);
        repo1.DataExportPropertyManager.SetValue(DataExportProperty.HashingAlgorithmPattern,"yarg");

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        Assert.AreEqual("yarg", repo2.DataExportPropertyManager.GetValue(DataExportProperty.HashingAlgorithmPattern));
    }

    [Test]
    public void PersistGovernancePeriods()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var period = UnitTests.WhenIHaveA<GovernancePeriod>(repo1);
        var cata = UnitTests.WhenIHaveA<Catalogue>(repo1);

        Assert.IsEmpty(repo1.GetAllGovernedCatalogues(period));
        repo1.Link(period, cata);
        Assert.IsNotEmpty(repo1.GetAllGovernedCatalogues(period));

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        Assert.IsNotEmpty(repo2.GetAllGovernedCatalogues(period));
    }


    [Test]
    public void PersistForcedJoins()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var ac = UnitTests.WhenIHaveA<AggregateConfiguration>(repo1);
        var t = UnitTests.WhenIHaveA<TableInfo>(repo1);

        Assert.IsEmpty(ac.ForcedJoins);
        Assert.IsEmpty(repo1.AggregateForcedJoinManager.GetAllForcedJoinsFor(ac));
        repo1.AggregateForcedJoinManager.CreateLinkBetween(ac,t);
        Assert.IsNotEmpty(ac.ForcedJoins);
        Assert.IsNotEmpty(repo1.AggregateForcedJoinManager.GetAllForcedJoinsFor(ac));

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        Assert.IsNotEmpty(ac.ForcedJoins);
        Assert.IsNotEmpty(repo2.AggregateForcedJoinManager.GetAllForcedJoinsFor(ac));
    }


    [Test]
    public void PersistCohortSubcontainers()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var root = UnitTests.WhenIHaveA<CohortAggregateContainer>(repo1);
        var sub1 = new CohortAggregateContainer(repo1,SetOperation.INTERSECT);
        var ac = UnitTests.WhenIHaveA<AggregateConfiguration>(repo1);
            
        sub1.Order = 2;
        sub1.SaveToDatabase();

        root.AddChild(sub1); 
        root.AddChild(ac, 0);

        Assert.IsNotEmpty(root.GetOrderedContents());
        Assert.AreEqual(ac,root.GetOrderedContents().ToArray()[0]);
        Assert.AreEqual(sub1, root.GetOrderedContents().ToArray()[1]);

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        root = repo2.GetObjectByID<CohortAggregateContainer>(root.ID);

        Assert.IsNotEmpty(root.GetOrderedContents());
        Assert.AreEqual(ac, root.GetOrderedContents().ToArray()[0]);
        Assert.AreEqual(sub1, root.GetOrderedContents().ToArray()[1]);
    }

    [Test]
    public void PersistFilterContainers()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var ac = UnitTests.WhenIHaveA<AggregateConfiguration>(repo1);
        ac.CreateRootContainerIfNotExists();

        var f = new AggregateFilter(repo1, "my filter");
        ac.RootFilterContainer.AddChild(f);
        var sub = new AggregateFilterContainer(repo1,FilterContainerOperation.AND);
        ac.RootFilterContainer.AddChild(sub);

        Assert.AreEqual(sub,ac.RootFilterContainer.GetSubContainers().Single());
        Assert.AreEqual(f, ac.RootFilterContainer.GetFilters().Single());

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        ac = repo2.GetObjectByID<AggregateConfiguration>(ac.ID);

        Assert.AreEqual(sub, ac.RootFilterContainer.GetSubContainers().Single());
        Assert.AreEqual(f, ac.RootFilterContainer.GetFilters().Single());
    }

    [Test]
    public void PersistFilterContainers_Orphans()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var ac = UnitTests.WhenIHaveA<AggregateConfiguration>(repo1);
        ac.CreateRootContainerIfNotExists();
        var root = ac.RootFilterContainer;

        var f = new AggregateFilter(repo1, "my filter");
        ac.RootFilterContainer.AddChild(f);
        var sub = new AggregateFilterContainer(repo1,FilterContainerOperation.AND);
        ac.RootFilterContainer.AddChild(sub);

        Assert.AreEqual(sub,ac.RootFilterContainer.GetSubContainers().Single());
        Assert.AreEqual(f, ac.RootFilterContainer.GetFilters().Single());

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        ac = repo2.GetObjectByID<AggregateConfiguration>(ac.ID);

        Assert.AreEqual(sub, ac.RootFilterContainer.GetSubContainers().Single());
        Assert.AreEqual(f, ac.RootFilterContainer.GetFilters().Single());

        // Make an orphan container by deleting the root

        // don't check before deleting stuff
        ((CatalogueObscureDependencyFinder)ac.CatalogueRepository.ObscureDependencyFinder).OtherDependencyFinders.Clear();

        // delete the root filter
        ac.RootFilterContainer.DeleteInDatabase();

        // A fresh repo loaded from the same directory 
        var repo3 = new YamlRepository(dir);

        // all these things should be gone
        Assert.IsFalse(repo3.StillExists(sub));
        Assert.IsFalse(repo3.StillExists(root));
        Assert.IsFalse(repo3.StillExists(f));

    }

    [Test]
    public void PersistCredentials()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var creds = UnitTests.WhenIHaveA<DataAccessCredentials>(repo1);
        var t = UnitTests.WhenIHaveA<TableInfo>(repo1);

        Assert.IsEmpty(creds.GetAllTableInfosThatUseThis().SelectMany(v=>v.Value));
        Assert.IsNull(t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.DataLoad));
        Assert.IsNull(t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.InternalDataProcessing));

        repo1.TableInfoCredentialsManager.CreateLinkBetween(creds,t, ReusableLibraryCode.DataAccess.DataAccessContext.DataLoad);

        Assert.AreEqual(t,creds.GetAllTableInfosThatUseThis().SelectMany(v => v.Value).Single());
        Assert.AreEqual(creds,t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.DataLoad));
        Assert.IsNull(t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.InternalDataProcessing));


        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        t = repo2.GetObjectByID<TableInfo>(t.ID);

        Assert.AreEqual(t, creds.GetAllTableInfosThatUseThis().SelectMany(v => v.Value).Single());
        Assert.AreEqual(creds, t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.DataLoad));
        Assert.IsNull(t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.InternalDataProcessing));


    }



    [Test]
    public void YamlRepository_LoadSavePluginClass()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);
        var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Blah.zip"));
        File.WriteAllBytes(fi.FullName, new byte[] { 0x1, 0x2 });

        var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

        var lma1 = UnitTests.WhenIHaveA<LoadModuleAssembly>(repo1);
        var lma2 = UnitTests.WhenIHaveA<LoadModuleAssembly>(repo1);


        lma1.Plugin.Name = "MyPlugin1.1.1.1.nupkg";
        lma1.Plugin.RdmpVersion = new Version(version); //the version of Rdmp.Core targetted
        lma1.Plugin.PluginVersion = new Version(1, 1, 1, 1); //the version of the plugin
        lma1.Plugin.SaveToDatabase();
        lma1.SaveToDatabase();

        lma2.Plugin.Name = "MyPlugin1.1.1.2.nupkg";
        lma2.Plugin.RdmpVersion = new Version(version);//the version of Rdmp.Core targetted (same as above)
        lma2.Plugin.PluginVersion = new Version(1, 1, 1, 2);//the version of the plugin (higher)
        lma2.Plugin.SaveToDatabase();
        lma2.SaveToDatabase();

        var plugins = repo1.PluginManager.GetCompatiblePlugins();
        Assert.That(plugins, Has.Length.EqualTo(1));
        Assert.That(plugins[0], Is.EqualTo(lma2.Plugin));


        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        plugins = repo2.PluginManager.GetCompatiblePlugins();
        Assert.That(plugins, Has.Length.EqualTo(1));
        Assert.That(plugins.Single(), Is.EqualTo(lma2.Plugin));
    }

    [Test]
    public void TestYamlRepository_LoadObjects()
    {
        var dir = new DirectoryInfo(GetUniqueDirectoryName());
        var repo = new YamlRepository(dir);

        var c = new Catalogue(repo, "yar");

        Assert.Contains(c, repo.AllObjects.ToArray());

        // creating a new repo should load the same object back
        var repo2 = new YamlRepository(dir);
        Assert.Contains(c, repo2.AllObjects.ToArray());
    }

    [Test]
    public void TestYamlRepository_Save()
    {
        var dir = new DirectoryInfo(GetUniqueDirectoryName());
        var repo = new YamlRepository(dir);

        var c = new Catalogue(repo, "yar")
        {
            Name = "ffff"
        };
        c.SaveToDatabase();

        // creating a new repo should load the same object back
        var repo2 = new YamlRepository(dir);
        Assert.Contains(c, repo2.AllObjects.ToArray());
        Assert.AreEqual("ffff", repo2.AllObjects.OfType<Catalogue>().Single().Name);
    }

    private static string GetUniqueDirectoryName()
    {
        return Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString().Replace("-", ""));
    }

    private DirectoryInfo GetUniqueDirectory()
    {
        return new DirectoryInfo(GetUniqueDirectoryName());
    }
}