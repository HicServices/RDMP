using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Cache;
using NUnit.Framework;

namespace CatalogueLibraryTests.Unit
{
    public class CacheLagPeriodUnitTests
    {
        [Test]
        public void TestOperator()
        {
            Assert.IsTrue(new TimeSpan(32, 0, 0, 0) > new CacheLagPeriod("1m"));
            Assert.IsTrue(new TimeSpan(24, 0, 0, 0) < new CacheLagPeriod("1m"));

            Assert.IsTrue(new TimeSpan(3, 0, 0, 0) > new CacheLagPeriod("2d"));
            Assert.IsFalse(new TimeSpan(3, 0, 0, 0) > new CacheLagPeriod("3d"));
            Assert.IsFalse(new TimeSpan(2, 0, 0, 0) < new CacheLagPeriod("2d"));
            Assert.IsTrue(new TimeSpan(1, 0, 0, 0) < new CacheLagPeriod("2d"));

            Assert.IsFalse(new TimeSpan(2, 0, 0, 1) < new CacheLagPeriod("2d"));
            Assert.IsFalse(new TimeSpan(2, 0, 0, 0) < new CacheLagPeriod("2d"));
            Assert.IsTrue(new TimeSpan(2, 0, 0, 1) > new CacheLagPeriod("2d"));
        }
    }
}
