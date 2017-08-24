using CatalogueLibrary.Data;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Unit
{
    public class SimpleColumnInfoTests : DatabaseTests
    {
        [Test]
        [TestCase("varchar(5)",5)]
        [TestCase("varchar()", null)]
        [TestCase("int", null)]
        [TestCase("datetime2", null)]
        [TestCase("nchar(100)", 100)]
        [TestCase("char(11)", 11)]
        public void GetColumnLength(string type, int? expectedLength)
        {
            var ti = new TableInfo(CatalogueRepository, "Foo");
            var c = new ColumnInfo(CatalogueRepository, "", type, ti);

            Assert.AreEqual(c.GetColumnLengthIfAny(),expectedLength);

            c.DeleteInDatabase();
            ti.DeleteInDatabase();
        }
    }
}
