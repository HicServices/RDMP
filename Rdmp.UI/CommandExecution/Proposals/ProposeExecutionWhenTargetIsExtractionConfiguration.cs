// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportManager.ProjectUI;
using CatalogueManager.Copying.Commands;
using Rdmp.Core.DataExport.Data.DataTables;
using Rdmp.Core.DataExport.Providers;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsExtractionConfiguration:RDMPCommandExecutionProposal<ExtractionConfiguration>
    {
        public ProposeExecutionWhenTargetIsExtractionConfiguration(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ExtractionConfiguration target)
        {
            return !target.IsReleased;
        }

        public override void Activate(ExtractionConfiguration target)
        {
            if (!target.IsReleased)
                ItemActivator.Activate<ExecuteExtractionUI, ExtractionConfiguration>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ExtractionConfiguration targetExtractionConfiguration, InsertOption insertOption = InsertOption.Default)
        {
            //user is trying to set the cohort of the configuration
            var sourceExtractableCohortComand = cmd as ExtractableCohortCommand;

            var sourceCatalogueCommand = cmd as CatalogueCommand;

            if (sourceCatalogueCommand != null)
            {
                var dataExportChildProvider = (DataExportChildProvider)ItemActivator.CoreChildProvider;
                var eds = dataExportChildProvider.ExtractableDataSets.SingleOrDefault(ds => ds.Catalogue_ID == sourceCatalogueCommand.Catalogue.ID);

                if (eds == null)
                    return new ImpossibleCommand("Catalogue is not Extractable");
                
                return new ExecuteCommandAddDatasetsToConfiguration(ItemActivator, eds,targetExtractionConfiguration);
                
            }

            if (sourceExtractableCohortComand != null)
                return new ExecuteCommandAddCohortToExtractionConfiguration(ItemActivator, sourceExtractableCohortComand, targetExtractionConfiguration);

            //user is trying to add datasets to a configuration
            var sourceExtractableDataSetCommand = cmd as ExtractableDataSetCommand;

            if (sourceExtractableDataSetCommand != null)
                return new ExecuteCommandAddDatasetsToConfiguration(ItemActivator, sourceExtractableDataSetCommand, targetExtractionConfiguration);

            return null;
        }
    }
}
