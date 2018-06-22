using System;
using System.Data;
using System.Linq;
using CatalogueLibrary.Data;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace Tests.Common
{
    public class SimpleExampleTests : DatabaseTests
    {
        [Test]
        public void Test1()
        {
            var cata = new Catalogue(CatalogueRepository, "My Test Cata");
            Assert.IsTrue(cata.Exists());
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void Test2(DatabaseType type)
        {
            var database = GetCleanedServer(type);

            Assert.IsTrue(database.Exists());
            Assert.IsEmpty(database.DiscoverTables(true));
            Assert.IsNotNull(database.GetRuntimeName());
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.Oracle)]
        [TestCase(DatabaseType.MYSQLServer)]
        public void TestReadDataLowPrivileges(DatabaseType type)
        {
            var database = GetCleanedServer(type);

            //create a table on the server
            var dt = new DataTable();
            dt.Columns.Add("MyCol");
            dt.Rows.Add("Hi");
            dt.PrimaryKey = new[] {dt.Columns[0]};

            var tbl = database.CreateTable("MyTable", dt);

            //at this point we are reading it with the credentials setup by GetCleanedServer
            Assert.AreEqual(1, tbl.GetRowCount());
            Assert.AreEqual(1, tbl.DiscoverColumns().Count());
            Assert.IsTrue(tbl.DiscoverColumn("MyCol").IsPrimaryKey);

            //create a reference to the table in RMDP
            TableInfo tableInfo;
            ColumnInfo[] columnInfos;
            Import(tbl, out tableInfo, out columnInfos);

            //setup credentials for the table in RDMP (this will be Inconclusive if you have not enabled it in TestDatabases.txt
            SetupLowPrivilegeUserRightsFor(tableInfo,true,false);

            //request access to the database using DataLoad context
            var newDatabase = DataAccessPortal.GetInstance().ExpectDatabase(tableInfo, DataAccessContext.DataLoad);

            //get new reference to the table
            var newTbl = newDatabase.ExpectTable(tableInfo.GetRuntimeName());

            //the credentials should be different
            Assert.AreNotEqual(tbl.Database.Server.ExplicitUsernameIfAny, newTbl.Database.Server.ExplicitUsernameIfAny);
            
            //try re-reading the data 
            Assert.AreEqual(1, newTbl.GetRowCount());
            Assert.AreEqual(1, newTbl.DiscoverColumns().Count());
            Assert.IsTrue(newTbl.DiscoverColumn("MyCol").IsPrimaryKey);

            //low priority user shouldn't be able to drop tables
            Assert.That(newTbl.Drop,Throws.Exception);

            //normal testing user should be able to
            tbl.Drop();
        }
    }
}