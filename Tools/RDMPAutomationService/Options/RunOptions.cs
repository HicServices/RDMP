using System.Data.SqlClient;
using CatalogueLibrary.Repositories;
using CommandLine;
using RDMPAutomationService.Properties;
using RDMPStartup;

namespace RDMPAutomationService.Options
{
    [Verb("run", HelpText = "Runs the Automation Service as an executable until you exit")]
    class RunOptions:StartupOptions
    {
        [Option('f', "ForceSlot", Required = false, HelpText = "Force the ID of the Slot to use")]
        public int ForceSlot { get; set; }

        public RunOptions()
        {
            ForceSlot = Settings.Default.ForceSlot;
        }

        
    }
}
