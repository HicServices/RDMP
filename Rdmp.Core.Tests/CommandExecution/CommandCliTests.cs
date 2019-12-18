using System;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Interactive;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    /// <summary>
    /// Base class for all tests which test RDMP CLI command line arguments to run <see cref="BasicCommandExecution"/> derrived
    /// classes
    /// </summary>
    abstract class CommandCliTests : UnitTests
    {
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            SetupMEF();
        }

        protected CommandInvoker GetInvoker()
        {
            var invoker = new CommandInvoker(new ConsoleInputManager(RepositoryLocator,new ThrowImmediatelyCheckNotifier())
            {
                DisallowInput = true
            });
            invoker.CommandImpossible +=(s,c)=> throw new Exception(c.Command.ReasonCommandImpossible);

            return invoker;
        }
    }
}