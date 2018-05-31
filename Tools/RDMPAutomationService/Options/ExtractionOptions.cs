using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using RDMPAutomationService.Options.Abstracts;

namespace RDMPAutomationService.Options
{
    [Verb("extract", HelpText = "Runs the Data Extraction Engine")]
    public class ExtractionOptions:ConcurrentRDMPCommandLineOptions
    {
        [Option('g',"Globals", HelpText = "Include extraction of globals (global SupportingDocuments etc")]
        public bool ExtractGlobals { get; set; }

        [Option('e',"ExtractionConfiguration",HelpText = "The ExtractionConfiguration ID to extract",Required = true)]
        public int ExtractionConfiguration { get; set; }

        [Option('s', "Datasets", HelpText = "Restrict extraction to only those ExtractableDatasets that have the provided list of IDs (must be part of the ExtractionConfiguration")]
        public IEnumerable<int> Datasets { get; set; }

        [Option('p', "Pipeline", HelpText = "The ID of the extraction Pipeline to use")]
        public int Pipeline { get; set; }

    }
}