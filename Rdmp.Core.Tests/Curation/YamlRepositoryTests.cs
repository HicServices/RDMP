// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Rdmp.Core.Tests.Curation
{
    internal class YamlRepositoryTests
    {
        [Test]
        public void BlankConstructorsForEveryone()
        {
            StringBuilder sb = new StringBuilder();

            foreach(var t in new YamlRepository(GetUniqueDirectory()).GetCompatibleTypes())
            {
                var blankConstructor = t.GetConstructor(Type.EmptyTypes);

                if (blankConstructor == null)
                    sb.AppendLine(t.Name);
            }

            if(sb.Length > 0)
            {
                Assert.Fail($"All data classes must have a blank constructor.  The following did not:{Environment.NewLine}{sb}");
            }
        }

        [Test]
        public void PersistDefaults()
        {
            var dir = GetUniqueDirectory();

            var repo1 = new YamlRepository(dir);
            var eds = new ExternalDatabaseServer(repo1,"myServer",null);
            repo1.SetDefault(PermissableDefaults.LiveLoggingServer_ID, eds);

            var repo2 = new YamlRepository(dir);
            Assert.AreEqual(eds, repo2.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID));
        }


        [Test]
        public void PersistDataExportPropertyManagerValues()
        {
            var dir = GetUniqueDirectory();

            var repo1 = new YamlRepository(dir);
            repo1.DataExportPropertyManager.SetValue(DataExportProperty.HashingAlgorithmPattern,"yarg");

            var repo2 = new YamlRepository(dir);
            Assert.AreEqual("yarg", repo2.DataExportPropertyManager.GetValue(DataExportProperty.HashingAlgorithmPattern));
        }

        [Test]
        public void TestYamlRepository_LoadObjects()
        {
            var dir = new DirectoryInfo(GetUniqueDirectoryName());
            var repo = new YamlRepository(dir);

            var c = new Catalogue(repo, "yar");

            Assert.Contains(c, repo.AllObjects.ToArray());

            // creating a new repo should load the same object back
            var repo2 = new YamlRepository(dir);
            Assert.Contains(c, repo2.AllObjects.ToArray());
        }

        [Test]
        public void TestYamlRepository_Save()
        {
            var dir = new DirectoryInfo(GetUniqueDirectoryName());
            var repo = new YamlRepository(dir);

            var c = new Catalogue(repo, "yar");
            c.Name = "ffff";
            c.SaveToDatabase();

            // creating a new repo should load the same object back
            var repo2 = new YamlRepository(dir);
            Assert.Contains(c, repo2.AllObjects.ToArray());
            Assert.AreEqual("ffff", repo2.AllObjects.OfType<Catalogue>().Single().Name);
        }

        private string GetUniqueDirectoryName()
        {
            return Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString().Replace("-", ""));
        }

        private DirectoryInfo GetUniqueDirectory()
        {
            return new DirectoryInfo(GetUniqueDirectoryName());
        }
    }
}
