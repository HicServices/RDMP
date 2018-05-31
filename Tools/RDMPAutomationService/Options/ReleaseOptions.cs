using CommandLine;
using RDMPAutomationService.Options.Abstracts;

namespace RDMPAutomationService.Options
{
    [Verb("release",HelpText = "Releases one or more ExtractionConfigurations (e.g. Cases & Controls) for an extraction Project that has been succesfully extracted via the Extraction Engine (see extract command)")]
    public class ReleaseOptions : ConcurrentRDMPCommandLineOptions
    {

    }
}