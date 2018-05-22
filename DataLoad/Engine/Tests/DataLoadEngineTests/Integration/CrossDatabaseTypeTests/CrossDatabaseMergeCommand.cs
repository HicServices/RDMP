using System.Data;
using DataLoadEngine.Migration;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace DataLoadEngineTests.Integration.CrossDatabaseTypeTests
{
    public class CrossDatabaseMergeCommand:DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void TestMerge(DatabaseType databaseType)
        {
            var db = GetCleanedServer(databaseType, "CrossDatabaseMergeCommand");

            var fromDt = new DataTable();
            var colName = new DataColumn("Name");
            var colAge = new DataColumn("Age");
            fromDt.Columns.Add(colName);
            fromDt.Columns.Add(colAge);
            fromDt.Columns.Add("Postcode");

            fromDt.PrimaryKey = new[]{colName,colAge};

            var from = db.CreateTable("FromTable",fromDt);

            Assert.IsTrue(from.DiscoverColumn("Name").IsPrimaryKey);
            Assert.IsTrue(from.DiscoverColumn("Age").IsPrimaryKey);
            Assert.IsFalse(from.DiscoverColumn("Postcode").IsPrimaryKey);
            
            var to = db.CreateTable("ToTable", fromDt);

            var processor = new StagingToLiveMigrationFieldProcessor();
            new MigrationColumnSet(from, to,processor);

        }
    }
}
