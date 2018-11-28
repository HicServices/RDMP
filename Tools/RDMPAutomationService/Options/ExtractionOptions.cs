using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using RDMPAutomationService.Options.Abstracts;

namespace RDMPAutomationService.Options
{
    /// <summary>
    /// Options for the Extraction Engine which performs cohort linkage against datasets and extracts anonymous datasets
    /// </summary>
    [Verb("extract", HelpText = "Runs the Data Extraction Engine")]
    public class ExtractionOptions:ConcurrentRDMPCommandLineOptions
    {
        [Option('g',"Globals", HelpText = "Include extraction of globals (global SupportingDocuments etc")]
        public bool ExtractGlobals { get; set; }

        [Option('e',"ExtractionConfiguration",HelpText = "The ExtractionConfiguration ID to extract",Required = true)]
        public int ExtractionConfiguration { get; set; }

        [Option('p', "Pipeline", HelpText = "The ID of the extraction Pipeline to use")]
        public int Pipeline { get; set; }

        [Option('s', "Datasets", HelpText = "Restrict extraction to only those ExtractableDatasets that have the provided list of IDs (must be part of the ExtractionConfiguration)")]
        public IEnumerable<int> Datasets { get; set; }
        
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("Check dataset 123 and 124 in configuration 32 for extraction using pipeline 2",
                    new ExtractionOptions()
                    {
                        Command = CommandLineActivity.check,
                        ExtractionConfiguration =  32,
                        Pipeline =  2,
                        Datasets = new int[] { 123,124}
                    }
                    );
                
            }
        }

    }
}