using System.Linq;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsLoadStageNode:RDMPCommandExecutionProposal<LoadStageNode>
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
            var sourceFileTaskCommand = cmd as FileCollectionCommand;
            
            if (sourceProcessTaskCommand != null)
                return new ExecuteCommandChangeLoadStage(ItemActivator, sourceProcessTaskCommand, targetStage);

            if (sourceFileTaskCommand != null && sourceFileTaskCommand.Files.Length == 1)
            {

                var f = sourceFileTaskCommand.Files.Single();

                if(f.Extension == ".sql")
                    return new ExecuteCommandCreateNewProcessTask(ItemActivator, ProcessTaskType.SQLFile,targetStage.LoadMetadata, targetStage.LoadStage,f);


                if (f.Extension == ".exe")
                    return new ExecuteCommandCreateNewProcessTask(ItemActivator, ProcessTaskType.Executable, targetStage.LoadMetadata, targetStage.LoadStage, f);
            }


            return null;
        }
    }
}
