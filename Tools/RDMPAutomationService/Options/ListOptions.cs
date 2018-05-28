using CommandLine;

namespace RDMPAutomationService.Options
{
    [Verb("list",HelpText = "Lists objects in the Catalogue / DataExport repositories")]
    class ListOptions : RDMPCommandLineOptions
    {
        [Option('t', "Type",Required = false, HelpText = "Type name you want to list e.g. Catalogue (does not have to be fully specified)")]
        public string Type { get; set; }

        [Option('p',"Pattern",Required = false, HelpText = "Regex pattern to match on ToString of class",Default =  ".*")]
        public string Pattern { get; set; }

        [Option('i',"ID",Required = false, HelpText = "The ID of the object to fetch")]
        public int? ID { get; set; }
    }
}