using System;
using System.Diagnostics;
using System.Windows.Forms;
using CommandLine;
using RDMPAutomationService.Options;

namespace CatalogueManager.SimpleControls
{
    partial class ExecuteInAutomationServerUI : UserControl
    {
        public const string AutomationServiceExecutable = "RDMPAutomationService.exe";

        public Func<StartupOptions> CommandGetter { get; set; }

        public ExecuteInAutomationServerUI()
        {
            InitializeComponent();
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
            return AutomationServiceExecutable + " " + p.FormatCommandLine(options);
        }
    }
}
