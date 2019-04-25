// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CatalogueLibrary;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Aggregation;
using Rdmp.Core.CatalogueLibrary.Data.DataLoad;
using Rdmp.Core.CatalogueLibrary.Data.Defaults;
using Rdmp.Core.CatalogueLibrary.DataHelper;
using Rdmp.Core.CatalogueLibrary.Repositories;
using ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace Rdmp.Core.Tests.CatalogueLibrary.MemoryRepositoryTests
{
    class MemoryRepositoryVsDatabaseRepository:DatabaseTests
    {
        [Test]
        public void TestMemoryVsDatabaseRepository_CatalogueConstructor()
        {
            var memoryRepository = new MemoryCatalogueRepository(CatalogueRepository.GetServerDefaults());

            Catalogue memCatalogue = new Catalogue(memoryRepository, "My New Catalogue");
            Catalogue dbCatalogue = new Catalogue(CatalogueRepository,"My New Catalogue");
            
            UnitTests.AssertAreEqual(memCatalogue,dbCatalogue);
        }

        [Test]
        public void TestMemoryVsDatabaseRepository_ProcessTaskConstructor()
        {
            var memoryRepository = new MemoryCatalogueRepository(CatalogueRepository.GetServerDefaults());

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
            var memoryRepository = new MemoryCatalogueRepository(CatalogueRepository.GetServerDefaults());

            Catalogue memCatalogue = new Catalogue(memoryRepository, "My New Catalogue");
            Catalogue dbCatalogue = new Catalogue(CatalogueRepository, "My New Catalogue");

            var memAggregate = new AggregateConfiguration(memoryRepository, memCatalogue, "My New Aggregate");
            var dbAggregate = new AggregateConfiguration(CatalogueRepository, dbCatalogue, "My New Aggregate");

            UnitTests.AssertAreEqual(memAggregate, dbAggregate);
        }
        
        [Test]
        public void TestMemoryRepository_LiveLogging()
        {
            var memoryRepository = new MemoryCatalogueRepository();

            var loggingServer = new ExternalDatabaseServer(memoryRepository, "My Logging Server",null);
            memoryRepository.SetDefault(PermissableDefaults.LiveLoggingServer_ID, loggingServer);

            Catalogue memCatalogue = new Catalogue(memoryRepository, "My New Catalogue");
            Assert.AreEqual(memCatalogue.LiveLoggingServer_ID,loggingServer.ID);
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MySql)]
        public void TestImportingATable(DatabaseType dbType)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Do");
            dt.Columns.Add("Ray");
            dt.Columns.Add("Me");
            dt.Columns.Add("Fa");
            dt.Columns.Add("So");

            var db = GetCleanedServer(dbType);
            var tbl = db.CreateTable("OmgTables",dt);

            var memoryRepository = new MemoryCatalogueRepository(CatalogueRepository.GetServerDefaults());
            
            var importer1 = new TableInfoImporter(memoryRepository, tbl, DataAccessContext.Any);

            TableInfo memTableInfo;
            ColumnInfo[] memColumnInfos;
            Catalogue memCatalogue;
            CatalogueItem[] memCatalogueItems;
            ExtractionInformation[] memExtractionInformations;

            importer1.DoImport(out memTableInfo,out memColumnInfos);
            var forwardEngineer1 = new ForwardEngineerCatalogue(memTableInfo, memColumnInfos);
            forwardEngineer1.ExecuteForwardEngineering(out memCatalogue,out memCatalogueItems,out memExtractionInformations);


            TableInfo dbTableInfo;
            ColumnInfo[] dbColumnInfos;
            Catalogue dbCatalogue;
            CatalogueItem[] dbCatalogueItems;
            ExtractionInformation[] dbExtractionInformations;

            var importerdb = new TableInfoImporter(CatalogueRepository, tbl, DataAccessContext.Any);
            importerdb.DoImport(out dbTableInfo, out dbColumnInfos);
            var forwardEngineer2 = new ForwardEngineerCatalogue(dbTableInfo, dbColumnInfos);
            forwardEngineer2.ExecuteForwardEngineering(out dbCatalogue, out dbCatalogueItems, out dbExtractionInformations);


            UnitTests.AssertAreEqual(memCatalogue,dbCatalogue);
            UnitTests.AssertAreEqual(memTableInfo,dbTableInfo);

            UnitTests.AssertAreEqual(memCatalogue.CatalogueItems,dbCatalogue.CatalogueItems);
            UnitTests.AssertAreEqual(memCatalogue.GetAllExtractionInformation(ExtractionCategory.Any), dbCatalogue.GetAllExtractionInformation(ExtractionCategory.Any));

            UnitTests.AssertAreEqual(memCatalogue.CatalogueItems.Select(ci => ci.ColumnInfo), dbCatalogue.CatalogueItems.Select(ci => ci.ColumnInfo));

        }

        

        
    }
}