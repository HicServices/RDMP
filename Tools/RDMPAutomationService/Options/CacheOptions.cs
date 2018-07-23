using CommandLine;
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
    }
}
