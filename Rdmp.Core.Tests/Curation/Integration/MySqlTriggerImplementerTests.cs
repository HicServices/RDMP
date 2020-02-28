using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.DataLoad.Triggers.Implementations;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration
{
    public class MySqlTriggerImplementerTests
    {
        [TestCase("4.0",true)]
        [TestCase("5.1",true)]
        [TestCase("8.5",false)]
        [TestCase("5.5.64-MariaDB",true)]
        [TestCase("10.5.64-MariaDB",false)]
        public void TestOldNew(string versionString, bool expectToUseOldMethod)
        {
            Assert.AreEqual(expectToUseOldMethod,MySqlTriggerImplementer.UseOldDateTimeDefaultMethod(versionString));
        }
    }
}