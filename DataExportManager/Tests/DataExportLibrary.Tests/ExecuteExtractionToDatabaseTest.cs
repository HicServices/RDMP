using System;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using MapsDirectlyToDatabaseTable;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace DataExportLibrary.Tests
{
    [Category("Database")]
    public class ExecuteExtractionToDatabaseTest : DatabaseTests
    {
        private TestDatabaseHelper _testDatabaseHelper;
        private TestDatabaseHelper _extractedCatalogueDatabaseHelper;

        [TestFixtureSetUp]
        protected override void SetUp()
        {
            base.SetUp();
        }

        [TestFixtureTearDown]
        protected void TearDown()
        {
            if (_testDatabaseHelper != null)
                _testDatabaseHelper.Destroy();

            if (_extractedCatalogueDatabaseHelper != null)
                _extractedCatalogueDatabaseHelper.Destroy();
        }

        [SetUp]
        public void BeforeEachTest()
        {
            // We will drop and recreate the databases before ever test (as this was the previous functionality)
            if (_testDatabaseHelper != null)
                _testDatabaseHelper.Destroy();

            _testDatabaseHelper = new TestDatabaseHelper(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("ExecuteExtractionToDatabaseTestDatabase"));
            
            if (_extractedCatalogueDatabaseHelper != null)
                _extractedCatalogueDatabaseHelper.Destroy();

            _extractedCatalogueDatabaseHelper = new TestDatabaseHelper(DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("ExecuteExtractionToDatabaseTestCatalogue"));
            _extractedCatalogueDatabaseHelper.Create();
        }
    }
}
