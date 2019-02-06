// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Copying.Commands
{
    public class CatalogueCommand : ICommand
    {
        public bool ContainsAtLeastOneExtractionIdentifier { get; private set; }
        public Catalogue Catalogue { get; set; }
        public CohortIdentificationConfiguration.ChooseWhichExtractionIdentifierToUseFromManyHandler ResolveMultipleExtractionIdentifiers { get; set; }

        public CatalogueCommand(Catalogue catalogue)
        {
            Catalogue = catalogue;
            ContainsAtLeastOneExtractionIdentifier = catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Any(e => e.IsExtractionIdentifier);
        }

        public string GetSqlString()
        {
            return null;
        }

        /// <summary>
        /// Creates a new AggregateConfiguration based on the Catalogue and returns it as an AggregateConfigurationCommand, you should only use this method during EXECUTE as you do not
        /// want to be randomly creating these as the user waves an object around over the user interface trying to decide where to drop it.
        /// </summary>
        /// <param name="cohortAggregateContainer"></param>
        /// <returns></returns>
        public AggregateConfigurationCommand GenerateAggregateConfigurationFor(CohortAggregateContainer cohortAggregateContainer,bool importMandatoryFilters=true, [CallerMemberName] string caller = null)
        {
            var cic = cohortAggregateContainer.GetCohortIdentificationConfiguration();

            if (cic == null)
                return null;

            return GenerateAggregateConfigurationFor(cic, importMandatoryFilters);
        }

        public AggregateConfigurationCommand GenerateAggregateConfigurationFor(CohortIdentificationConfiguration cic, bool importMandatoryFilters = true, [CallerMemberName] string caller = null)
        {
            var newAggregate = cic.CreateNewEmptyConfigurationForCatalogue(Catalogue, ResolveMultipleExtractionIdentifiers ?? CohortCommandHelper.PickOneExtractionIdentifier, importMandatoryFilters);
            return new AggregateConfigurationCommand(newAggregate);
        }
    }
}