using System;
using System.Collections.Generic;
using System.Text;
using NPOI.Util;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    class TestExecuteCommandSet : CommandCliTests
    {
        [Test]
        public void Test_CatalogueDescription_Normal()
        {
            var cata = new Catalogue(Repository.CatalogueRepository, "Bob");

            GetInvoker().ExecuteCommand(typeof(ExecuteCommandSet),new CommandLineObjectPicker(new []{"Catalogue:" + cata.ID,"Description","Some long description"},RepositoryLocator));

            cata.RevertToDatabaseState();
            Assert.AreEqual("Some long description",cata.Description);

        }

        [Test]
        public void Test_CatalogueDescription_Null()
        {
            var cata = new Catalogue(Repository.CatalogueRepository, "Bob");
            cata.Description = "something cool";
            cata.SaveToDatabase();

            GetInvoker().ExecuteCommand(typeof(ExecuteCommandSet),new CommandLineObjectPicker(new []{"Catalogue:" + cata.ID,"Description","NULL"},RepositoryLocator));

            cata.RevertToDatabaseState();
            Assert.IsNull(cata.Description);

        }
    }
}
