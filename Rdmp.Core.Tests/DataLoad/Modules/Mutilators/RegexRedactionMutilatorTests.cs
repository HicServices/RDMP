using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.DataLoad.Modules.Mutilators;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Tests.DataLoad.Engine.Integration;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TypeGuesser;

namespace Rdmp.Core.Tests.DataLoad.Modules.Mutilators;

internal class RegexRedactionMutilatorTests : DataLoadEngineTestsBase
{

    [Test]
    public void RedactionMutilator_BasicTest()
    {
        var defaults = CatalogueRepository;
        var logServer = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        var logManager = new LogManager(logServer);
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var raw = db.Server.ExpectDatabase($"{db.GetRuntimeName()}_RAW");
        if (raw.Exists())
            raw.Drop();

        var dleStaging = db.Server.ExpectDatabase($"DLE_STAGING");
        if (!dleStaging.Exists())
            db.Server.CreateDatabase("DLE_STAGING");

        using var dt = new DataTable("MyTable");
        dt.Columns.Add("Name");
        dt.Columns.Add("DateOfBirth");
        dt.Columns.Add("FavouriteColour");
        dt.Rows.Add("Bob", "2001-01-01", "Pink");
        dt.Rows.Add("Frank", "2001-01-01", "Orange");

        var nameCol = new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 20), false)
        { IsPrimaryKey = true };
        DiscoveredTable tbl = db.CreateTable("MyTable", dt, new[]
                {
                    nameCol,
                    new DatabaseColumnRequest("DateOfBirth", new DatabaseTypeRequest(typeof(DateTime)), false)
                        { IsPrimaryKey = true }
                });

        Assert.That(tbl.GetRowCount(), Is.EqualTo(2));

        //define a new load configuration
        var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");

        lmd.IgnoreTrigger = true;
        lmd.SaveToDatabase();
        var ti = Import(tbl, lmd, logManager);

        var projectDirectory = SetupLoadDirectory(lmd);

        CreateCSVProcessTask(lmd, ti, "*.csv");


        var regex = new RegexRedactionConfiguration(CatalogueRepository, "No Yelling", new Regex("Yella"), "FFF");
        regex.SaveToDatabase();

        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustStaging)
        {
            Path = typeof(RegexRedactionMutilator).FullName,
            ProcessTaskType = ProcessTaskType.MutilateDataTable,
            Name = $"Mutilate {ti.GetRuntimeName()}"
        };
        pt.SaveToDatabase();

        pt.CreateArgumentsForClassIfNotExists<RegexRedactionMutilator>();
        pt.SetArgumentValue("ColumnRegexPattern", ".*");
        pt.SetArgumentValue("redactionConfiguration", regex);
        pt.SetArgumentValue("TableRegexPattern", ".*");

        pt.Check(ThrowImmediatelyCheckNotifier.Quiet);

        File.WriteAllText(
            Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"),
            @"Name,DateOfBirth,FavouriteColour
MrMurder,2001-01-01,Yella");

        var dbConfig = new HICDatabaseConfiguration(lmd, null);

        var job = new DataLoadJob(RepositoryLocator, "Go go go!", logManager, lmd, projectDirectory,
                ThrowImmediatelyDataLoadEventListener.Quiet, dbConfig);

        var loadFactory = new HICDataLoadFactory(
            lmd,
            dbConfig,
            new HICLoadConfigurationFlags(),
            CatalogueRepository,
            logManager
        );

        var exe = loadFactory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);

        var exitCode = exe.Run(
          job,
            new GracefulCancellationToken());

        var redactions = CatalogueRepository.GetAllObjects<RegexRedaction>();
        Assert.That(redactions.Count(), Is.EqualTo(1));
        Assert.That(redactions[0].RedactionConfiguration_ID, Is.EqualTo(regex.ID));
        Assert.That(redactions[0].StartingIndex, Is.EqualTo(0));
        Assert.That(redactions[0].RedactedValue, Is.EqualTo("Yella"));
        Assert.That(redactions[0].ReplacementValue, Is.EqualTo("<FFF>"));

        var redactionKeys = CatalogueRepository.GetAllObjects<RegexRedactionKey>();
        Assert.That(redactionKeys.Length, Is.EqualTo(2));
        foreach (var r in redactions)
        {
            r.DeleteInDatabase();
        }
        foreach (var r in redactionKeys)
        {
            r.DeleteInDatabase();
        }
    }

    [Test]
    public void RedactionMutilator_OddStringLength()
    {
        var defaults = CatalogueRepository;
        var logServer = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        var logManager = new LogManager(logServer);
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var raw = db.Server.ExpectDatabase($"{db.GetRuntimeName()}_RAW");
        if (raw.Exists())
            raw.Drop();

        var dleStaging = db.Server.ExpectDatabase($"DLE_STAGING");
        if (!dleStaging.Exists())
            db.Server.CreateDatabase("DLE_STAGING");

        using var dt = new DataTable("MyTable");
        dt.Columns.Add("Name");
        dt.Columns.Add("DateOfBirth");
        dt.Columns.Add("FavouriteColour");
        dt.Rows.Add("Bob", "2001-01-01", "Pink");
        dt.Rows.Add("Frank", "2001-01-01", "Orange");

        var nameCol = new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 20), false)
        { IsPrimaryKey = true };
        DiscoveredTable tbl = db.CreateTable("MyTable", dt, new[]
                {
                    nameCol,
                    new DatabaseColumnRequest("DateOfBirth", new DatabaseTypeRequest(typeof(DateTime)), false)
                        { IsPrimaryKey = true }
                });

        Assert.That(tbl.GetRowCount(), Is.EqualTo(2));

        //define a new load configuration
        var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");

        lmd.IgnoreTrigger = true;
        lmd.SaveToDatabase();
        var ti = Import(tbl, lmd, logManager);

        var projectDirectory = SetupLoadDirectory(lmd);

        CreateCSVProcessTask(lmd, ti, "*.csv");


        var regex = new RegexRedactionConfiguration(CatalogueRepository, "No Yelling", new Regex("Yell"), "FFF");
        regex.SaveToDatabase();

        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustRaw)
        {
            Path = typeof(RegexRedactionMutilator).FullName,
            ProcessTaskType = ProcessTaskType.MutilateDataTable,
            Name = $"Mutilate {ti.GetRuntimeName()}"
        };
        pt.SaveToDatabase();

        pt.CreateArgumentsForClassIfNotExists<RegexRedactionMutilator>();
        pt.SetArgumentValue("ColumnRegexPattern", ".*");
        pt.SetArgumentValue("redactionConfiguration", regex);
        pt.SetArgumentValue("TableRegexPattern", ".*");

        pt.Check(ThrowImmediatelyCheckNotifier.Quiet);

        File.WriteAllText(
            Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"),
            @"Name,DateOfBirth,FavouriteColour
MrMurder,2001-01-01,Yella");

        var dbConfig = new HICDatabaseConfiguration(lmd, null);

        var job = new DataLoadJob(RepositoryLocator, "Go go go!", logManager, lmd, projectDirectory,
                ThrowImmediatelyDataLoadEventListener.Quiet, dbConfig);

        var loadFactory = new HICDataLoadFactory(
            lmd,
            dbConfig,
            new HICLoadConfigurationFlags(),
            CatalogueRepository,
            logManager
        );

        var exe = loadFactory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);

        var exitCode = exe.Run(
          job,
            new GracefulCancellationToken());

        var redactions = CatalogueRepository.GetAllObjects<RegexRedaction>();
        Assert.That(redactions.Count(), Is.EqualTo(1));
        Assert.That(redactions[0].RedactionConfiguration_ID, Is.EqualTo(regex.ID));
        Assert.That(redactions[0].StartingIndex, Is.EqualTo(0));
        Assert.That(redactions[0].RedactedValue, Is.EqualTo("Yell"));
        Assert.That(redactions[0].ReplacementValue, Is.EqualTo("FFF>"));

        var redactionKeys = CatalogueRepository.GetAllObjects<RegexRedactionKey>();
        Assert.That(redactionKeys.Length, Is.EqualTo(2));
        foreach (var r in redactions)
        {
            r.DeleteInDatabase();
        }
        foreach (var r in redactionKeys)
        {
            r.DeleteInDatabase();
        }
    }

    [Test]
    public void RedactionMutilator_RedactionTooLong()
    {
        var defaults = CatalogueRepository;
        var logServer = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        var logManager = new LogManager(logServer);
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var raw = db.Server.ExpectDatabase($"{db.GetRuntimeName()}_RAW");
        if (raw.Exists())
            raw.Drop();

        var dleStaging = db.Server.ExpectDatabase($"DLE_STAGING");
        if (!dleStaging.Exists())
            db.Server.CreateDatabase("DLE_STAGING");

        using var dt = new DataTable("MyTable");
        dt.Columns.Add("Name");
        dt.Columns.Add("DateOfBirth");
        dt.Columns.Add("FavouriteColour");
        dt.Rows.Add("Bob", "2001-01-01", "Pink");
        dt.Rows.Add("Frank", "2001-01-01", "Orange");

        var nameCol = new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 20), false)
        { IsPrimaryKey = true };
        DiscoveredTable tbl = db.CreateTable("MyTable", dt, new[]
                {
                    nameCol,
                    new DatabaseColumnRequest("DateOfBirth", new DatabaseTypeRequest(typeof(DateTime)), false)
                        { IsPrimaryKey = true }
                });

        Assert.That(tbl.GetRowCount(), Is.EqualTo(2));

        //define a new load configuration
        var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");

        lmd.IgnoreTrigger = true;
        lmd.SaveToDatabase();
        var ti = Import(tbl, lmd, logManager);

        var projectDirectory = SetupLoadDirectory(lmd);

        CreateCSVProcessTask(lmd, ti, "*.csv");


        var regex = new RegexRedactionConfiguration(CatalogueRepository, "Too Long!", new Regex("Yella"), "FFFFFFFF");
        regex.SaveToDatabase();

        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustRaw)
        {
            Path = typeof(RegexRedactionMutilator).FullName,
            ProcessTaskType = ProcessTaskType.MutilateDataTable,
            Name = $"Mutilate {ti.GetRuntimeName()}"
        };
        pt.SaveToDatabase();

        pt.CreateArgumentsForClassIfNotExists<RegexRedactionMutilator>();
        pt.SetArgumentValue("ColumnRegexPattern", ".*");
        pt.SetArgumentValue("redactionConfiguration", regex);
        pt.SetArgumentValue("TableRegexPattern", ".*");

        pt.Check(ThrowImmediatelyCheckNotifier.Quiet);

        File.WriteAllText(
            Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"),
            @"Name,DateOfBirth,FavouriteColour
MrMurder,2001-01-01,Yella");

        var dbConfig = new HICDatabaseConfiguration(lmd, null);

        var job = new DataLoadJob(RepositoryLocator, "Go go go!", logManager, lmd, projectDirectory,
                ThrowImmediatelyDataLoadEventListener.Quiet, dbConfig);

        var loadFactory = new HICDataLoadFactory(
            lmd,
            dbConfig,
            new HICLoadConfigurationFlags(),
            CatalogueRepository,
            logManager
        );

        var exe = loadFactory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);

        var exitCode = exe.Run(
          job,
            new GracefulCancellationToken());

        var redactions = CatalogueRepository.GetAllObjects<RegexRedaction>();
        Assert.That(redactions.Count(), Is.EqualTo(0));

        var redactionKeys = CatalogueRepository.GetAllObjects<RegexRedactionKey>();
        Assert.That(redactionKeys.Length, Is.EqualTo(0));
    }

    [Test]
    public void RedactionMutilator_RedactAPK()
    {
        var defaults = CatalogueRepository;
        var logServer = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        var logManager = new LogManager(logServer);
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var raw = db.Server.ExpectDatabase($"{db.GetRuntimeName()}_RAW");
        if (raw.Exists())
            raw.Drop();

        var dleStaging = db.Server.ExpectDatabase($"DLE_STAGING");
        if (!dleStaging.Exists())
            db.Server.CreateDatabase("DLE_STAGING");

        using var dt = new DataTable("MyTable");
        dt.Columns.Add("Name");
        dt.Columns.Add("DateOfBirth");
        dt.Columns.Add("FavouriteColour");
        dt.Rows.Add("Bob", "2001-01-01", "Pink");
        dt.Rows.Add("Frank", "2001-01-01", "Orange");

        var nameCol = new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 20), false)
        { IsPrimaryKey = true };
        DiscoveredTable tbl = db.CreateTable("MyTable", dt, new[]
                {
                    nameCol,
                    new DatabaseColumnRequest("DateOfBirth", new DatabaseTypeRequest(typeof(DateTime)), false)
                        { IsPrimaryKey = true }
                });

        Assert.That(tbl.GetRowCount(), Is.EqualTo(2));

        //define a new load configuration
        var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");

        lmd.IgnoreTrigger = true;
        lmd.SaveToDatabase();
        var ti = Import(tbl, lmd, logManager);

        var projectDirectory = SetupLoadDirectory(lmd);

        CreateCSVProcessTask(lmd, ti, "*.csv");


        var regex = new RegexRedactionConfiguration(CatalogueRepository, "No Yelling", new Regex("Yella"), "FFF");
        regex.SaveToDatabase();

        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustRaw)
        {
            Path = typeof(RegexRedactionMutilator).FullName,
            ProcessTaskType = ProcessTaskType.MutilateDataTable,
            Name = $"Mutilate {ti.GetRuntimeName()}"
        };
        pt.SaveToDatabase();

        pt.CreateArgumentsForClassIfNotExists<RegexRedactionMutilator>();
        pt.SetArgumentValue("ColumnRegexPattern", "DateOfBirth");
        pt.SetArgumentValue("redactionConfiguration", regex);
        pt.SetArgumentValue("TableRegexPattern", ".*");

        pt.Check(ThrowImmediatelyCheckNotifier.Quiet);

        File.WriteAllText(
            Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"),
            @"Name,DateOfBirth,FavouriteColour
MrMurder,2001-01-01,Yella");

        var dbConfig = new HICDatabaseConfiguration(lmd, null);

        var job = new DataLoadJob(RepositoryLocator, "Go go go!", logManager, lmd, projectDirectory,
                ThrowImmediatelyDataLoadEventListener.Quiet, dbConfig);

        var loadFactory = new HICDataLoadFactory(
            lmd,
            dbConfig,
            new HICLoadConfigurationFlags(),
            CatalogueRepository,
            logManager
        );

        var exe = loadFactory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);

        var exitCode = exe.Run(
          job,
            new GracefulCancellationToken());

        var redactions = CatalogueRepository.GetAllObjects<RegexRedaction>();
        Assert.That(redactions.Count(), Is.EqualTo(0));

        var redactionKeys = CatalogueRepository.GetAllObjects<RegexRedactionKey>();
        Assert.That(redactionKeys.Length, Is.EqualTo(0));
    }

    [Test]
    public void RedactionMutilator_NoPKS()
    {
        var defaults = CatalogueRepository;
        var logServer = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        var logManager = new LogManager(logServer);
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var raw = db.Server.ExpectDatabase($"{db.GetRuntimeName()}_RAW");
        if (raw.Exists())
            raw.Drop();

        var dleStaging = db.Server.ExpectDatabase($"DLE_STAGING");
        if (!dleStaging.Exists())
            db.Server.CreateDatabase("DLE_STAGING");

        using var dt = new DataTable("MyTable");
        dt.Columns.Add("Name");
        dt.Columns.Add("DateOfBirth");
        dt.Columns.Add("FavouriteColour");
        dt.Rows.Add("Bob", "2001-01-01", "Pink");
        dt.Rows.Add("Frank", "2001-01-01", "Orange");

        var nameCol = new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 20), false)
        { IsPrimaryKey = true };
        DiscoveredTable tbl = db.CreateTable("MyTable", dt, new[]
                {
                    nameCol,
                    new DatabaseColumnRequest("DateOfBirth", new DatabaseTypeRequest(typeof(DateTime)), false)
                        { IsPrimaryKey = false }
                });

        Assert.That(tbl.GetRowCount(), Is.EqualTo(2));

        //define a new load configuration
        var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");

        lmd.IgnoreTrigger = true;
        lmd.SaveToDatabase();
        var ti = Import(tbl, lmd, logManager);

        var projectDirectory = SetupLoadDirectory(lmd);

        CreateCSVProcessTask(lmd, ti, "*.csv");


        var regex = new RegexRedactionConfiguration(CatalogueRepository, "No Yelling", new Regex("Yella"), "FFF");
        regex.SaveToDatabase();

        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustRaw)
        {
            Path = typeof(RegexRedactionMutilator).FullName,
            ProcessTaskType = ProcessTaskType.MutilateDataTable,
            Name = $"Mutilate {ti.GetRuntimeName()}"
        };
        pt.SaveToDatabase();

        pt.CreateArgumentsForClassIfNotExists<RegexRedactionMutilator>();
        pt.SetArgumentValue("ColumnRegexPattern", "DateOfBirth");
        pt.SetArgumentValue("redactionConfiguration", regex);
        pt.SetArgumentValue("TableRegexPattern", ".*");

        pt.Check(ThrowImmediatelyCheckNotifier.Quiet);

        File.WriteAllText(
            Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"),
            @"Name,DateOfBirth,FavouriteColour
MrMurder,2001-01-01,Yella");

        var dbConfig = new HICDatabaseConfiguration(lmd, null);

        var job = new DataLoadJob(RepositoryLocator, "Go go go!", logManager, lmd, projectDirectory,
                ThrowImmediatelyDataLoadEventListener.Quiet, dbConfig);

        var loadFactory = new HICDataLoadFactory(
            lmd,
            dbConfig,
            new HICLoadConfigurationFlags(),
            CatalogueRepository,
            logManager
        );

        var exe = loadFactory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);

        var exitCode = exe.Run(
          job,
            new GracefulCancellationToken());

        var redactions = CatalogueRepository.GetAllObjects<RegexRedaction>();
        Assert.That(redactions.Count(), Is.EqualTo(0));

        var redactionKeys = CatalogueRepository.GetAllObjects<RegexRedactionKey>();
        Assert.That(redactionKeys.Length, Is.EqualTo(0));
    }

    [Test]
    public void RedactionMutilator_MultipleInOneCell()
    {
        var defaults = CatalogueRepository;
        var logServer = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        var logManager = new LogManager(logServer);
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var raw = db.Server.ExpectDatabase($"{db.GetRuntimeName()}_RAW");
        if (raw.Exists())
            raw.Drop();

        var dleStaging = db.Server.ExpectDatabase($"DLE_STAGING");
        if (!dleStaging.Exists())
            db.Server.CreateDatabase("DLE_STAGING");

        using var dt = new DataTable("MyTable");
        dt.Columns.Add("Name");
        dt.Columns.Add("DateOfBirth");
        dt.Columns.Add("FavouriteColour");
        dt.Rows.Add("Bob", "2001-01-01", "PinkPinkPink");
        dt.Rows.Add("Frank", "2001-01-01", "OrangeOrangeOrange");

        var nameCol = new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 20), false)
        { IsPrimaryKey = true };
        DiscoveredTable tbl = db.CreateTable("MyTable", dt, new[]
                {
                    nameCol,
                    new DatabaseColumnRequest("DateOfBirth", new DatabaseTypeRequest(typeof(DateTime)), false)
                        { IsPrimaryKey = true }
                });

        Assert.That(tbl.GetRowCount(), Is.EqualTo(2));

        //define a new load configuration
        var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");

        lmd.IgnoreTrigger = true;
        lmd.SaveToDatabase();
        var ti = Import(tbl, lmd, logManager);

        var projectDirectory = SetupLoadDirectory(lmd);

        CreateCSVProcessTask(lmd, ti, "*.csv");


        var regex = new RegexRedactionConfiguration(CatalogueRepository, "No Yelling", new Regex("Yella"), "FFF");
        regex.SaveToDatabase();

        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustRaw)
        {
            Path = typeof(RegexRedactionMutilator).FullName,
            ProcessTaskType = ProcessTaskType.MutilateDataTable,
            Name = $"Mutilate {ti.GetRuntimeName()}"
        };
        pt.SaveToDatabase();

        pt.CreateArgumentsForClassIfNotExists<RegexRedactionMutilator>();
        pt.SetArgumentValue("ColumnRegexPattern", ".*");
        pt.SetArgumentValue("redactionConfiguration", regex);
        pt.SetArgumentValue("TableRegexPattern", ".*");

        pt.Check(ThrowImmediatelyCheckNotifier.Quiet);

        File.WriteAllText(
            Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"),
            @"Name,DateOfBirth,FavouriteColour
MrMurder,2001-01-01,YellaUUUYella");

        var dbConfig = new HICDatabaseConfiguration(lmd, null);

        var job = new DataLoadJob(RepositoryLocator, "Go go go!", logManager, lmd, projectDirectory,
                ThrowImmediatelyDataLoadEventListener.Quiet, dbConfig);

        var loadFactory = new HICDataLoadFactory(
            lmd,
            dbConfig,
            new HICLoadConfigurationFlags(),
            CatalogueRepository,
            logManager
        );

        var exe = loadFactory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);

        var exitCode = exe.Run(
          job,
            new GracefulCancellationToken());

        var redactions = CatalogueRepository.GetAllObjects<RegexRedaction>();
        Assert.That(redactions.Count(), Is.EqualTo(2));
        Assert.That(redactions[0].RedactionConfiguration_ID, Is.EqualTo(regex.ID));
        Assert.That(redactions[0].StartingIndex, Is.EqualTo(0));
        Assert.That(redactions[0].RedactedValue, Is.EqualTo("Yella"));
        Assert.That(redactions[0].ReplacementValue, Is.EqualTo("<FFF>"));
        Assert.That(redactions[1].RedactionConfiguration_ID, Is.EqualTo(regex.ID));
        Assert.That(redactions[1].StartingIndex, Is.EqualTo(8));
        Assert.That(redactions[1].RedactedValue, Is.EqualTo("Yella"));
        Assert.That(redactions[1].ReplacementValue, Is.EqualTo("<FFF>"));

        var redactionKeys = CatalogueRepository.GetAllObjects<RegexRedactionKey>();
        Assert.That(redactionKeys.Length, Is.EqualTo(4));
        foreach (var r in redactions)
        {
            r.DeleteInDatabase();
        }
        foreach (var r in redactionKeys)
        {
            r.DeleteInDatabase();
        }
    }

    [Test]
    public void RedactionMutilator_SpeedTest()
    {
        var defaults = CatalogueRepository;
        var logServer = defaults.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
        var logManager = new LogManager(logServer);
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        var raw = db.Server.ExpectDatabase($"{db.GetRuntimeName()}_RAW");
        if (raw.Exists())
            raw.Drop();

        var dleStaging = db.Server.ExpectDatabase($"DLE_STAGING");
        if (!dleStaging.Exists())
            db.Server.CreateDatabase("DLE_STAGING");

        using var dt = new DataTable("MyTable");
        dt.Columns.Add("Name");
        dt.Columns.Add("DateOfBirth");
        dt.Columns.Add("FavouriteColour");
        dt.Rows.Add("Bob", "2001-01-01", "Pink");
        dt.Rows.Add("Frank", "2001-01-01", "Orange");

        var nameCol = new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 20), false)
        { IsPrimaryKey = true };
        DiscoveredTable tbl = db.CreateTable("MyTable", dt, new[]
                {
                    nameCol,
                    new DatabaseColumnRequest("DateOfBirth", new DatabaseTypeRequest(typeof(DateTime)), false)
                        { IsPrimaryKey = true }
                });

        Assert.That(tbl.GetRowCount(), Is.EqualTo(2));

        //define a new load configuration
        var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");

        lmd.IgnoreTrigger = true;
        lmd.SaveToDatabase();
        var ti = Import(tbl, lmd, logManager);

        var projectDirectory = SetupLoadDirectory(lmd);

        CreateCSVProcessTask(lmd, ti, "*.csv");


        var regex = new RegexRedactionConfiguration(CatalogueRepository, "PM ONLY", new Regex("amber"), "XX");
        regex.SaveToDatabase();

        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.AdjustRaw)
        {
            Path = typeof(RegexRedactionMutilator).FullName,
            ProcessTaskType = ProcessTaskType.MutilateDataTable,
            Name = $"Mutilate {ti.GetRuntimeName()}"
        };
        pt.SaveToDatabase();

        pt.CreateArgumentsForClassIfNotExists<RegexRedactionMutilator>();
        pt.SetArgumentValue("ColumnRegexPattern", ".*");
        pt.SetArgumentValue("redactionConfiguration", regex);
        pt.SetArgumentValue("TableRegexPattern", ".*");

        pt.Check(ThrowImmediatelyCheckNotifier.Quiet);

        File.WriteAllText(
            Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"),
            @$"Name,DateOfBirth,FavouriteColour
MrMurder,2001-01-01,Yella
");

        var data = Enumerable.Repeat("name,2001-01-01,amber", 1000000).ToArray();


        File.AppendAllLines(Path.Combine(projectDirectory.ForLoading.FullName, "LoadMe.csv"), data);


        var dbConfig = new HICDatabaseConfiguration(lmd, null);

        var job = new DataLoadJob(RepositoryLocator, "Go go go!", logManager, lmd, projectDirectory,
                ThrowImmediatelyDataLoadEventListener.Quiet, dbConfig);

        var loadFactory = new HICDataLoadFactory(
            lmd,
            dbConfig,
            new HICLoadConfigurationFlags(),
            CatalogueRepository,
            logManager
        );

        var exe = loadFactory.Create(ThrowImmediatelyDataLoadEventListener.Quiet);
        var exitCode = exe.Run(
          job,
            new GracefulCancellationToken());

    }
}
