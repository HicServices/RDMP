using System;
using System.Collections.Generic;
using System.Data;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Job;
using HIC.Logging;
using LoadModules.Generic.Attachers;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class RemoteDatabaseAttacherTests:DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.AllRawColumns)]
        [TestCase(DatabaseType.MYSQLServer, Scenario.AllRawColumns)]
        [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.AllColumns)]
        [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.MissingPreLoadDiscardedColumn)]
        [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.MissingPreLoadDiscardedColumnButSelectStar)]
        public void TestRemoteDatabaseAttach(DatabaseType dbType,Scenario scenario)
        {
            var db = GetCleanedServer(dbType);

            DataTable dt = new DataTable();

            dt.Columns.Add("Fish");
            dt.Columns.Add("hic_Heroism");

            dt.Rows.Add("123", 11);

            var tbl = db.CreateTable("MyTable",dt);

            Assert.AreEqual(1, tbl.GetRowCount());
            
            TableInfo ti;
            ColumnInfo[] cols;
            Import(tbl, out ti, out cols);

            //Create a virtual RAW column
            if (scenario == Scenario.MissingPreLoadDiscardedColumn || scenario == Scenario.MissingPreLoadDiscardedColumnButSelectStar)
                new PreLoadDiscardedColumn(CatalogueRepository, ti, "MyMissingCol");

            var externalServer = new ExternalDatabaseServer(CatalogueRepository, "MyFictionalRemote");
            externalServer.SetProperties(db);
            
            var attacher = new RemoteDatabaseAttacher();
            attacher.Initialize(null,db);

            attacher.LoadRawColumnsOnly = scenario == Scenario.AllRawColumns || scenario == Scenario.MissingPreLoadDiscardedColumn;
            attacher.RemoteSource = externalServer;

            var job = MockRepository.GenerateMock<IDataLoadJob>();
            job.Stub(p => p.RegularTablesToLoad).Return(new List<TableInfo> {ti});
            job.Stub(p => p.LookupTablesToLoad).Return(new List<TableInfo>());
            
            var lm = new LogManager(new ServerDefaults(CatalogueRepository).GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID));
            lm.CreateNewLoggingTaskIfNotExists("amagad");
            var dli = lm.CreateDataLoadInfo("amagad", "p", "a", "", true);

            job.Stub(p => p.DataLoadInfo).Return(dli);

            switch (scenario)
            {
                case Scenario.AllRawColumns:
                    break;
                case Scenario.AllColumns:
                    break;
                case Scenario.MissingPreLoadDiscardedColumn:
                    var ex = Assert.Throws<Exception>(() => attacher.Attach(job));

                    Assert.AreEqual("Invalid column name 'MyMissingCol'.", (ex.InnerException.InnerException).InnerException.Message);
                    return;
                case Scenario.MissingPreLoadDiscardedColumnButSelectStar:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("scenario");
            }
            attacher.Attach(job);

            Assert.AreEqual(2,tbl.GetRowCount());

            dt = tbl.GetDataTable();

            VerifyRowExist(dt,123,11);

            if (scenario == Scenario.AllRawColumns)
                VerifyRowExist(dt, 123, DBNull.Value);

            attacher.LoadCompletedSoDispose(ExitCodeType.Success, new ThrowImmediatelyDataLoadEventListener());

            externalServer.DeleteInDatabase();
        }

        public enum Scenario
        {
            AllRawColumns,
            AllColumns,
            MissingPreLoadDiscardedColumn,
            MissingPreLoadDiscardedColumnButSelectStar
        }
    }
}
