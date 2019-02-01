using System;
using CatalogueManager.ItemActivation;
using CommandLine;
using RDMPAutomationService.Options.Abstracts;

namespace CatalogueManager.CommandExecution.AtomicCommands.Automation
{
    public abstract class AutomationCommandExecution : BasicUICommandExecution
    {
        protected readonly Func<RDMPCommandLineOptions> CommandGetter;
        public const string AutomationServiceExecutable = "RDMPAutomationService.exe";

        protected AutomationCommandExecution(IActivateItems activator, Func<RDMPCommandLineOptions> commandGetter) : base(activator)
        {
            CommandGetter = commandGetter;
        }

        protected string GetCommandText()
        {
            Parser p = new Parser();
            var options = CommandGetter();

            PopulateConnectionStringOptions(options);

            return AutomationServiceExecutable + " " + p.FormatCommandLine(options);
        }

        private void PopulateConnectionStringOptions(RDMPCommandLineOptions options)
        {
            if (Activator == null)
                return;

            if (string.IsNullOrWhiteSpace(options.CatalogueConnectionString))
                options.CatalogueConnectionString = Activator.RepositoryLocator.CatalogueRepository.ConnectionStringBuilder.ConnectionString;

            if (string.IsNullOrWhiteSpace(options.DataExportConnectionString))
                options.DataExportConnectionString = Activator.RepositoryLocator.DataExportRepository.ConnectionStringBuilder.ConnectionString;
        }
    }
}