using CatalogueLibrary.DataHelper;
using NUnit.Framework;

namespace CatalogueLibraryTests.Unit
{
    public class SqlSyntaxHelperTests
    {
        [Test]
        public void GetNullSubstituteTests()
        {
            Assert.AreEqual("-999",RDMPQuerySyntaxHelper.GetNullSubstituteForComparisonsWithDataType("decimal(3)", true));
            Assert.AreEqual("-9999999999", RDMPQuerySyntaxHelper.GetNullSubstituteForComparisonsWithDataType("decimal(10)", true));
            Assert.AreEqual("-99.9", RDMPQuerySyntaxHelper.GetNullSubstituteForComparisonsWithDataType("decimal(3,1)", true));
            Assert.AreEqual("-.9999", RDMPQuerySyntaxHelper.GetNullSubstituteForComparisonsWithDataType("decimal(4,4)", true));


            Assert.AreEqual("999", RDMPQuerySyntaxHelper.GetNullSubstituteForComparisonsWithDataType("decimal(3)", false));
            Assert.AreEqual("9999999999", RDMPQuerySyntaxHelper.GetNullSubstituteForComparisonsWithDataType("decimal(10)", false));
            Assert.AreEqual("99.9", RDMPQuerySyntaxHelper.GetNullSubstituteForComparisonsWithDataType("decimal(3,1)", false));
            Assert.AreEqual(".9999", RDMPQuerySyntaxHelper.GetNullSubstituteForComparisonsWithDataType("decimal(4,4)", false));

        }
    }
}
