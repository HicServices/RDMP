using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs.SubComponents;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsTableInfo : RDMPCommandExecutionProposal<TableInfo>
    {
        public ProposeExecutionWhenTargetIsTableInfo(IActivateItems itemActivator)
            : base(itemActivator)
        {
        }

        public override bool CanActivate(TableInfo target)
        {
            return true;
        }

        public override void Activate(TableInfo target)
        {
            ItemActivator.Activate<TableInfoUI, TableInfo>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, TableInfo target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}