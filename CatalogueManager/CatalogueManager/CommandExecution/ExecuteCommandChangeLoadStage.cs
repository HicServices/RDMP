using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandChangeLoadStage : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly ProcessTask _sourceProcessTask;
        private readonly LoadStageNode _targetStage;

        public ExecuteCommandChangeLoadStage(IActivateItems activator, ProcessTaskCommand sourceProcessTaskCommand, LoadStageNode targetStage)
        {
            _activator = activator;
            _sourceProcessTask = sourceProcessTaskCommand.ProcessTask;
            _targetStage = targetStage;

            if(sourceProcessTaskCommand.ProcessTask.LoadMetadata_ID != targetStage.LoadMetadata.ID)
                SetImpossible("ProcessTask belongs to a different LoadMetadata");

            if (!ProcessTask.IsCompatibleStage(_sourceProcessTask.ProcessTaskType, _targetStage.LoadStage))
                SetImpossible("Task type '" + _sourceProcessTask.ProcessTaskType +"' cannot run in " + _targetStage.LoadStage);
        }

        public override void Execute()
        {
            base.Execute();

            _sourceProcessTask.LoadStage = _targetStage.LoadStage;
            _sourceProcessTask.SaveToDatabase();
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_sourceProcessTask.LoadMetadata));
        }
    }
}