using System;
using System.Data.SqlClient;
using System.Linq;
using FAnsi.Discovery;
using HIC.Logging.PastEvents;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace HIC.Logging.Tests.Integration
{
    public class LogManagerTest : DatabaseTests
    {
        private DataLoadTaskHelper _dataLoadTaskHelper;

        private IDataLoadInfo _failedLoad;
        private IDataLoadInfo _successfulLoad;
        private IDataLoadInfo _anotherSuccessfulLoad;

        private Exception _setupException;
        private string _dataLoadTaskName;
        private LogManager _logManager;

        /// <summary>
        /// Add a bunch of data load runs for the tests in this fixture
        /// </summary>
        protected override void SetUp()
        {
            try
            {
                base.SetUp();

                var lds = new DiscoveredServer(UnitTestLoggingConnectionString);

                _dataLoadTaskName = "LogTest";

                _dataLoadTaskHelper = new DataLoadTaskHelper(lds);
                _dataLoadTaskHelper.SetUp();

                try
                {
                    _dataLoadTaskHelper.CreateDataLoadTask(_dataLoadTaskName);
                }
                catch (Exception)
                {
                    Console.WriteLine("Could not create data load task called " + _dataLoadTaskName +" (probably because it already exists eh?)");
                }
                // Insert some data load runs that are used by all the tests
                _logManager = new LogManager(lds);

                _failedLoad = _logManager.CreateDataLoadInfo(_dataLoadTaskName, _dataLoadTaskName, _dataLoadTaskName, "", true);
                _failedLoad.LogFatalError("", "");
                _failedLoad.CloseAndMarkComplete();

                _successfulLoad = _logManager.CreateDataLoadInfo(_dataLoadTaskName, _dataLoadTaskName, _dataLoadTaskName, "", true);
                _successfulLoad.LogProgress(DataLoadInfo.ProgressEventType.OnProgress, "", "");
                _successfulLoad.CloseAndMarkComplete();

                var tableLoadInfo = _successfulLoad.CreateTableLoadInfo("ignoreme", "Nowhereland",
                    new DataSource[]
                    {new DataSource("Firehouse", DateTime.Now.AddDays(-1)), new DataSource("WaterHaus")}, 100);

                tableLoadInfo.Inserts = 500;
                tableLoadInfo.Updates = 100;
                tableLoadInfo.CloseAndArchive();

                _anotherSuccessfulLoad = _logManager.CreateDataLoadInfo(_dataLoadTaskName, _dataLoadTaskName, _dataLoadTaskName, "", true);
                _anotherSuccessfulLoad.LogProgress(DataLoadInfo.ProgressEventType.OnProgress, "", "");
                _anotherSuccessfulLoad.CloseAndMarkComplete();
            }
            catch (Exception e)
            {
                _setupException = e;
            }
        }

        [SetUp]
        protected void BeforeEachTest()
        {
            if (_setupException != null)
                Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(_setupException,true));
        }

        [OneTimeTearDown]
        protected void TearDown()
        {
            using (var conn = new SqlConnection(UnitTestLoggingConnectionString.ConnectionString))
            {
                conn.Open();
                new SqlCommand("DELETE FROM DataLoadRun", conn).ExecuteNonQuery();
            }

            _dataLoadTaskHelper.TearDown();  
        }

        [Test]
        public void TestLastLoadStatusassemblage()
        {
             var loadHistoryForTask = ArchivalDataLoadInfo.GetLoadHistoryForTask(_dataLoadTaskName, new DiscoveredServer(UnitTestLoggingConnectionString)).ToArray();

            Assert.Greater(loadHistoryForTask.Length , 0);//some records

            Assert.Greater(loadHistoryForTask.Count(load => load.Errors.Count > 0), 0);//some with some errors
            Assert.Greater(loadHistoryForTask.Count(load => load.Progress.Count > 0), 0);//some with some progress


            Assert.Greater(loadHistoryForTask.Count(load => load.TableLoadInfos.Count == 1), 0);//some with some table loads
            
            
            Console.WriteLine("Records fetched:"+loadHistoryForTask.Length);
            Console.WriteLine("Errors fetched:" + loadHistoryForTask.Aggregate(0, (p, c) => p + c.Errors.Count));
            Console.WriteLine("Progress fetched:" + loadHistoryForTask.Aggregate(0, (p, c) => p + c.Progress.Count));

        }

        [Test]
        public void TestLoggingVsDynamicSQLHacker()
        {
            CleanupTruncateCommand();

            Assert.AreEqual(0, _logManager.ListDataTasks().Count(t => t.Equals("','') Truncate Table Fishes")));
            _logManager.CreateNewLoggingTaskIfNotExists("','') Truncate Table Fishes");
            Assert.AreEqual(1,_logManager.ListDataTasks().Count(t => t.Equals("','') Truncate Table Fishes")));
             
            CleanupTruncateCommand();
            Assert.AreEqual(0, _logManager.ListDataTasks().Count(t => t.Equals("','') Truncate Table Fishes")));

        }

        private void CleanupTruncateCommand()
        {

            var lds = new DiscoveredServer(UnitTestLoggingConnectionString);
            using (var con = lds.GetConnection())
            {
                con.Open();
                lds.GetCommand("DELETE FROM DataLoadTask where LOWER(dataSetID) like '%truncate%'", con).ExecuteNonQuery();
                lds.GetCommand("DELETE FROM DataSet where LOWER(dataSetID) like '%truncate%'", con).ExecuteNonQuery();
            }
        }


        [Test]
        public void TestLastLoadStatusassemblage_Top1()
        {
            var loadHistoryForTask = ArchivalDataLoadInfo.GetLoadHistoryForTask(_dataLoadTaskName, new DiscoveredServer(UnitTestLoggingConnectionString), top1: true).ToArray();

            Assert.AreEqual(loadHistoryForTask.Length, 1);//some records

            Assert.Greater(loadHistoryForTask.Count(load => load.Progress.Count > 0), 0);//some with some progress
            
            Console.WriteLine("Records fetched:" + loadHistoryForTask.Length);
            Console.WriteLine("Errors fetched:" + loadHistoryForTask.Aggregate(0, (p, c) => p + c.Errors.Count));
            Console.WriteLine("Progress fetched:" + loadHistoryForTask.Aggregate(0, (p, c) => p + c.Progress.Count));


            int totalErrors = loadHistoryForTask.Sum(status => status.Errors.Count);
            Console.WriteLine("total errors:" + totalErrors);

        }


        [Test]
        public void TestLastLoadStatusassemblage_MostRecent()
        {
            var server = new DiscoveredServer(UnitTestLoggingConnectionString);
            LogManager lm = new LogManager(server);

            var mostRecent = lm.GetLoadStatusOf(PastEventType.MostRecent, _dataLoadTaskName);
            var all = ArchivalDataLoadInfo.GetLoadHistoryForTask(_dataLoadTaskName, server, top1: true).ToArray();


            foreach (ArchivalDataLoadInfo status in all)
                Assert.GreaterOrEqual(mostRecent.StartTime, status.StartTime);
        }

        [Test]
        public void TestLastLoadStatusassemblage_Lastsuccessful()
        {
            var server = new DiscoveredServer(UnitTestLoggingConnectionString);
            LogManager lm = new LogManager(server);

            var mostRecent = lm.GetLoadStatusOf(PastEventType.LastsuccessfulLoad, _dataLoadTaskName);
            var all = ArchivalDataLoadInfo.GetLoadHistoryForTask(_dataLoadTaskName, server, top1: true).ToArray();

            Assert.AreEqual(mostRecent.Errors.Count,0);
            
            foreach (ArchivalDataLoadInfo status in all)
                if(status.Errors.Count == 0)
                    Assert.GreaterOrEqual(mostRecent.StartTime, status.StartTime);
        }



    }
}
