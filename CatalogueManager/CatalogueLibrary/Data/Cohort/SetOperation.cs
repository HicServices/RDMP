using CatalogueLibrary.Data.Aggregation;

namespace CatalogueLibrary.Data.Cohort
{
    /// <summary>
    /// The Sql set operation for combining sets (lists of patient identifiers) in a <see cref="CohortAggregateContainer"/>.  This is done by compiling each 
    /// <see cref="AggregateConfiguration"/> and <see cref="CohortAggregateContainer"/> in a given container into queries identifying distinct patients.  A master
    /// query is then built in which each subquery is interspersed by the appropriate <see cref="SetOperation"/>. 
    /// </summary>
    public enum SetOperation
    {
        /// <summary>
        /// Result set is everyone in any sub set
        /// </summary>
        UNION,

        /// <summary>
        /// Result set is only people appearing in every subset
        /// </summary>
        INTERSECT,

        /// <summary>
        /// Result set is the people in the first set who do not appear in any of the subsequent sets
        /// </summary>
        EXCEPT
    }
}