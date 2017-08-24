using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using NUnit.Framework;

namespace CatalogueLibraryTests.Unit
{
    public class CatalogueNamingTests
    {

        [Test]
        [TestCase("###")]
        [TestCase("Bob\\bob")]
        [TestCase("Frank.txt")]
        [TestCase("<Catalogue>WTF?</Catalogue>")]
        public void StupidCatalogueNames(string name)
        {
            Assert.IsFalse(Catalogue.IsAcceptableName(name));
        }
        [Test]
        [TestCase("Hi")]
        [TestCase("MyhExchiting dAtaset")]
        [TestCase("Bobs dataset (123)")]
        [TestCase("(Break in case of emergency)")]
        [TestCase("Bob&Betty")]
        public void SensibleCatalogueNames(string name)
        {
            Assert.IsTrue(Catalogue.IsAcceptableName(name));
        }
    }
}
