using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Validation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsStandardRegex:RDMPCommandExecutionProposal<StandardRegex>
    {
        public ProposeExecutionWhenTargetIsStandardRegex(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(StandardRegex target)
        {
            return true;
        }

        public override void Activate(StandardRegex target)
        {
            ItemActivator.Activate<StandardRegexUI, StandardRegex>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, StandardRegex target, InsertOption insertOption = InsertOption.Default)
        {
            //no drag and drop support
            return null;
        }
    }
}
