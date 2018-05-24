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
            @"Commands: 'run' the dle, 'list' available loads",Required = true)]
        public DLECommands Command { get; set; }
        
        [Option('l',"LoadMetadata",HelpText = "The ID of the LoadMetadata you want to run")]
        public int LoadMetadata { get; set; }
        
        [Option('a',"Any")]
        public bool Any { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Run load LoadMetadata with ID 30",new DleOptions(){Command = DLECommands.run,LoadMetadata = 30});
                yield return new Example("Run the next due load (if any)", new DleOptions() { Command = DLECommands.run, Any = true });
                yield return new Example("Override for RDMP platform databases (specified in .config)", new DleOptions() { Command = DLECommands.run, LoadMetadata = 30,ServerName =@"localhost\sqlexpress",CatalogueDatabaseName = "RDMP_Catalogue",DataExportDatabaseName =  "RDMP_DataExport"});
                

                
            }
        }
    }

    public enum DLECommands
    {
        NONE,
        run,
        list
    }
}
