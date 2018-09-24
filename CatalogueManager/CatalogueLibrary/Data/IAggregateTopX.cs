using System;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See AggregateTopX
    /// </summary>
    public interface IAggregateTopX:IMapsDirectlyToDatabaseTable
    {
        /// <summary>
        /// The number of records to return from the TopX e.g. Top 10
        /// </summary>
        int TopX { get; }

        /// <summary>
        /// The dimension which the top X applies to, if null it will be the count / sum etc column (The AggregateCountColumn)
        /// </summary>
        IColumn OrderByColumn { get; }

        /// <summary>
        /// When applying a TopX to an aggregate, this is the direction (Ascending/Descending) for the ORDER BY statement.  Descending means pick top X where
        /// count / sum etc is highest.
        /// </summary>
        AggregateTopXOrderByDirection OrderByDirection { get; }

    }
}