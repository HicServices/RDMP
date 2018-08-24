using CatalogueLibrary.Data.Aggregation;

namespace CatalogueLibrary.QueryBuilding.Options
{
    /// <summary>
    /// Builds <see cref="IAggregateBuilderOptions"/> from the current state of <see cref="AggregateConfiguration"/>s.
    /// </summary>
    public class AggregateBuilderOptionsFactory
    {
        /// <summary>
        /// Creates an <see cref="IAggregateBuilderOptions"/> appropriate to the <see cref="AggregateConfiguration"/>.  These options indicate whether
        /// it is functioning as a graph or cohort set and therefore which parts of the <see cref="AggregateBuilder"/> are elligible to be modified.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public IAggregateBuilderOptions Create(AggregateConfiguration config)
        {
            var cohortIdentificationConfiguration = config.GetCohortIdentificationConfigurationIfAny();

            if (cohortIdentificationConfiguration != null)
                return new AggregateBuilderCohortOptions(cohortIdentificationConfiguration.GetAllParameters());

            return new AggregateBuilderBasicOptions();
        }
    }
}
