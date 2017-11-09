using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Automation;
using CatalogueManager.AggregationUIs.Advanced;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.Automation;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.Proposals;

namespace CatalogueManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsAutomationServiceSlot:RDMPCommandExecutionProposal<AutomationServiceSlot>
    {
        public ProposeExecutionWhenTargetIsAutomationServiceSlot(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(AutomationServiceSlot target)
        {
            return true;
        }

        public override void Activate(AutomationServiceSlot target)
        {
            ItemActivator.Activate<AutomationServiceSlotUI, AutomationServiceSlot>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, AutomationServiceSlot targetAggregateConfiguration, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
