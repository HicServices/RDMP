using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;
using Tests.Common;

namespace DataLoadEngineTests.Integration.CrossDatabaseTypeTests
{
    public class CrossDatabaseInsertTests : DatabaseTests
    {
        [TestCase(DatabaseType.MicrosoftSQLServer,"Dave")]
        [TestCase(DatabaseType.MYSQLServer,"Dave")]
        [TestCase(DatabaseType.Oracle, "Dave")]
        
        [TestCase(DatabaseType.MicrosoftSQLServer, @"].;\""ffff 
[")]

        [TestCase(DatabaseType.MYSQLServer, @"].;\""ffff 
[")]

        [TestCase(DatabaseType.Oracle, @"].;\""ffff 
[")]

        [TestCase(DatabaseType.MYSQLServer, "Dave")]
        [TestCase(DatabaseType.Oracle, "Dave")]
        [TestCase(DatabaseType.MicrosoftSQLServer, 1.5)]
        [TestCase(DatabaseType.MYSQLServer, 1.5)]
        [TestCase(DatabaseType.Oracle, 1.5)]
        public void CreateTableAndInsertAValue_ColumnOverload(DatabaseType type, object value)
        {
            var db = GetCleanedServer(type);
            var tbl = db.CreateTable("InsertTable",
                new []
                {
                    new DatabaseColumnRequest("Name",new DatabaseTypeRequest(value.GetType(),100,new DecimalSize(5,5)))
                });
            
            var nameCol = tbl.DiscoverColumn("Name");

            tbl.Insert(new Dictionary<DiscoveredColumn, object>()
            {
                {nameCol,value}
            });

            var result = tbl.GetDataTable();
            Assert.AreEqual(1,result.Rows.Count);
            Assert.AreEqual(value,result.Rows[0][0]);

            tbl.Drop();
        }

        [TestCase(DatabaseType.MicrosoftSQLServer, 1.5)]
        [TestCase(DatabaseType.MYSQLServer, 1.5)]
        [TestCase(DatabaseType.Oracle, 1.5)]
        public void CreateTableAndInsertAValue_StringOverload(DatabaseType type, object value)
        {
            var db = GetCleanedServer(type);
            var tbl = db.CreateTable("InsertTable",
                new[]
                {
                    new DatabaseColumnRequest("Name",new DatabaseTypeRequest(value.GetType(),100,new DecimalSize(5,5)))
                });

            tbl.Insert(new Dictionary<string, object>()
            {
                {"Name",value}
            });

            var result = tbl.GetDataTable();
            Assert.AreEqual(1, result.Rows.Count);
            Assert.AreEqual(value, result.Rows[0][0]);

            tbl.Drop();
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MYSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        public void CreateTableAndInsertAValue_ReturnsIdentity(DatabaseType type)
        {
            var db = GetCleanedServer(type);
            var tbl = db.CreateTable("InsertTable",
                new[]
                {
                    new DatabaseColumnRequest("myidentity",new DatabaseTypeRequest(typeof(int))){IsPrimaryKey = true,IsAutoIncrement = true}, 
                    new DatabaseColumnRequest("Name",new DatabaseTypeRequest(typeof(string),100))
                });

            var nameCol = tbl.DiscoverColumn("Name");

            int result = tbl.Insert(new Dictionary<DiscoveredColumn, object>()
            {
                {nameCol,"fish"}
            });

            Assert.AreEqual(1,result);


            result = tbl.Insert(new Dictionary<DiscoveredColumn, object>()
            {
                {nameCol,"fish"}
            });

            Assert.AreEqual(2, result);
        }
    }
}
