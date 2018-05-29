using CommandLine;
using CommandLine.Text;

namespace RDMPAutomationService.Options
{
    public class ExtractionOptions:RDMPCommandLineOptions
    {
        [Option('g',"Globals", HelpText = "Include extraction of globals (global SupportingDocuments etc")]
        public bool ExtractGlobals { get; set; }

        [Option('s', "SelectedDatasets", HelpText = "List of selections which must be extracted")]
        public int[] SelectedDatasets { get; set; }
    }
}