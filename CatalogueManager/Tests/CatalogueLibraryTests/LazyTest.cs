using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CatalogueLibraryTests
{
    public class LazyTest
    {
        [Test]
        public void TestLazy()
        {
            Lazy<string> s = new Lazy<string>(GetStringNull);
            
            Assert.IsFalse(s.IsValueCreated);
            Console.WriteLine(s.Value);
            Assert.IsNull(s.Value);
            Assert.IsTrue(s.IsValueCreated);
        }

        private string GetStringNull()
        {
            return null;
        }
    }
}
