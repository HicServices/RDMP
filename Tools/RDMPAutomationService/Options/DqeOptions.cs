using CatalogueLibrary.Data;
using CommandLine;
using RDMPAutomationService.Options.Abstracts;

namespace RDMPAutomationService.Options
{
    [Verb("dqe", HelpText = "Runs the Data Quality Engine")]
    class DqeOptions:RDMPCommandLineOptions
    {
        [Option('c',"Catalogue",HelpText = "ID of the Catalogue to run the DQE on",Required = true)]
        public int Catalogue{ get; set; }
    }
}