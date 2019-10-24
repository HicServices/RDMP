using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandLine.Runners
{
    class ExecuteCommandRunner:IRunner
    {
        private readonly ExecuteCommandOptions _options;
        private ConsoleInputManager _input;
        private CommandInvoker _invoker;
        private Dictionary<string, Type> _commands;
        private CommandLineObjectPicker _picker;
        private IDataLoadEventListener _listener;

        public ExecuteCommandRunner(ExecuteCommandOptions options)
        {
            _options = options;
        }
        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,
            ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            _input = new ConsoleInputManager(repositoryLocator,checkNotifier);
            _listener = listener;
            _invoker = new CommandInvoker(_input);
            
            _commands = _invoker.GetSupportedCommands().ToDictionary(
                k=>BasicCommandExecution.GetCommandName(k.Name),
                v=>v,StringComparer.CurrentCultureIgnoreCase);

            _picker = 
                _options.CommandArgs != null && _options.CommandArgs.Any() ?
                 new CommandLineObjectPicker(_options.CommandArgs, repositoryLocator) :
                null;
            
            if (string.IsNullOrWhiteSpace(_options.CommandName))
                RunCommandExecutionLoop();
            else
                RunCommand(_options.CommandName);
            
            return 0;
        }

        private void RunCommand(string command)
        {
            if(_commands.ContainsKey(command))
                _invoker.ExecuteCommand(_commands[command],_picker);
            else
                _listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error,$"Unknown Command '{command}', use {BasicCommandExecution.GetCommandName<ExecuteCommandListSupportedCommands>()} to see available commands" ));
        }

        private void RunCommandExecutionLoop()
        {
            //when running a command loop don't use command line arguments (shouldn't really be possible anyway)
            _picker = null;

            while (true)
            {
                Console.WriteLine("Enter Command (or 'exit')");
                var command = _input.GetString("Command", _commands.Keys.ToList());

                if (string.Equals(command, "exit", StringComparison.CurrentCultureIgnoreCase))
                    break;

                RunCommand(command);
            }
        }
    }
}
