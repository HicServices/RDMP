using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace RDMPAutomationService.Options
{
    /// <summary>
    /// Command line options for the Data Load Engine
    /// </summary>
    [Verb("dle", HelpText = "Runs the Data Load Engine")]
    public class DleOptions : RDMPCommandLineOptions
    {
        [Option('l',"LoadMetadata",HelpText = "The ID of the LoadMetadata you want to run", Required = false, Default = 0)]
        public int LoadMetadata { get; set; }

        [Option('p', "LoadProgress", HelpText = "If your LoadMetadata has multiple LoadProgresses, you can run only one of them by specifying the ID of the LoadProgress to run here", Required = false, Default = 0)]
        public int LoadProgress { get; set; }

        [Option('i', "Iterative", HelpText = "If the LoadMetadata has LoadProgress(es) then they will be run until available data is exhausted (if false then only one batch will be loaded e.g. 5 days)",Required = false,Default = false)]
        public bool Iterative { get; set; }

        [Option('d',"DaysToLoad", HelpText = "Only applies if using a LoadProgress, overrides how much is loaded at once")]
        public int? DaysToLoad { get; set; }

        [Option(HelpText = "Do not copy files from ForLoading into the file Archive")]
        public bool DoNotArchiveData { get; set; }

        [Option(HelpText = "Abort the data load after populating the RAW environment only")]
        public bool StopAfterRAW { get; set; }

        [Option(HelpText = "Abort the data load after populating the STAGING environment (no data will be merged with LIVE)")]
        public bool StopAfterSTAGING { get; set; }

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
