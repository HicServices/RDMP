using CatalogueLibrary.Data;
using CatalogueManager.CredentialsUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsDataAccessCredentials : RDMPCommandExecutionProposal<DataAccessCredentials>
    {
        public ProposeExecutionWhenTargetIsDataAccessCredentials(IActivateItems itemActivator)
            : base(itemActivator)
        {
        }

        public override bool CanActivate(DataAccessCredentials target)
        {
            return true;
        }

        public override void Activate(DataAccessCredentials target)
        {
            ItemActivator.Activate<DataAccessCredentialsUI, DataAccessCredentials>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, DataAccessCredentials target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}