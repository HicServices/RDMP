using CatalogueLibrary.Data.Remoting;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.Remoting;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsRemoteRDMP:RDMPCommandExecutionProposal<RemoteRDMP>
    {
        public ProposeExecutionWhenTargetIsRemoteRDMP(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(RemoteRDMP target)
        {
            return true;
        }

        public override void Activate(RemoteRDMP target)
        {
            ItemActivator.Activate<RemoteRDMPUI, RemoteRDMP>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, RemoteRDMP targetAggregateConfiguration, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
