using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.ItemActivation;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsLoadStageNode:RDMPCommandExecutionProposal<LoadStageNode>
    {
        public ProposeExecutionWhenTargetIsLoadStageNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }
        
        public override bool CanActivate(LoadStageNode target)
        {
            return false;
        }

        public override void Activate(LoadStageNode target)
        {
            
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, LoadStageNode targetStage, InsertOption insertOption = InsertOption.Default)
        {
            var sourceProcessTaskCommand = cmd as ProcessTaskCommand;
            if (sourceProcessTaskCommand != null)
                return new ExecuteCommandChangeLoadStage(ItemActivator, sourceProcessTaskCommand, targetStage);

            return null;
        }
    }
}
