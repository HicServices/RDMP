// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.CommandExecution.Proposals;

internal class ProposeExecutionWhenTargetIsProjectSavedCohortsNode:RDMPCommandExecutionProposal<ProjectSavedCohortsNode>
{
    public ProposeExecutionWhenTargetIsProjectSavedCohortsNode(IActivateItems itemActivator) : base(itemActivator)
    {
    }

    public override bool CanActivate(ProjectSavedCohortsNode target)
    {
        return false;
    }

    public override void Activate(ProjectSavedCohortsNode target)
    {
            
    }

    public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, ProjectSavedCohortsNode target, InsertOption insertOption = InsertOption.Default)
    {
        //drop a cic on a Saved Cohorts Node to commit it to that project
        if (cmd is CohortIdentificationConfigurationCommand cicCommand)
            return
                new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(ItemActivator,null).SetTarget(cicCommand.CohortIdentificationConfiguration).SetTarget(target.Project);
            
        //drop a file on the SavedCohorts node to commit it
        if(cmd is FileCollectionCombineable fileCommand && fileCommand.Files.Length == 1)
            return new ExecuteCommandCreateNewCohortFromFile(ItemActivator,fileCommand.Files[0],null).SetTarget(target.Project);

        //drop a Project Specific Catalogue onto it
        if (cmd is CatalogueCombineable catalogueCombineable)
            return new ExecuteCommandCreateNewCohortFromCatalogue(ItemActivator, catalogueCombineable.Catalogue).SetTarget(target.Project);

        if (cmd is ColumnCombineable columnCommand && columnCommand.Column is ExtractionInformation column)
            return new ExecuteCommandCreateNewCohortFromCatalogue(ItemActivator,column);

        return null;
    }
}