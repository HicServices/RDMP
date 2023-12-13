// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsLoadStageNode : RDMPCommandExecutionProposal<LoadStageNode>
{
    public ProposeExecutionWhenTargetIsLoadStageNode(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override bool CanActivate(LoadStageNode target) => false;

    public override void Activate(LoadStageNode target)
    {
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, LoadStageNode targetStage,
        InsertOption insertOption = InsertOption.Default)
    {
        var sourceFileTaskCommand = cmd as FileCollectionCombineable;

        if (cmd is ProcessTaskCombineable sourceProcessTaskCommand)
            return new ExecuteCommandChangeLoadStage(ItemActivator, sourceProcessTaskCommand, targetStage);

        if (sourceFileTaskCommand?.Files.Length == 1)
        {
            var f = sourceFileTaskCommand.Files.Single();
            switch (f.Extension)
            {
                case ".sql":
                    return new ExecuteCommandCreateNewFileBasedProcessTask(ItemActivator, ProcessTaskType.SQLFile,
                        targetStage.LoadMetadata, targetStage.LoadStage, f);
                case ".bak":
                    return new ExecuteCommandCreateNewFileBasedProcessTask(ItemActivator, ProcessTaskType.SQLBakFile,
                        targetStage.LoadMetadata, targetStage.LoadStage, f);
                case ".exe":
                    return new ExecuteCommandCreateNewFileBasedProcessTask(ItemActivator, ProcessTaskType.Executable,
                        targetStage.LoadMetadata, targetStage.LoadStage, f);
            }
        }

        return null;
    }
}