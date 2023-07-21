// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.MainFormUITabs;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsExtractionProgress : RDMPCommandExecutionProposal<ExtractionProgress>
{
    public ProposeExecutionWhenTargetIsExtractionProgress(IActivateItems itemActivator) : base(itemActivator)
    {

    }
    public override bool CanActivate(ExtractionProgress target)
    {
        return true;
    }

    public override void Activate(ExtractionProgress target)
    {
        ItemActivator.Activate<ExtractionProgressUI, ExtractionProgress>(target);
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, ExtractionProgress target, InsertOption insertOption = InsertOption.Default)
    {
        // no drag and drop support
        return null;
    }
}