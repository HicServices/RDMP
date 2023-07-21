// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsProject : RDMPCommandExecutionProposal<Project>
{
    public ProposeExecutionWhenTargetIsProject(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override bool CanActivate(Project target) => true;

    public override void Activate(Project target)
    {
        ItemActivator.Activate<ProjectUI.ProjectUI, Project>(target);
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, Project project,
        InsertOption insertOption = InsertOption.Default)
    {
        return cmd switch
        {
            //drop a cic on a Project to associate it with that project
            CohortIdentificationConfigurationCommand cicCommand =>
                new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(ItemActivator)
                    .SetTarget(cicCommand.CohortIdentificationConfiguration)
                    .SetTarget(project),
            CatalogueCombineable cataCommand => new ExecuteCommandMakeCatalogueProjectSpecific(ItemActivator)
                .SetTarget(cataCommand.Catalogue)
                .SetTarget(project),
            FileCollectionCombineable file when file.Files.Length == 1 =>
                new ExecuteCommandCreateNewCatalogueByImportingFileUI(ItemActivator, file.Files[0]).SetTarget(project),
            AggregateConfigurationCombineable aggCommand =>
                new ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration(ItemActivator,
                    aggCommand.Aggregate).SetTarget(project),
            _ => null
        };
    }
}