using CatalogueLibrary.Data.Aggregation;

namespace CatalogueLibrary.QueryBuilding.Options
{
    public class AggregateBuilderOptionsFactory
    {
        public IAggregateBuilderOptions Create(AggregateConfiguration config)
        {
            var cohortIdentificationConfiguration = config.GetCohortIdentificationConfigurationIfAny();

            if (cohortIdentificationConfiguration != null)
                return new AggregateBuilderCohortOptions(cohortIdentificationConfiguration.GetAllParameters());

            return new AggregateBuilderBasicOptions();
        }
    }
}
