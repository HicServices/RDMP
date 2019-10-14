using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using TB.ComponentModel;

namespace Rdmp.Core.CommandLine.Runners
{
    class ExecuteCommandRunner:IRunner
    {
        private readonly ExecuteCommandOptions _options;

        public ExecuteCommandRunner(ExecuteCommandOptions options)
        {
            _options = options;
        }
        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,
            ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            var input = new ConsoleInputManager(repositoryLocator,checkNotifier);

            var invoker = new CommandInvoker(input);
            
            var commands = invoker.GetSupportedCommands().ToDictionary(
                k=>BasicCommandExecution.GetCommandName(k.Name),
                v=>v,StringComparer.CurrentCultureIgnoreCase);

            CommandLineObjectPicker picker = 
                _options.CommandArgs.Any() ?
                picker = new CommandLineObjectPicker(_options.CommandArgs, repositoryLocator) :
                null;

            var command = string.IsNullOrWhiteSpace(_options.CommandName) ?
                input.GetString("Command", commands.Keys.ToList()) :
                _options.CommandName;
            
            if (!string.IsNullOrWhiteSpace(command) && commands.ContainsKey(command))
                invoker.ExecuteCommand(commands[command],picker);
            else
                Console.WriteLine($"Unknown Command '{command}', use {BasicCommandExecution.GetCommandName<ExecuteCommandListSupportedCommands>()} to see available commands" );
            
            return 0;
        }
    }
}
