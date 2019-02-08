// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsExtractionInformation:RDMPCommandExecutionProposal<ExtractionInformation>
    {
        public ProposeExecutionWhenTargetIsExtractionInformation(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ExtractionInformation target)
        {
            return true;
        }

        public override void Activate(ExtractionInformation target)
        {
            ItemActivator.Activate<ExtractionInformationUI, ExtractionInformation>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ExtractionInformation target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
