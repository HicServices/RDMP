using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.ANOUIs.PreLoadDiscarding;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsPreLoadDiscardedColumn : RDMPCommandExecutionProposal<PreLoadDiscardedColumn>
    {
        public ProposeExecutionWhenTargetIsPreLoadDiscardedColumn(IActivateItems itemActivator)
            : base(itemActivator)
        {
        }

        public override bool CanActivate(PreLoadDiscardedColumn target)
        {
            return true;
        }

        public override void Activate(PreLoadDiscardedColumn target)
        {
            ItemActivator.Activate<PreLoadDiscardedColumnUI, PreLoadDiscardedColumn>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, PreLoadDiscardedColumn target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}