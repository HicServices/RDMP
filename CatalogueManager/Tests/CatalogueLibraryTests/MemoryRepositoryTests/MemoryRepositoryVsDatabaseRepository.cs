// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Defaults;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using FAnsi;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace CatalogueLibraryTests.MemoryRepositoryTests
{
    class MemoryRepositoryVsDatabaseRepository:DatabaseTests
    {
        //Fields that can be safely ignored when comparing an object created in memory with one created into the database.
        private static readonly string[] IgnorePropertiesWhenDiffing = new[] {"ID","Repository","CatalogueRepository","SoftwareVersion"};
        
        [Test]
        public void TestMemoryRepository_CatalogueConstructor()
        {
            var memoryRepository = new MemoryCatalogueRepository(CatalogueRepository.GetServerDefaults());

            Catalogue memCatalogue = new Catalogue(memoryRepository, "My New Catalogue");
            Catalogue dbCatalogue = new Catalogue(CatalogueRepository,"My New Catalogue");

            AssertAreEqual(memCatalogue,dbCatalogue);
        }


        [Test]
        public void TestMemoryRepository_AggregateConfigurationConstructor()
        {
            var memoryRepository = new MemoryCatalogueRepository(CatalogueRepository.GetServerDefaults());

            Catalogue memCatalogue = new Catalogue(memoryRepository, "My New Catalogue");
            Catalogue dbCatalogue = new Catalogue(CatalogueRepository, "My New Catalogue");

            var memAggregate = new AggregateConfiguration(memoryRepository, memCatalogue, "My New Aggregate");
            var dbAggregate = new AggregateConfiguration(CatalogueRepository, dbCatalogue, "My New Aggregate");

            AssertAreEqual(memAggregate, dbAggregate);
        }
        
        [Test]
        public void TestMemoryRepository_LiveLogging()
        {
            var memoryRepository = new MemoryCatalogueRepository();

            var loggingServer = new ExternalDatabaseServer(memoryRepository, "My Logging Server");
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


            AssertAreEqual(memCatalogue,dbCatalogue);
            AssertAreEqual(memTableInfo,dbTableInfo);

            AssertAreEqual(memCatalogue.CatalogueItems,dbCatalogue.CatalogueItems);
            AssertAreEqual(memCatalogue.GetAllExtractionInformation(ExtractionCategory.Any), dbCatalogue.GetAllExtractionInformation(ExtractionCategory.Any));

            AssertAreEqual(memCatalogue.CatalogueItems.Select(ci => ci.ColumnInfo), dbCatalogue.CatalogueItems.Select(ci => ci.ColumnInfo));

        }

        Dictionary<PropertyInfo,HashSet<object>> _alreadyChecked = new Dictionary<PropertyInfo, HashSet<object>>();

        private void AssertAreEqual(IEnumerable<IMapsDirectlyToDatabaseTable> memObjects, IEnumerable<IMapsDirectlyToDatabaseTable> dbObjects)
        {
            var memObjectsArr = memObjects.OrderBy(o => o.ID).ToArray();
            var dbObjectsArr = dbObjects.OrderBy(o => o.ID).ToArray();

            Assert.AreEqual(memObjectsArr.Count(), dbObjectsArr.Count());

            for (int i = 0; i < memObjectsArr.Count(); i++)
                AssertAreEqual(memObjectsArr[i], dbObjectsArr[i]);
        }

        private void AssertAreEqual(IMapsDirectlyToDatabaseTable memObj, IMapsDirectlyToDatabaseTable dbObj)
        {
            foreach (PropertyInfo property in memObj.GetType().GetProperties())
            {
                if (IgnorePropertiesWhenDiffing.Contains(property.Name) || property.Name.EndsWith("_ID"))
                    continue;

                if (!_alreadyChecked.ContainsKey(property))
                    _alreadyChecked.Add(property,new HashSet<object>());

                //if we have already checked this property
                if(_alreadyChecked[property].Contains(memObj))
                    return; //don't check it again

                _alreadyChecked[property].Add(memObj);

                object memValue = null;
                object dbValue = null;
                try
                {
                    memValue = property.GetValue(memObj);
                }
                catch (Exception e)
                {
                    Assert.Fail("{0} Property {1} could not be read from Memory:\r\n{2}", memObj.GetType().Name, property.Name,e);
                }
                try
                {
                    dbValue = property.GetValue(dbObj);
                }
                catch (Exception e)
                {
                    Assert.Fail("{0} Property {1} could not be read from Database:\r\n{2}", dbObj.GetType().Name, property.Name, e);
                }

                if(memValue is IMapsDirectlyToDatabaseTable)
                {
                    AssertAreEqual((IMapsDirectlyToDatabaseTable)memValue, (IMapsDirectlyToDatabaseTable)dbValue);
                    return;
                }
                if (memValue is IEnumerable<IMapsDirectlyToDatabaseTable>)
                {
                    AssertAreEqual((IEnumerable<IMapsDirectlyToDatabaseTable>)memValue, (IEnumerable<IMapsDirectlyToDatabaseTable>)dbValue);
                    return;
                }

                if (memValue is DateTime && dbValue is DateTime)
                    if (!AreAboutTheSameTime((DateTime) memValue, (DateTime) dbValue))
                        Assert.Fail("Dates differed, {0} Property {1} differed Memory={2} and Db={3}",memObj.GetType().Name, property.Name, memValue, dbValue);
                    else
                        return;

                //all other properties should be legit
                Assert.AreEqual(memValue, dbValue, "{0} Property {1} differed Memory={2} and Db={3}", memObj.GetType().Name,property.Name, memValue, dbValue);
            }
        }

        private bool AreAboutTheSameTime(DateTime memValue, DateTime dbValue)
        {
            return Math.Abs(memValue.Subtract(dbValue).TotalSeconds) < 10;
        }
    }
}