using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using FAnsi.Discovery;
using NUnit.Framework;
using Tests.Common;

namespace HIC.Logging.Tests.Integration
{
    
    class FatalErrorLoggingTest : DatabaseTests
    {
        [OneTimeTearDown]
        public void TearDown()
        {
            using (var conn = new SqlConnection(UnitTestLoggingConnectionString.ConnectionString))
            {
                conn.Open();

                new SqlCommand("DELETE FROM DataLoadRun", conn).ExecuteNonQuery();
            }
        }

        [TestCase]
        public void CreateNewDataLoadTask()
        {
            
            LogManager lm = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));
            
            lm.CreateNewLoggingTaskIfNotExists("Fish");

            Assert.Contains("Fish",lm.ListDataTasks());
            Assert.Contains("Fish",lm.ListDataSets());

            lm.CreateNewLoggingTaskIfNotExists("Fish");
            lm.CreateNewLoggingTaskIfNotExists("Fish");
            lm.CreateNewLoggingTaskIfNotExists("Fish");
        }

        [TestCase]
        public void FataErrorLoggingTest()
        {
            DataLoadInfo d = new DataLoadInfo("Internal", "HICSSISLibraryTests.FataErrorLoggingTest",
                "Test case for fatal error generation",
                "No rollback is possible/required as no database rows are actually inserted",
                true, new DiscoveredServer(UnitTestLoggingConnectionString));
           
            DataSource[] ds = new DataSource[]{ new DataSource("nothing",DateTime.Now)};

            

            TableLoadInfo t = new TableLoadInfo(d, "Unit test only", "Unit test only", ds, 5);
            t.Inserts += 3; //simulate that it crashed after 3

            d.LogFatalError("HICSSISLibraryTests.FataErrorLoggingTest","Some terrible event happened");

            Assert.IsTrue(d.IsClosed);
        }

        [Test]
        public void MD5Test()
        {
            string fileContents = "TestStringThatCouldBeSomethingInAFile";
            byte[] hashAsBytes;

            MemoryStream memory = new MemoryStream();
            StreamWriter writeToMemory = new StreamWriter(memory);
            writeToMemory.Write(fileContents);
            memory.Flush();
            memory.Position = 0;

            using (var md5 = MD5.Create())
            {
                hashAsBytes = md5.ComputeHash(memory);    
            }

            DataSource[] ds = new DataSource[] { new DataSource("nothing", DateTime.Now) };

            ds[0].MD5 = hashAsBytes; //MD5 is a property so confirm write and read are the same - and dont bomb

            Assert.AreEqual(ds[0].MD5, hashAsBytes);

            DataLoadInfo d = new DataLoadInfo("Internal", "HICSSISLibraryTests.FataErrorLoggingTest",
                                              "Test case for fatal error generation",
                                              "No rollback is possible/required as no database rows are actually inserted",
                                              true,
                                              new DiscoveredServer(UnitTestLoggingConnectionString));

            TableLoadInfo t = new TableLoadInfo(d, "Unit test only", "Unit test only", ds, 5);
            t.Inserts += 5; //simulate that it crashed after 3
            t.CloseAndArchive();

            d.CloseAndMarkComplete();
            
            
            
        }
    

    }
}
