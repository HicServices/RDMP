// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CatalogueLibrary.Data.Governance;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Copying.Commands;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.Governance;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
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

        public override ICommandExecution ProposeExecution(ICommand cmd, GovernancePeriod target, InsertOption insertOption = InsertOption.Default)
        {
            var files = cmd as FileCollectionCommand;

            if (files != null && files.Files.Length == 1)
                return new ExecuteCommandAddNewGovernanceDocument(ItemActivator, target, files.Files[0]);

            //no drag and drop support
            return null;
        }
    }
}
