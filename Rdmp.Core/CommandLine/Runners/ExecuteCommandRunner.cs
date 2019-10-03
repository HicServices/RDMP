using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rdmp.Core.CommandExecution;
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
            const string cmdPrefix = "ExecuteCommand";

            var input = new ConsoleInputManager(repositoryLocator,checkNotifier);

            var invoker = new CommandInvoker(input, repositoryLocator);
            
            var commands = invoker.GetSupportedCommands(repositoryLocator.CatalogueRepository.MEF).ToDictionary(k=>
                k.Name.StartsWith(cmdPrefix) ? k.Name.Substring(cmdPrefix.Length): k.Name,v=>v,StringComparer.CurrentCultureIgnoreCase);

            if (string.IsNullOrWhiteSpace(_options.CommandText))
            {
                string commandName = input.GetString("Command",commands.Keys.ToList());

                if (!string.IsNullOrWhiteSpace(commandName) && commands.ContainsKey(commandName))
                    invoker.ExecuteCommand(commands[commandName]);
            }

            return 0;
        }
    }
}
