// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.DataLoad.Modules.Exceptions;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;
using TypeGuesser;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class FlatFileAttacherTests : DatabaseTests
{
    private LoadDirectory _loadDirectory;
    private DirectoryInfo _parentDir;
    private DiscoveredDatabase _database;
    private DiscoveredTable _table;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        var workingDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        _parentDir = workingDir.CreateSubdirectory("FlatFileAttacherTests");

        var toCleanup = _parentDir.GetDirectories().SingleOrDefault(d => d.Name.Equals("Test_CSV_Attachment"));
        toCleanup?.Delete(true);

        _loadDirectory = LoadDirectory.CreateDirectoryStructure(_parentDir, "Test_CSV_Attachment");

        // create a separate builder for setting an initial catalogue on (need to figure out how best to stop child classes changing ServerICan... as this then causes TearDown to fail)
        _database = GetCleanedServer(DatabaseType.MicrosoftSQLServer);

        using (var con = _database.Server.GetConnection())
        {
            con.Open();

            var cmdCreateTable = _database.Server.GetCommand(
                $"CREATE Table {_database.GetRuntimeName()}..Bob([name] [varchar](500),[name2] [varchar](500))", con);
            cmdCreateTable.ExecuteNonQuery();
        }

        _table = _database.ExpectTable("Bob");
    }

    [Test]
    [TestCase(",", false)]
    [TestCase("|", false)] //wrong separator
    [TestCase(",", true)]
    public void Test_CSV_Attachment(string separator, bool overrideHeaders)
    {
        var filename = Path.Combine(_loadDirectory.ForLoading.FullName, "bob.csv");
        using (var sw = new StreamWriter(filename))
        {

            sw.WriteLine("name,name2");
            sw.WriteLine("Bob,Munchousain");
            sw.WriteLine("Franky,Hollyw9ood");

            sw.Flush();
            sw.Close();
        }


        var filename2 = Path.Combine(_loadDirectory.ForLoading.FullName, "bob2.csv");
        using (var sw2 = new StreamWriter(filename2))
        {
            sw2.WriteLine("name,name2");
            sw2.WriteLine("Manny2,Ok");

            sw2.Flush();
            sw2.Close();
        };

        var attacher = new AnySeparatorFileAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.Separator = separator;
        attacher.FilePattern = "bob*";
        attacher.TableName = "Bob";

        if (overrideHeaders)
        {
            attacher.ForceHeaders = "name,name2";
            attacher.ForceHeadersReplacesFirstLineInFile = true;
        }

        //Case when you are using the wrong separator
        if (separator == "|")
        {
            var ex = Assert.Throws<FlatFileLoadException>(() =>
                attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));

            Assert.That(ex.InnerException, Is.Not.Null);
            Assert.That(
                ex.InnerException.Message, Does.StartWith("Your separator does not appear in the headers line of your file (bob.csv) but the separator ',' does"));
            return;
        }

        //other cases (i.e. correct separator)
        attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

        var table = _database.ExpectTable("Bob");
        Assert.That(table.Exists());

        table.DiscoverColumn("name");
        table.DiscoverColumn("name2");

        using (var con = _database.Server.GetConnection())
        {
            con.Open();
            var r = _database.Server.GetCommand("Select * from Bob order by name", con).ExecuteReader();
            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Bob"));
                Assert.That(r["name2"], Is.EqualTo("Munchousain"));
            });

            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Franky"));
                Assert.That(r["name2"], Is.EqualTo("Hollyw9ood"));
            });

            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Manny2"));
                Assert.That(r["name2"], Is.EqualTo("Ok"));
            });
        }

        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);

        File.Delete(filename);
    }


    [Test]
    public void Test_ExplicitDateTimeFormat_Attachment()
    {
        var filename = Path.Combine(_loadDirectory.ForLoading.FullName, "bob.csv");
        using (var sw = new StreamWriter(filename))
        {

            sw.WriteLine("name,name2");
            sw.WriteLine("Bob,20011301");
            sw.WriteLine("Franky,20021301");

            sw.Flush();
            sw.Close();
        }

        var attacher = new AnySeparatorFileAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.Separator = ",";
        attacher.FilePattern = "bob*";
        attacher.TableName = "Bob";
        attacher.ExplicitDateTimeFormat = "yyyyddMM";


        var table = _database.ExpectTable("Bob");
        table.Truncate();

        Assert.That(table.Exists());
        table.DiscoverColumn("name");
        var name2 = table.DiscoverColumn("name2");
        name2.DataType.AlterTypeTo("datetime2");

        //other cases (i.e. correct separator)
        attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

        using (var con = _database.Server.GetConnection())
        {
            con.Open();
            var r = _database.Server.GetCommand("Select * from Bob", con).ExecuteReader();
            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Bob"));
                Assert.That(r["name2"], Is.EqualTo(new DateTime(2001, 01, 13)));
            });

            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Franky"));
                Assert.That(r["name2"], Is.EqualTo(new DateTime(2002, 01, 13)));
            });
        }

        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);

        File.Delete(filename);
    }

    [Test]
    public void TabTestWithOverrideHeaders()
    {
        var filename = Path.Combine(_loadDirectory.ForLoading.FullName, "bob.csv");
        using (var sw = new StreamWriter(filename))
        {

            sw.WriteLine("Face\tBasher");
            sw.WriteLine("Candy\tCrusher");

            sw.Flush();
            sw.Close();
        }

        var attacher = new AnySeparatorFileAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.Separator = "\\t";
        attacher.FilePattern = "bob*";
        attacher.TableName = "Bob";
        attacher.ForceHeaders = "name\tname2";

        var exitCode = attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());
        Assert.That(exitCode, Is.EqualTo(ExitCodeType.Success));

        using (var con = _database.Server.GetConnection())
        {
            con.Open();
            var r = _database.Server.GetCommand("Select name,name2 from Bob", con).ExecuteReader();
            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Face"));
                Assert.That(r["name2"], Is.EqualTo("Basher"));
            });

            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Candy"));
                Assert.That(r["name2"], Is.EqualTo("Crusher"));
            });
        }

        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);

        File.Delete(filename);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TabTestWithOverrideHeaders_IncludePath(bool columnExistsInRaw)
    {
        var filename = Path.Combine(_loadDirectory.ForLoading.FullName, "bob.csv");
        using (var sw = new StreamWriter(filename))
        {

            sw.WriteLine("Face\tBasher");
            sw.WriteLine("Candy\tCrusher");

            sw.Flush();
            sw.Close();
        }

        if (columnExistsInRaw)
            _table.AddColumn("FilePath", new DatabaseTypeRequest(typeof(string), 500), true, 30);

        var attacher = new AnySeparatorFileAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.Separator = "\\t";
        attacher.FilePattern = "bob*";
        attacher.TableName = "Bob";
        attacher.ForceHeaders = "name\tname2";
        attacher.AddFilenameColumnNamed = "FilePath";

        if (!columnExistsInRaw)
        {
            var ex = Assert.Throws<FlatFileLoadException>(() =>
                attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
            Assert.That(ex.InnerException.Message, Is.EqualTo("AddFilenameColumnNamed is set to 'FilePath' but the column did not exist in RAW"));
            return;
        }


        var exitCode = attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());
        Assert.That(exitCode, Is.EqualTo(ExitCodeType.Success));

        using (var con = _database.Server.GetConnection())
        {
            con.Open();
            var r = _database.Server.GetCommand("Select name,name2,FilePath from Bob", con).ExecuteReader();
            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Face"));
                Assert.That(r["name2"], Is.EqualTo("Basher"));
                Assert.That(r["FilePath"], Is.EqualTo(filename));
            });

            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Candy"));
                Assert.That(r["name2"], Is.EqualTo("Crusher"));
            });
        }

        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);

        File.Delete(filename);
    }


    [TestCase(true)]
    [TestCase(false)]
