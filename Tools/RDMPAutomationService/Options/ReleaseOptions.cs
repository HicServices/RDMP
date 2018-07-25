using System.Collections.Generic;
using CommandLine;
using RDMPAutomationService.Options.Abstracts;

namespace RDMPAutomationService.Options
{
    /// <summary>
    /// Options for the Release Engine which is responsible for gathering all the artifacts produced by the Extraction Engine (anonymised project extracts, bundled lookups and documents etc)
    /// and transmitting them somewhere as a final released package.
    /// </summary>
    [Verb("release",HelpText = "Releases one or more ExtractionConfigurations (e.g. Cases & Controls) for an extraction Project that has been succesfully extracted via the Extraction Engine (see extract command)")]
    public class ReleaseOptions : ConcurrentRDMPCommandLineOptions
    {
        [Option('c',"Configurations",HelpText = "List of ExtractionConfiguration IDs to release, they must all belong to the same Project")]
        public IEnumerable<int> Configurations{ get; set; }

        [Option('p', "Pipeline", HelpText = "The ID of the release Pipeline to use")]
        public int Pipeline { get; set; }

        [Option('s',"SelectedDatasets", HelpText = "List of SelectedDatasets IDs to release, they must all belong to ExtractionConfigurations within the same Project")]
        public IEnumerable<int> SelectedDataSets { get; set; }

        [Option('g', "Globals", HelpText = "True to release extracted globals (default) or false to skip them", Default = true)]
        public bool ReleaseGlobals { get; set; }
    }
}