using LoadModules.Generic.DataProvider;
using NUnit.Framework;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class WebServiceConfigurationTests : DatabaseTests
    {
        [Test]
        public void TestXmlSerialization()
        {
            var config = new WebServiceConfiguration(CatalogueRepository) {Username = "foo", Password = "bar"};
            var state = config.SaveStateToString();
            config.RestoreStateFrom(state);
        }
    }
}