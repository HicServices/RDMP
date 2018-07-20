using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsConnectionStringKeyword :RDMPCommandExecutionProposal<ConnectionStringKeyword>
    {
        public ProposeExecutionWhenTargetIsConnectionStringKeyword(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ConnectionStringKeyword target)
        {
            return true;
        }

        public override void Activate(ConnectionStringKeyword target)
        {
            ItemActivator.Activate<ConnectionStringKeywordUI,ConnectionStringKeyword>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ConnectionStringKeyword target, InsertOption insertOption = InsertOption.Default)
        {
            //no drag and drop
            return null;
        }
    }
}
