using System.Linq;
using CatalogueLibrary.Data;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Tests.Common;

namespace CatalogueLibraryTests.Unit
{
    public class SimpleColumnInfoTests : DatabaseTests
    {
        [Test]
        [TestCase("varchar(5)",5)]
        [TestCase("int", -1)]
        [TestCase("datetime2", -1)]
        [TestCase("nchar(100)", 100)]
        [TestCase("char(11)", 11)]
        [TestCase("text", int.MaxValue)]
        [TestCase("varchar(max)", int.MaxValue)]
        public void GetColumnLength(string type, int? expectedLength)
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            var t = db.CreateTable("MyTable", new[]
            {
                new DatabaseColumnRequest("MyCol", type)
            });

            ColumnInfo[] cis;
            TableInfo ti;
            Import(t, out ti, out cis);
            
            Assert.AreEqual(expectedLength,cis.Single().Discover(DataAccessContext.InternalDataProcessing).DataType.GetLengthIfString());

            ti.DeleteInDatabase();
        }
    }
}
