// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable : BasicUICommandExecution,IAtomicCommand
    {
        private readonly CohortIdentificationConfiguration _configuration;
        private CatalogueCommand _catalogue;

        public ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(IActivateItems activator, CohortIdentificationConfiguration configuration) : base(activator)
        {
            _configuration = configuration;
        }

        public ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(IActivateItems activator,CatalogueCommand catalogue, CohortIdentificationConfiguration configuration):this(activator,configuration)
        {
            _catalogue = catalogue;
            if(!_catalogue.ContainsAtLeastOneExtractionIdentifier)
                SetImpossible("Catalogue " + _catalogue.Catalogue + " does not contain any IsExtractionIdentifier columns");
        }

        public override string GetCommandHelp()
        {
            return "Creates a new patient index table query that fetches a subset of data from the chosen dataset.  This query will be used as part of a cohort identification configuration";
        }

        public override void Execute()
        {
            base.Execute();

            if (_catalogue == null)
            {
                Catalogue cata;
                if(!SelectOne(Activator.RepositoryLocator.CatalogueRepository.GetAllCatalogues(), out cata))
                    return;
                
                _catalogue = new CatalogueCommand(cata);
            }
            
            AggregateConfigurationCommand aggregateCommand = _catalogue.GenerateAggregateConfigurationFor(_configuration);

            var joinableCommandExecution = new ExecuteCommandConvertAggregateConfigurationToPatientIndexTable(Activator, aggregateCommand, _configuration);
            joinableCommandExecution.Execute();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Import);
        }
    }
}