// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Lists all known commands, optionally restricted to those matching pattern
/// </summary>
[Alias("lc")]
[Alias("ListCommands")]
internal class ExecuteCommandListSupportedCommands : BasicCommandExecution
{
    private readonly string _pattern;
    private readonly bool _verbose;

    public ExecuteCommandListSupportedCommands(IBasicActivateItems basicActivator,
        [DemandsInitialization(
            "Optional. A term to look for in command names.  Supports wildcards e.g. new*cata.  If not supplied then all will be shown")]
        string pattern = null,
        [DemandsInitialization(
            "Optional. Set to true to display information about the command.  If specified with pattern then pattern will also search the description")]
        bool verbose = false) : base(basicActivator)
    {
        _pattern = pattern;
        _verbose = verbose;
    }

    public override void Execute()
    {
        var commandCaller = new CommandInvoker(BasicActivator);


        var commands = commandCaller.GetSupportedCommands().ToArray();
        var names = commands.Select(c => GetCommandName(c.Name)).ToArray();
        string[] descriptions;


        if (_verbose)
        {
            var help = BasicActivator.CommentStore ?? CreateCommentStore();
            descriptions = commands.Select(c => help.GetTypeDocumentationIfExists(c)).ToArray();
        }
        else
        {
            descriptions = new string[commands.Length];
        }

        var onlyShowIndexes = new HashSet<int>();

        if (!string.IsNullOrWhiteSpace(_pattern))
        {
            var tokens = _pattern.Split('*', StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < commands.Length; i++)
            {
                if (tokens.All(t => names[i].Contains(t, StringComparison.InvariantCultureIgnoreCase)))
                    onlyShowIndexes.Add(i);
                if (tokens.All(t => descriptions[i]?.Contains(t, StringComparison.InvariantCultureIgnoreCase) ?? false))
                    onlyShowIndexes.Add(i);
            }

            // Nothing matches pattern
            if (!onlyShowIndexes.Any())
            {
                BasicActivator.GlobalErrorCheckNotifier.OnCheckPerformed(
                    new CheckEventArgs("No commands matched supplied pattern", CheckResult.Warning));
                return;
            }
        }

        var showAll = onlyShowIndexes.Count == 0;
        var outputCommandDictionary = new Dictionary<string, string>();

        for (var i = 0; i < commands.Length; i++)
            if (showAll || onlyShowIndexes.Contains(i))
            {
                var desc = string.IsNullOrWhiteSpace(descriptions[i])
                    ? ""
                    : $"{Environment.NewLine}{descriptions[i]}{Environment.NewLine}";
                outputCommandDictionary.Add(names[i], desc);
            }

        var output = string.Join(Environment.NewLine,
            outputCommandDictionary.Select(kvp => kvp.Key + kvp.Value)
                .OrderBy(s => s));

        BasicActivator.Show(output);

        base.Execute();
    }
}