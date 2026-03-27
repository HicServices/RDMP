// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.ProcessTasks;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsProcessTask : RDMPCommandExecutionProposal<Core.EntityFramework.Models.ProcessTask>
{
    public ProposeExecutionWhenTargetIsProcessTask(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override bool CanActivate(Core.EntityFramework.Models.ProcessTask target) => true;

    public override void Activate(Core.EntityFramework.Models.ProcessTask processTask)
    {
        if (processTask.IsPluginType())
            ItemActivator.Activate<PluginProcessTaskUI, Core.EntityFramework.Models.ProcessTask>(processTask);

        switch ((ProcessTaskType)processTask.ProcessTaskType)
        {
            case ProcessTaskType.Executable:
                ItemActivator.Activate<ExeProcessTaskUI, Core.EntityFramework.Models.ProcessTask>(processTask);
                break;
            case ProcessTaskType.SQLFile:
                ItemActivator.Activate<SqlProcessTaskUI, Core.EntityFramework.Models.ProcessTask>(processTask);
                break;
            case ProcessTaskType.SQLBakFile:
                ItemActivator.Activate<SqlBakFileProcessTaskUI, Core.EntityFramework.Models.ProcessTask>(processTask);
                break;
        }
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, Core.EntityFramework.Models.ProcessTask target,
        InsertOption insertOption = InsertOption.Default) => null;
}