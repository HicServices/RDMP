using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    class TestExecuteCommandNewObject : UnitTests
    {
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            SetupMEF();
        }

        private CommandInvoker GetInvoker()
        {
            var invoker = new CommandInvoker(new ConsoleInputManager(RepositoryLocator,new ThrowImmediatelyCheckNotifier())
            {
                DisallowInput = true
            });
            invoker.CommandImpossible +=(s,c)=> throw new Exception(c.Command.ReasonCommandImpossible);

            return invoker;
        }

        [Test]
        public void Test_NewObjectCommand_NoArguments()
        {
            
            var ex = Assert.Throws<Exception>(() => GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),
                new CommandLineObjectPicker(new string[0], RepositoryLocator)));

            StringAssert.StartsWith("First parameter must be a Type",ex.Message);
        }

        [Test]
        public void Test_NewObjectCommand_NonExistentTypeArgument()
        {
            var ex = Assert.Throws<Exception>(() =>  GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),
                new CommandLineObjectPicker(new[]{"Fissdlkfldfj"}, RepositoryLocator)));

            StringAssert.StartsWith("First parameter must be a Type",ex.Message);
        }

        [Test]
        public void Test_NewObjectCommand_WrongTypeArgument()
        {
            var picker = new CommandLineObjectPicker(new[] {"UnitTests"},RepositoryLocator);
            Assert.AreEqual(typeof(UnitTests),picker[0].Type);

            var ex = Assert.Throws<Exception>(() =>  GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),picker));

            StringAssert.StartsWith("Type must be derived from DatabaseEntity",ex.Message);
        }

        [Test]
        public void Test_NewObjectCommand_MissingNameArgument()
        {
            var picker = new CommandLineObjectPicker(new[] {"Catalogue"},RepositoryLocator);
            Assert.AreEqual(typeof(Catalogue),picker[0].Type);

            var ex = Assert.Throws<ArgumentException>(() =>  GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),picker));

            StringAssert.StartsWith("Value needed for parameter 'name' (of type 'System.String')",ex.Message);
        }

        [Test]
        public void Test_NewObjectCommand_Success()
        {
            var picker = new CommandLineObjectPicker(new[] {"Catalogue","lolzeeeyeahyeah"},RepositoryLocator);
            Assert.AreEqual(typeof(Catalogue),picker[0].Type);

            Assert.DoesNotThrow(() =>  GetInvoker().ExecuteCommand(typeof(ExecuteCommandNewObject),picker));
            
            Assert.Contains("lolzeeeyeahyeah",RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().Select(c=>c.Name).ToArray());
        }
    }
}
