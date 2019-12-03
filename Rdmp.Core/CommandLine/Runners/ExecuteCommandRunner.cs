// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
                RunCommandExecutionLoop(repositoryLocator);
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

        private void RunCommandExecutionLoop(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            //when running a command loop don't use command line arguments (shouldn't really be possible anyway)
            _picker = null;

            while (true)
            {
                Console.WriteLine("Enter Command (or 'exit')");
                var command = _input.GetString("Command", _commands.Keys.ToList());

                if (command.Contains(' '))
                {
                    _picker = new CommandLineObjectPicker(SplitCommandLine(command).Skip(1).ToArray(),repositoryLocator);
                    command = command.Substring(0, command.IndexOf(' '));
                }

                if (string.Equals(command, "exit", StringComparison.CurrentCultureIgnoreCase))
                    break;

                RunCommand(command);

                _picker = null;
            }
        }

        public static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            bool inQuotes = false;

            return commandLine.Split(c =>
                {
                    if (c == '\"')
                        inQuotes = !inQuotes;

                    return !inQuotes && c == ' ';
                })
                .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
                .Where(arg => !string.IsNullOrEmpty(arg));
        }
    }
}

public static class StringExtensions
{

    public static IEnumerable<string> Split(this string str, 
        Func<char, bool> controller)
    {
        int nextPiece = 0;

        for (int c = 0; c < str.Length; c++)
        {
            if (controller(str[c]))
            {
                yield return str.Substring(nextPiece, c - nextPiece);
                nextPiece = c + 1;
            }
        }

        yield return str.Substring(nextPiece);
    }

    public static string TrimMatchingQuotes(this string input, char quote)
    {
        if ((input.Length >= 2) && 
            (input[0] == quote) && (input[input.Length - 1] == quote))
            return input.Substring(1, input.Length - 2);

        return input;
    }
}