// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.Governance;

namespace Rdmp.UI.CommandExecution.Proposals;

class ProposeExecutionWhenTargetIsGovernancePeriod:RDMPCommandExecutionProposal<GovernancePeriod>
{
    public ProposeExecutionWhenTargetIsGovernancePeriod(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override bool CanActivate(GovernancePeriod target)
    {
        return true;
    }

    public override void Activate(GovernancePeriod target)
    {
        ItemActivator.Activate<GovernancePeriodUI, GovernancePeriod>(target);
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, GovernancePeriod target, InsertOption insertOption = InsertOption.Default)
    {
            

        if (cmd is FileCollectionCombineable files && files.Files.Length == 1)
            return new ExecuteCommandAddNewGovernanceDocument(ItemActivator, target, files.Files[0]);

        if(cmd is CatalogueCombineable c)
            return new ExecuteCommandAddCatalogueToGovernancePeriod(ItemActivator,target,c.Catalogue);

        if(cmd is ManyCataloguesCombineable mcat)
            return new ExecuteCommandAddCatalogueToGovernancePeriod(ItemActivator,target,mcat.Catalogues);

        //no drag and drop support
        return null;
    }
}