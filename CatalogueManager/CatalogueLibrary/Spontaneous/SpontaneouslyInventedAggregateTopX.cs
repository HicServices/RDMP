using CatalogueLibrary.Data;

namespace CatalogueLibrary.Spontaneous
{
    /// <summary>
    /// Spontaneous (memory only) version of AggregateTopX (a DatabaseEntity class).  See AggregateTopX for description.
    /// </summary>
    public class SpontaneouslyInventedAggregateTopX : SpontaneousObject, IAggregateTopX
    {
        /// <inheritdoc/>
        public int TopX { get; private set; }

        /// <inheritdoc/>
        public IColumn OrderByColumn { get; private set; }

        /// <inheritdoc/>
        public AggregateTopXOrderByDirection OrderByDirection { get; private set; }


        /// <summary>
        /// Creates a ne memory only TopX constraint for use with <see cref="CatalogueLibrary.QueryBuilding.AggregateBuilder"/>.
        /// </summary>
        /// <param name="topX"></param>
        /// <param name="orderByDirection"></param>
        /// <param name="orderByColumn"></param>
        public SpontaneouslyInventedAggregateTopX(int topX, AggregateTopXOrderByDirection orderByDirection, IColumn orderByColumn)
        {
            TopX = topX;
            OrderByDirection = orderByDirection;
            OrderByColumn = orderByColumn;
        }
    }
}