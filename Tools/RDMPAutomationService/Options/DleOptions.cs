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
    public class DleOptions : RDMPCommandLineOptions
    {
        [Option('l',"LoadMetadata",HelpText = "The ID of the LoadMetadata you want to run", Required = false, Default = 0)]
        public int LoadMetadata { get; set; }

        [Option('p', "LoadProgress", HelpText = "If your LoadMetadata has multiple LoadProgresses, you can run only one of them by specifying the ID of the LoadProgress to run here", Required = false, Default = 0)]
        public int LoadProgress { get; set; }

        [Option('i', "Iterative", HelpText = "If the LoadMetadata has LoadProgress(es) then they will be run until available data is exhausted (if false then only one batch will be loaded e.g. 5 days)",Required = false,Default = false)]
        public bool Iterative { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Run load LoadMetadata with ID 30",new DleOptions(){Command = CommandLineActivity.run,LoadMetadata = 30});
                yield return new Example("Override for RDMP platform databases (specified in .config)", new DleOptions() { Command = CommandLineActivity.run, LoadMetadata = 30, ServerName = @"localhost\sqlexpress", CatalogueDatabaseName = "RDMP_Catalogue", DataExportDatabaseName = "RDMP_DataExport" });
            }
        }
    }
}
