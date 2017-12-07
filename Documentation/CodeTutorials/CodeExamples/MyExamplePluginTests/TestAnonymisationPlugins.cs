using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataHelper;
using Common.Logging;
using HIC.Logging.Listeners;
using MyExamplePlugin.PipelineComponents;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents.Progress;
using roundhouse.infrastructure.logging;
using Tests.Common;
using LogManager = HIC.Logging.LogManager;

namespace MyExamplePluginTests
{
    public class TestAnonymisationPlugins : DatabaseTests
    {
        [Test]
        public void TestBasicDataTableAnonymiser1()
        {
            var dt = new DataTable();
            dt.Columns.Add("Story");
            dt.Rows.Add(new[] {"Thomas went to school regularly"});
            dt.Rows.Add(new[] {"It seems like Wallace went less regularly"});
            dt.Rows.Add(new[] {"Mr Smitty was the teacher"});

            var a = new BasicDataTableAnonymiser1();
            var resultTable = a.ProcessPipelineData(dt,new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());

            Assert.AreEqual(resultTable.Rows.Count,3);
            Assert.AreEqual("REDACTED went to school regularly",resultTable.Rows[0][0]);
            Assert.AreEqual("It seems like REDACTED went less regularly",resultTable.Rows[1][0]);
            Assert.AreEqual("Mr Smitty was the teacher",resultTable.Rows[2][0]);
        }

        [Test]
        public void TestBasicDataTableAnonymiser3()
        { 
            //Create a names table that will go into the database
            var dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Rows.Add(new[] {"Thomas"});
            dt.Rows.Add(new[] {"Wallace"});
            dt.Rows.Add(new[] {"Frank"});

            //upload the DataTable from memory into the database
            var discoveredTable = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable("ForbiddenNames", dt);

            //import the persistent TableInfo reference
            var importer = new TableInfoImporter(CatalogueRepository, discoveredTable);
            
            TableInfo tableInfo;
            ColumnInfo[] columnInfos;
            importer.DoImport(out tableInfo,out columnInfos);

            //Create the test dataset chunk that will be anonymised
            var dtStories = new DataTable();
            dtStories.Columns.Add("Story");
            dtStories.Rows.Add(new[] { "Thomas went to school regularly" });
            dtStories.Rows.Add(new[] { "It seems like Wallace went less regularly" });
            dtStories.Rows.Add(new[] { "Mr Smitty was the teacher" });

            //Create the anonymiser
            var a = new BasicDataTableAnonymiser3();

            //Tell it about the database table
            a.NamesTable = tableInfo;

            //run the anonymisation
            var resultTable = a.ProcessPipelineData(dtStories, new ThrowImmediatelyDataLoadEventListener(),new GracefulCancellationToken());

            //check the results
            Assert.AreEqual(resultTable.Rows.Count, 3);
            Assert.AreEqual("REDACTED went to school regularly", resultTable.Rows[0][0]);
            Assert.AreEqual("It seems like REDACTED went less regularly", resultTable.Rows[1][0]);
            Assert.AreEqual("Mr Smitty was the teacher", resultTable.Rows[2][0]);

            //finally drop the database table
            discoveredTable.Drop();
        }

