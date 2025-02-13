// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.UI.Tests;

internal class ChildProviderTests : UITests
{
    [Test]
    public void ChildProviderGiven_TableInfoWith_NullServer()
    {
        var ti = WhenIHaveA<TableInfo>();
        ti.Server = null;
        ti.SaveToDatabase();

        //creating a child provider when there are TableInfos with null servers should not crash the API!
        var provider = new CatalogueChildProvider(Repository.CatalogueRepository, null,
            ThrowImmediatelyCheckNotifier.Quiet, null);
        var desc = provider.GetDescendancyListIfAnyFor(ti);
        Assert.That(desc, Is.Not.Null);

        //instead we should get a parent node with the name "Null Server"
        var parent = (TableInfoServerNode)desc.Parents[^2];
        Assert.That(parent.ServerName, Is.EqualTo(TableInfoServerNode.NullServerNode));
    }

    [Test]
    public void ChildProviderGiven_TableInfoWith_NullDatabase()
    {
        var ti = WhenIHaveA<TableInfo>();
        ti.Database = null;
        ti.SaveToDatabase();

        //creating a child provider when there are TableInfos with null servers should not crash the API!
        var provider = new CatalogueChildProvider(Repository.CatalogueRepository, null,
            ThrowImmediatelyCheckNotifier.Quiet, null);
        var desc = provider.GetDescendancyListIfAnyFor(ti);
        Assert.That(desc, Is.Not.Null);

        //instead we should get a parent node with the name "Null Server"
        var parent = (TableInfoDatabaseNode)desc.Parents[^1];
        Assert.That(parent.DatabaseName, Is.EqualTo(TableInfoDatabaseNode.NullDatabaseNode));
    }

