using System;
using FAnsi.Discovery;
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

            invoker.ExecuteCommand(typeof(GenericTestCommand<DatabaseEntity>),GetPicker("Catalogue:*"));
            invoker.ExecuteCommand(typeof(GenericTestCommand<Type>),GetPicker("Pipeline"));
            invoker.ExecuteCommand(typeof(GenericTestCommand<DiscoveredDatabase,bool>),
                GetPicker(
                    "DatabaseType:MicrosoftSqlServer:Name:imaging:Server=localhost\\sqlexpress;Database=master;Trusted_Connection=True;",
                    "true"));
            
        }

        private CommandLineObjectPicker GetPicker(params string[] args)
        {
            return new CommandLineObjectPicker(args, RepositoryLocator);
        }

        private class GenericTestCommand<T> : BasicCommandExecution
        {
            private readonly T _arg;
            
            public GenericTestCommand(T a)
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

        private class GenericTestCommand<T1,T2> : BasicCommandExecution
        {
            private readonly T1 _a;
            private readonly T2 _b;
            
            public GenericTestCommand(T1 a, T2 b)
            {
                _a = a;
                _b = b;
            }

            public override void Execute()
            {
                base.Execute();
                Console.Write("_a was " + _a);
                Console.Write("_b was " + _b);
                Assert.IsNotNull(_a);
                Assert.IsNotNull(_b);
            }
        }
    }
}