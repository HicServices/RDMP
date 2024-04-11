// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsPipeline : RDMPCommandExecutionProposal<Pipeline>
{
    public ProposeExecutionWhenTargetIsPipeline(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override void Activate(Pipeline target)
    {
        if (ItemActivator.SelectObject(new DialogArgs
        {
            TaskDescription =
                    $"The Pipeline '{target.Name}' is not compatible with any known Pipeline use cases.  Select which use case you want to edit it under (which activity best describes what how the Pipeline is supposed to be used?)."
        }, ItemActivator.CoreChildProvider.PipelineUseCases.Value.ToArray(), out var selected))
        {
            var cmd = new ExecuteCommandEditPipelineWithUseCase(ItemActivator, target, selected.UseCase);
            cmd.Execute();
        }
    }

    public override bool CanActivate(Pipeline target) => true;

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, Pipeline target,
        InsertOption insertOption = InsertOption.Default) => null;
}