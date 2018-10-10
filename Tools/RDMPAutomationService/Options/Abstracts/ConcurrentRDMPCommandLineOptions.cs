using CommandLine;

namespace RDMPAutomationService.Options.Abstracts
{
    public abstract class ConcurrentRDMPCommandLineOptions:RDMPCommandLineOptions
    {
        [Option('m', "MaxConcurrentExtractions", HelpText = "Maximum number of datasets to extract at once",Default = 3)]
        public int? MaxConcurrentExtractions { get; set; }
    }
}