using System.Linq;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    internal class ExecuteCommandReOrderProcessTask : BasicUICommandExecution
    {
        private readonly ProcessTask _targetProcessTask;
        private readonly InsertOption _insertOption;
        private ProcessTask _sourceProcessTask;

        public ExecuteCommandReOrderProcessTask(IActivateItems activator, ProcessTaskCommand sourceProcessTaskCommand, ProcessTask targetProcessTask, InsertOption insertOption) : base(activator)
        {
            _targetProcessTask = targetProcessTask;
            _insertOption = insertOption;
            _sourceProcessTask = sourceProcessTaskCommand.ProcessTask;

            if (_sourceProcessTask.LoadMetadata_ID != targetProcessTask.LoadMetadata_ID)
                SetImpossible("ProcessTasks must belong to the same Load");
            else
            if (_sourceProcessTask.LoadStage != targetProcessTask.LoadStage)
                SetImpossible("ProcessTasks must belong in the same LoadStage to be ReOrdered");
            else
            if(_insertOption == InsertOption.Default)
                SetImpossible("Drag above or below to ReOrder");
        }

        public override void Execute()
        {
            base.Execute();

            var destinationOrder = 0;

            var lmd = _targetProcessTask.LoadMetadata;

            if (_insertOption == InsertOption.InsertAbove)
            {

                destinationOrder = _targetProcessTask.Order - 1;
                
                foreach (var pt in lmd.ProcessTasks)
                {
                    //don't change the current one again
                    if (pt.Equals(_sourceProcessTask))
                        continue;

                    if (pt.Order <= destinationOrder)
                    {
                        pt.Order--;
                        pt.SaveToDatabase();
                    }
                }
            }
            else
            {

                destinationOrder = _targetProcessTask.Order + 1;

                foreach (var pt in lmd.ProcessTasks)
                {
                    //don't change the current one again
                    if (pt.Equals(_sourceProcessTask))
                        continue;

                    if (pt.Order >= destinationOrder)
                    {
                        pt.Order++;
                        pt.SaveToDatabase();
                    }
                }
            }

            _sourceProcessTask.Order = destinationOrder;
            _sourceProcessTask.SaveToDatabase();
            Publish(lmd);
        }
    }
}