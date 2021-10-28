using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Logging;
using System.Threading;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    class ExecuteCommandConfirmLogsTests : DatabaseTests
    {
        [Test]
        public void ConfirmLogs_NoEntries_Throws()
        {
            var lmd = new LoadMetadata(CatalogueRepository, "MyLmd");
            var cata = new Catalogue(CatalogueRepository, "myCata");
            cata.LoadMetadata_ID = lmd.ID;
            cata.LoggingDataTask = "GGG";
            cata.SaveToDatabase();

            var lm = new LogManager(lmd.GetDistinctLoggingDatabase());
            lm.CreateNewLoggingTaskIfNotExists("GGG");

            var cmd = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator), lmd);
            var ex = Assert.Throws<ExecuteCommandConfirmLogs.LogsNotConfirmedException>(()=>cmd.Execute());

            Assert.AreEqual("There are no log entries for MyLmd", ex.Message);

        }
        [TestCase(true)]
        [TestCase(false)]
        public void ConfirmLogs_HappyEntries_Passes(bool withinTime)
        {
            var lmd = new LoadMetadata(CatalogueRepository, "MyLmd");
            var cata = new Catalogue(CatalogueRepository, "myCata");
            cata.LoadMetadata_ID = lmd.ID;
            cata.LoggingDataTask = "FFF";
            cata.SaveToDatabase();

            var lm = new LogManager(lmd.GetDistinctLoggingDatabase());
            lm.CreateNewLoggingTaskIfNotExists("FFF");
            var logEntry = lm.CreateDataLoadInfo("FFF", "pack o' cards", "going down gambling", null, true);
            
            // we mark it as completed successfully - this is a good, happy log entry
            logEntry.CloseAndMarkComplete();

            
            var cmd = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator),
                //within last 10 hours
                lmd, withinTime ? "10:00:00":null);
            Assert.DoesNotThrow(() => cmd.Execute());
        }

        [Test]
        public void ConfirmLogs_SadEntry_BecauseNeverEnded_Throws()
        {
            var lmd = new LoadMetadata(CatalogueRepository, "MyLmd");
            var cata = new Catalogue(CatalogueRepository, "myCata");
            cata.LoadMetadata_ID = lmd.ID;
            cata.LoggingDataTask = "FFF";
            cata.SaveToDatabase();

            var lm = new LogManager(lmd.GetDistinctLoggingDatabase());
            lm.CreateNewLoggingTaskIfNotExists("FFF");
            var logEntry = lm.CreateDataLoadInfo("FFF", "pack o' cards", "going down gambling", null, true);
            
            // we have created log entry but it did not have an end time.  This is a sad entry because it never completed

            var cmd = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator), lmd);
            var ex = Assert.Throws<ExecuteCommandConfirmLogs.LogsNotConfirmedException>(() => cmd.Execute());

            StringAssert.IsMatch("Latest logs for MyLmd .* indicate that it did not complete", ex.Message);
        }
        [Test]
        public void ConfirmLogs_SadEntryWithEx_Throws()
        {
            var lmd = new LoadMetadata(CatalogueRepository, "MyLmd");
            var cata = new Catalogue(CatalogueRepository, "myCata");
            cata.LoadMetadata_ID = lmd.ID;
            cata.LoggingDataTask = "FFF";
            cata.SaveToDatabase();

            var lm = new LogManager(lmd.GetDistinctLoggingDatabase());
            lm.CreateNewLoggingTaskIfNotExists("FFF");
            var logEntry = lm.CreateDataLoadInfo("FFF", "pack o' cards", "going down gambling", null, true);
            logEntry.LogFatalError("vegas", "we lost it all on a pair of deuces");

            var cmd = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator), lmd);
            var ex = Assert.Throws<ExecuteCommandConfirmLogs.LogsNotConfirmedException>(() => cmd.Execute());

            StringAssert.IsMatch("Latest logs for MyLmd .* indicate that it failed", ex.Message);

        }


        [Test]
        public void ConfirmLogs_NotWithinTime_Throws()
        {

            var lmd = new LoadMetadata(CatalogueRepository, "MyLmd");
            var cata = new Catalogue(CatalogueRepository, "myCata");
            cata.LoadMetadata_ID = lmd.ID;
            cata.LoggingDataTask = "FFF";
            cata.SaveToDatabase();

            var lm = new LogManager(lmd.GetDistinctLoggingDatabase());
            lm.CreateNewLoggingTaskIfNotExists("FFF");
            var logEntry = lm.CreateDataLoadInfo("FFF", "pack o' cards", "going down gambling", null, true);

            // we mark it as completed successfully - this is a good, happy log entry
            logEntry.CloseAndMarkComplete();

            Thread.Sleep(5000);

            // but we want this to have finished in the last second
            var cmd = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator), lmd,"00:00:01");
            var ex = Assert.Throws<ExecuteCommandConfirmLogs.LogsNotConfirmedException>(() => cmd.Execute());

            StringAssert.IsMatch("Latest logged activity for MyLmd is .*.  This is older than the requested date threshold:.*", ex.Message);
        }
    }
}
