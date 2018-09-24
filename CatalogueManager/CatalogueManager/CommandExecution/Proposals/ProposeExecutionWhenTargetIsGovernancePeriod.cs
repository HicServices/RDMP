using CatalogueLibrary.Data.Governance;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Copying.Commands;
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
            var files = cmd as FileCollectionCommand;

            if (files != null && files.Files.Length == 1)
                return new ExecuteCommandAddNewGovernanceDocument(ItemActivator, target, files.Files[0]);

            //no drag and drop support
            return null;
        }
    }
}
