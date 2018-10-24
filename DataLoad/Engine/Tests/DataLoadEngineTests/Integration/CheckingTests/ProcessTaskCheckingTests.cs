using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Checks.Checkers;
using LoadModules.Generic.Attachers;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace DataLoadEngineTests.Integration.CheckingTests
{
    public class ProcessTaskCheckingTests:DatabaseTests
    {
        private LoadMetadata _lmd;
        private ProcessTask _task;
        private ProcessTaskChecks _checker;
        private DirectoryInfo _dir;

        [SetUp]
        public void CreateTask()
        {
            _lmd = new LoadMetadata(CatalogueRepository);

            _dir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"ProcessTaskCheckingTests"));
            _dir.Create();

            var hicdir = HICProjectDirectory.CreateDirectoryStructure(_dir, "ProjDir", true);
            _lmd.LocationOfFlatFiles = hicdir.RootPath.FullName;
            _lmd.SaveToDatabase();

            Catalogue c = new Catalogue(CatalogueRepository,"c");
            CatalogueItem ci = new CatalogueItem(CatalogueRepository,c,"ci");
            TableInfo t = new TableInfo(CatalogueRepository,"t");
            t.Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name;
            t.Database = "mydb";
            t.SaveToDatabase();
            ColumnInfo col = new ColumnInfo(CatalogueRepository,"col","bit",t);
            ci.SetColumnInfo(col);
            c.LoadMetadata_ID = _lmd.ID;
            c.SaveToDatabase();
            
            _task = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles);
            _checker = new ProcessTaskChecks(_lmd);
        }

        [TearDown]
        public void DeleteTask()
        {
            _task.DeleteInDatabase();

            _lmd.GetDistinctTableInfoList(true).ForEach(t => t.DeleteInDatabase());
            _lmd.GetAllCatalogues().Cast<IDeleteable>().Single().DeleteInDatabase();
            
            _lmd.DeleteInDatabase();
            
            _dir.Delete(true);
        }


        [Test]
        [TestCase(null,ProcessTaskType.Executable)]
        [TestCase("",ProcessTaskType.Executable)]
        [TestCase("     ",ProcessTaskType.Executable)]
        [TestCase(null,ProcessTaskType.SQLFile)]
        [TestCase("",ProcessTaskType.SQLFile)]
        [TestCase("     ",ProcessTaskType.SQLFile)]
        public void EmptyFilePath(string path, ProcessTaskType typeThatRequiresFiles)
        {
            _task.ProcessTaskType = typeThatRequiresFiles;
            _task.Path = path;
            _task.SaveToDatabase();
            var ex = Assert.Throws<Exception>(()=>_checker.Check(new ThrowImmediatelyCheckNotifier()));
            StringAssert.Contains("does not have a path specified",ex.Message);
        }

        [Test]
        [TestCase(null, ProcessTaskType.MutilateDataTable,LoadStage.AdjustStaging)]
        [TestCase("", ProcessTaskType.MutilateDataTable, LoadStage.AdjustStaging)]
        [TestCase("     ", ProcessTaskType.MutilateDataTable, LoadStage.AdjustRaw)]
        [TestCase(null, ProcessTaskType.Attacher, LoadStage.Mounting)]
        [TestCase(null, ProcessTaskType.DataProvider, LoadStage.GetFiles)]
        public void EmptyClassPath(string path, ProcessTaskType typeThatRequiresMEF, LoadStage stage)
        {
            _task.ProcessTaskType = typeThatRequiresMEF;
            _task.Path = path;
            _task.LoadStage = stage;
            _task.SaveToDatabase();
            var ex = Assert.Throws<ArgumentException>(()=>_checker.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.IsTrue(Regex.IsMatch(ex.Message,"Path is blank for ProcessTask 'New Process.*' - it should be a class name of type"));
        }

        [Test]
        public void MEFIncompatibleType()
        {
            _task.LoadStage = LoadStage.AdjustStaging;
            _task.ProcessTaskType = ProcessTaskType.MutilateDataTable;
            _task.Path = typeof(object).ToString();
            _task.SaveToDatabase();
            var ex = Assert.Throws<KeyNotFoundException>(() => _checker.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.AreEqual("Could not find [Export] of type System.Object using MEF  possibly because it is not declared as [Export(typeof(IMutilateDataTables))].", ex.Message);
        }
        [Test]
        public void MEFCompatibleType_NoProjectDirectory()
        {
            _lmd.LocationOfFlatFiles = null;
            _lmd.SaveToDatabase();

            _task.ProcessTaskType = ProcessTaskType.Attacher;
            _task.LoadStage = LoadStage.Mounting;
            _task.Path = typeof(AnySeparatorFileAttacher).FullName;
            _task.SaveToDatabase();
            _task.CreateArgumentsForClassIfNotExists<AnySeparatorFileAttacher>();

            var ex = Assert.Throws<Exception>(()=>_checker.Check(new ThrowImmediatelyCheckNotifier(){ThrowOnWarning = true}));
            Assert.AreEqual(@"No Project Directory (LocationOfFlatFiles) has been configured on LoadMetadata " + _lmd.Name, ex.InnerException.Message);
            
        }
        [Test]
        public void MEFCompatibleType_NoArgs()
        {

            var projDir = HICProjectDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.WorkDirectory), "DelMeProjDir", true);
            try
            {
                _lmd.LocationOfFlatFiles = projDir.RootPath.FullName;
                _task.ProcessTaskType = ProcessTaskType.Attacher;
                _task.LoadStage = LoadStage.Mounting;
                _task.Path = typeof(AnySeparatorFileAttacher).FullName;
                _task.SaveToDatabase();


                var ex = Assert.Throws<ArgumentException>(() => _checker.Check(new ThrowImmediatelyCheckNotifier() { ThrowOnWarning = true }));

                Assert.AreEqual(@"Class AnySeparatorFileAttacher has a property Separatormarked with DemandsInitialization but no corresponding argument was provided in ArgumentCollection",ex.Message);
                

            }
            finally
            {
                //delete everything for real
                projDir.RootPath.Delete(true);
            }
        }

        [Test]
        public void MEFCompatibleType_Passes()
        {
            var projDir = HICProjectDirectory.CreateDirectoryStructure(new DirectoryInfo("."), "DelMeProjDir", true);
            try
            {
                _lmd.LocationOfFlatFiles = projDir.RootPath.FullName;
                _task.ProcessTaskType = ProcessTaskType.Attacher;
                _task.LoadStage = LoadStage.Mounting;
                _task.Path = typeof(AnySeparatorFileAttacher).FullName;
                _task.SaveToDatabase();
                
                //create the arguments
                var args = ProcessTaskArgument.CreateArgumentsForClassIfNotExists<AnySeparatorFileAttacher>(_task);

                var tblName = (ProcessTaskArgument)args.Single(a => a.Name.Equals("TableName"));
                tblName.Value = "MyExcitingTable";
                tblName.SaveToDatabase();

                var filePattern = (ProcessTaskArgument)args.Single(a => a.Name.Equals("FilePattern"));
                filePattern.Value = "*.csv";
                filePattern.SaveToDatabase();

                var separator = (ProcessTaskArgument)args.Single(a => a.Name.Equals("Separator"));
                separator.Value = ",";
                separator.SaveToDatabase();
                
                var results = new ToMemoryCheckNotifier();
                _checker.Check(results);

                foreach (var msg in results.Messages)
                {
                    Console.WriteLine("(" + msg.Result + ")" + msg.Message);

                    if (msg.Ex != null)
                        Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(msg.Ex));
                }

                Assert.AreEqual( CheckResult.Success,results.GetWorst());
            }
            finally
            {
                //delete everything for real
                projDir.RootPath.Delete(true);
            }
        }

        [Test]
        [TestCase("bob.exe")]
        [TestCase(@"""C:\ProgramFiles\My Software With Spaces\bob.exe""")]
        [TestCase(@"""C:\ProgramFiles\My Software With Spaces\bob.exe"" arg1 arg2 -f ""c:\my folder\arg3.exe""")]
        public void ImaginaryFile(string path)
        {
            _task.ProcessTaskType = ProcessTaskType.Executable;
            _task.Path = path;
            _task.SaveToDatabase();
            var ex = Assert.Throws<Exception>(()=>_checker.Check(new ThrowImmediatelyCheckNotifier(){ThrowOnWarning=true}));
            StringAssert.Contains("bob.exe which does not exist at this time.",ex.Message);
        }

    }
}
