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

            var input = new ConsoleInputManager();

            var caller = new CommandCaller(input, repositoryLocator);

            var commands = caller.GetSupportedCommands(repositoryLocator.CatalogueRepository.MEF).ToDictionary(k=>
                k.Name.StartsWith(cmdPrefix) ? k.Name.Substring(cmdPrefix.Length): k.Name,v=>v);

            if (string.IsNullOrWhiteSpace(_options.CommandText))
            {
                input.GetString("Command",commands.Keys.ToList());
            }

            return 0;
        }
    }
}
