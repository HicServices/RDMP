using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using NUnit.Framework;
using Rhino.Mocks;

namespace CatalogueLibraryTests.Unit
{
    public class QueryTimeColumnTests
    {
        [Test]
        public void WrapTestNoAlias()
        {
            var col = MockRepository.GenerateStub<IColumn>();
            var qtc = new QueryTimeColumn(col);

            col.SelectSQL = "fish";

            qtc.WrapIColumnSelectSql("LTRIM(RTRIM(","))");
            
            Assert.AreEqual("LTRIM(RTRIM(fish))",col.SelectSQL);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void WrapTestAlias(bool caps)
        {
            var col = MockRepository.GenerateStub<IColumn>();
            var qtc = new QueryTimeColumn(col);

            col.SelectSQL = caps ? "fish AS durdur" : "fish as durdur";

            qtc.WrapIColumnSelectSql("LTRIM(RTRIM(", "))");

            Assert.AreEqual("LTRIM(RTRIM(fish)) AS durdur", col.SelectSQL); //actually we always upper  'as' it so thats a thing
        }

    }
}
