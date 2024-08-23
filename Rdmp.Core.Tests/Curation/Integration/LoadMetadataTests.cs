// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Checks.Checkers;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class LoadMetadataTests : DatabaseTests
{
    [Test]
    public void CreateNewAndGetBackFromDatabase()
    {
        var loadMetadata = new LoadMetadata(CatalogueRepository);

        try
        {
            loadMetadata.LocationOfForLoadingDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, loadMetadata.DefaultForLoadingPath);
            loadMetadata.LocationOfForArchivingDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, loadMetadata.DefaultForArchivingPath);
            loadMetadata.LocationOfExecutablesDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, loadMetadata.DefaultExecutablesPath);
            loadMetadata.LocationOfCacheDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, loadMetadata.DefaultCachePath);
            loadMetadata.SaveToDatabase();

            var loadMetadataWithIdAfterwards = CatalogueRepository.GetObjectByID<LoadMetadata>(loadMetadata.ID);
            Assert.That(Path.Combine(TestContext.CurrentContext.TestDirectory, loadMetadata.DefaultForLoadingPath), Is.EqualTo(loadMetadataWithIdAfterwards.LocationOfForLoadingDirectory));
            Assert.That(Path.Combine(TestContext.CurrentContext.TestDirectory, loadMetadata.DefaultForArchivingPath), Is.EqualTo(loadMetadataWithIdAfterwards.LocationOfForArchivingDirectory));
            Assert.That(Path.Combine(TestContext.CurrentContext.TestDirectory, loadMetadata.DefaultExecutablesPath), Is.EqualTo(loadMetadataWithIdAfterwards.LocationOfExecutablesDirectory));
            Assert.That(Path.Combine(TestContext.CurrentContext.TestDirectory, loadMetadata.DefaultCachePath), Is.EqualTo(loadMetadataWithIdAfterwards.LocationOfCacheDirectory));
        }
        finally
        {
            loadMetadata.DeleteInDatabase();
        }
    }

    [Test]
    public void Test_IgnoreTrigger_GetSet()
    {
        var loadMetadata = new LoadMetadata(CatalogueRepository);

        try
        {
            //default
            Assert.That(loadMetadata.IgnoreTrigger, Is.False);
            loadMetadata.SaveToDatabase();
            Assert.That(loadMetadata.IgnoreTrigger, Is.False);
            loadMetadata.SaveToDatabase();

            loadMetadata.IgnoreTrigger = true;
            Assert.That(loadMetadata.IgnoreTrigger);
            loadMetadata.RevertToDatabaseState();
            Assert.That(loadMetadata.IgnoreTrigger, Is.False);


            loadMetadata.IgnoreTrigger = true;
            Assert.That(loadMetadata.IgnoreTrigger);
            loadMetadata.SaveToDatabase();
            var lmd2 = RepositoryLocator.CatalogueRepository.GetObjectByID<LoadMetadata>(loadMetadata.ID);
            Assert.That(lmd2.IgnoreTrigger);
        }
        finally
        {
            loadMetadata.DeleteInDatabase();
        }
    }

    [Test]
    public void TestPreExecutionChecker_TablesDontExist()
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var tbl = db.ExpectTable("Imaginary");

        Assert.That(tbl.Exists(), Is.False);

        var lmd = RdmpMockFactory.Mock_LoadMetadataLoadingTable(tbl);
        var checker = new PreExecutionChecker(lmd, new HICDatabaseConfiguration(db.Server));
        var ex = Assert.Throws<Exception>(() => checker.Check(ThrowImmediatelyCheckNotifier.Quiet));

        Assert.That(ex.Message, Does.Match("Table '.*Imaginary.*' does not exist"));
    }

    [Test]
    public void TestPreExecutionChecker_TableIsTableValuedFunction()
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var f = new TestableTableValuedFunction();
        f.Create(db, CatalogueRepository);

        var tbl = f.TableInfoCreated.Discover(DataAccessContext.DataLoad);
        Assert.That(tbl.Exists());

        var lmd = RdmpMockFactory.Mock_LoadMetadataLoadingTable(f.TableInfoCreated);
        var checker = new PreExecutionChecker(lmd, new HICDatabaseConfiguration(db.Server));
        var ex = Assert.Throws<Exception>(() => checker.Check(ThrowImmediatelyCheckNotifier.Quiet));

        Assert.That(ex.Message, Does.Match("Table '.*MyAwesomeFunction.*' is a TableValuedFunction"));
    }
}