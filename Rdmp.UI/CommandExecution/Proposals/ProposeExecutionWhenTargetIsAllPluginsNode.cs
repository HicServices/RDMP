// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsAllPluginsNode : RDMPCommandExecutionProposal<AllPluginsNode>
{
    public ProposeExecutionWhenTargetIsAllPluginsNode(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override void Activate(AllPluginsNode target)
    {
    }

    public override bool CanActivate(AllPluginsNode target) => true;

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, AllPluginsNode target,
        InsertOption insertOption = InsertOption.Default) =>
        //drop files on to attempt to upload plugins
        cmd is FileCollectionCombineable f ? new ExecuteCommandAddPlugins(ItemActivator, f) : (ICommandExecution)null;
}