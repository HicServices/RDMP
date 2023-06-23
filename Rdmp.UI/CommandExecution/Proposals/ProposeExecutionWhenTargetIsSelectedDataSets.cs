// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ProjectUI.Datasets;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsSelectedDataSets:RDMPCommandExecutionProposal<SelectedDataSets>
{
    public ProposeExecutionWhenTargetIsSelectedDataSets(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override bool CanActivate(SelectedDataSets target) => true;

    public override void Activate(SelectedDataSets target)
    {
        ItemActivator.Activate<ConfigureDatasetUI, SelectedDataSets>(target);
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, SelectedDataSets target,
        InsertOption insertOption = InsertOption.Default)
    {
        return cmd switch
        {
            // if use drops a reusable template aggregate (e.g. from Cohort Builder)
            AggregateConfigurationCombineable { IsTemplate: true } ac when ac.Aggregate.RootFilterContainer_ID != null
                => new ExecuteCommandImportFilterContainerTree(ItemActivator, target, ac.Aggregate),
            ContainerCombineable cc => new ExecuteCommandImportFilterContainerTree(ItemActivator, target, cc.Container),
            _ => null
        };
    }
}