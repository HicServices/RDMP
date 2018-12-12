using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsParametersNode:RDMPCommandExecutionProposal<ParametersNode>
    {
        public ProposeExecutionWhenTargetIsParametersNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ParametersNode target)
        {
            return true;
        }

        public override void Activate(ParametersNode target)
        {
            var cmd = new ExecuteCommandViewSqlParameters(ItemActivator,target.Collector);
            
            if(!cmd.IsImpossible)
                cmd.Execute();
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ParametersNode target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
