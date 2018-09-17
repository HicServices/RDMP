using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;

namespace ReusableCodeTests
{
    public class ParameterNameExtractionTests
    {
        [TestCase("[bob]=@bobby")]
        [TestCase("[bob]=   @bobby")]
        [TestCase("[bob]=@bobby OR [bob2]=@bobby")]
        [TestCase("[bob]=@bobby OR [bob2]=@BObby")]
        [TestCase("@bobby='active'")]
        public void TestExtractionOfParmaetersFromSQL_FindOne(string sql)
        {
            var querySyntaxHelper = new MicrosoftQuerySyntaxHelper();
            
            Assert.AreEqual("@bobby",querySyntaxHelper.GetAllParameterNamesFromQuery(sql).SingleOrDefault());
        }

        [TestCase("[bob]='@bobby'")]
        [TestCase("[bob]='myfriend@bobby.ac.uk'")]
        [TestCase("[bob]=   ':bobby'")]
        public void TestExtractionOfParmaetersFromSQL_NoneOne(string sql)
        {
            var querySyntaxHelper = new MicrosoftQuerySyntaxHelper();

            Assert.AreEqual(0,querySyntaxHelper.GetAllParameterNamesFromQuery(sql).Count);
        }
    }
}
