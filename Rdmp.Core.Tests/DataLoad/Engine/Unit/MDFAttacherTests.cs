// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.Attachers;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.DataLoad.Modules.Exceptions;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Unit;

public class MDFAttacherTests : DatabaseTests
{
    [Test]
    public void TestNoMDFFileFoundException()
    {
        var workingDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        var testDir = workingDir.CreateSubdirectory("MDFAttacherTests");
        var loadDirectory = LoadDirectory.CreateDirectoryStructure(testDir, "TestNoMDFFileFoundException", true);

        try
        {
            var attacher = new MDFAttacher();
            attacher.Initialize(loadDirectory, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer));
            Assert.Throws<FileNotFoundException>(() =>
                attacher.Attach(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken()));
        }
        finally
        {
            try
            {
                testDir.Delete(true);
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }
    }

    /// <summary>
    /// RDMPDEV-1550 tests system behaviour of <see cref="MDFAttacher"/> when the MDF file in ForLoading already exists
    /// in the data path of the server to be loaded
    /// </summary>
    [Test]
    public void Test_MDFFile_AlreadyExists()
    {
        var workingDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);

        var data = workingDir.CreateSubdirectory("data");

        var testDir = workingDir.CreateSubdirectory("MDFAttacherTests");
        var loadDirectory = LoadDirectory.CreateDirectoryStructure(testDir, "TestNoMDFFileFoundException", true);

        try
        {
            // create mdf and ldf files (in ForLoading
            File.WriteAllText(Path.Combine(loadDirectory.ForLoading.FullName, "MyFile.mdf"), "fish");
            File.WriteAllText(Path.Combine(loadDirectory.ForLoading.FullName, "MyFile_log.ldf"), "fish");

            //create an already existing file in the 'data' directory (imitates the copy to location)
            File.WriteAllText(Path.Combine(data.FullName, "MyFile.mdf"), "fish");


            var attacher = new MDFAttacher
            {
                OverrideMDFFileCopyDestination = data.FullName
            };

            attacher.Initialize(loadDirectory, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer));

            //should be a warning since overwriting is default behaviour
            var ex = Assert.Throws<Exception>(() =>
                attacher.Attach(
                    new ThrowImmediatelyDataLoadJob(ThrowImmediatelyDataLoadEventListener.QuietPicky)
                    , new GracefulCancellationToken())
            );

