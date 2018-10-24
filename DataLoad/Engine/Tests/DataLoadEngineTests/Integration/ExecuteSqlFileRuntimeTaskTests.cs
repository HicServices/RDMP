using System.Collections.Generic;
using System.Data;
using System.IO;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;
using DataLoadEngineTests.Integration.Mocks;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    class ExecuteSqlFileRuntimeTaskTests:DatabaseTests
    {
        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void ExecuteSqlFileRuntimeTask_BasicScript(DatabaseType dbType)
        {
            var dt = new DataTable();
            dt.Columns.Add("Lawl");
            dt.Rows.Add(new object []{2});

            var db = GetCleanedServer(dbType,true);

            var tbl = db.CreateTable("Fish",dt);
            
            FileInfo f = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"Bob.sql"));

            File.WriteAllText(f.FullName,@"UPDATE Fish Set Lawl = 1");

            var pt = MockRepository.GenerateMock<IProcessTask>();
            pt.Stub(x => x.Path).Return(f.FullName);

            var dir = HICProjectDirectory.CreateDirectoryStructure(new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"ExecuteSqlFileRuntimeTaskTests")), true);

            var task = new ExecuteSqlFileRuntimeTask(pt, new RuntimeArgumentCollection(new IArgument[0], new StageArgs(LoadStage.AdjustRaw, db, dir)));

            task.Check(new ThrowImmediatelyCheckNotifier());

            IDataLoadJob job = MockRepository.GenerateMock<IDataLoadJob>();

            task.Run(job, new GracefulCancellationToken());

            Assert.AreEqual(1,tbl.GetDataTable().Rows[0][0]);

            tbl.Drop();
        }

        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void ExecuteSqlFileRuntimeTask_InvalidID(DatabaseType dbType)
        {
            var dt = new DataTable();
            dt.Columns.Add("Lawl");
            dt.Rows.Add(new object[] { 2 });

            var db = GetCleanedServer(dbType, true);

            var tbl = db.CreateTable("Fish", dt);

            TableInfo ti;
            ColumnInfo[] cols;
            Import(tbl,out ti,out cols);

            FileInfo f = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Bob.sql"));
            
            File.WriteAllText(f.FullName, @"UPDATE {T:0} Set {C:0} = 1");

            var pt = MockRepository.GenerateMock<IProcessTask>();
            pt.Stub(x => x.Path).Return(f.FullName);

            var dir = HICProjectDirectory.CreateDirectoryStructure(new DirectoryInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory,"ExecuteSqlFileRuntimeTaskTests")), true);

            var task = new ExecuteSqlFileRuntimeTask(pt, new RuntimeArgumentCollection(new IArgument[0], new StageArgs(LoadStage.AdjustRaw, db, dir)));

            task.Check(new ThrowImmediatelyCheckNotifier());

            IDataLoadJob job = MockRepository.GenerateMock<IDataLoadJob>();
            job.Stub(x => x.RegularTablesToLoad).Return(new List<TableInfo> {ti});
            job.Stub(x => x.LookupTablesToLoad).Return(new List<TableInfo>());

            HICDatabaseConfiguration configuration = new HICDatabaseConfiguration(db.Server);
            job.Stub(x => x.Configuration).Return(configuration);
            
            var ex = Assert.Throws<ExecuteSqlFileRuntimeTaskException>(()=>task.Run(job, new GracefulCancellationToken()));
            StringAssert.Contains("Failed to find a TableInfo in the load with ID 0",ex.Message);
            StringAssert.Contains("Bob.sql",ex.Message);
        }

        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void ExecuteSqlFileRuntimeTask_ValidID_CustomNamer(DatabaseType dbType)
        {
            var dt = new DataTable();
            dt.Columns.Add("Lawl");
            dt.Rows.Add(new object[] { 2 });

            var db = GetCleanedServer(dbType, true);

            var tbl = db.CreateTable("Fish", dt);

            var tableName = "AAAAAAA";

            TableInfo ti;
            ColumnInfo[] cols;
            Import(tbl, out ti, out cols);
            
            FileInfo f = new FileInfo(Path.Combine(TestContext.CurrentContext.WorkDirectory, "Bob.sql"));

            File.WriteAllText(f.FullName, @"UPDATE {T:"+ti.ID+ "} Set {C:"+cols[0].ID+ "} = 1");

            tbl.Rename(tableName);

            //we renamed the table to simulate RAW, confirm TableInfo doesn't think it exists
            Assert.IsFalse(ti.Discover(DataAccessContext.InternalDataProcessing).Exists());

            var pt = MockRepository.GenerateMock<IProcessTask>();
            pt.Stub(x => x.Path).Return(f.FullName);

            var dir = HICProjectDirectory.CreateDirectoryStructure(new DirectoryInfo("ExecuteSqlFileRuntimeTaskTests"), true);

            var task = new ExecuteSqlFileRuntimeTask(pt, new RuntimeArgumentCollection(new IArgument[0], new StageArgs(LoadStage.AdjustRaw, db, dir)));

            task.Check(new ThrowImmediatelyCheckNotifier());

            IDataLoadJob job = MockRepository.GenerateMock<IDataLoadJob>();
            job.Stub(x => x.RegularTablesToLoad).Return(new List<TableInfo> { ti });
            job.Stub(x => x.LookupTablesToLoad).Return(new List<TableInfo>());

            //create a namer that tells the user 
            var namer = RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(db, tableName);
            
            HICDatabaseConfiguration configuration = new HICDatabaseConfiguration(db.Server,namer);
            job.Stub(x => x.Configuration).Return(configuration);

            task.Run(job, new GracefulCancellationToken());

            Assert.AreEqual(1, tbl.GetDataTable().Rows[0][0]);

            tbl.Drop();
        }
    }
}
