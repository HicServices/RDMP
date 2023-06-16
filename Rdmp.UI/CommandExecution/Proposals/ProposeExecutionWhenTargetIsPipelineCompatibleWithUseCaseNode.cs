// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsPipelineCompatibleWithUseCaseNode : RDMPCommandExecutionProposal<PipelineCompatibleWithUseCaseNode>
{
    public ProposeExecutionWhenTargetIsPipelineCompatibleWithUseCaseNode(IActivateItems itemActivator) : base(
        itemActivator)
    {
    }

    public override bool CanActivate(PipelineCompatibleWithUseCaseNode target) => true;

    public override void Activate(PipelineCompatibleWithUseCaseNode target)
    {
        var cmd = new ExecuteCommandEditPipelineWithUseCase(ItemActivator, target.Pipeline, target.UseCase);
        cmd.Execute();
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd,
        PipelineCompatibleWithUseCaseNode target,
        InsertOption insertOption = InsertOption.Default) =>
        null;
}