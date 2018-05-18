using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.DatabaseManagement.Operations;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    class DatabaseOperationTests : DatabaseTests
    {
        Stack<IDeleteable> toCleanUp = new Stack<IDeleteable>();

        [Test]
        // This no longer copies between servers, but the original test didn't guarantee that would happen anyway
        public void CloneDatabaseAndTable()
        {
            string testLiveDatabaseName = TestDatabaseNames.GetConsistentName("TEST");
            
            var testDb = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(testLiveDatabaseName);
            var raw = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(testLiveDatabaseName + "_RAW");

            foreach (DiscoveredDatabase db in new[] { raw ,testDb})
                if (db.Exists())
                {
                    foreach (DiscoveredTable table in db.DiscoverTables(true))
                        table.Drop();

                    db.Drop();
                }
        
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.CreateDatabase(testLiveDatabaseName);
            Assert.IsTrue(testDb.Exists());

            testDb.CreateTable("Table_1", new[] {new DatabaseColumnRequest("Id", "int")});
            

            //clone the builder
            var builder = new SqlConnectionStringBuilder(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Builder.ConnectionString)
            {
                InitialCatalog = testLiveDatabaseName
            };
            
            var dbConfiguration = new HICDatabaseConfiguration(new DiscoveredServer(builder),null,new ServerDefaults(CatalogueRepository));
            
            var cloner = new DatabaseCloner(dbConfiguration);
            try
            {
                var cloneDb = cloner.CreateDatabaseForStage(LoadBubble.Raw);

                //confirm database appeared
                Assert.IsTrue(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(testLiveDatabaseName + "_RAW").Exists());

                //now create a catalogue and wire it up to the table TEST on the test database server 
                Catalogue cata = SetupATestCatalogue(builder, testLiveDatabaseName, "Table_1"); 

                //now clone the catalogue data structures to MachineName
                foreach (TableInfo tableInfo in cata.GetTableInfoList(false))
                    cloner.CreateTablesInDatabaseFromCatalogueInfo(tableInfo, LoadBubble.Raw);
                
                Assert.IsTrue(raw.Exists());
                Assert.IsTrue(raw.ExpectTable("Table_1").Exists());

            }
            finally
            {
                cloner.LoadCompletedSoDispose(ExitCodeType.Success, new ThrowImmediatelyDataLoadEventListener());

                while (toCleanUp.Count > 0)
                    try
                    {
                        toCleanUp.Pop().DeleteInDatabase();
                    }
                    catch (Exception e)
                    {
                        //always clean up everything 
                        Console.WriteLine(e);
                    }
            }
        }

        private Catalogue SetupATestCatalogue(SqlConnectionStringBuilder builder, string database, string table)
        {
            //create a new catalogue for test data (in the test data catalogue)
            var cat = new Catalogue(CatalogueRepository, "DeleteMe");
            TableInfoImporter importer = new TableInfoImporter(CatalogueRepository, builder.DataSource, database, table, DatabaseType.MicrosoftSQLServer, builder.UserID, builder.Password);

            TableInfo tableInfo;
            ColumnInfo[] columnInfos;
            importer.DoImport(out tableInfo, out columnInfos);

            toCleanUp.Push(cat);

            //push the credentials if there are any
            var creds = tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
            if (creds != null)
                toCleanUp.Push(creds);
            
            //and the TableInfo
            toCleanUp.Push(tableInfo);
            
            //for each column we will add a new one to the 
            foreach (ColumnInfo col in columnInfos)
            {
                //create it with the same name
                var cataItem = new CatalogueItem(CatalogueRepository, cat, col.Name.Substring(col.Name.LastIndexOf(".") + 1).Trim('[', ']', '`'));
                toCleanUp.Push(cataItem);

                cataItem.SetColumnInfo(col);

                toCleanUp.Push(col);
            }


            return cat;
        }
    }
}
