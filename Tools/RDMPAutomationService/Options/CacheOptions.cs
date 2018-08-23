using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using RDMPAutomationService.Options.Abstracts;

namespace RDMPAutomationService.Options
{
    /// <summary>
    /// Command line options for the caching engine
    /// </summary>
    [Verb("cache",HelpText ="Run the Caching engine which fetches data by date from a remote endpoint in batches of a given size (independently from loading it to any relational databases)")]
    public class CacheOptions:RDMPCommandLineOptions
    {
        [Option('c', "CacheProgress", HelpText = "The ID of the CacheProgress you want to run", Required = true, Default = 0)]
        public int CacheProgress { get; set; }

        [Option('r',"RetryMode",HelpText = "True to attempt to process archival CacheFetchFailure dates instead of new (uncached) dates.")]
        public bool RetryMode { get; set; }

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Check the cache is runnable", new CacheOptions() { Command = CommandLineActivity.check, CacheProgress = 2});
                yield return new Example("Check the cache is runnable and returns error code " +
                                         "instead of success if there are warnings", 
                                         new CacheOptions() { Command = CommandLineActivity.check, CacheProgress = 2, FailOnWarnings = true});
                yield return new Example("Run cache progress overriding RDMP platform databases (specified in .config)", new CacheOptions() { Command = CommandLineActivity.run, CacheProgress = 2, ServerName = @"localhost\sqlexpress", CatalogueDatabaseName = "RDMP_Catalogue", DataExportDatabaseName = "RDMP_DataExport" });
            }
        }
    }
}
