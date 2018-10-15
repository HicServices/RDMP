using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Job;
using LoadModules.Generic.Mutilators;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using ReusableLibraryCode.Progress;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class TableVarcharMaxerTests : DatabaseTests
    {
        [TestCase(DatabaseType.MYSQLServer,true)]
        [TestCase(DatabaseType.MYSQLServer, false)]
        [TestCase(DatabaseType.MicrosoftSQLServer,true)]
        [TestCase(DatabaseType.MicrosoftSQLServer,false)]
        public void TestTableVarcharMaxer(DatabaseType dbType,bool allDataTypes)
        {
            var db = GetCleanedServer(dbType,true);

            var tbl = db.CreateTable("Fish",new[]
            {
                new DatabaseColumnRequest("Dave",new DatabaseTypeRequest(typeof(string),100)), 
                new DatabaseColumnRequest("Frank",new DatabaseTypeRequest(typeof(int))) 
            });

            TableInfo ti;
            ColumnInfo[] cols;
            Import(tbl, out ti, out cols);

            var maxer = new TableVarcharMaxer();
            maxer.AllDataTypes = allDataTypes;
            maxer.TableRegexPattern = new Regex(".*");
            maxer.DestinationType = db.Server.GetQuerySyntaxHelper().TypeTranslater.GetSQLDBTypeForCSharpType(new DatabaseTypeRequest(typeof(string),int.MaxValue));
            
            maxer.Initialize(db,LoadStage.AdjustRaw);
            maxer.Check(new ThrowImmediatelyCheckNotifier(){ThrowOnWarning = true});

            var job = MockRepository.GenerateMock<IDataLoadJob>();

            job.Stub(x => x.RegularTablesToLoad).Return(new List<TableInfo>(){ti});

            maxer.Mutilate(job);

            switch (dbType)
            {
                case DatabaseType.MicrosoftSQLServer:
                    Assert.AreEqual("varchar(max)",tbl.DiscoverColumn("Dave").DataType.SQLType);
                    Assert.AreEqual(allDataTypes ? "varchar(max)" : "int", tbl.DiscoverColumn("Frank").DataType.SQLType);
                    break;
                case DatabaseType.MYSQLServer:
                    Assert.AreEqual("text",tbl.DiscoverColumn("Dave").DataType.SQLType);
                    Assert.AreEqual(allDataTypes ? "text" : "int", tbl.DiscoverColumn("Frank").DataType.SQLType);
                    break;
                case DatabaseType.Oracle:
                    Assert.AreEqual("varchar(max)",tbl.DiscoverColumn("Dave").DataType.SQLType);
                Assert.AreEqual(allDataTypes ? "varchar(max)" : "int", tbl.DiscoverColumn("Frank").DataType.SQLType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dbType");
            }
        }
    }
}