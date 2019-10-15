using System;
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
    public class CommandInvokerTests : UnitTests
    {
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            SetupMEF();
        }

        [Test]
        [Timeout(5000)]
        public void Test_ListSupportedCommands_NoPicker()
        {
            var mgr = new ConsoleInputManager(RepositoryLocator,new ThrowImmediatelyCheckNotifier());
            var invoker = new CommandInvoker(mgr);
            
            invoker.ExecuteCommand(typeof(ExecuteCommandListSupportedCommands),null);
        }

        [Test]
        [Timeout(5000)]
        public void Test_Delete_WithPicker()
        {
            var mgr = new ConsoleInputManager(RepositoryLocator,new ThrowImmediatelyCheckNotifier());
            var invoker = new CommandInvoker(mgr);

            WhenIHaveA<Catalogue>();

            var picker = new CommandLineObjectPicker(new[] {"Catalogue:*"}, RepositoryLocator);
            invoker.ExecuteCommand(typeof(ExecuteCommandDelete),picker);
        }

        
        [Test]
        [Timeout(5000)]
        public void Test_Generic_WithPicker()
        {
            var mgr = new ConsoleInputManager(RepositoryLocator,new ThrowImmediatelyCheckNotifier());
            var invoker = new CommandInvoker(mgr);

            WhenIHaveA<Catalogue>();

            invoker.ExecuteCommand(typeof(GenericCommand<DatabaseEntity>),GetPicker("Catalogue:*"));
            invoker.ExecuteCommand(typeof(GenericCommand<Type>),GetPicker("Pipeline"));
            
        }

        private CommandLineObjectPicker GetPicker(params string[] args)
        {
            return new CommandLineObjectPicker(args, RepositoryLocator);
        }

        private class GenericCommand<T> : BasicCommandExecution
        {
            private readonly T _arg;
            
            public GenericCommand(T a)
            {
                _arg = a;
            }

            public override void Execute()
            {
                base.Execute();
                Console.Write("Arg was " + _arg);
                Assert.IsNotNull(_arg);
            }
        }
    }
}