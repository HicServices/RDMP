using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using CommandLine;
using RDMPAutomationService.Options;
using RDMPAutomationService.Options.Abstracts;

namespace CatalogueManager.SimpleControls
{
    /// <summary>
    /// Translates a given RDMPCommandLineOptions into a command line text that can be run.  This is either copied onto the users clipboard or executed as a standalone process
    /// </summary>
    partial class ExecuteInAutomationServerUI : UserControl
    {
        private IActivateItems _activator;
        public const string AutomationServiceExecutable = "RDMPAutomationService.exe";

        public Func<RDMPCommandLineOptions> CommandGetter { get; set; }

        public ExecuteInAutomationServerUI()
        {
            InitializeComponent();
        }

        public void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
        }

        private void btnCopyCommandToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(GetCommandText());
        }

        private void btnExecuteDetatched_Click(object sender, EventArgs e)
        {
            string command = GetCommandText();

            if(!command.StartsWith(AutomationServiceExecutable))
                throw new Exception("Expected command to start with " + AutomationServiceExecutable);

            var psi = new ProcessStartInfo(AutomationServiceExecutable);
            psi.Arguments = command.Substring(AutomationServiceExecutable.Length);
            Process.Start(psi);
        }

        private string GetCommandText()
        {
            Parser p = new Parser();
            var options = CommandGetter();

            PopulateConnectionStringOptions(options);
            
            return AutomationServiceExecutable + " " + p.FormatCommandLine(options);
        }

        private void PopulateConnectionStringOptions(RDMPCommandLineOptions options)
        {
            if(_activator == null)
                return;

            if (string.IsNullOrWhiteSpace(options.ServerName))
                options.ServerName = _activator.RepositoryLocator.CatalogueRepository.DiscoveredServer.Name;

            if (string.IsNullOrWhiteSpace(options.CatalogueDatabaseName))
                options.CatalogueDatabaseName = _activator.RepositoryLocator.CatalogueRepository.DiscoveredServer.GetCurrentDatabase().GetRuntimeName();

            if (string.IsNullOrWhiteSpace(options.DataExportDatabaseName))
                options.DataExportDatabaseName = _activator.RepositoryLocator.DataExportRepository.DiscoveredServer.GetCurrentDatabase().GetRuntimeName();
        }
    }
}