#pragma warning disable VSSpell001 // Spell Check
    public void TestTableInfo(bool username)
#pragma warning restore VSSpell001 // Spell Check
    {
        var filename = Path.Combine(_loadDirectory.ForLoading.FullName, "bob.csv");
        using (var sw = new StreamWriter(filename))
        {

            sw.WriteLine("name,name2");
            sw.WriteLine("Bob,Munchousain");
            sw.WriteLine("Franky,Hollyw9ood");

            sw.Flush();
            sw.Close();
        }
        Import(_table, out var ti, out _);

        var attacher = new AnySeparatorFileAttacher();
        attacher.Initialize(_loadDirectory, _database);
        attacher.Separator = ",";
        attacher.FilePattern = "bob*";
        attacher.TableToLoad = ti;

        INameDatabasesAndTablesDuringLoads namer = null;

        if (username)
        {
            _table.Rename("AAA");
            namer = RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(_database, "AAA");
        }

        var job = new ThrowImmediatelyDataLoadJob(new HICDatabaseConfiguration(_database.Server, namer), ti);

        var exitCode = attacher.Attach(job, new GracefulCancellationToken());
        Assert.That(exitCode, Is.EqualTo(ExitCodeType.Success));

        using (var con = _database.Server.GetConnection())
        {
            con.Open();
            var r = _database.Server.GetCommand($"Select name,name2 from {_table.GetRuntimeName()}", con)
                .ExecuteReader();
            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Bob"));
                Assert.That(r["name2"], Is.EqualTo("Munchousain"));
            });

            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Franky"));
                Assert.That(r["name2"], Is.EqualTo("Hollyw9ood"));
            });
        }

        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);

        File.Delete(filename);
    }


    [Test]
    public void Test_FlatFileAttacher_IgnoreColumns()
    {
        var filename = Path.Combine(_loadDirectory.ForLoading.FullName, "bob.csv");
        using (var sw = new StreamWriter(filename))
        {

            sw.WriteLine("name,name2,address");
            sw.WriteLine("Bob,Munchousain,\"67, franklin\"");
            sw.WriteLine("Franky,Hollyw9ood,32 dodgery");

            sw.Flush();
            sw.Close();
        }
        Import(_table, out var ti, out _);

        var attacher = new AnySeparatorFileAttacher
        {
            Separator = ",",
            FilePattern = "bob*",
            TableToLoad = ti,
            IgnoreColumns = "address"
        };
        attacher.Initialize(_loadDirectory, _database);

        var job = new ThrowImmediatelyDataLoadJob(new HICDatabaseConfiguration(_database.Server, null), ti);

        var exitCode = attacher.Attach(job, new GracefulCancellationToken());
        Assert.That(exitCode, Is.EqualTo(ExitCodeType.Success));

        using (var con = _database.Server.GetConnection())
        {
            con.Open();
            var r = _database.Server.GetCommand($"Select name,name2 from {_table.GetRuntimeName()}", con)
                .ExecuteReader();
            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Bob"));
                Assert.That(r["name2"], Is.EqualTo("Munchousain"));
            });

            Assert.Multiple(() =>
            {
                Assert.That(r.Read());
                Assert.That(r["name"], Is.EqualTo("Franky"));
                Assert.That(r["name2"], Is.EqualTo("Hollyw9ood"));
            });
        }

        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);

        File.Delete(filename);
    }

    [TestCase(DatabaseType.MySql, "27/01/2001", "en-GB", "en-GB")]
    [TestCase(DatabaseType.MySql, "27/01/2001", "en-GB", "en-us")]
    [TestCase(DatabaseType.MySql, "01/27/2001", "en-us", "en-us")]
    [TestCase(DatabaseType.MySql, "01/27/2001", "en-us", "en-GB")]
    [TestCase(DatabaseType.MicrosoftSQLServer, "27/01/2001", "en-GB", "en-GB")]
    [TestCase(DatabaseType.MicrosoftSQLServer, "27/01/2001", "en-GB", "en-us")]
    [TestCase(DatabaseType.MicrosoftSQLServer, "01/27/2001", "en-us", "en-us")]
    [TestCase(DatabaseType.MicrosoftSQLServer, "01/27/2001", "en-us", "en-GB")]
    [TestCase(DatabaseType.Oracle, "27/01/2001", "en-GB", "en-GB")]
    [TestCase(DatabaseType.Oracle, "27/01/2001", "en-GB", "en-us")]
    [TestCase(DatabaseType.Oracle, "01/27/2001", "en-us", "en-us")]
    [TestCase(DatabaseType.Oracle, "01/27/2001", "en-us", "en-GB")]
    public void Test_FlatFileAttacher_AmbiguousDates(DatabaseType type, string val, string attacherCulture,
        string threadCulture)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo(threadCulture);

        var filename = Path.Combine(_loadDirectory.ForLoading.FullName, "bob.csv");
        using (var sw = new StreamWriter(filename))
        {
            sw.WriteLine("dob");
            sw.WriteLine(val);

            sw.Flush();
            sw.Close();
        }

        var db = GetCleanedServer(type);

        var tbl =
            db.CreateTable("AmbiguousDatesTestTable",
                new[] { new DatabaseColumnRequest("dob", new DatabaseTypeRequest(typeof(DateTime))) }
            );


        Import(tbl, out var ti, out _);
        var attacher = new AnySeparatorFileAttacher
        {
            Separator = ",",
            FilePattern = "bob*",
            TableName = tbl.GetRuntimeName(),
            Culture = new CultureInfo(attacherCulture)
        };
        attacher.Initialize(_loadDirectory, db);

        var job = new ThrowImmediatelyDataLoadJob(new HICDatabaseConfiguration(_database.Server, null), ti);

        var exitCode = attacher.Attach(job, new GracefulCancellationToken());
        Assert.That(exitCode, Is.EqualTo(ExitCodeType.Success));

        attacher.LoadCompletedSoDispose(ExitCodeType.Success, ThrowImmediatelyDataLoadEventListener.Quiet);

        Assert.That(tbl.GetDataTable().Rows[0][0], Is.EqualTo(new DateTime(2001, 1, 27)));

        File.Delete(filename);
        tbl.Drop();
    }

    [Test]
    public void Test_TableToLoad_IDNotInLoadMetadata()
    {
        var source = new AnySeparatorFileAttacher();

        var tiInLoad = new TableInfo(CatalogueRepository, "TableInLoad");
        var tiNotInLoad = new TableInfo(CatalogueRepository, "TableNotInLoad");

        source.TableToLoad = tiNotInLoad;

        var job = new ThrowImmediatelyDataLoadJob(ThrowImmediatelyDataLoadEventListener.QuietPicky)
        {
            RegularTablesToLoad = new System.Collections.Generic.List<ITableInfo>(new[] { tiInLoad })
        };


        var ex = Assert.Throws<Exception>(() => source.Attach(job, new GracefulCancellationToken()));

        Assert.That(
            ex.Message, Does.Match("FlatFileAttacher TableToLoad was 'TableNotInLoad' \\(ID=\\d+\\) but that table was not one of the tables in the load:'TableInLoad'"));
    }
}