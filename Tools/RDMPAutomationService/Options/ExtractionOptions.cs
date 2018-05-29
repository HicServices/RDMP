using CommandLine;
using CommandLine.Text;

namespace RDMPAutomationService.Options
{
    public class ExtractionOptions:RDMPCommandLineOptions
    {
        [Option('g',"Globals", HelpText = "Include extraction of globals (global SupportingDocuments etc")]
        public bool ExtractGlobals { get; set; }

        [Option('e',"ExtractionConfiguration",HelpText = "The ExtractionConfiguration ID to extract")]
        public int ExtractionConfiguration { get; set; }

        [Option('s', "Datasets", HelpText = "Restrict extraction to only those ExtractableDatasets that have the provided list of IDs (must be part of the ExtractionConfiguration")]
        public int[] Datasets { get; set; }

        [Option('p', "Pipeline", HelpText = "The ID of the extraction Pipeline to use")]
        public int Pipeline { get; set; }
    }
}