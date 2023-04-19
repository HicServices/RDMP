// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

class ProposeExecutionWhenTargetIsAllCataloguesUsedByLoadMetadataNode : RDMPCommandExecutionProposal<AllCataloguesUsedByLoadMetadataNode>
{
    public ProposeExecutionWhenTargetIsAllCataloguesUsedByLoadMetadataNode(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override bool CanActivate(AllCataloguesUsedByLoadMetadataNode target)
    {
        return false;
    }

    public override void Activate(AllCataloguesUsedByLoadMetadataNode target)
    {
        throw new NotSupportedException();
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, AllCataloguesUsedByLoadMetadataNode target,InsertOption insertOption = InsertOption.Default)
    {
        var cata = cmd as CatalogueCombineable;
        var manyCata = cmd as ManyCataloguesCombineable;

        ICommandExecution cmdExecution = null;

        if (cata != null)
            cmdExecution = new ExecuteCommandAssociateCatalogueWithLoadMetadata(ItemActivator,target.LoadMetadata).SetTarget(new[]{cata.Catalogue});

        if(manyCata != null)
            cmdExecution = new ExecuteCommandAssociateCatalogueWithLoadMetadata(ItemActivator, target.LoadMetadata).SetTarget(manyCata.Catalogues);


        return cmdExecution;
    }
}