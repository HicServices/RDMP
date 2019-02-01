using System;
using System.Drawing;
using System.Windows;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using RDMPAutomationService.Options.Abstracts;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands.Automation
{
    public class ExecuteCommandCopyRunCommandToClipboard : AutomationCommandExecution, IAtomicCommand
    {
        public ExecuteCommandCopyRunCommandToClipboard(IActivateItems activator, Func<RDMPCommandLineOptions> commandGetter)
            : base(activator, commandGetter)
        {
            
        }

        public override string GetCommandHelp()
        {
            return "Generates the execute command line invocation (including arguments) and copies it to the Clipboard";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Clipboard);
        }

        public override void Execute()
        {
            base.Execute();

            Clipboard.SetText(GetCommandText());
        }
    }
}