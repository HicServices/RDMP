// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Databases;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.Logging;

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
    ///     Add a bunch of data load runs for the tests in this fixture
    /// </summary>
    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        try
        {
            var lds = new DiscoveredServer(UnitTestLoggingConnectionString);

            var manager = new LogManager(lds);

            _dataLoadTaskName = "LogTest";

            _dataLoadTaskHelper = new DataLoadTaskHelper(lds);
            _dataLoadTaskHelper.SetUp();

            manager.CreateNewLoggingTaskIfNotExists(_dataLoadTaskName);

            // Insert some data load runs that are used by all the tests
            _logManager = new LogManager(lds);

            _failedLoad =
                _logManager.CreateDataLoadInfo(_dataLoadTaskName, _dataLoadTaskName, _dataLoadTaskName, "", true);
            _failedLoad.LogFatalError("", "");
            _failedLoad.CloseAndMarkComplete();

            _successfulLoad =
                _logManager.CreateDataLoadInfo(_dataLoadTaskName, _dataLoadTaskName, _dataLoadTaskName, "", true);
            _successfulLoad.LogProgress(DataLoadInfo.ProgressEventType.OnProgress, "", "");
            _successfulLoad.CloseAndMarkComplete();

            var tableLoadInfo = _successfulLoad.CreateTableLoadInfo("ignoreme", "Nowhereland",
                new DataSource[]
                    { new("Firehouse", DateTime.Now.AddDays(-1)), new("WaterHaus") }, 100);

            tableLoadInfo.Inserts = 500;
            tableLoadInfo.Updates = 100;
            tableLoadInfo.CloseAndArchive();


            _anotherSuccessfulLoad =
                _logManager.CreateDataLoadInfo(_dataLoadTaskName, _dataLoadTaskName, _dataLoadTaskName, "", true);
            _anotherSuccessfulLoad.LogProgress(DataLoadInfo.ProgressEventType.OnProgress, "", "");
            _anotherSuccessfulLoad.CloseAndMarkComplete();
        }
        catch (Exception e)
        {
            _setupException = e;
        }
    }

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        if (_setupException != null)
            Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(_setupException, true));
    }


    [Test]
    public void TestLastLoadStatusAssemblage()
    {
        var lm = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));

        var loadHistoryForTask = lm.GetArchivalDataLoadInfos(_dataLoadTaskName).ToArray();

        Assert.That(loadHistoryForTask, Is.Not.Empty); //some records

        Assert.Multiple(() =>
        {
            Assert.That(loadHistoryForTask.Count(static load => load.Errors.Count > 0),
                Is.GreaterThan(0)); //some with some errors
            Assert.That(loadHistoryForTask.Count(static load => load.Progress.Count > 0),
                Is.GreaterThan(0)); //some with some progress


            Assert.That(loadHistoryForTask.Count(static load => load.TableLoadInfos.Count == 1),
                Is.GreaterThan(0)); //some with some table loads
        });
    }

    [Test]
    public void TestLoggingVsDynamicSQLHacker()
    {
        CleanupTruncateCommand();

        Assert.That(_logManager.ListDataTasks().Count(t => t.Equals("','') Truncate Table Fishes")), Is.EqualTo(0));
        _logManager.CreateNewLoggingTaskIfNotExists("','') Truncate Table Fishes");
        Assert.That(_logManager.ListDataTasks().Count(t => t.Equals("','') Truncate Table Fishes")), Is.EqualTo(1));

        CleanupTruncateCommand();
        Assert.That(_logManager.ListDataTasks().Count(t => t.Equals("','') Truncate Table Fishes")), Is.EqualTo(0));
    }

    private void CleanupTruncateCommand()
    {
        var lds = new DiscoveredServer(UnitTestLoggingConnectionString);
        using var con = lds.GetConnection();
        con.Open();
        lds.GetCommand("DELETE FROM DataLoadTask where LOWER(dataSetID) like '%truncate%'", con).ExecuteNonQuery();
        lds.GetCommand("DELETE FROM DataSet where LOWER(dataSetID) like '%truncate%'", con).ExecuteNonQuery();
    }


    [Test]
    public void TestLastLoadStatusAssemblage_Top1()
    {
        var lm = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));

        var loadHistoryForTask = lm.GetArchivalDataLoadInfos(_dataLoadTaskName).First();

        Assert.That(loadHistoryForTask.Progress, Is.Not.Empty); //some with some progress

        Console.WriteLine($"Errors fetched:{loadHistoryForTask.Errors.Count}");
        Console.WriteLine($"Progress fetched:{loadHistoryForTask.Progress.Count}");


        var totalErrors = loadHistoryForTask.Errors.Count;
        Console.WriteLine($"total errors:{totalErrors}");
    }


    [Test]
    public void TestLastLoadStatusAssemblage_MostRecent()
    {
        var server = new DiscoveredServer(UnitTestLoggingConnectionString);
        var lm = new LogManager(server);

        var mostRecent = lm.GetArchivalDataLoadInfos(_dataLoadTaskName).First();
        var all = lm.GetArchivalDataLoadInfos(_dataLoadTaskName).ToArray();

        all = all.OrderByDescending(d => d.StartTime).ToArray();

        Assert.That(all[0].StartTime, Is.EqualTo(mostRecent.StartTime));
    }

    /// <summary>
    ///     Tests the ability of the logging database / API to maintain only one set of tasks not
    ///     one upper case and one lower or otherwise mixed cases.
    /// </summary>
    [Test]
    public void TestLogging_CreateTasks_MixedCases()
    {
        var server = new DiscoveredServer(UnitTestLoggingConnectionString);
        var lm = new LogManager(server);


        lm.CreateNewLoggingTaskIfNotExists("ff");

        Assert.That(lm.ListDataSets().Count(s => s.Equals("ff", StringComparison.CurrentCultureIgnoreCase)),
            Is.EqualTo(1));

        //don't create another one just because it's changed case
        lm.CreateNewLoggingTaskIfNotExists("FF");

        Assert.That(lm.ListDataSets().Count(s => s.Equals("ff", StringComparison.CurrentCultureIgnoreCase)),
            Is.EqualTo(1));

        lm.CreateDataLoadInfo("ff", "tests", "hi there", null, true);
        lm.CreateDataLoadInfo("FF", "tests", "hi there", null, true);

        Assert.That(lm.ListDataSets().Count(s => s.Equals("ff", StringComparison.CurrentCultureIgnoreCase)),
            Is.EqualTo(1));
    }

    [TestCaseSource(typeof(All), nameof(All.DatabaseTypes))]
    public void LoggingDatabase_TestActuallyCreatingIt(DatabaseType type)
    {
        var db = GetCleanedServer(type);

        var creator = new MasterDatabaseScriptExecutor(db);
        creator.CreateAndPatchDatabase(new LoggingDatabasePatcher(), new AcceptAllCheckNotifier());

        var lm = new LogManager(db.Server);

        lm.CreateNewLoggingTaskIfNotExists("blarg");

        var dli = lm.CreateDataLoadInfo("blarg", "tests", "doing nothing interesting", null, true);
        var tli = dli.CreateTableLoadInfo("", "mytbl",
            new[] { new DataSource("ff.csv"), new DataSource("bb.csv", new DateTime(2001, 1, 1)) }, 40);

        tli.Inserts = 500;
        tli.CloseAndArchive();

        dli.LogFatalError("bad.cs", "it went bad");
        dli.LogProgress(DataLoadInfo.ProgressEventType.OnInformation, "good.cs", "Wrote some records");
        dli.CloseAndMarkComplete();

        var id = dli.ID;
        var archival = lm.GetArchivalDataLoadInfos("blarg", null, id).Single();
        Assert.Multiple(() =>
        {
            Assert.That(archival.TableLoadInfos.Single().Inserts, Is.EqualTo(500));
            Assert.That(archival.TableLoadInfos.Single().Updates, Is.EqualTo(0));
            Assert.That(archival.TableLoadInfos.Single().Deletes, Is.EqualTo(0));
            Assert.That(archival.StartTime.Date, Is.EqualTo(DateTime.Now.Date));
            Assert.That(archival.EndTime.Value.Date, Is.EqualTo(DateTime.Now.Date));

            Assert.That(archival.Errors.Single().Description, Is.EqualTo("it went bad"));
            Assert.That(archival.Errors.Single().Source, Is.EqualTo("bad.cs"));
            Assert.That(archival.Progress.Single().Description, Is.EqualTo("Wrote some records"));
        });

        var fatal = archival.Errors.Single();
        lm.ResolveFatalErrors(new[] { fatal.ID }, DataLoadInfo.FatalErrorStates.Resolved,
            "problem resolved by building more towers");
    }
}