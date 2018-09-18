using CatalogueLibrary.Data.Governance;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.Governance;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsGovernancePeriod:RDMPCommandExecutionProposal<GovernancePeriod>
    {
        public ProposeExecutionWhenTargetIsGovernancePeriod(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(GovernancePeriod target)
        {
            return true;
        }

        public override void Activate(GovernancePeriod target)
        {
            ItemActivator.Activate<GovernancePeriodUI, GovernancePeriod>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, GovernancePeriod target, InsertOption insertOption = InsertOption.Default)
        {
            //no drag and drop support
            return null;
        }
    }
}
