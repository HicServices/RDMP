using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace RDMPAutomationService.Options
{
    [Verb("dle", HelpText = "Runs the Data Load Engine")]
    class DleOptions : StartupOptions
    {
       
        [Value(0, HelpText = 
            @"Commands: 'run' - Execute DLE for the specified load metadata,'list' - List load metadata by name/id")]
        public DLECommands Command { get; set; }
        
        [Value(1,HelpText = "The ID of the LoadMetadata you want to run")]
        public int LoadMetadata { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Run the DLE to load LoadMetadata with ID 30",new DleOptions(){Command = DLECommands.run,LoadMetadata = 30}); 
                
            }
        }
    }

    public enum DLECommands
    {
        run,
        list
    }
}
