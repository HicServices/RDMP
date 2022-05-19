using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using System;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    public class TestCommandsAreSupported : UnitTests
    {
        [TestCase(typeof(ExecuteCommandCreateNewCatalogueByImportingExistingDataTable))]
        public void TestIsSupported(Type t)
        {
            var activator = GetActivator();
            var invoker = new CommandInvoker(activator);

            Assert.IsTrue(invoker.IsSupported(t), $"Type {t} was not supported by CommandInvoker");
        }
    }
}
