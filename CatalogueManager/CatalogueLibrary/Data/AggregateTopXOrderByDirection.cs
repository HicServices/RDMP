namespace CatalogueLibrary.Data
{
    /// <summary>
    /// The direction to sort the results of an Aggregate Graph
    /// </summary>
    public enum AggregateTopXOrderByDirection
    {
        /// <summary>
        /// Alphabetically A->Z numerically 0->99999
        /// </summary>
        Ascending,

        /// <summary>
        /// Alphabetically Z->A numerically 99999->0
        /// </summary>
        Descending
    }
}