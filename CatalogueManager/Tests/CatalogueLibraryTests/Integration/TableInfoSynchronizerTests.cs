// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CatalogueLibrary;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.DataHelper;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class TableInfoSynchronizerTests:DatabaseTests
    {
        private DiscoveredServer _server;
        private TableInfo tableInfoCreated;
        private ColumnInfo[] columnInfosCreated;

        private const string TABLE_NAME = "TableInfoSynchronizerTests";

        [SetUp]
        public void CreateDataset()
        {
            _server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;

            using (var con = _server.GetConnection())
            {
                con.Open();
                _server.GetCommand("CREATE TABLE " + TABLE_NAME + "(Name varchar(10), Address varchar(500))",con).ExecuteNonQuery();
            }

            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("TableInfoSynchronizerTests");
            
            TableInfoImporter importer = new TableInfoImporter(CatalogueRepository,tbl);
            importer.DoImport(out tableInfoCreated,out columnInfosCreated);
        }

        [Test]
        public void SynchronizationTests_NoChanges()
        {
            Assert.AreEqual(TABLE_NAME , tableInfoCreated.GetRuntimeName());
            
            TableInfoSynchronizer synchronizer = new TableInfoSynchronizer(tableInfoCreated);
            Assert.AreEqual(true,synchronizer.Synchronize(new ThrowImmediatelyCheckNotifier()));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SynchronizationTests_ColumnDropped(bool acceptChanges)
        {
            Assert.AreEqual(TABLE_NAME, tableInfoCreated.GetRuntimeName());

            var table = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable(TABLE_NAME);
            var colToDrop = table.DiscoverColumn("Address");
            table.DropColumn(colToDrop);
            
            TableInfoSynchronizer synchronizer = new TableInfoSynchronizer(tableInfoCreated);

            if (acceptChanges)
            {
                //accept changes should result in a synchronized table
                Assert.AreEqual(true,synchronizer.Synchronize(new AcceptAllCheckNotifier()));
                Assert.AreEqual(1,tableInfoCreated.ColumnInfos.Length);//should only be 1 remaining 
            }
            else
            {
                var ex = Assert.Throws<Exception>(() => synchronizer.Synchronize(new ThrowImmediatelyCheckNotifier()));
                Assert.AreEqual("The ColumnInfo Address no longer appears in the live table.", ex.Message);    
            }
            
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SynchronizationTests_ColumnAdded(bool acceptChanges)
        {
            using (var con = DiscoveredDatabaseICanCreateRandomTablesIn.Server.GetConnection())
            {
                con.Open();
                _server.GetCommand("ALTER TABLE " + TABLE_NAME + " ADD Birthday datetime not null", con).ExecuteNonQuery();
            }


            TableInfoSynchronizer synchronizer = new TableInfoSynchronizer(tableInfoCreated);

            if (acceptChanges)
            {
                //accept changes should result in a synchronized table
                Assert.AreEqual(true, synchronizer.Synchronize(new AcceptAllCheckNotifier()));
                Assert.AreEqual(3, tableInfoCreated.ColumnInfos.Length);//should 3 now
            }
            else
            {
                var ex = Assert.Throws<Exception>(() => synchronizer.Synchronize(new ThrowImmediatelyCheckNotifier()));
                Assert.AreEqual("The following columns are missing from the TableInfo:Birthday", ex.Message);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SynchronizationTests_ColumnAddedWithCatalogue(bool acceptChanges)
        {
            ForwardEngineerCatalogue cataEngineer = new ForwardEngineerCatalogue(tableInfoCreated,columnInfosCreated,true);

            Catalogue cata;
            CatalogueItem[] cataItems;
            ExtractionInformation[] extractionInformations;
            
            cataEngineer.ExecuteForwardEngineering(out cata, out cataItems, out extractionInformations);

            try
            {
                Assert.AreEqual(TABLE_NAME,cata.Name);
                Assert.AreEqual(2, cataItems.Length);
                Assert.AreEqual(2, extractionInformations.Length);
            
                using (var con = DiscoveredDatabaseICanCreateRandomTablesIn.Server.GetConnection())
                {
                    con.Open();
                    _server.GetCommand("ALTER TABLE " + TABLE_NAME + " ADD Birthday datetime not null", con).ExecuteNonQuery();
                }
            
                TableInfoSynchronizer synchronizer = new TableInfoSynchronizer(tableInfoCreated);

                if (acceptChanges)
                {
                    //accept changes should result in a synchronized table
                    Assert.AreEqual(true, synchronizer.Synchronize(new AcceptAllCheckNotifier()));
                    Assert.AreEqual(3, tableInfoCreated.ColumnInfos.Length);//should 3 now
                    Assert.AreEqual(3, cata.CatalogueItems.Length);//should 3 now
                    Assert.AreEqual(3, cata.GetAllExtractionInformation(ExtractionCategory.Any).Length);//should 3 now

                    Assert.AreEqual(1,cata.GetAllExtractionInformation(ExtractionCategory.Any).Count(e=>e.SelectSQL.Contains("Birthday")));
                    Assert.AreEqual(1,cata.CatalogueItems.Count(ci => ci.Name.Contains("Birthday")));
                }
                else
                {
                    var ex = Assert.Throws<Exception>(() => synchronizer.Synchronize(new ThrowImmediatelyCheckNotifier()));
                    Assert.AreEqual("The following columns are missing from the TableInfo:Birthday", ex.Message);
                }
            }
            finally
            {
                cata.DeleteInDatabase();
            }
        }



        [TearDown]
        public void DropTables()
        {
            var credentials = (DataAccessCredentials)tableInfoCreated.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);

            //if credentials were created, we should only be one user
            if(credentials != null)
                Assert.AreEqual(1,credentials.GetAllTableInfosThatUseThis().Count());
            
            //delete the table
            tableInfoCreated.DeleteInDatabase();

            //also delete any credentials created as part of TableInfoImport
            if(credentials != null)
                credentials.DeleteInDatabase();

            DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable(TABLE_NAME).Drop();
        }

    }
}
