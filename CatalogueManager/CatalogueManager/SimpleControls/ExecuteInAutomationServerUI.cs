using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace CatalogueManager.SimpleControls
{
    public partial class ExecuteInAutomationServerUI : UserControl
    {
        public const string AutomationServiceExecutable = "RDMPAutomationService.exe";

        public Func<string> CommandGetter { get; set; }

        public ExecuteInAutomationServerUI()
        {
            InitializeComponent();
        }

        private void btnCopyCommandToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(CommandGetter());
        }

        private void btnExecuteDetatched_Click(object sender, EventArgs e)
        {
            string command = CommandGetter();

            if(!command.StartsWith(AutomationServiceExecutable))
                throw new Exception("Expected command to start with " + AutomationServiceExecutable);

            var psi = new ProcessStartInfo(AutomationServiceExecutable);
            psi.Arguments = command.Substring(AutomationServiceExecutable.Length);
            Process.Start(psi);
        }
    }
}
