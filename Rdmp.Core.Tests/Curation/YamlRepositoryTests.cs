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

        foreach (var t in new YamlRepository(GetUniqueDirectory()).GetCompatibleTypes())
        {
            var blankConstructor = t.GetConstructor(Type.EmptyTypes);

            if (blankConstructor == null)
                sb.AppendLine(t.Name);
        }

        if (sb.Length > 0)
            Assert.Fail(
                $"All data classes must have a blank constructor.  The following did not:{Environment.NewLine}{sb}");
    }

    [Test]
    public void PersistDefaults()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);
        var eds = new ExternalDatabaseServer(repo1, "myServer", null);
        repo1.SetDefault(PermissableDefaults.LiveLoggingServer_ID, eds);

        var repo2 = new YamlRepository(dir);
        Assert.That(repo2.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID), Is.EqualTo(eds));
    }

    [Test]
    public void PersistPackageContents()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var ds = UnitTests.WhenIHaveA<ExtractableDataSet>(repo1);

        var package = UnitTests.WhenIHaveA<ExtractableDataSetPackage>(repo1);

        Assert.That(repo1.GetPackageContentsDictionary(), Is.Empty);
        repo1.PackageManager.AddDataSetToPackage(package, ds);
        Assert.That(repo1.GetPackageContentsDictionary(), Is.Not.Empty);

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        Assert.That(repo2.GetPackageContentsDictionary(), Is.Not.Empty);
    }

    [Test]
    public void PersistDataExportPropertyManagerValues()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);
        repo1.DataExportPropertyManager.SetValue(DataExportProperty.HashingAlgorithmPattern, "yarg");

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        Assert.That(repo2.DataExportPropertyManager.GetValue(DataExportProperty.HashingAlgorithmPattern), Is.EqualTo("yarg"));
    }

    [Test]
    public void PersistGovernancePeriods()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var period = UnitTests.WhenIHaveA<GovernancePeriod>(repo1);
        var cata = UnitTests.WhenIHaveA<Catalogue>(repo1);

        Assert.That(repo1.GetAllGovernedCatalogues(period), Is.Empty);
        repo1.Link(period, cata);
        Assert.That(repo1.GetAllGovernedCatalogues(period), Is.Not.Empty);

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        Assert.That(repo2.GetAllGovernedCatalogues(period), Is.Not.Empty);
    }


    [Test]
    public void PersistForcedJoins()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var ac = UnitTests.WhenIHaveA<AggregateConfiguration>(repo1);
        var t = UnitTests.WhenIHaveA<TableInfo>(repo1);

        Assert.Multiple(() =>
        {
            Assert.That(ac.ForcedJoins, Is.Empty);
            Assert.That(repo1.AggregateForcedJoinManager.GetAllForcedJoinsFor(ac), Is.Empty);
        });
        repo1.AggregateForcedJoinManager.CreateLinkBetween(ac, t);
        Assert.Multiple(() =>
        {
            Assert.That(ac.ForcedJoins, Is.Not.Empty);
            Assert.That(repo1.AggregateForcedJoinManager.GetAllForcedJoinsFor(ac), Is.Not.Empty);
        });

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        Assert.Multiple(() =>
        {
            Assert.That(ac.ForcedJoins, Is.Not.Empty);
            Assert.That(repo2.AggregateForcedJoinManager.GetAllForcedJoinsFor(ac), Is.Not.Empty);
        });
    }


    [Test]
    public void PersistCohortSubcontainers()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var root = UnitTests.WhenIHaveA<CohortAggregateContainer>(repo1);
        var sub1 = new CohortAggregateContainer(repo1, SetOperation.INTERSECT);
        var ac = UnitTests.WhenIHaveA<AggregateConfiguration>(repo1);

        sub1.Order = 2;
        sub1.SaveToDatabase();

        root.AddChild(sub1);
        root.AddChild(ac, 0);

        Assert.That(root.GetOrderedContents(), Is.Not.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(root.GetOrderedContents().ToArray()[0], Is.EqualTo(ac));
            Assert.That(root.GetOrderedContents().ToArray()[1], Is.EqualTo(sub1));
        });

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        root = repo2.GetObjectByID<CohortAggregateContainer>(root.ID);

        Assert.That(root.GetOrderedContents(), Is.Not.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(root.GetOrderedContents().ToArray()[0], Is.EqualTo(ac));
            Assert.That(root.GetOrderedContents().ToArray()[1], Is.EqualTo(sub1));
        });
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
        var sub = new AggregateFilterContainer(repo1, FilterContainerOperation.AND);
        ac.RootFilterContainer.AddChild(sub);

        Assert.Multiple(() =>
        {
            Assert.That(ac.RootFilterContainer.GetSubContainers().Single(), Is.EqualTo(sub));
            Assert.That(ac.RootFilterContainer.GetFilters().Single(), Is.EqualTo(f));
        });

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        ac = repo2.GetObjectByID<AggregateConfiguration>(ac.ID);

        Assert.Multiple(() =>
        {
            Assert.That(ac.RootFilterContainer.GetSubContainers().Single(), Is.EqualTo(sub));
            Assert.That(ac.RootFilterContainer.GetFilters().Single(), Is.EqualTo(f));
        });
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
        var sub = new AggregateFilterContainer(repo1, FilterContainerOperation.AND);
        ac.RootFilterContainer.AddChild(sub);

        Assert.Multiple(() =>
        {
            Assert.That(ac.RootFilterContainer.GetSubContainers().Single(), Is.EqualTo(sub));
            Assert.That(ac.RootFilterContainer.GetFilters().Single(), Is.EqualTo(f));
        });

        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        ac = repo2.GetObjectByID<AggregateConfiguration>(ac.ID);

        Assert.Multiple(() =>
        {
            Assert.That(ac.RootFilterContainer.GetSubContainers().Single(), Is.EqualTo(sub));
            Assert.That(ac.RootFilterContainer.GetFilters().Single(), Is.EqualTo(f));
        });

        // Make an orphan container by deleting the root

        // don't check before deleting stuff
        ((CatalogueObscureDependencyFinder)ac.CatalogueRepository.ObscureDependencyFinder).OtherDependencyFinders
            .Clear();

        // delete the root filter
        ac.RootFilterContainer.DeleteInDatabase();

        // A fresh repo loaded from the same directory
        var repo3 = new YamlRepository(dir);

        Assert.Multiple(() =>
        {
            // all these things should be gone
            Assert.That(repo3.StillExists(sub), Is.False);
            Assert.That(repo3.StillExists(root), Is.False);
            Assert.That(repo3.StillExists(f), Is.False);
        });
    }

    [Test]
    public void PersistCredentials()
    {
        var dir = GetUniqueDirectory();

        var repo1 = new YamlRepository(dir);

        var creds = UnitTests.WhenIHaveA<DataAccessCredentials>(repo1);
        var t = UnitTests.WhenIHaveA<TableInfo>(repo1);

        Assert.Multiple(() =>
        {
            Assert.That(creds.GetAllTableInfosThatUseThis().SelectMany(v => v.Value), Is.Empty);
            Assert.That(t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.DataLoad), Is.Null);
            Assert.That(
                t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.InternalDataProcessing), Is.Null);
        });

        repo1.TableInfoCredentialsManager.CreateLinkBetween(creds, t,
            ReusableLibraryCode.DataAccess.DataAccessContext.DataLoad);

        Assert.Multiple(() =>
        {
            Assert.That(creds.GetAllTableInfosThatUseThis().SelectMany(v => v.Value).Single(), Is.EqualTo(t));
            Assert.That(t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.DataLoad), Is.EqualTo(creds));
            Assert.That(
                t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.InternalDataProcessing), Is.Null);
        });


        // A fresh repo loaded from the same directory should have persisted object relationships
        var repo2 = new YamlRepository(dir);
        t = repo2.GetObjectByID<TableInfo>(t.ID);

        Assert.Multiple(() =>
        {
            Assert.That(creds.GetAllTableInfosThatUseThis().SelectMany(v => v.Value).Single(), Is.EqualTo(t));
            Assert.That(t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.DataLoad), Is.EqualTo(creds));
            Assert.That(
                t.GetCredentialsIfExists(ReusableLibraryCode.DataAccess.DataAccessContext.InternalDataProcessing), Is.Null);
        });
    }


    [Test]
    public void TestYamlRepository_LoadObjects()
    {
        var dir = new DirectoryInfo(GetUniqueDirectoryName());
        var repo = new YamlRepository(dir);

        var c = new Catalogue(repo, "yar");

        Assert.That(repo.AllObjects.ToArray(), Does.Contain(c));

        // creating a new repo should load the same object back
        var repo2 = new YamlRepository(dir);
        Assert.That(repo2.AllObjects.ToArray(), Does.Contain(c));
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
        Assert.Multiple(() =>
        {
            Assert.That(repo2.AllObjects.ToArray(), Does.Contain(c));
            Assert.That(repo2.AllObjects.OfType<Catalogue>().Single().Name, Is.EqualTo("ffff"));
        });
    }

    private static string GetUniqueDirectoryName() => Path.Combine(TestContext.CurrentContext.WorkDirectory,
        Guid.NewGuid().ToString().Replace("-", ""));

    private static DirectoryInfo GetUniqueDirectory() => new(GetUniqueDirectoryName());
}