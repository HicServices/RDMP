using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataHelper;
using DataLoadEngine.Job;
using LoadModules.Generic.Mutilators;
using NUnit.Framework;
using ReusableLibraryCode;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class DistincterTests : DatabaseTests
    {
        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void TestDistincter_Duplicates(DatabaseType type)
        {
            var db = GetCleanedServer(type, "TestCoalescer",true);

            int batchCount = 1000;

            DataTable dt = new DataTable("TestCoalescer_RampantNullness");
            dt.Columns.Add("pk");
            dt.Columns.Add("f1");
            dt.Columns.Add("f2");
            dt.Columns.Add("f3");
            dt.Columns.Add("f4");

            Random r = new Random(123);

            for (int i = 0; i < batchCount; i++)
            {
                int randInt = r.Next(int.MaxValue);
                
                dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, randInt });
                dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, randInt });
            }

            var tbl = db.CreateTable(dt.TableName, dt);

            var importer = new TableInfoImporter(CatalogueRepository, tbl);
            TableInfo tableInfo;
            ColumnInfo[] colInfos;
            importer.DoImport(out tableInfo, out colInfos);

            //lie about what hte primary key is because this component is designed to run in the RAW environment and we are simulating a LIVE TableInfo (correctly)
            var pk = colInfos.Single(c => c.GetRuntimeName().Equals("pk"));
            pk.IsPrimaryKey = true;
            pk.SaveToDatabase();

            var rowsBefore = tbl.GetRowCount();

            var distincter = new Distincter();
            distincter.TableRegexPattern = new Regex(".*");
            distincter.Initialize(db, LoadStage.AdjustRaw);

            var job = MockRepository.GenerateMock<IDataLoadJob>();
            job.Expect(p => p.RegularTablesToLoad).Return(new List<TableInfo>(new[] { tableInfo }));

            distincter.Mutilate(job);

            var rowsAfter = tbl.GetRowCount();

            Assert.AreEqual(rowsBefore/2,rowsAfter);

            db.ForceDrop();
        }

        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void TestDistincter_NoDuplicates(DatabaseType type)
        {
            var db = GetCleanedServer(type, "TestCoalescer",true);

            int batchCount = 1000;

            DataTable dt = new DataTable("TestCoalescer_RampantNullness");
            dt.Columns.Add("pk");
            dt.Columns.Add("f1");
            dt.Columns.Add("f2");
            dt.Columns.Add("f3");
            dt.Columns.Add("f4");

            Random r = new Random(123);

            for (int i = 0; i < batchCount; i++)
            {
                int randInt = r.Next(int.MaxValue);

                dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, randInt });
                dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, randInt+1 });
            }

            var tbl = db.CreateTable(dt.TableName, dt);

            var importer = new TableInfoImporter(CatalogueRepository, tbl);
            TableInfo tableInfo;
            ColumnInfo[] colInfos;
            importer.DoImport(out tableInfo, out colInfos);

            //lie about what hte primary key is because this component is designed to run in the RAW environment and we are simulating a LIVE TableInfo (correctly)
            var pk = colInfos.Single(c => c.GetRuntimeName().Equals("pk"));
            pk.IsPrimaryKey = true;
            pk.SaveToDatabase();

            var rowsBefore = tbl.GetRowCount();

            var distincter = new Distincter();
            distincter.TableRegexPattern = new Regex(".*");
            distincter.Initialize(db, LoadStage.AdjustRaw);

            var job = MockRepository.GenerateMock<IDataLoadJob>();
            job.Expect(p => p.RegularTablesToLoad).Return(new List<TableInfo>(new[] { tableInfo }));

            distincter.Mutilate(job);

            var rowsAfter = tbl.GetRowCount();

            Assert.AreEqual(rowsBefore, rowsAfter);

            db.ForceDrop();
        }
    }
}