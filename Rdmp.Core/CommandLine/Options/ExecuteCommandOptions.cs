using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Rdmp.Core.CommandLine.Options
{
    [Verb("cmd",HelpText = "Run the named IAtomicCommand")]
    public class ExecuteCommandOptions : RDMPCommandLineOptions
    {

        [Value(0,HelpText = "The command to run.  Can be blank for interactive or wrapped in quotes e.g. pass \"--help\" for help on formatting this argument")]
        public string CommandText { get; set; }
        
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Runs the delete command prompting you what object to delete",new ExecuteCommandOptions(){Command = CommandLineActivity.run,CommandText = "Delete"});
                yield return new Example("Prompts you which command to run",new ExecuteCommandOptions(){Command = CommandLineActivity.run});
            }
        }
    }
}