        [Test]
        public void TestBasicDataTableAnonymiser4_FailConditions()
        {
            var a = new BasicDataTableAnonymiser4();
            var ex = Assert.Throws<Exception>(()=>a.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.IsTrue(ex.Message.Contains("No NamesTable has been set"));
        }

        [Test]
        public void TestBasicDataTableAnonymiser4()
        {
            //Create a names table that will go into the database
            var dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Rows.Add(new[] { "Thomas" });
            dt.Rows.Add(new[] { "Wallace" });
            dt.Rows.Add(new[] { "Frank" });

            //upload the DataTable from memory into the database
            var discoveredTable = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable("ForbiddenNames", dt);

            //import the persistent TableInfo reference
            var importer = new TableInfoImporter(CatalogueRepository, discoveredTable);

            TableInfo tableInfo;
            ColumnInfo[] columnInfos;
            importer.DoImport(out tableInfo, out columnInfos);

            //Create the test dataset chunk that will be anonymised
            var dtStories = new DataTable();
            dtStories.Columns.Add("Story");
            dtStories.Rows.Add(new[] { "Thomas went to school regularly" });
            dtStories.Rows.Add(new[] { "It seems like Wallace went less regularly" });
            dtStories.Rows.Add(new[] { "Mr Smitty was the teacher" });

            //Create the anonymiser
            var a = new BasicDataTableAnonymiser4();

            //Tell it about the database table
            a.NamesTable = tableInfo;

            //run the anonymisation
            var resultTable = a.ProcessPipelineData(dtStories, new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            //check the results
            Assert.AreEqual(resultTable.Rows.Count, 3);
            Assert.AreEqual("REDACTED went to school regularly", resultTable.Rows[0][0]);
            Assert.AreEqual("It seems like REDACTED went less regularly", resultTable.Rows[1][0]);
            Assert.AreEqual("Mr Smitty was the teacher", resultTable.Rows[2][0]);

            //finally drop the database table
            discoveredTable.Drop();
        }

        public enum LoggerTestCase
        {
            ToConsole,
            ToMemory,
            ToDatabase
        }

        [Test]
        [TestCase(LoggerTestCase.ToConsole)]
        [TestCase(LoggerTestCase.ToMemory)]
        [TestCase(LoggerTestCase.ToDatabase)]
        public void TestBasicDataTableAnonymiser5(LoggerTestCase testCase)
        {
            //Create a names table that will go into the database
            var dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Rows.Add(new[] { "Thomas" });
            dt.Rows.Add(new[] { "Wallace" });
            dt.Rows.Add(new[] { "Frank" });

            //upload the DataTable from memory into the database
            var discoveredTable = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable("ForbiddenNames", dt);
            try
            {
                //import the persistent TableInfo reference
                var importer = new TableInfoImporter(CatalogueRepository, discoveredTable);

                TableInfo tableInfo;
                ColumnInfo[] columnInfos;
                importer.DoImport(out tableInfo, out columnInfos);

                //Create the test dataset chunks that will be anonymised
                var dtStories1 = new DataTable();
                dtStories1.Columns.Add("Story");
                dtStories1.Rows.Add(new[] { "Thomas went to school regularly" }); //1st redact
                dtStories1.Rows.Add(new[] { "It seems like Wallace went less regularly" }); //2nd redact
                dtStories1.Rows.Add(new[] { "Mr Smitty was the teacher" });

                var dtStories2 = new DataTable();
                dtStories2.Columns.Add("Story");
                dtStories2.Rows.Add(new[] { "Things were going so well" });
                dtStories2.Rows.Add(new[] { "And then it all turned bad for Wallace" }); //3rd redact
            
                var dtStories3 = new DataTable();
                dtStories3.Columns.Add("Story");
                dtStories3.Rows.Add(new[] { "There were things creeping in the dark" });
                dtStories3.Rows.Add(new[] { "Surely Frank would know what to do.  Frank was a genius" }); //4th redact
                dtStories3.Rows.Add(new[] { "Mr Smitty was the teacher" });
            
                //Create the anonymiser
                var a = new BasicDataTableAnonymiser5();

                //Tell it about the database table
                a.NamesTable = tableInfo;

                //Create a listener according to the test case
                IDataLoadEventListener listener = null;

                switch (testCase)
                {
                    case LoggerTestCase.ToConsole:
                        listener = new ThrowImmediatelyDataLoadEventListener();
                        break;
                    case LoggerTestCase.ToMemory:
                        listener = new ToMemoryDataLoadEventListener(true);
                        break;
                    case LoggerTestCase.ToDatabase:
                    
                        //get the default logging server
                        var loggingServer = new ServerDefaults(CatalogueRepository).GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

                        //create a log manager for the server
                        var logManager = new LogManager(loggingServer);

                        //create a new super task Anonymising Data Tables
                        logManager.CreateNewLoggingTaskIfNotExists("Anonymising Data Tables");

                        //setup a listener that goes to this logging database 
                        listener = new ToLoggingDatabaseDataLoadEventListener(this,logManager ,"Anonymising Data Tables","Run on " + DateTime.Now);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("testCase");
                }

                //run the anonymisation
                //process all 3 batches
                a.ProcessPipelineData(dtStories1, listener, new GracefulCancellationToken());
                a.ProcessPipelineData(dtStories2, listener, new GracefulCancellationToken());
                a.ProcessPipelineData(dtStories3, listener, new GracefulCancellationToken());

                //check the results
                switch (testCase)
                {
                    case LoggerTestCase.ToMemory:
                        Assert.AreEqual(4, ((ToMemoryDataLoadEventListener)listener).LastProgressRecieivedByTaskName["REDACTING Names"].Progress.Value);
                        break;
                    case LoggerTestCase.ToDatabase:
                        ((ToLoggingDatabaseDataLoadEventListener)listener).FinalizeTableLoadInfos();
                        break;
                }
            }
            finally
            {
                //finally drop the database table
                discoveredTable.Drop();
            }
        }
    }
}
