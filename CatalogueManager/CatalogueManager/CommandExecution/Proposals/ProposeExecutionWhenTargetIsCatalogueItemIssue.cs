using CatalogueLibrary.Data;
using CatalogueManager.Issues;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsCatalogueItemIssue : RDMPCommandExecutionProposal<CatalogueItemIssue>
    {
        public ProposeExecutionWhenTargetIsCatalogueItemIssue(IActivateItems itemActivator)
            : base(itemActivator)
        {
        }

        public override bool CanActivate(CatalogueItemIssue target)
        {
            return true;
        }

        public override void Activate(CatalogueItemIssue target)
        {
            ItemActivator.Activate<IssueUI, CatalogueItemIssue>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, CatalogueItemIssue target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}