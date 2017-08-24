using System;
using System.Data.SqlClient;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace HIC.Logging.Tests.Integration
{
    [TestFixture]
    class RowErrorLoggingTest : DatabaseTests
    {
        private DataLoadTaskHelper _dataLoadTaskHelper;

        protected override void SetUp()
        {
            base.SetUp();

            // nuke everything that a previous run or other test may have set up
            using (var conn = new SqlConnection(UnitTestLoggingConnectionString.ConnectionString))
            {
                conn.Open();

                new SqlCommand("DELETE FROM DataLoadRun", conn).ExecuteNonQuery();
                new SqlCommand("DELETE FROM DataLoadTask", conn).ExecuteNonQuery();
                new SqlCommand("DELETE FROM DataSet", conn).ExecuteNonQuery();
                new SqlCommand("DELETE FROM FatalError", conn).ExecuteNonQuery();
                new SqlCommand("DELETE FROM ProgressLog", conn).ExecuteNonQuery();
            }

            _dataLoadTaskHelper = new DataLoadTaskHelper(new DiscoveredServer(UnitTestLoggingConnectionString));
            _dataLoadTaskHelper.SetUp();

            _dataLoadTaskHelper.CreateDataLoadTask("Nothing");
        }

        [TestFixtureTearDown]
        protected void TearDown()
        {
            using (var conn = new SqlConnection(UnitTestLoggingConnectionString.ConnectionString))
            {
                conn.Open();
                new SqlCommand("DELETE FROM DataLoadRun", conn).ExecuteNonQuery();
            }

            _dataLoadTaskHelper.TearDown();
        }

        [TestCase]
        public void LogRowErrorTest()
        {
            //create a new imaginary load
            DataLoadInfo info = new DataLoadInfo("Nothing", "HICSSISLibraryTests.LogRowErrorTest",
                                                 "Test case for error row generation",
                                                 "No rollback is possible/required as no database rows are actually inserted",
                                                 true,
                                                 new DiscoveredServer(UnitTestLoggingConnectionString));

            DataSource[] ds = new DataSource[] {
                new DataSource("nothing", DateTime.Now) ,
                new DataSource("nowhere", DateTime.Now) 
            };

            TableLoadInfo tableinfo = new TableLoadInfo(info, "DELETE FROM monkeyfish", "monkeyfish", ds, -1);



             RowErrorLogging.GetInstance()
                           .LogRowError(tableinfo, RowErrorLogging.RowErrorType.Unknown,
                                        "Test error created by HICSSISLibraryTests.RowErrorLoggingTest.LogRowErrorTest",
                                        "nowhere", false,"unknown column!!");

             RowErrorLogging.GetInstance()
                            .LogRowError(tableinfo, RowErrorLogging.RowErrorType.Validation,
                                         "Test error created by HICSSISLibraryTests.RowErrorLoggingTest.LogRowErrorTest",
                                         "nowhere", false);

            //Put tests down here because the last thing we want is hanging records (even test ones)

            //make sure it was logged as a test
            Assert.IsTrue(info.IsTest);

            //make sure the error logged is recorded in the DataLoadInfo class
            Assert.IsTrue(tableinfo.ErrorRows == 2);

            //for each data source you should indicate where it was archived
            tableinfo.DataSources[0].Archive = "doesntexistButIsTest.txt";
            tableinfo.DataSources[1].Archive = "doesntexistButIsTest.txt";

            //mark the table load as complete
            tableinfo.CloseAndArchive();

            //mark the DataLoad as complete
            info.CloseAndMarkComplete();

            //check that the data load is now closed
            Assert.IsTrue(info.IsClosed);
        }
    }
}
