using CatalogueLibrary.Data.Governance;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.Governance;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsGovernanceDocument : RDMPCommandExecutionProposal<GovernanceDocument>
    {
        public ProposeExecutionWhenTargetIsGovernanceDocument(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(GovernanceDocument target)
        {
            return true;
        }

        public override void Activate(GovernanceDocument target)
        {
            ItemActivator.Activate<GovernanceDocumentUI, GovernanceDocument>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, GovernanceDocument target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}