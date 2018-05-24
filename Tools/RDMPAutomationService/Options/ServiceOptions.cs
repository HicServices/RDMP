using System.Data.SqlClient;
using CatalogueLibrary.Repositories;
using CommandLine;
using RDMPAutomationService.Properties;
using RDMPStartup;

namespace RDMPAutomationService.Options
{
    [Verb("service", HelpText = "Runs the Automation Service as an executable until you exit")]
    class ServiceOptions:StartupOptions
    {
        [Value(0,Required = true,HelpText = "Command to run e.g. install, uninstall etc")]
        public ServiceCommands Command { get; set; }

        [Option('f', "ForceSlot", Required = false, HelpText = "Force the ID of the Slot to use")]
        public int ForceSlot { get; set; }

        public ServiceOptions()
        {
            ForceSlot = Settings.Default.ForceSlot;
        }
    }

    internal enum ServiceCommands
    {
        run,
        install,
        uninstall
    }
}
