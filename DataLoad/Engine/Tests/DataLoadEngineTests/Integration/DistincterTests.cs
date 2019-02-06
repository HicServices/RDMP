// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataHelper;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job;
using FAnsi;
using LoadModules.Generic.Mutilators;
using NUnit.Framework;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class DistincterTests : DatabaseTests
    {
        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MySql)]
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
            job.Expect(p => p.RegularTablesToLoad).Return(new List<ITableInfo>(new[] { tableInfo }));
            job.Expect(p => p.Configuration).Return(new HICDatabaseConfiguration(db.Server));

            distincter.Mutilate(job);

            var rowsAfter = tbl.GetRowCount();

            Assert.AreEqual(rowsBefore/2,rowsAfter);

            db.Drop();
        }

        [Test]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MySql)]
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
            job.Expect(p => p.RegularTablesToLoad).Return(new List<ITableInfo>(new[] { tableInfo }));
            job.Expect(p => p.Configuration).Return(new HICDatabaseConfiguration(db.Server));

            distincter.Mutilate(job);

            var rowsAfter = tbl.GetRowCount();

            Assert.AreEqual(rowsBefore, rowsAfter);

            db.Drop();
        }
    }
}