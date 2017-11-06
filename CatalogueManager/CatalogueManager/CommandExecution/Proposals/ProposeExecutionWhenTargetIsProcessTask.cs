using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.ProcessTasks;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsProcessTask:RDMPCommandExecutionProposal<ProcessTask>
    {
        public ProposeExecutionWhenTargetIsProcessTask(IActivateItems itemActivator) : base(itemActivator)
        {

        }

        public override bool CanActivate(ProcessTask target)
        {
            return true;
        }

        public override void Activate(ProcessTask processTask)
        {

            if (processTask.IsPluginType())
                ItemActivator.Activate<PluginProcessTaskUI, ProcessTask>(processTask);

            if (processTask.ProcessTaskType == ProcessTaskType.Executable)
                ItemActivator.Activate<ExeProcessTaskUI, ProcessTask>(processTask);

            if (processTask.ProcessTaskType == ProcessTaskType.SQLFile)
                ItemActivator.Activate<SqlProcessTaskUI, ProcessTask>(processTask);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ProcessTask target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
