using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsSupportingSQLTable : RDMPCommandExecutionProposal<SupportingSQLTable>
    {
        public ProposeExecutionWhenTargetIsSupportingSQLTable(IActivateItems itemActivator)
            : base(itemActivator)
        {
        }

        public override bool CanActivate(SupportingSQLTable target)
        {
            return true;
        }

        public override void Activate(SupportingSQLTable target)
        {
            ItemActivator.Activate<SupportingSQLTableUI, SupportingSQLTable>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, SupportingSQLTable target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}