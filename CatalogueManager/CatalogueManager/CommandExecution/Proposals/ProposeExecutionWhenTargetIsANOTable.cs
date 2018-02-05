using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.ANOEngineeringUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsANOTable:RDMPCommandExecutionProposal<ANOTable>
    {
        public ProposeExecutionWhenTargetIsANOTable(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ANOTable target)
        {
            return true;
        }

        public override void Activate(ANOTable target)
        {
            ItemActivator.Activate<ANOTableUI, ANOTable>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ANOTable target, InsertOption insertOption = InsertOption.Default)
        {
            //no drag and drop support
            return null;
        }
    }
}
