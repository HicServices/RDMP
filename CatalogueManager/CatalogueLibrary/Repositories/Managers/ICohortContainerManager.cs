using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueLibrary.Repositories.Managers
{
    /// <summary>
    /// Manages information about what set containers / subcontainers exist under a <see cref="CohortIdentificationConfiguration"/>
    /// </summary>
    public interface ICohortContainerManager
    {
        /// <summary>
        /// If the AggregateConfiguration is set up as a cohort identification set in a <see cref="CohortIdentificationConfiguration"/> then this method will return the set container
        /// (e.g. UNION / INTERSECT / EXCEPT) that it is in.  Returns null if it is not in a <see cref="CohortAggregateContainer"/>.
        /// </summary>
        /// <returns></returns>
        CohortAggregateContainer GetCohortAggregateContainerIfAny(AggregateConfiguration aggregateConfiguration);
        
        /// <summary>
        /// Makes the configuration a member of the given container with the given <paramref name="order"/> relative to other things (if any) in the container.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="cohortAggregateContainer"></param>
        /// <param name="order"></param>
        void AddConfigurationToContainer(AggregateConfiguration configuration, CohortAggregateContainer cohortAggregateContainer, int order);

        /// <summary>
        /// Removes the configuration from the given container (to which it must belong already)
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="cohortAggregateContainer"></param>
        void RemoveConfigurationFromContainer(AggregateConfiguration configuration, CohortAggregateContainer cohortAggregateContainer);

        /// <summary>
        /// If the configuration is part of any aggregate container anywhere this method will return the order within that container
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        int? GetOrderIfExistsFor(AggregateConfiguration configuration);

        /// <summary>
        /// Gets all the subcontainers of the current container (if any)
        /// </summary>
        /// <returns></returns>
        CohortAggregateContainer[] GetSubContainers(CohortAggregateContainer cohortAggregateContainer);

        /// <summary>
        /// Returns all the cohort identifier set queries (See <see cref="AggregateConfiguration"/>) declared as immediate children of the container.  These exist in 
        /// order defined by <see cref="IOrderable.Order"/> and can be interspersed with subcontainers.
        /// </summary>
        /// <returns></returns>
        AggregateConfiguration[] GetAggregateConfigurations(CohortAggregateContainer cohortAggregateContainer);

        /// <summary>
        /// Gets the parent container of the current container (if it is not a root / orphan container)
        /// </summary>
        /// <returns></returns>
        CohortAggregateContainer GetParentContainerIfAny(CohortAggregateContainer cohortAggregateContainer);

        /// <summary>
        /// Removes the given container from it's host parent container
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        void RemoveSubContainerFrom(CohortAggregateContainer parent, CohortAggregateContainer child);
    }
}