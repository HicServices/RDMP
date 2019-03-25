using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes;
using CatalogueManager.CredentialsUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsDataAccessCredentialUsageNode :RDMPCommandExecutionProposal<DataAccessCredentialUsageNode>
    {
        public ProposeExecutionWhenTargetIsDataAccessCredentialUsageNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(DataAccessCredentialUsageNode target)
        {
            return true;
        }

        public override void Activate(DataAccessCredentialUsageNode target)
        {
            ItemActivator.Activate<DataAccessCredentialsUI,DataAccessCredentials>(target.Credentials);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, DataAccessCredentialUsageNode target,
            InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}