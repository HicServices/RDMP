using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Triggers;
using FAnsi.Discovery;
using LoadModules.Generic.Mutilators.QueryBuilders;
using NUnit.Framework;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class BackfillSqlHelperTests : DatabaseTests
    {
        private DiscoveredDatabase _stagingDatabase;
        private DiscoveredDatabase _liveDatabase;
        private Catalogue _catalogue;

        private const string DatabaseName = "BackfillSqlHelperTests";

        #region Housekeeping

        [SetUp]
        public void BeforeEachTest()
        {
            _catalogue = CatalogueRepository.GetAllObjects<Catalogue>("WHERE Name='BackfillSqlHelperTests'").SingleOrDefault();
            if (_catalogue != null)
            {
                // Previous test run has not exited cleanly
                foreach (var ti in _catalogue.GetTableInfoList(false))
                    ti.DeleteInDatabase();

                _catalogue.DeleteInDatabase();
            }

            _stagingDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(DatabaseName + "_STAGING");
            _liveDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(DatabaseName);
            
            // ensure the test staging and live databases are empty
            _stagingDatabase.Create(true);
            _liveDatabase.Create(true);
            
            CleanCatalogueDatabase();
        }

        private void CleanCatalogueDatabase()
        {
            if (_catalogue != null)
                _catalogue.DeleteInDatabase();

            // ensure the database is cleared of test remnants
            foreach (var ji in CatalogueRepository.JoinInfoFinder.GetAllJoinInfos())
                ji.DeleteInDatabase();

            // column infos don't appear to delete
            foreach (var ci in CatalogueRepository.GetAllObjects<ColumnInfo>())
                ci.DeleteInDatabase();

            foreach (var ti in CatalogueRepository.GetAllObjects<TableInfo>())
                ti.DeleteInDatabase();

            foreach (var credentials in CatalogueRepository.GetAllObjects<DataAccessCredentials>())
                credentials.DeleteInDatabase();
        }

        [TearDown]
        public void AfterEachTest()
        {
            _stagingDatabase.Drop();
            _liveDatabase.Drop();

            CleanCatalogueDatabase();
        }
        #endregion

        [Test]
        public void TestGenerateSqlForThreeLevelJoinPath_TimePeriodIsGrandparent()
        {
            ThreeTableSetupWhereTimePeriodIsGrandparent();

            var ciTimePeriodicity = CatalogueRepository.GetAllObjects<ColumnInfo>().SingleOrDefault(c => c.GetRuntimeName().Equals("HeaderDate"));
            if (ciTimePeriodicity == null)
                throw new InvalidOperationException("Could not find TimePeriodicity column");

            var sqlHelper = new BackfillSqlHelper(ciTimePeriodicity, _stagingDatabase, _liveDatabase);

            var tiHeader = CatalogueRepository.GetAllObjects<TableInfo>().Single(t=>t.GetRuntimeName().Equals("Headers"));
            var tiSamples = CatalogueRepository.GetAllObjects<TableInfo>().Single(t => t.GetRuntimeName().Equals("Samples"));
            var tiResults = CatalogueRepository.GetAllObjects<TableInfo>().Single(t => t.GetRuntimeName().Equals("Results"));

            var joinInfos = CatalogueRepository.JoinInfoFinder.GetAllJoinInfos();
            var joinPath = new List<JoinInfo>
            {
                joinInfos.Single(info => info.PrimaryKey.TableInfo_ID == tiHeader.ID),
                joinInfos.Single(info => info.PrimaryKey.TableInfo_ID == tiSamples.ID)
            };

            var sql = sqlHelper.CreateSqlForJoinToTimePeriodicityTable("CurrentTable", tiResults, "TimePeriodicityTable", _stagingDatabase, joinPath);

            Assert.AreEqual(@"SELECT CurrentTable.*, TimePeriodicityTable.HeaderDate AS TimePeriodicityField 
FROM [BackfillSqlHelperTests_STAGING]..[Results] CurrentTable
LEFT JOIN [BackfillSqlHelperTests_STAGING]..[Samples] j1 ON j1.ID = CurrentTable.SampleID
LEFT JOIN [BackfillSqlHelperTests_STAGING]..[Headers] TimePeriodicityTable ON TimePeriodicityTable.ID = j1.HeaderID", sql);
        }

        private void ThreeTableSetupWhereTimePeriodIsGrandparent()
        {
            CreateTables("Headers", "ID int NOT NULL, HeaderDate DATETIME, Discipline varchar(32)", "ID");
            CreateTables("Samples", "ID int NOT NULL, HeaderID int NOT NULL, SampleType varchar(32)", "ID", "CONSTRAINT [FK_Headers_Samples] FOREIGN KEY (HeaderID) REFERENCES Headers (ID)");
            CreateTables("Results", "ID int NOT NULL, SampleID int NOT NULL, Result int", "ID", "CONSTRAINT [FK_Samples_Results] FOREIGN KEY (SampleID) REFERENCES Samples (ID)");

            // Set up catalogue entities
            ColumnInfo[] ciHeaders;
            ColumnInfo[] ciSamples;
            ColumnInfo[] ciResults;

            var tiHeaders = AddTableToCatalogue(DatabaseName, "Headers", "ID", out ciHeaders, true);
            AddTableToCatalogue(DatabaseName, "Samples", "ID", out ciSamples);
            AddTableToCatalogue(DatabaseName, "Results", "ID", out ciResults);

            _catalogue.Time_coverage = "[Headers].[HeaderDate]";
            _catalogue.SaveToDatabase();

            tiHeaders.IsPrimaryExtractionTable = true;
            tiHeaders.SaveToDatabase();

            Assert.AreEqual(15, _catalogue.CatalogueItems.Count(), "Unexpected number of items in catalogue");

            // Headers (1:M) Samples join
            CatalogueRepository.JoinInfoFinder.AddJoinInfo(ciSamples.Single(ci => ci.GetRuntimeName().Equals("HeaderID")),
                ciHeaders.Single(ci => ci.GetRuntimeName().Equals("ID")),
                ExtractionJoinType.Left, "");

            // Samples (1:M) Results join
            CatalogueRepository.JoinInfoFinder.AddJoinInfo(ciResults.Single(info => info.GetRuntimeName().Equals("SampleID")),
                ciSamples.Single(info => info.GetRuntimeName().Equals("ID")),
                ExtractionJoinType.Left, "");
        }

        private void CreateTables(string tableName, string columnDefinitions, string pkColumn, string fkConstraintString = null)
        {
            // todo: doesn't do combo primary keys yet

            if (pkColumn == null || string.IsNullOrWhiteSpace(pkColumn))
                throw new InvalidOperationException("Primary Key column is required.");

            var pkConstraint = String.Format("CONSTRAINT PK_{0} PRIMARY KEY ({1})", tableName, pkColumn);
            var stagingTableDefinition = columnDefinitions + ", " + pkConstraint;
            var liveTableDefinition = columnDefinitions + String.Format(", "+SpecialFieldNames.ValidFrom+" DATETIME, "+SpecialFieldNames.DataLoadRunID+" int, " + pkConstraint);

            if (fkConstraintString != null)
            {
                stagingTableDefinition += ", " + fkConstraintString;
                liveTableDefinition += ", " + fkConstraintString;
            }

            CreateTableWithColumnDefinitions(_stagingDatabase,tableName, stagingTableDefinition);
            CreateTableWithColumnDefinitions(_liveDatabase,tableName, liveTableDefinition);
        }

        private TableInfo AddTableToCatalogue(string databaseName, string tableName, string pkName, out ColumnInfo[] ciList, bool createCatalogue = false)
        {
            TableInfo ti;

            var expectedTable = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(databaseName).ExpectTable(tableName);

            var resultsImporter = new TableInfoImporter(CatalogueRepository, expectedTable);

            resultsImporter.DoImport(out ti, out ciList);

            var pkResult = ciList.Single(info => info.GetRuntimeName().Equals(pkName));
            pkResult.IsPrimaryKey = true;
            pkResult.SaveToDatabase();

            var forwardEngineer = new ForwardEngineerCatalogue(ti, ciList);
            if (createCatalogue)
            {
                CatalogueItem[] cataItems;
                ExtractionInformation[] extractionInformations;

                forwardEngineer.ExecuteForwardEngineering(out _catalogue, out cataItems, out extractionInformations);
            }
            else
                forwardEngineer.ExecuteForwardEngineering(_catalogue);

            return ti;
        }

        public void CreateTableWithColumnDefinitions(DiscoveredDatabase db, string tableName, string columnDefinitions)
        {
            using (var conn = db.Server.GetConnection())
            {
                conn.Open();
                CreateTableWithColumnDefinitions(db,tableName, columnDefinitions, conn);
            }
        }

        public void CreateTableWithColumnDefinitions(DiscoveredDatabase db, string tableName, string columnDefinitions, DbConnection conn)
        {
            var sql = "CREATE TABLE " + tableName + " (" + columnDefinitions + ")";
            db.Server.GetCommand(sql, conn).ExecuteNonQuery();
        }
    }
}