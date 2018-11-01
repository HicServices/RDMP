using System.Collections.Generic;
using DataExportLibrary.Data;
using NUnit.Framework;
using Tests.Common;

namespace DataExportLibrary.Tests
{
    public class ConfigurationPropertiesTests : DatabaseTests
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CreateNewArgumentAndGetValue(bool withCaching)
        {
            var configurationProperties = new ConfigurationProperties(withCaching, DataExportRepository);
            configurationProperties.SetValue("fishes","hi");
            Assert.AreEqual(configurationProperties.GetValue("fishes"),"hi");

            //make sure delete results in 1 affected row
            Assert.AreEqual(configurationProperties.DeleteValue("fishes"),1);
        }

        [Test]
        public void GetNonExistantValue_KeyNotFound()
        {
            ConfigurationProperties properties = new ConfigurationProperties(true, DataExportRepository);
            Assert.Throws<KeyNotFoundException>(()=>properties.GetValue("asdfasljdfljsadkflkasjdflkjasdfljk"));
        }


        [Test]
        public void GetNonExistantValueUsingTry_ReturnsNull()
        {
            ConfigurationProperties properties = new ConfigurationProperties(true, DataExportRepository);
            Assert.IsNull(properties.TryGetValue("asdfasljdfljsadkflkasjdflkjasdfljk"));
        }


    }
}