            Assert.That(ex?.Message, Does.Contain("Overwriting"));
        }
        finally
        {
            try
            {
                data.Delete(true);
                testDir.Delete(true);
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }
    }

    [Test]
    public void TestLocations_NoNetworkPath()
    {
        foreach (var remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.mdf"))
            File.Delete(remnant);

        foreach (var remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.ldf"))
            File.Delete(remnant);

        var mdf = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile.mdf");
        var ldf = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile_log.ldf");

        try
        {
            File.WriteAllText(mdf, "fish");
            File.WriteAllText(ldf, "fish");

            var serverDatabasePath = @"H:/Program Files/Microsoft SQL Server/MSSQL13.SQLEXPRESS/MSSQL/DATA/";
            var locations = new MdfFileAttachLocations(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
                serverDatabasePath, null);

            Assert.Multiple(() =>
            {
                Assert.That(locations.OriginLocationMdf, Is.EqualTo(new FileInfo(mdf).FullName));
                Assert.That(locations.OriginLocationLdf, Is.EqualTo(new FileInfo(ldf).FullName));

                Assert.That(locations.CopyToLdf, Is.EqualTo(@"H:/Program Files/Microsoft SQL Server/MSSQL13.SQLEXPRESS/MSSQL/DATA/MyFile_log.ldf"));
                Assert.That(locations.CopyToMdf, Is.EqualTo(@"H:/Program Files/Microsoft SQL Server/MSSQL13.SQLEXPRESS/MSSQL/DATA/MyFile.mdf"));

                Assert.That(locations.AttachMdfPath, Is.EqualTo(@"H:/Program Files/Microsoft SQL Server/MSSQL13.SQLEXPRESS/MSSQL/DATA/MyFile.mdf"));
            });
        }
        finally
        {
            File.Delete(mdf);
            File.Delete(ldf);
        }
    }

    [Test]
    public void TestTwoFiles()
    {
        foreach (var remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.mdf"))
            File.Delete(remnant);

        foreach (var remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.ldf"))
            File.Delete(remnant);

        var mdf1 = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile1.mdf");
        var mdf2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile2.mdf");

        var ldf1 = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile1_log.ldf");
        var ldf2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile2_log.ldf");
        try
        {
            File.WriteAllText(mdf1, "fish");
            File.WriteAllText(mdf2, "fish");
            File.WriteAllText(ldf1, "fish");
            File.WriteAllText(ldf2, "fish");

            var serverDatabasePath = TestContext.CurrentContext.WorkDirectory;
            Assert.Throws<MultipleMatchingFilesException>(() =>
                new MdfFileAttachLocations(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
                    serverDatabasePath, null));
        }
        finally
        {
            File.Delete(mdf1);
            File.Delete(mdf2);
            File.Delete(ldf1);
            File.Delete(ldf2);
        }
    }

    [Test]
    public void ConnectToServer()
    {
        var hicProjDir =
            LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
                "MDFAttacherTest", true);

        var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("MyImaginaryDB_RAW");
        Assert.That(db.Exists(), Is.False);

        var mdf = new MDFAttacher();
        mdf.Initialize(hicProjDir, db);
        try
        {
            var memory = new ToMemoryCheckNotifier(ThrowImmediatelyCheckNotifier.Quiet);
            mdf.Check(memory);
            Assert.That(memory.Messages.Any(m =>
                m.Message.Contains("Found server DATA folder") && m.Result == CheckResult.Success));
        }
        catch (Exception e)
        {
            if (!e.Message.Contains(
                    "Proposed server DATA folder (that we will copy mdf/ldf files to) was not found")) //this message is allowed too if the SQL server is remote and not localhost then it is quite likely that the DATA path is inaccessible from the unit test server
                throw;
        }

        var memory2 = new ToMemoryCheckNotifier(ThrowImmediatelyCheckNotifier.Quiet);
        mdf.OverrideMDFFileCopyDestination = TestContext.CurrentContext.WorkDirectory;
        mdf.Check(memory2);
        Assert.That(memory2.Messages.Any(m => Regex.IsMatch(m.Message,
                                                    $@"Found server DATA folder .*{Regex.Escape(TestContext.CurrentContext.WorkDirectory)}") &&
                                                m.Result == CheckResult.Success));

        hicProjDir.RootPath.Delete(true);
    }

    [Test]
    public void TestLocations_NetworkPath()
    {
        foreach (var remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.mdf"))
            File.Delete(remnant);

        foreach (var remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.ldf"))
            File.Delete(remnant);

        var mdf = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile.mdf");
        var ldf = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile_log.ldf");

        try
        {
            File.WriteAllText(mdf, "fish");
            File.WriteAllText(ldf, "fish");

            var serverDatabasePath = @"H:/Program Files/Microsoft SQL Server/MSSQL13.SQLEXPRESS/MSSQL/DATA/";
            var locations = new MdfFileAttachLocations(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
                serverDatabasePath, @"//MyDbServer1/Share/Database");

            Assert.Multiple(() =>
            {
                Assert.That(locations.OriginLocationMdf, Is.EqualTo(new FileInfo(mdf).FullName));
                Assert.That(locations.OriginLocationLdf, Is.EqualTo(new FileInfo(ldf).FullName));

                Assert.That(locations.CopyToLdf, Does.Match(@"//MyDbServer1/Share/Database[/\\]MyFile_log.ldf"));
                Assert.That(locations.CopyToMdf, Does.Match(@"//MyDbServer1/Share/Database[/\\]MyFile.mdf"));

                Assert.That(locations.AttachMdfPath, Is.EqualTo(@"H:/Program Files/Microsoft SQL Server/MSSQL13.SQLEXPRESS/MSSQL/DATA/MyFile.mdf"));
            });
        }
        finally
        {
            File.Delete(mdf);
            File.Delete(ldf);
        }
    }

    public class MyClass : IAttacher
    {
        public ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken) =>
            throw new NotImplementedException();


        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public ILoadDirectory LoadDirectory { get; set; }

        public string DatabaseServer { get; private set; }
        public string DatabaseName { get; private set; }
        public bool RequestsExternalDatabaseCreation { get; private set; }

        public void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
        {
        }

        public static string GetDescription() => "Test class that does nothing";


        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
        {
        }
    }

    [Test]
    public void TestConstructors()
    {
        var actualName = typeof(MyClass).ToString();

        Console.WriteLine($"About to instantiate a :{actualName}");

        //convert type name as a string into a legit Type
        var type = Type.GetType(actualName, true, false);

        //ensure that the Type implements IAttacher
        if (!typeof(IAttacher).IsAssignableFrom(type))
            throw new TypeLoadException($"Type {type} does not implement IAttacher");

        //find the blank constructor
        var constructorInfo = type.GetConstructor(Array.Empty<Type>()) ??
                              throw new TypeLoadException($"Type {type} does not have a blank constructor");

        //call the blank constructor and return the results
        _ = (IAttacher)constructorInfo.Invoke(Array.Empty<object>());


        //call the blank constructor and return the results
        var bob = (IAttacher)constructorInfo.Invoke(Array.Empty<Type>());
    }

    [Test]
    public void TestFactory()
    {
        var workingDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        var testDir = workingDir.CreateSubdirectory("MDFAttacherTests_TestFactory");
        var loadDirectory = LoadDirectory.CreateDirectoryStructure(testDir, "TestFactory", true);

        var attacher = MEF.CreateA<IAttacher>(typeof(MDFAttacher).FullName);
        try
        {
            attacher.Initialize(loadDirectory, GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer));

            Assert.That(attacher, Is.Not.Null);
            Assert.That(attacher, Is.InstanceOf<MDFAttacher>());
        }
        finally
        {
            try
            {
                testDir.Delete(true);
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}