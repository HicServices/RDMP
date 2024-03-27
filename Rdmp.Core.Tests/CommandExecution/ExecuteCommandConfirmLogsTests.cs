// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Logging;
using System.Threading;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandConfirmLogsTests : DatabaseTests
{
    [Test]
    public void ConfirmLogs_NoEntries_Throws()
    {
        var lmd = new LoadMetadata(CatalogueRepository, "MyLmd");
        var cata = new Catalogue(CatalogueRepository, "myCata")
        {
            LoggingDataTask = "GGG"
        };
        cata.SaveToDatabase();
        lmd.LinkToCatalogue(cata);
        var lm = new LogManager(lmd.GetDistinctLoggingDatabase());
        lm.CreateNewLoggingTaskIfNotExists("GGG");

        var cmd = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator), lmd);
        var ex = Assert.Throws<LogsNotConfirmedException>(() => cmd.Execute());

        Assert.That(ex.Message, Is.EqualTo("There are no log entries for MyLmd"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ConfirmLogs_HappyEntries_Passes(bool withinTime)
    {
        var lmd = new LoadMetadata(CatalogueRepository, "MyLmd");
        var cata = new Catalogue(CatalogueRepository, "myCata")
        {
            LoggingDataTask = "FFF"
        };
        cata.SaveToDatabase();
        lmd.LinkToCatalogue(cata);
        var lm = new LogManager(lmd.GetDistinctLoggingDatabase());
        lm.CreateNewLoggingTaskIfNotExists("FFF");
        var logEntry = lm.CreateDataLoadInfo("FFF", "pack o' cards", "going down gambling", null, true);

        // we mark it as completed successfully - this is a good, happy log entry
        logEntry.CloseAndMarkComplete();


        var cmd = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator),
            //within last 10 hours
            lmd, withinTime ? "10:00:00" : null);
        Assert.DoesNotThrow(() => cmd.Execute());
    }

    [Test]
    public void ConfirmLogs_SadEntry_BecauseNeverEnded_Throws()
    {
        var lmd = new LoadMetadata(CatalogueRepository, "MyLmd");
        var cata = new Catalogue(CatalogueRepository, "myCata")
        {
            LoggingDataTask = "FFF"
        };
        cata.SaveToDatabase();
        lmd.LinkToCatalogue(cata);
        var lm = new LogManager(lmd.GetDistinctLoggingDatabase());
        lm.CreateNewLoggingTaskIfNotExists("FFF");

        // we have created log entry, but it did not have an end time.  This is a sad entry because it never completed
        lm.CreateDataLoadInfo("FFF", "pack o' cards", "going down gambling", null, true);

        var cmd = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator), lmd);
        var ex = Assert.Throws<LogsNotConfirmedException>(() => cmd.Execute());

        Assert.That(ex.Message, Does.Match("Latest logs for MyLmd .* indicate that it did not complete"));
    }

    [Test]
    public void ConfirmLogs_SadEntryWithEx_Throws()
    {
        var lmd = new LoadMetadata(CatalogueRepository, "MyLmd");
        var cata = new Catalogue(CatalogueRepository, "myCata")
        {
            LoggingDataTask = "FFF"
        };
        cata.SaveToDatabase();
        lmd.LinkToCatalogue(cata);
        var lm = new LogManager(lmd.GetDistinctLoggingDatabase());
        lm.CreateNewLoggingTaskIfNotExists("FFF");
        var logEntry = lm.CreateDataLoadInfo("FFF", "pack o' cards", "going down gambling", null, true);
        logEntry.LogFatalError("vegas", "we lost it all on a pair of deuces");

        var cmd = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator), lmd);
        var ex = Assert.Throws<LogsNotConfirmedException>(() => cmd.Execute());

        Assert.That(ex.Message, Does.Match("Latest logs for MyLmd .* indicate that it failed"));
    }


    [Test]
    public void ConfirmLogs_NotWithinTime_Throws()
    {
        var lmd = new LoadMetadata(CatalogueRepository, "MyLmd");
        var cata = new Catalogue(CatalogueRepository, "myCata")
        {
            LoggingDataTask = "FFF"
        };
        cata.SaveToDatabase();
        lmd.LinkToCatalogue(cata);
        var lm = new LogManager(lmd.GetDistinctLoggingDatabase());
        lm.CreateNewLoggingTaskIfNotExists("FFF");
        var logEntry = lm.CreateDataLoadInfo("FFF", "pack o' cards", "going down gambling", null, true);

        // we mark it as completed successfully - this is a good, happy log entry
        logEntry.CloseAndMarkComplete();

        Thread.Sleep(5000);

        // but we want this to have finished in the last second
        var cmd = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator), lmd, "00:00:01");
        var ex = Assert.Throws<LogsNotConfirmedException>(() => cmd.Execute());

        Assert.That(
ex.Message, Does.Match("Latest logged activity for MyLmd is .*.  This is older than the requested date threshold:.*"));
    }

    [Test]
    public void ConfirmLogs_With2CacheProgress_Throws()
    {
        var lmd1 = new LoadMetadata(CatalogueRepository, "MyLmd");
        var cata = new Catalogue(CatalogueRepository, "myCata")
        {
            LoggingDataTask = "B"
        };
        cata.SaveToDatabase();
        lmd1.LinkToCatalogue(cata);
        var lmd2 = new LoadMetadata(CatalogueRepository, "MyLmd");
        var cata2 = new Catalogue(CatalogueRepository, "myCata")
        {
            LoggingDataTask = "A"
        };
        cata2.SaveToDatabase();
        var linkage2 = new LoadMetadataCatalogueLinkage(CatalogueRepository, lmd2, cata2);
        linkage2.SaveToDatabase();
        var lp1 = new LoadProgress(CatalogueRepository, lmd1);
        var lp2 = new LoadProgress(CatalogueRepository, lmd2);

        var cp1 = new CacheProgress(CatalogueRepository, lp1);
        var cp2 = new CacheProgress(CatalogueRepository, lp2)
        {
            Name = "MyCoolCache"
        };
        cp2.SaveToDatabase();

        var lm = new LogManager(cp1.GetDistinctLoggingDatabase());
        lm.CreateNewLoggingTaskIfNotExists(cp1.GetDistinctLoggingTask());

        // create a log entry for cp1 only
        var logEntry = lm.CreateDataLoadInfo(cp1.GetDistinctLoggingTask(), "pack o' cards", cp1.GetLoggingRunName(),
            null, true);

        // we mark it as completed successfully - this is a good, happy log entry
        logEntry.CloseAndMarkComplete();

        // The first cache has logged success so should be happy
        var cmd1 = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator), cp1, null);
        Assert.DoesNotThrow(() => cmd1.Execute());

        // The second cache has not logged any successes so should be unhappy
        var cmd2 = new ExecuteCommandConfirmLogs(new ThrowImmediatelyActivator(RepositoryLocator), cp2, null);
        var ex = Assert.Throws<LogsNotConfirmedException>(() => cmd2.Execute());

        Assert.That(ex.Message, Is.EqualTo("There are no log entries for MyCoolCache"));
    }
}