using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsConcreteFilter:RDMPCommandExecutionProposal<ConcreteFilter>
    {
        public ProposeExecutionWhenTargetIsConcreteFilter(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ConcreteFilter target)
        {
            return true;
        }

        public override void Activate(ConcreteFilter target)
        {
            ItemActivator.Activate<ExtractionFilterUI, ConcreteFilter>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ConcreteFilter target, InsertOption insertOption = InsertOption.Default)
        {
            //currently nothing can be dropped onto a filter
            return null;
        }
    }
}
