using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Job;
using LoadModules.Generic.Attachers;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class FlatFileAttacherTests : DatabaseTests
    {
        private HICProjectDirectory hicProjectDirectory;
        DirectoryInfo parentDir;
        private DiscoveredDatabase _database;

        [SetUp]
        public void CreateTestDatabase()
        {
            var workingDir = new DirectoryInfo(".");
            parentDir = workingDir.CreateSubdirectory("FlatFileAttacherTests");

            DirectoryInfo toCleanup = parentDir.GetDirectories().SingleOrDefault(d => d.Name.Equals("Test_CSV_Attachment"));
            if(toCleanup != null)
                toCleanup.Delete(true);

            hicProjectDirectory = HICProjectDirectory.CreateDirectoryStructure(parentDir, "Test_CSV_Attachment");
            
            var database = TestDatabaseNames.GetConsistentName("FlatFileAttacher");

            // create a separate builder for setting an initial catalog on (need to figure out how best to stop child classes changing ServerICan... as this then causes TearDown to fail)

            _database = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(database);
            _database.Create(true);

            using (var con = _database.Server.GetConnection())
            {
                con.Open();
                
                var cmdCreateTable = _database.Server.GetCommand("CREATE Table "+database+"..Bob([name] [varchar](500),[name2] [varchar](500))" ,con);
                cmdCreateTable.ExecuteNonQuery();
            }

        }

        [Test]
        [TestCase(",",false)]
        [TestCase("|",false)]//wrong separator
        [TestCase(",",true)]
        public void Test_CSV_Attachment(string separator, bool overrideHeaders)
        {
            
            string filename = Path.Combine(hicProjectDirectory.ForLoading.FullName, "bob.csv");
            var sw = new StreamWriter(filename);

            sw.WriteLine("name,name2");
            sw.WriteLine("Bob,Munchousain");
            sw.WriteLine("Franky,Hollyw9ood");

            sw.Flush();
            sw.Close();
            sw.Dispose();


            string filename2 = Path.Combine(hicProjectDirectory.ForLoading.FullName, "bob2.csv");
            var sw2 = new StreamWriter(filename2);

            sw2.WriteLine("name,name2");
            sw2.WriteLine("Manny2,Ok");

            sw2.Flush();
            sw2.Close();
            sw2.Dispose();

            var attacher = new AnySeparatorFileAttacher();
            attacher.Initialize(hicProjectDirectory, _database);
            attacher.Separator = separator;
            attacher.FilePattern = "bob*";
            attacher.TableName = "Bob";

            if (overrideHeaders)
            {
                attacher.ForceHeaders = "name,name2";
                attacher.ForceHeadersReplacesFirstLineInFile = true;
            }

            try
            {
                attacher.Attach(new ThrowImmediatelyDataLoadJob());
                if(separator == "|")
                    Assert.Fail("Expected it to crash because of giving it the wrong separator");
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.Message.Contains("Your separator '|' does not appear in the headers line of your file (bob.csv) but the separator ',' does"))
                    Assert.Pass();

                throw;
            }

            var table = _database.ExpectTable("Bob");
            Assert.IsTrue(table.Exists());

            table.DiscoverColumn("name");
            table.DiscoverColumn("name2");

            using (var con = _database.Server.GetConnection())
            {

                con.Open();
                var r = _database.Server.GetCommand("Select * from Bob", con).ExecuteReader();
                Assert.IsTrue(r.Read());
                Assert.AreEqual("Bob",r["name"]);
                Assert.AreEqual("Munchousain", r["name2"]);
                    
                Assert.IsTrue(r.Read());
                Assert.AreEqual("Franky", r["name"]);
                Assert.AreEqual("Hollyw9ood", r["name2"]);
                
                Assert.IsTrue(r.Read());
                Assert.AreEqual("Manny2", r["name"]);
                Assert.AreEqual("Ok", r["name2"]);
            }
            
            attacher.LoadCompletedSoDispose(ExitCodeType.Success,new ThrowImmediatelyDataLoadEventListener());

            File.Delete(filename);
        }

        [Test]
        public void TabTestWithOverrideHeaders()
        {
            string filename = Path.Combine(hicProjectDirectory.ForLoading.FullName, "bob.csv");
            var sw = new StreamWriter(filename);

            sw.WriteLine("Face\tBasher");
            sw.WriteLine("Candy\tCrusher");

            sw.Flush();
            sw.Close();
            sw.Dispose();

            var attacher = new AnySeparatorFileAttacher();
            attacher.Initialize(hicProjectDirectory, _database);
            attacher.Separator = "\\t";
            attacher.FilePattern = "bob*";
            attacher.TableName = "Bob";
            attacher.ForceHeaders = "name\tname2";

            var exitCode = attacher.Attach(new ThrowImmediatelyDataLoadJob());
            Assert.AreEqual(ExitCodeType.Success,exitCode);

            using (var con = _database.Server.GetConnection())
            {

                con.Open();
                var r = _database.Server.GetCommand("Select name,name2 from Bob", con).ExecuteReader();
                Assert.IsTrue(r.Read());
                Assert.AreEqual("Face", r["name"]);
                Assert.AreEqual("Basher", r["name2"]);

                Assert.IsTrue(r.Read());
                Assert.AreEqual("Candy", r["name"]);
                Assert.AreEqual("Crusher", r["name2"]);
            }

            attacher.LoadCompletedSoDispose(ExitCodeType.Success, new ThrowImmediatelyDataLoadEventListener());

            File.Delete(filename);


        }
        
        [TearDown]
        public void TearDown()
        {
            parentDir.Delete(true);
            _database.ForceDrop();
        }
    }
}

        

