using System;
using System.Collections.Generic;
using System.Data;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using FAnsi;
using HIC.Logging;
using LoadModules.Generic.Attachers;
using NUnit.Framework;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class RemoteDatabaseAttacherTests:DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.AllRawColumns)]
        [TestCase(DatabaseType.MySql, Scenario.AllRawColumns)]
        [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.AllColumns)]
        [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.MissingPreLoadDiscardedColumn)]
        [TestCase(DatabaseType.MicrosoftSQLServer, Scenario.MissingPreLoadDiscardedColumnButSelectStar)]
        public void TestRemoteDatabaseAttach(DatabaseType dbType, Scenario scenario)
        {
            var db = GetCleanedServer(dbType,true);

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
            job.Stub(p => p.RegularTablesToLoad).Return(new List<ITableInfo> {ti});
            job.Stub(p => p.LookupTablesToLoad).Return(new List<ITableInfo>());
            
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
                    var ex = Assert.Throws<Exception>(() => attacher.Attach(job, new GracefulCancellationToken()));

                    Assert.AreEqual("Invalid column name 'MyMissingCol'.", (ex.InnerException.InnerException).InnerException.Message);
                    return;
                case Scenario.MissingPreLoadDiscardedColumnButSelectStar:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("scenario");
            }
            attacher.Attach(job, new GracefulCancellationToken());

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
            /// <summary>
            /// Tests the ability of the DLE to load RAW columns from a remote database by identifying tables matching
            /// by name and fetching all columns which are expected to be in RAW.
            /// </summary>
            AllRawColumns,

            /// <summary>
            /// Tests the ability of the DLE to load RAW columns from a remote database by identifying tables matching
            /// by name and fetching all columns using SELECT *.
            /// </summary>
            AllColumns,

            /// <summary>
            /// Tests the behaviour of the system when there is a RAW only column which does not appear in the remote
            /// database when using the <see cref="RemoteDatabaseAttacher.LoadRawColumnsOnly"/> option.
            /// </summary>
            MissingPreLoadDiscardedColumn,

            /// <summary>
            /// Tests the behaviour of the system when there is a RAW only column which does not appear in the remote
            /// database but the mode fetch mode is SELECT *
            /// </summary>
            MissingPreLoadDiscardedColumnButSelectStar
        }
    }
}
