// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Spectre.Console;

namespace Rdmp.Core.CommandLine.Runners;

internal class ExecuteCommandRunner : IRunner
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
        ICheckNotifier checkNotifier, GracefulCancellationToken token, int? dataLoadId = null)
    {
        _input = new ConsoleInputManager(repositoryLocator, checkNotifier)
        {
            // if there is a single command we are running then disable user input
            // but allow it if the input is ./rdmp cmd (i.e. run in a loop prompting for commands)
            // prevent user input if we are running a script file
            DisallowInput = !string.IsNullOrWhiteSpace(_options.CommandName) ||
                            !string.IsNullOrWhiteSpace(_options.File)
        };


        var log = LogManager.GetCurrentClassLogger();

        _listener = listener;
        _invoker = new CommandInvoker(_input);
        _invoker.CommandImpossible += (s, c) => log.Error($"Command Impossible:{c.Command.ReasonCommandImpossible}");
        _invoker.CommandCompleted += (s, c) => log.Info("Command Completed");

        var commandTypes = _invoker.GetSupportedCommands().ToArray();
        _commands = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var type in commandTypes)
        {
            var name = BasicCommandExecution.GetCommandName(type.Name);
            if (!_commands.TryAdd(name, type))
                _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"Found duplicate commands both called '{name}'.  They were '{type.FullName}' and '{_commands[name].FullName}'"));
        }

        // add Aliases (commands that can be invoked with an alternate shorthand)
        foreach (var type in commandTypes)
            foreach (var alias in type.GetCustomAttributes(false).OfType<AliasAttribute>())
                if (!_commands.TryAdd(alias.Name, type))
                    _listener.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Warning,
                            $"Bad command alias '{alias.Name}', it is already in use by '{_commands[alias.Name].FullName}' so cannot be used for '{type.FullName}'"));


        _picker =
            _options.CommandArgs != null && _options.CommandArgs.Any()
                ? new CommandLineObjectPicker(_options.CommandArgs, _input)
                : null;

        if (!string.IsNullOrWhiteSpace(_options.File) && _options.Script == null)
            throw new Exception(
                "Command line option File was provided but Script property was null.  The host API failed to deserialize the file or correctly use the ExecuteCommandOptions class");

        if (_options.Script != null)
            RunScript(_options.Script, repositoryLocator);
        else if (string.IsNullOrWhiteSpace(_options.CommandName))
            RunCommandExecutionLoop(repositoryLocator);
        else
            RunCommand(_options.CommandName);

        return 0;
    }

    private void RunCommand(string command)
    {
        if (_commands.TryGetValue(command, out var runner))
        {
            _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Trace, $"Running Command '{runner.Name}'"));
            _invoker.ExecuteCommand(runner, _picker);
        }
        else
        {
            var suggestions =
                _commands.Keys.Where(c => c.Contains(command, StringComparison.CurrentCultureIgnoreCase)).ToArray();

            var msg = new StringBuilder(
                $"Unknown or Unsupported Command '{command}', use {BasicCommandExecution.GetCommandName<ExecuteCommandListSupportedCommands>()} to see available commands");

            if (suggestions.Any())
                msg.AppendLine(
                    $"Similar commands include:{Environment.NewLine}{string.Join(Environment.NewLine, suggestions)}");

            _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, msg.ToString()));
        }
    }

    /// <summary>
    /// Runs a main loop in which the user types many commands one after the other
    /// </summary>
    /// <param name="repositoryLocator"></param>
    private void RunCommandExecutionLoop(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        //when running a command loop don't use command line arguments (shouldn't really be possible anyway)
        _picker = null;

        while (true)
        {
            var command = _input.GetString(new DialogArgs { WindowTitle = "Enter Command (or Ctrl+C)" },
                _commands.Keys.ToList());
            try
            {
                command = GetCommandAndPickerFromLine(command, out _picker, repositoryLocator);

                if (string.Equals(command, "exit", StringComparison.CurrentCultureIgnoreCase))
                    break;

                RunCommand(command);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            }


            _picker = null;
        }
    }

    /// <summary>
    /// Takes a single command line e.g. "list Catalogue" and spits it into a command "list" (returned) and the arguments list (as <paramref name="picker"/>)
    /// </summary>
    /// <param name="command"></param>
    /// <param name="picker"></param>
    /// <param name="repositoryLocator"></param>
    /// <returns></returns>
    private string GetCommandAndPickerFromLine(string command, out CommandLineObjectPicker picker,
        IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        if (command.Contains(' '))
        {
            picker = new CommandLineObjectPicker(SplitCommandLine(command).Skip(1).ToArray(), _input);
            return command[..command.IndexOf(' ')];
        }

        picker = null;
        return command;
    }

    /// <summary>
    /// Runs all commands in the provided script
    /// </summary>
    /// <param name="script">Location of the file to run</param>
    /// <param name="repositoryLocator"></param>
    private void RunScript(RdmpScript script, IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        if ((script.Commands?.Length ?? 0) == 0)
            throw new ArgumentException("script was empty", nameof(script));

        using (script.UseScope ? NewObjectPool.StartSession() : null)
        {
            foreach (var s in script.Commands)
                try
                {
                    if (StartsWithEngineVerb(s))
                    {
                        var exitCode = RdmpCommandLineBootStrapper.HandleArgumentsWithStandardRunner(
                            SplitCommandLine(s).ToArray(), LogManager.GetCurrentClassLogger(), repositoryLocator);
                        if (exitCode != 0)
                            throw new Exception("Exit code from runner was non zero");
                    }
                    else
                    {
                        var cmd = GetCommandAndPickerFromLine(s, out _picker, repositoryLocator);
                        RunCommand(cmd);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error executing script.  Problem line was '{s}':{ex.Message}", ex);
                }
        }
    }

    private static bool StartsWithEngineVerb(string s)
    {
        var verbs = new[] { "cache", "cohort", "dle", "dqe", "extract", "release" };
        return verbs.Any(v => s.TrimStart().StartsWith(v, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Splits a command line string into a sequence of discrete arguments as you would get in Program.cs main
    /// string[] args.  Includes support for wrapping arguments in spaces and escaping etc
    /// </summary>
    /// <param name="commandLine"></param>
    /// <returns></returns>
    public static IEnumerable<string> SplitCommandLine(string commandLine)
    {
        char? inQuotes = null;

        var word = new StringBuilder();

        foreach (var c in commandLine)
        {
            //if we enter quotes and it's the first letter in the word
            if (inQuotes == null && c is '\'' or '"' && word.Length == 0)
            {
                inQuotes = c;
            }
            else if (c == inQuotes)
            {
                //if we exit quotes
                inQuotes = null;
            }
            else if (c == ' ' && inQuotes == null)
            {
                //break character outside of quotes
                var resultWord = word.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(resultWord))
                    yield return resultWord;

                word.Clear();
            }
            else
            {
                word.Append(c); //regular character
            }
        }

        var finalWord = word.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(finalWord))
            yield return finalWord;
    }
}