using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandLine
{
    class NewObjectPoolTests : UnitTests
    {
        [Test]
        public void TwoCataloguesWithSameName_NoSession()
        {
            SetupMEF();

            var cata1 = new Catalogue(Repository,"Hey");

            // When there is only one object we can pick it by name
            var picker = new CommandLineObjectPicker(new string[] { "Catalogue:Hey" }, RepositoryLocator);
            Assert.IsTrue(picker.HasArgumentOfType(0, typeof(Catalogue)));
            Assert.AreEqual(cata1, picker.Arguments.First().GetValueForParameterOfType(typeof(Catalogue)));

            // But when there are 2 objects we don't know which to pick so cannot pick a Catalogue
            new Catalogue(Repository, "Hey");
            var picker2 = new CommandLineObjectPicker(new string[] { "Catalogue:Hey" }, RepositoryLocator);
            Assert.IsFalse(picker2.HasArgumentOfType(0, typeof(Catalogue)));
        }

        [Test]
        public void TwoCataloguesWithSameName_WithSession()
        {
            SetupMEF();

            using(NewObjectPool.StartSession())
            {
                var cata1 = new Catalogue(Repository, "Hey");

                // When there is only one object we can pick it by name
                var picker = new CommandLineObjectPicker(new string[] { "Catalogue:Hey" }, RepositoryLocator);
                Assert.IsTrue(picker.HasArgumentOfType(0, typeof(Catalogue)));
                Assert.AreEqual(cata1, picker.Arguments.First().GetValueForParameterOfType(typeof(Catalogue)));

                // There are now 2 objects with the same name but because we are in a session we can pick the latest
                var cata2 = new Catalogue(Repository, "Hey");
                var picker2 = new CommandLineObjectPicker(new string[] { "Catalogue:Hey" }, RepositoryLocator);

                Assert.IsTrue(picker2.HasArgumentOfType(0, typeof(Catalogue)));
                Assert.AreEqual(cata2, picker2.Arguments.First().GetValueForParameterOfType(typeof(Catalogue)));
            }
        }
    }
}
