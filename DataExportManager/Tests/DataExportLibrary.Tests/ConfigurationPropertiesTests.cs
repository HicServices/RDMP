// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
