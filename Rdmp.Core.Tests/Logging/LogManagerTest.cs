// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Threading.Tasks;
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
    private IDataLoadInfo _anotherSuccessfulLoad;
    private DataLoadTaskHelper _dataLoadTaskHelper;
    private string _dataLoadTaskName;

    private IDataLoadInfo _failedLoad;
    private LogManager _logManager;

    private Exception _setupException;
    private IDataLoadInfo _successfulLoad;


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

            Task.Delay(1000).Wait();

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
    public void TestLastLoadStatusassemblage()
    {
        var lm = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));

        var loadHistoryForTask = lm.GetArchivalDataLoadInfos(_dataLoadTaskName).ToArray();

        Assert.Greater(loadHistoryForTask.Length, 0); //some records

        Assert.Greater(loadHistoryForTask.Count(load => load.Errors.Count > 0), 0); //some with some errors
        Assert.Greater(loadHistoryForTask.Count(load => load.Progress.Count > 0), 0); //some with some progress


        Assert.Greater(loadHistoryForTask.Count(load => load.TableLoadInfos.Count == 1),
            0); //some with some table loads


        Console.WriteLine($"Records fetched:{loadHistoryForTask.Length}");
        Console.WriteLine($"Errors fetched:{loadHistoryForTask.Aggregate(0, (p, c) => p + c.Errors.Count)}");
        Console.WriteLine($"Progress fetched:{loadHistoryForTask.Aggregate(0, (p, c) => p + c.Progress.Count)}");
    }

    [Test]
    public void TestLoggingVsDynamicSQLHacker()
    {
        CleanupTruncateCommand();

        Assert.AreEqual(0, _logManager.ListDataTasks().Count(t => t.Equals("','') Truncate Table Fishes")));
        _logManager.CreateNewLoggingTaskIfNotExists("','') Truncate Table Fishes");
        Assert.AreEqual(1, _logManager.ListDataTasks().Count(t => t.Equals("','') Truncate Table Fishes")));

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
        var lm = new LogManager(new DiscoveredServer(UnitTestLoggingConnectionString));

        var loadHistoryForTask = lm.GetArchivalDataLoadInfos(_dataLoadTaskName).First();

        Assert.Greater(loadHistoryForTask.Progress.Count, 0); //some with some progress

        Console.WriteLine($"Errors fetched:{loadHistoryForTask.Errors.Count}");
        Console.WriteLine($"Progress fetched:{loadHistoryForTask.Progress.Count}");


        var totalErrors = loadHistoryForTask.Errors.Count;
        Console.WriteLine($"total errors:{totalErrors}");
    }


    [Test]
    public void TestLastLoadStatusassemblage_MostRecent()
    {
        var server = new DiscoveredServer(UnitTestLoggingConnectionString);
        var lm = new LogManager(server);

        var mostRecent = lm.GetArchivalDataLoadInfos(_dataLoadTaskName).First();
        var all = lm.GetArchivalDataLoadInfos(_dataLoadTaskName).ToArray();

        all = all.OrderByDescending(d => d.StartTime).ToArray();

        Assert.AreEqual(mostRecent.StartTime, all[0].StartTime);
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

        Assert.AreEqual(1, lm.ListDataSets().Count(s => s.Equals("ff", StringComparison.CurrentCultureIgnoreCase)));

        //don't create another one just because it's changed case
        lm.CreateNewLoggingTaskIfNotExists("FF");

        Assert.AreEqual(1, lm.ListDataSets().Count(s => s.Equals("ff", StringComparison.CurrentCultureIgnoreCase)));

        var dli1 = lm.CreateDataLoadInfo("ff", "tests", "hi there", null, true);
        var dli2 = lm.CreateDataLoadInfo("FF", "tests", "hi there", null, true);

        Assert.AreEqual(1, lm.ListDataSets().Count(s => s.Equals("ff", StringComparison.CurrentCultureIgnoreCase)));
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

        Assert.AreEqual(500, archival.TableLoadInfos.Single().Inserts);
        Assert.AreEqual(0, archival.TableLoadInfos.Single().Updates);
        Assert.AreEqual(0, archival.TableLoadInfos.Single().Deletes);
        Assert.AreEqual(DateTime.Now.Date, archival.StartTime.Date);
        Assert.AreEqual(DateTime.Now.Date, archival.EndTime.Value.Date);

        Assert.AreEqual("it went bad", archival.Errors.Single().Description);
        Assert.AreEqual("bad.cs", archival.Errors.Single().Source);

        Assert.AreEqual("Wrote some records", archival.Progress.Single().Description);

        var fatal = archival.Errors.Single();
        lm.ResolveFatalErrors(new[] { fatal.ID }, DataLoadInfo.FatalErrorStates.Resolved,
            "problem resolved by building more towers");
    }
}