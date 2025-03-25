// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.MemoryRepositoryTests;

internal class MemoryRepositoryVsDatabaseRepository : DatabaseTests
{
    [Test]
    public void TestMemoryVsDatabaseRepository_CatalogueConstructor()
    {
        var memoryRepository = new MemoryCatalogueRepository(CatalogueRepository);

        var memCatalogue = new Catalogue(memoryRepository, "My New Catalogue");
        var dbCatalogue = new Catalogue(CatalogueRepository, "My New Catalogue");

        UnitTests.AssertAreEqual(memCatalogue, dbCatalogue);
    }

    [Test]
    public void TestMemoryVsDatabaseRepository_ProcessTaskConstructor()
    {
        var memoryRepository = new MemoryCatalogueRepository(CatalogueRepository);

        var memLmd = new LoadMetadata(memoryRepository, "My New Load");
        var dbLmd = new LoadMetadata(CatalogueRepository, "My New Load");

        UnitTests.AssertAreEqual(memLmd, dbLmd);

        var memPt = new ProcessTask(memoryRepository, memLmd, LoadStage.AdjustRaw) { Name = "MyPt" };
        var dbPt = new ProcessTask(CatalogueRepository, dbLmd, LoadStage.AdjustRaw) { Name = "MyPt" };

        UnitTests.AssertAreEqual(memPt, dbPt);
    }


    [Test]
    public void TestMemoryRepository_AggregateConfigurationConstructor()
    {
        var memoryRepository = new MemoryCatalogueRepository(CatalogueRepository);

        var memCatalogue = new Catalogue(memoryRepository, "My New Catalogue");
        var dbCatalogue = new Catalogue(CatalogueRepository, "My New Catalogue");

        var memAggregate = new AggregateConfiguration(memoryRepository, memCatalogue, "My New Aggregate");
        var dbAggregate = new AggregateConfiguration(CatalogueRepository, dbCatalogue, "My New Aggregate");

        UnitTests.AssertAreEqual(memAggregate, dbAggregate);
    }

    [Test]
    public void TestMemoryRepository_LiveLogging()
    {
        var memoryRepository = new MemoryCatalogueRepository();

        var loggingServer = new ExternalDatabaseServer(memoryRepository, "My Logging Server", null);
        memoryRepository.SetDefault(PermissableDefaults.LiveLoggingServer_ID, loggingServer);

        var memCatalogue = new Catalogue(memoryRepository, "My New Catalogue");
        Assert.That(loggingServer.ID, Is.EqualTo(memCatalogue.LiveLoggingServer_ID));
    }

    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void TestImportingATable(DatabaseType dbType)
    {
        var dt = new DataTable();
        dt.Columns.Add("Do");
        dt.Columns.Add("Ray");
        dt.Columns.Add("Me");
        dt.Columns.Add("Fa");
        dt.Columns.Add("So");

        var db = GetCleanedServer(dbType);
        var tbl = db.CreateTable("OmgTables", dt);

        var memoryRepository = new MemoryCatalogueRepository(CatalogueRepository);

        var importer1 = new TableInfoImporter(memoryRepository, tbl);

        importer1.DoImport(out var memTableInfo, out var memColumnInfos);
        var forwardEngineer1 = new ForwardEngineerCatalogue(memTableInfo, memColumnInfos);
        forwardEngineer1.ExecuteForwardEngineering(out var memCatalogue, out _,
            out _);


        var importerdb = new TableInfoImporter(CatalogueRepository, tbl);
        importerdb.DoImport(out var dbTableInfo, out var dbColumnInfos);
        var forwardEngineer2 = new ForwardEngineerCatalogue(dbTableInfo, dbColumnInfos);
        forwardEngineer2.ExecuteForwardEngineering(out var dbCatalogue, out _,
            out _);


        UnitTests.AssertAreEqual(memCatalogue, dbCatalogue);
        UnitTests.AssertAreEqual(memTableInfo, dbTableInfo);

        UnitTests.AssertAreEqual(memCatalogue.CatalogueItems, dbCatalogue.CatalogueItems);
        UnitTests.AssertAreEqual(memCatalogue.GetAllExtractionInformation(ExtractionCategory.Any),
            dbCatalogue.GetAllExtractionInformation(ExtractionCategory.Any));

        UnitTests.AssertAreEqual(memCatalogue.CatalogueItems.Select(ci => ci.ColumnInfo),
            dbCatalogue.CatalogueItems.Select(ci => ci.ColumnInfo));
    }
}