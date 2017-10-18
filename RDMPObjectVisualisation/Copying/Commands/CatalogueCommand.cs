using System;
using System.Runtime.CompilerServices;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using ReusableUIComponents.Copying;

namespace RDMPObjectVisualisation.Copying.Commands
{
    public class CatalogueCommand : ICommand
    {
        public Catalogue Catalogue { get; set; }
        public CohortIdentificationConfiguration.ChooseWhichExtractionIdentifierToUseFromManyHandler ResolveMultipleExtractionIdentifiers { get; set; }

        public CatalogueCommand(Catalogue catalogue)
        {
            Catalogue = catalogue;
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

            return GenerateAggregateConfigurationFor(cic);
        }

        public AggregateConfigurationCommand GenerateAggregateConfigurationFor(CohortIdentificationConfiguration cic, bool importMandatoryFilters = true, [CallerMemberName] string caller = null)
        {
            var newAggregate = cic.CreateNewEmptyConfigurationForCatalogue(Catalogue, ResolveMultipleExtractionIdentifiers ?? CohortCommandHelper.PickOneExtractionIdentifier, importMandatoryFilters);
            return new AggregateConfigurationCommand(newAggregate);
        }
    }
}