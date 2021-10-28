// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
            var ex = Assert.Throws<LogsNotConfirmedException>(()=>cmd.Execute());

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

            // we have created log entry but it did not have an end time.  This is a sad entry because it never completed
            lm.CreateDataLoadInfo("FFF", "pack o' cards", "going down gambling", null, true);
            
            var cmd = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator), lmd);
            var ex = Assert.Throws<LogsNotConfirmedException>(() => cmd.Execute());

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
            var ex = Assert.Throws<LogsNotConfirmedException>(() => cmd.Execute());

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
            var ex = Assert.Throws<LogsNotConfirmedException>(() => cmd.Execute());

            StringAssert.IsMatch("Latest logged activity for MyLmd is .*.  This is older than the requested date threshold:.*", ex.Message);
        }
    }
}
