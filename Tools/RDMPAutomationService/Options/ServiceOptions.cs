using System.Data.SqlClient;
using CatalogueLibrary.Repositories;
using CommandLine;
using RDMPAutomationService.Properties;
using RDMPStartup;

namespace RDMPAutomationService.Options
{
    [Verb("service", HelpText = "Runs/Installs the Automation Service as in job polling mode")]
    class ServiceOptions:StartupOptions
    {
        [Value(0, Required = true, HelpText = @"Commands: 'run' the service manually, 'install' as a windows service, 'uninstall' windows service")]
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
        NONE,
        run,
        install,
        uninstall
    }
}
