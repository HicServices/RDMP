using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.DataHelper;
using CatalogueLibraryTests.Mocks;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job;
using LoadModules.Generic.Mutilators;
using NUnit.Framework;
using ReusableLibraryCode;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class CoalescerTests:DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer,true)]
        [TestCase(DatabaseType.MicrosoftSQLServer, false)]
        [TestCase(DatabaseType.MYSQLServer,true)]
        [TestCase(DatabaseType.MYSQLServer, false)]
        public void TestCoalescer_RampantNullness(DatabaseType type,bool useCustomNamer)
        {
            var db = GetCleanedServer(type, "TestCoalescer");

            int batchCount = 1000;

            DataTable dt = new DataTable("TestCoalescer_RampantNullness");
            dt.Columns.Add("pk");
            dt.Columns.Add("f1");
            dt.Columns.Add("f2");
            dt.Columns.Add("f3");
            dt.Columns.Add("f4");

            Random r = new Random();

            for (int i = 0; i < batchCount; i++)
            {
                int randInt = r.Next(250);
                int randCompleteness = r.Next(4);

                dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, randInt });
                dt.Rows.Add(new object[] { randInt, DBNull.Value, DBNull.Value, DBNull.Value, randInt });
                dt.Rows.Add(new object[] { randInt, DBNull.Value, DBNull.Value, randInt, DBNull.Value });
                dt.Rows.Add(new object[] { randInt, DBNull.Value, DBNull.Value, randInt, randInt });

                if (randCompleteness >=1)
                {
                    dt.Rows.Add(new object[] { randInt, DBNull.Value, randInt, DBNull.Value, DBNull.Value });
                    dt.Rows.Add(new object[] { randInt, DBNull.Value, randInt, DBNull.Value, randInt });
                    dt.Rows.Add(new object[] { randInt, DBNull.Value, randInt, randInt, DBNull.Value });
                    dt.Rows.Add(new object[] { randInt, DBNull.Value, randInt, randInt, randInt });
                }
                 
                if(randCompleteness >=2)
                {
                    dt.Rows.Add(new object[] { randInt, randInt, DBNull.Value, DBNull.Value, DBNull.Value });
                    dt.Rows.Add(new object[] { randInt, randInt, DBNull.Value, DBNull.Value, randInt });
                    dt.Rows.Add(new object[] { randInt, randInt, DBNull.Value, randInt, DBNull.Value });
                    dt.Rows.Add(new object[] { randInt, randInt, DBNull.Value, randInt, randInt });
                }


                if(randCompleteness >= 3)
                {
                    dt.Rows.Add(new object[] { randInt, randInt, randInt, DBNull.Value, DBNull.Value });
                    dt.Rows.Add(new object[] { randInt, randInt, randInt, DBNull.Value, randInt });
                    dt.Rows.Add(new object[] { randInt, randInt, randInt, randInt, DBNull.Value });
                    dt.Rows.Add(new object[] { randInt, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value });
                }
            }

            var tbl = db.CreateTable(dt.TableName, dt);

            var importer = new TableInfoImporter(CatalogueRepository, tbl);
            TableInfo tableInfo;
            ColumnInfo[] colInfos;
            importer.DoImport(out tableInfo,out colInfos);

            //lie about what hte primary key is because this component is designed to run in the RAW environment and we are simulating a LIVE TableInfo (correctly)
            var pk = colInfos.Single(c => c.GetRuntimeName().Equals("pk"));
            pk.IsPrimaryKey = true;
            pk.SaveToDatabase();

            INameDatabasesAndTablesDuringLoads namer = null;

            if (useCustomNamer)
            {
                tbl.Rename("AAAA");
                namer = RdmpMockFactory.Mock_INameDatabasesAndTablesDuringLoads(db, "AAAA");
            }
            
            HICDatabaseConfiguration configuration = new HICDatabaseConfiguration(db.Server,namer);
            
            var coalescer = new Coalescer();
            coalescer.TableRegexPattern = new Regex(".*");
            coalescer.CreateIndex = true;
            coalescer.Initialize(db,LoadStage.AdjustRaw);


            var job = new ThrowImmediatelyDataLoadJob(configuration, tableInfo);
            coalescer.Mutilate(job);

            var dt2 = tbl.GetDataTable();

            foreach (DataRow row in dt2.Rows)
            {
                Assert.AreNotEqual(DBNull.Value,row["f1"]);
                Assert.AreNotEqual(DBNull.Value, row["f2"]);
                Assert.AreNotEqual(DBNull.Value, row["f3"]);
                Assert.AreNotEqual(DBNull.Value, row["f4"]);
            }

            db.ForceDrop();
        }

    }
}
