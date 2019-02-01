using System;
using System.Diagnostics;
using System.Drawing;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using RDMPAutomationService.Options.Abstracts;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands.Automation
{
    public class ExecuteCommandRunDetached : AutomationCommandExecution, IAtomicCommand
    {
        public ExecuteCommandRunDetached(IActivateItems activator, Func<RDMPCommandLineOptions> commandGetter) : base(activator,commandGetter)
        {
        }

        public override void Execute()
        {
            base.Execute();

            string command = GetCommandText();

            var psi = new ProcessStartInfo(AutomationServiceExecutable);
            psi.Arguments = command.Substring(AutomationServiceExecutable.Length);
            Process.Start(psi);
        }

        public override string GetCommandHelp()
        {
            return "Runs the activity in a seperate console process.";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.Exe;
        }
    }
}