    [Test]
    public void TestUpTo()
    {
        string[] skip =
        {
            "AllAggregateContainers", "_dataExportFilterManager", "dataExportRepository", "WriteLock",
            "_oProjectNumberToCohortsDictionary", "_errorsCheckNotifier", "ProgressStopwatch","CohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations"
        };

        // We have 2 providers and want to suck all the data out of one into the other
        var cp1 = new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);
        var cp2 = new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);

        //to start with let's make sure all fields and properties are different on the two classes except where we expect them to be the same
        const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        foreach (var prop in typeof(DataExportChildProvider).GetProperties().Where(p => !skip.Contains(p.Name)))
        {
            var val1 = prop.GetValue(cp1);
            var val2 = prop.GetValue(cp2);

            // these are exempt, I guess 2 separate empty arrays are now considered 'same'
            if (val1 is Array a1 && val2 is Array a2 && a1.Length == 0 && a2.Length == 0)
                continue;

            Assert.That(val2, Is.Not.SameAs(val1), $"Prop {prop} was unexpectedly the same between child providers");
        }


        foreach (var field in typeof(DataExportChildProvider).GetFields(bindFlags).Where(p => !skip.Contains(p.Name)))
        {
            var val1 = field.GetValue(cp1);
            var val2 = field.GetValue(cp2);

            // these are exempt, I guess 2 separate empty arrays are now considered 'same'
            if (val1 is Array a1 && val2 is Array a2 && a1.Length == 0 && a2.Length == 0)
                continue;

            Assert.That(val2, Is.Not.SameAs(val1), $"Field {field} was unexpectedly the same between child providers");
        }


        // Now call UpdateTo to make cp1 look like cp2
        cp1.UpdateTo(cp2);

        var badProps = new List<string>();

        foreach (var prop in typeof(DataExportChildProvider).GetProperties().Where(p => !skip.Contains(p.Name)))
            try
            {
                Assert.That(prop.GetValue(cp2), Is.SameAs(prop.GetValue(cp1)),
                    $"Prop {prop} was not the same between child providers - after UpdateTo");
            }
            catch (Exception)
            {
                badProps.Add(prop.Name);
            }

        Assert.That(badProps, Is.Empty);

        var badFields = new List<string>();

        foreach (var field in typeof(DataExportChildProvider).GetFields(bindFlags).Where(p => !skip.Contains(p.Name)))
            try
            {
                Assert.That(field.GetValue(cp2), Is.SameAs(field.GetValue(cp1)),
                    $"Field {field} was not the same between child providers - after UpdateTo");
            }
            catch (Exception)
            {
                badFields.Add(field.Name);
            }

        Assert.That(badFields, Is.Empty);
    }

    [Test]
    public void TestDuplicateTableInfos_Identical()
    {
        var t1 = WhenIHaveA<TableInfo>();
        var t2 = WhenIHaveA<TableInfo>();

        var cp = new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);

        cp.GetAllObjects(typeof(TableInfo), false).Contains(t1);
        cp.GetAllObjects(typeof(TableInfo), false).Contains(t2);

        var p1 = cp.GetDescendancyListIfAnyFor(t1).Parents;
        var p2 = cp.GetDescendancyListIfAnyFor(t2).Parents;

        // both objects should have identical path
        Assert.That(p2, Is.EqualTo(p1));
    }

    [Test]
    public void TestDuplicateTableInfos_DifferentServers()
    {
        var t1 = WhenIHaveA<TableInfo>();
        t1.Server = "127.0.0.1";

        var t2 = WhenIHaveA<TableInfo>();
        t2.Server = "localhost";

        var cp = new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);

        cp.GetAllObjects(typeof(TableInfo), false).Contains(t1);
        cp.GetAllObjects(typeof(TableInfo), false).Contains(t2);

        var p1 = cp.GetDescendancyListIfAnyFor(t1).Parents;
        var p2 = cp.GetDescendancyListIfAnyFor(t2).Parents;

        Assert.Multiple(() =>
        {
            // both objects should have identical path
            Assert.That(p1.SequenceEqual(p2), Is.False);

            Assert.That(p2[0], Is.EqualTo(p1[0])); // Data Repository Servers


            Assert.That(p1[1], Is.InstanceOf<TableInfoServerNode>());
            Assert.That(p2[1], Is.InstanceOf<TableInfoServerNode>());
        });
        Assert.Multiple(() =>
        {
            Assert.That(p2[1], Is.Not.EqualTo(p1[1])); // Server (e.g. localhost/127.0.0.1)

            Assert.That(p1[2], Is.InstanceOf<TableInfoDatabaseNode>());
            Assert.That(p2[2], Is.InstanceOf<TableInfoDatabaseNode>());
        });
        Assert.That(p2[2], Is.Not.EqualTo(p1[2])); // Database (must not be equal because the server is different!)
    }

    /// <summary>
    /// If two TableInfo differ by DatabaseType then they should have separate hierarchies.
    /// </summary>
    [Test]
    public void TestDuplicateTableInfos_DifferentServers_DatabaseTypeOnly()
    {
        var t1 = WhenIHaveA<TableInfo>();
        t1.DatabaseType = FAnsi.DatabaseType.MySql;

        var t2 = WhenIHaveA<TableInfo>();
        t2.DatabaseType = FAnsi.DatabaseType.PostgreSql;

        var cp = new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);

        cp.GetAllObjects(typeof(TableInfo), false).Contains(t1);
        cp.GetAllObjects(typeof(TableInfo), false).Contains(t2);

        var p1 = cp.GetDescendancyListIfAnyFor(t1).Parents;
        var p2 = cp.GetDescendancyListIfAnyFor(t2).Parents;

        Assert.Multiple(() =>
        {
            // both objects should have identical path
            Assert.That(p1.SequenceEqual(p2), Is.False);

            Assert.That(p2[0], Is.EqualTo(p1[0])); // Data Repository Servers


            Assert.That(p1[1], Is.InstanceOf<TableInfoServerNode>());
            Assert.That(p2[1], Is.InstanceOf<TableInfoServerNode>());
        });
        Assert.Multiple(() =>
        {
            Assert.That(p2[1], Is.Not.EqualTo(p1[1])); // Server (must not be equal because DatabaseType differs)

            Assert.That(p1[2], Is.InstanceOf<TableInfoDatabaseNode>());
            Assert.That(p2[2], Is.InstanceOf<TableInfoDatabaseNode>());
        });
        Assert.That(p2[2], Is.Not.EqualTo(p1[2])); // Database (must not be equal because the server is different!)
    }

    [Test]
    public void TestDuplicateTableInfos_DifferentDatabases()
    {
        var t1 = WhenIHaveA<TableInfo>();
        t1.Database = "Frank";

        var t2 = WhenIHaveA<TableInfo>();
        t2.Database = "Biff";

        var cp = new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);

        cp.GetAllObjects(typeof(TableInfo), false).Contains(t1);
        cp.GetAllObjects(typeof(TableInfo), false).Contains(t2);

        var p1 = cp.GetDescendancyListIfAnyFor(t1).Parents;
        var p2 = cp.GetDescendancyListIfAnyFor(t2).Parents;

        Assert.Multiple(() =>
        {
            // both objects should have identical path
            Assert.That(p1.SequenceEqual(p2), Is.False);

            Assert.That(p2[0], Is.EqualTo(p1[0])); // Data Repository Servers


            Assert.That(p1[1], Is.InstanceOf<TableInfoServerNode>());
            Assert.That(p2[1], Is.InstanceOf<TableInfoServerNode>());
        });
        Assert.Multiple(() =>
        {
            Assert.That(p2[1], Is.EqualTo(p1[1])); // Server

            Assert.That(p1[2], Is.InstanceOf<TableInfoDatabaseNode>());
            Assert.That(p2[2], Is.InstanceOf<TableInfoDatabaseNode>());
        });
        Assert.That(p2[2], Is.Not.EqualTo(p1[2])); // Database (i.e. Frank/Biff)
    }

    /// <summary>
    /// Capitalization changes are not considered different.  This test confirms that
    /// when user has 2 nodes that have SERVER names with different caps they get grouped
    /// together ok.
    /// </summary>
    [Test]
    public void TestDuplicateTableInfos_DifferentServers_CapsOnly()
    {
        var t1 = WhenIHaveA<TableInfo>();
        t1.Server = "LocalHost";

        var t2 = WhenIHaveA<TableInfo>();
        t2.Server = "localhost";

        var cp = new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);

        cp.GetAllObjects(typeof(TableInfo), false).Contains(t1);
        cp.GetAllObjects(typeof(TableInfo), false).Contains(t2);

        var p1 = cp.GetDescendancyListIfAnyFor(t1).Parents;
        var p2 = cp.GetDescendancyListIfAnyFor(t2).Parents;

        // both objects should have identical path
        Assert.That(p2, Is.EqualTo(p1));
    }

    /// <summary>
    /// Capitalization changes are not considered different.  This test confirms that
    /// when user has 2 nodes that have DATABASE names with different caps they get grouped
    /// together ok.
    /// </summary>
    [Test]
    public void TestDuplicateTableInfos_DifferentDatabases_CapsOnly()
    {
        var t1 = WhenIHaveA<TableInfo>();
        t1.Database = "frank";

        var t2 = WhenIHaveA<TableInfo>();
        t2.Database = "fRAnk";

        var cp = new DataExportChildProvider(RepositoryLocator, null, ThrowImmediatelyCheckNotifier.Quiet, null);

        cp.GetAllObjects(typeof(TableInfo), false).Contains(t1);
        cp.GetAllObjects(typeof(TableInfo), false).Contains(t2);

        var p1 = cp.GetDescendancyListIfAnyFor(t1).Parents;
        var p2 = cp.GetDescendancyListIfAnyFor(t2).Parents;

        // both objects should have identical path
        Assert.That(p2, Is.EqualTo(p1));
    }
}