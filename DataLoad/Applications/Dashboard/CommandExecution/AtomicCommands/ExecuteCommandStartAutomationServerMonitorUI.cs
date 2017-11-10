using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using Dashboard.Automation;
using ReusableUIComponents.Icons.IconProvision;

namespace Dashboard.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandStartAutomationServerMonitorUI:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private AutomationServiceSlot _slot;

        public ExecuteCommandStartAutomationServerMonitorUI(IActivateItems activator) : base(activator)
        {
        }

        public override void Execute()
        {
            base.Execute();
            
            Activator.Activate<AutomationServerMonitorUI, AutomationServiceSlot>(_slot);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.AggregateGraph);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _slot = (AutomationServiceSlot) target;
            return this;
        }
    }
}