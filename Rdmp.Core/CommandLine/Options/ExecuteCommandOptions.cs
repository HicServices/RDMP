// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using Rdmp.Core.CommandLine.Runners;

namespace Rdmp.Core.CommandLine.Options;

/// <summary>
///     Runs a given RDMP command specified by <see cref="CommandName" /> from the CLI.  See
///     <see cref="ExecuteCommandRunner" /> for implementation.
/// </summary>
[Verb("cmd", true, HelpText = "Run the specified command.  To list commands run 'rdmp cmd ListSupportedCommands'")]
public class ExecuteCommandOptions : RDMPCommandLineOptions
{
    [Value(0, HelpText = "The command to run e.g. Delete.  Leave blank for interactive mode")]
    public string CommandName { get; set; }


    [Value(1, HelpText = "The arguments to provide for the command e.g. Catalogue:12")]
    public IEnumerable<string> CommandArgs { get; set; }

    [Option('f', "file", HelpText = "Runs commands in the given yaml file")]
    public string File { get; set; }

    /// <summary>
    ///     The deserialized contents of File or null if File is not provided.  It is up to the hosting API to populate this
    ///     property
    /// </summary>
    /// <value></value>
    public RdmpScript Script { get; set; }

    [Usage]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Runs the delete command on Catalogue with ID 1",
                new ExecuteCommandOptions { CommandName = "Delete", CommandArgs = new[] { "Catalogue:1" } });
            yield return new Example("List available commands",
                new ExecuteCommandOptions { CommandName = "ListSupportedCommands" });
            yield return new Example("Runs all commands in the file",
                new ExecuteCommandOptions { File = "./myfile.yaml" });
            yield return new Example("Prompts you which command to run", new ExecuteCommandOptions());
        }
    }
}