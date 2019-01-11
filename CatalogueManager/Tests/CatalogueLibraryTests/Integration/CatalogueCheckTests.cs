using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using FAnsi;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class CatalogueCheckTests:DatabaseTests
    {
        [Test]
        public void CatalogueCheck_DodgyName()
        {
            var cata = new Catalogue(CatalogueRepository, "fish");
            
            //name broken
            cata.Name = @"c:\bob.txt#";
            var ex = Assert.Throws<Exception>(()=>cata.Check(new ThrowImmediatelyCheckNotifier()));
            Assert.IsTrue(ex.Message.Contains("The following invalid characters were found:'\\','.','#'"));

            cata.DeleteInDatabase();
        }

        [TestCase(DatabaseType.MicrosoftSQLServer)]
        [TestCase(DatabaseType.MySql)]
        public void CatalogueCheck_FetchData(DatabaseType databaseType)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Rows.Add("Frank");
            dt.Rows.Add("Peter");

            var database = GetCleanedServer(databaseType);
            var tbl = database.CreateTable("CatalogueCheck_CanReadText",dt);

            var cata = Import(tbl);

            //shouldn't be any errors
            var tomemory = new ToMemoryCheckNotifier();
            cata.Check(tomemory);
            Assert.AreEqual(CheckResult.Success,tomemory.GetWorst());

            //delete all the records in the table
            tbl.Truncate();
            cata.Check(tomemory);

            //now it should warn us that it is empty 
            Assert.AreEqual(CheckResult.Warning, tomemory.GetWorst());

            tbl.Drop();


            cata.Check(tomemory);

            //now it should fail checks
            Assert.AreEqual(CheckResult.Fail, tomemory.GetWorst());


        }
    }
}
