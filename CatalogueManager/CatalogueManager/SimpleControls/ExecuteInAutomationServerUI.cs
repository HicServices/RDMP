using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using CommandLine;
using RDMPAutomationService.Options;
using RDMPAutomationService.Options.Abstracts;
using ReusableUIComponents.TransparentHelpSystem;

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
        public List<HelpStage> HelpStages { get; set; }

        public ExecuteInAutomationServerUI()
        {
            InitializeComponent();
            HelpStages = BuildHelpStages();
        }

        private List<HelpStage> BuildHelpStages()
        {
            return new List<HelpStage>
            {
                new HelpStage(btnExecuteDetatched, "This button will execute the required operation in a detached windows prompt.\r\n" +
                                                   "Results will be shown in that window and will also be available from the LogViewer screen.\r\n" +
                                                   "\r\n" +
                                                   "You can keep using RDMP as normal."),
                new HelpStage(btnCopyCommandToClipboard, "This button will copy the required command with the correct parameters to the clipboard.\r\n" +
                                                         "This will NOT execute the command.\r\n" +
                                                         "\r\n" +
                                                         "You can use the copied command to schedule a run using your favourite tool.")
            };
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

            if (string.IsNullOrWhiteSpace(options.CatalogueConnectionString))
                options.CatalogueConnectionString = _activator.RepositoryLocator.CatalogueRepository.ConnectionStringBuilder.ConnectionString;

            if (string.IsNullOrWhiteSpace(options.DataExportConnectionString))
                options.DataExportConnectionString = _activator.RepositoryLocator.DataExportRepository.ConnectionStringBuilder.ConnectionString;
        }

        public void SetEnabled(bool e)
        {
            //always enabled
            btnCopyCommandToClipboard.Enabled = true;
            btnExecuteDetatched.Enabled = e;
        }
    }
}
