// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsProcessTaskArgument: RDMPCommandExecutionProposal<ProcessTaskArgument>
{
    private IActivateItems _activator;

    public ProposeExecutionWhenTargetIsProcessTaskArgument(IActivateItems itemActivator) : base(itemActivator)
    {
        _activator = itemActivator;
    }

    public override bool CanActivate(ProcessTaskArgument target) => true;

    public override void Activate(ProcessTaskArgument processTaskArgument)
    {
        var setArgumentCommand = new ExecuteCommandSetArgument(_activator, processTaskArgument);
        setArgumentCommand.Execute();
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, ProcessTaskArgument target,
        InsertOption insertOption = InsertOption.Default) => null;
}