// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CatalogueLibrary;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Attachers;
using DataLoadEngine.Job;
using FAnsi.Discovery;
using LoadModules.Generic.Attachers;
using LoadModules.Generic.Exceptions;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataLoadEngineTests.Unit
{
    [Category("Integration")]
    public class MDFAttacherTests : DatabaseTests
    {
        [Test]
        public void TestNoMDFFileFoundException()
        {
            var workingDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            var testDir = workingDir.CreateSubdirectory("MDFAttacherTests");
            var loadDirectory = LoadDirectory.CreateDirectoryStructure(testDir, "TestNoMDFFileFoundException",true);

            try
            {
                var attacher = new MDFAttacher();
                attacher.Initialize(loadDirectory, DiscoveredDatabaseICanCreateRandomTablesIn);
                Assert.Throws<FileNotFoundException>(() => attacher.Attach(null, new GracefulCancellationToken()));
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

        [Test]
        public void TestLocations_NoNetworkPath()
        {
            foreach (string remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.mdf"))
                File.Delete(remnant);

            foreach (string remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.ldf"))
                File.Delete(remnant);

            var mdf = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile.mdf");
            var ldf = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile_log.ldf");

            try
            {
                File.WriteAllText(mdf, "fish");
                File.WriteAllText(ldf, "fish");

                string serverDatabasePath = @"H:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\DATA\";
                var locations = new MdfFileAttachLocations(new DirectoryInfo(TestContext.CurrentContext.TestDirectory), serverDatabasePath, null);
                

                Assert.AreEqual(new FileInfo(mdf).FullName, locations.OriginLocationMdf);
                Assert.AreEqual(new FileInfo(ldf).FullName, locations.OriginLocationLdf);
                
                Assert.AreEqual(@"H:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\DATA\MyFile_log.ldf", locations.CopyToLdf);
                Assert.AreEqual(@"H:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\DATA\MyFile.mdf", locations.CopyToMdf);

                Assert.AreEqual(@"H:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\DATA\MyFile.mdf", locations.AttachMdfPath);
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
            foreach (string remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.mdf"))
                File.Delete(remnant);

            foreach (string remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.ldf"))
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

                string serverDatabasePath = @"c:\temp\";
                Assert.Throws<MultipleMatchingFilesException>(()=>new MdfFileAttachLocations(new DirectoryInfo(TestContext.CurrentContext.TestDirectory), serverDatabasePath, null));
                
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
            var hicProjDir = LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),"MDFAttacherTest", true);

            var db = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase("MyImaginaryDB_RAW");
            Assert.IsFalse(db.Exists());

            var mdf = new MDFAttacher();
            mdf.Initialize(hicProjDir, db);
            try
            {
                var memory = new ToMemoryCheckNotifier(new ThrowImmediatelyCheckNotifier());
                mdf.Check(memory);
                Assert.IsTrue(memory.Messages.Any(m=>m.Message.Contains("Found server DATA folder") && m.Result == CheckResult.Success));
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("Proposed server DATA folder (that we will copy mdf/ldf files to) was not found"))//this message is allowed too if the SQL server is remote and not localhost then it is quite likely that the DATA path is inaccessible from the unit test server
                    throw;
            }

            var memory2 = new ToMemoryCheckNotifier(new ThrowImmediatelyCheckNotifier());
            mdf.OverrideMDFFileCopyDestination = @"C:\temp";
            mdf.Check(memory2);
            Assert.IsTrue(memory2.Messages.Any(m => Regex.IsMatch(m.Message,@"Found server DATA folder .*C:\\temp") && m.Result == CheckResult.Success));

            hicProjDir.RootPath.Delete(true);

        }

        [Test]
        public void TestLocations_NetworkPath()
        {
            foreach (string remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.mdf"))
                File.Delete(remnant);

            foreach (string remnant in Directory.EnumerateFiles(TestContext.CurrentContext.TestDirectory, "MyFile*.ldf"))
                File.Delete(remnant);

            var mdf = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile.mdf");
            var ldf = Path.Combine(TestContext.CurrentContext.TestDirectory, "MyFile_log.ldf");

            try
            {
                File.WriteAllText(mdf, "fish");
                File.WriteAllText(ldf, "fish");

                string serverDatabasePath = @"H:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\DATA\";
                var locations = new MdfFileAttachLocations(new DirectoryInfo(TestContext.CurrentContext.TestDirectory), serverDatabasePath, @"\\MyDbServer1\Share\Database");


                Assert.AreEqual(new FileInfo(mdf).FullName, locations.OriginLocationMdf);
                Assert.AreEqual(new FileInfo(ldf).FullName, locations.OriginLocationLdf);

                Assert.AreEqual(@"\\MyDbServer1\Share\Database\MyFile_log.ldf", locations.CopyToLdf);
                Assert.AreEqual(@"\\MyDbServer1\Share\Database\MyFile.mdf", locations.CopyToMdf);

                Assert.AreEqual(@"H:\Program Files\Microsoft SQL Server\MSSQL13.SQLEXPRESS\MSSQL\DATA\MyFile.mdf", locations.AttachMdfPath);
            }
            finally
            {
                File.Delete(mdf);
                File.Delete(ldf);
            }
        }
       
        public class MyClass:IAttacher,ICheckable
        {
            public ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            

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

            public string GetDescription()
            {
                return "Test class that does nothing";
            }

            

            public void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
            {
                
            }
        }

        [Test]
        public void TestConstructors()
        {
            string actualName = typeof(MyClass).ToString();

            Console.WriteLine("About to instantiate a :" + actualName);

            //convert type name as a string into a legit Type
            var type = Type.GetType(actualName, true, false);

            //ensure that the Type implements IAttacher
            if(!typeof(IAttacher).IsAssignableFrom(type))
                throw new TypeLoadException("Type " + type + " does not implement IAttacher");

            //find the blank constructor
            ConstructorInfo constructorInfo = type.GetConstructor(new Type[] {});
            
            //if it doesnt have one
            if(constructorInfo == null)
                throw new TypeLoadException("Type " + type + " does not have a blank constructor");

            //call the blank constructor and return the reuslts
            IAttacher bob = (IAttacher) constructorInfo.Invoke(new Type[] {});

        
        }
        [Test]
        public void TestFactory()
        {
            var workingDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);;
            var testDir = workingDir.CreateSubdirectory("MDFAttacherTests_TestFactory");
            var loadDirectory = LoadDirectory.CreateDirectoryStructure(testDir, "TestFactory", true);

            try
            {
                
                var attacher = CatalogueRepository.MEF.FactoryCreateA<IAttacher>(typeof(MDFAttacher).FullName);
                attacher.Initialize(loadDirectory, DiscoveredDatabaseICanCreateRandomTablesIn);

                Assert.IsNotNull(attacher);
                Assert.IsInstanceOf<MDFAttacher>(attacher);
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
}
