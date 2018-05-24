using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace RDMPAutomationService.Options
{
    [Verb("dle", HelpText = "Runs the Data Load Engine")]
    class DleOptions : StartupOptions
    {
        [Value(0, HelpText = "What you want to do")]
        public DLECommands Command { get; set; }
        
        [Value(1,HelpText = "The ID of the LoadMetadata you want to run")]
        public int LoadMetadata { get; set; }

    }

    public enum DLECommands
    {
        run,
        list
    }
}
