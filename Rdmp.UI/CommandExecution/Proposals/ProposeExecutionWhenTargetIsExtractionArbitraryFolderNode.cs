// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsExtractionArbitraryFolderNode : RDMPCommandExecutionProposal<ExtractionArbitraryFolderNode>
{
    public ProposeExecutionWhenTargetIsExtractionArbitraryFolderNode(IActivateItems activator):base(activator)
    {

    }
    public override void Activate(ExtractionArbitraryFolderNode target)
    {
    }

    public override bool CanActivate(ExtractionArbitraryFolderNode target)
    {
        return false;
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, ExtractionArbitraryFolderNode target, InsertOption insertOption = InsertOption.Default)
    {
        if (target.Configuration == null)
        {
            return null;
        }

        // for drag and drop onto this node the options are whatever they would be for dropping
        // onto the ExtractionConfiguration itself
        var config = new ProposeExecutionWhenTargetIsExtractionConfiguration(ItemActivator);
        return config.ProposeExecution(cmd, target.Configuration, insertOption);
    }
}