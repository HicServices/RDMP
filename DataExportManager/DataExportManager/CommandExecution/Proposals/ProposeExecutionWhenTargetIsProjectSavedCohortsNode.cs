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
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Providers.Nodes.ProjectCohortNodes;
using DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsProjectSavedCohortsNode:RDMPCommandExecutionProposal<ProjectSavedCohortsNode>
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

        public override ICommandExecution ProposeExecution(ICommand cmd, ProjectSavedCohortsNode target, InsertOption insertOption = InsertOption.Default)
        {
            //drop a cic on a Saved Cohorts Node to commit it to that project
            var cicCommand = cmd as CohortIdentificationConfigurationCommand;
            if (cicCommand != null)
                return
                    new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(ItemActivator).SetTarget(cicCommand.CohortIdentificationConfiguration).SetTarget(target.Project);
            
            //drop a file on the SavedCohorts node to commit it
            var fileCommand = cmd as FileCollectionCommand;
            if(fileCommand != null && fileCommand.Files.Length == 1)
                return new ExecuteCommandCreateNewCohortFromFile(ItemActivator,fileCommand.Files[0]).SetTarget(target.Project);

            //drop a Project Specific Catalogue onto it
            var catalogueCommand = cmd as CatalogueCommand;
            if (catalogueCommand != null)
                return new ExecuteCommandCreateNewCohortFromCatalogue(ItemActivator, catalogueCommand.Catalogue).SetTarget(target.Project);

            var columnCommand = cmd as ColumnCommand;

            if (columnCommand != null && columnCommand.Column is ExtractionInformation)
                return new ExecuteCommandCreateNewCohortFromCatalogue(ItemActivator,(ExtractionInformation) columnCommand.Column);

            return null;
        }
    }
}
