using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Rdmp.Core.CommandLine.Options
{
    [Verb("cmd",HelpText = "Run the named IAtomicCommand")]
    public class ExecuteCommandOptions : RDMPCommandLineOptions
    {

        [Value(0,HelpText = "The command to run e.g. Delete.  Leave blank for interactive mode")]
        public string CommandName { get; set; }

        
        [Value(1,HelpText = "The arguments to provide for the command e.g. Catalogue:12")]
        public IEnumerable<string> CommandArgs { get; set; }
        
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Runs the delete command on Catalogue with ID 1",new ExecuteCommandOptions(){Command = CommandLineActivity.run,CommandName = "Delete", CommandArgs = new string[]{"Catalogue:1"}});
                yield return new Example("Prompts you which command to run",new ExecuteCommandOptions(){Command = CommandLineActivity.run});
            }
        }

    }
}
