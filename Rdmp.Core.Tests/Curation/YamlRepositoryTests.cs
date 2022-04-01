using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using System;
using System.IO;
using System.Linq;

namespace Rdmp.Core.Tests.Curation
{
    internal class YamlRepositoryTests
    {

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
    }
}
