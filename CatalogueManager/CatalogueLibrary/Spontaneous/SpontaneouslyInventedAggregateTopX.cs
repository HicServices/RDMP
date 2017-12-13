using CatalogueLibrary.Data;

namespace CatalogueLibrary.Spontaneous
{
    /// <summary>
    /// Spontaneous (memory only) version of AggregateTopX (a DatabaseEntity class).  See AggregateTopX for description.
    /// </summary>
    public class SpontaneouslyInventedAggregateTopX : IAggregateTopX
    {
        public int TopX { get; private set; }
        public IColumn OrderByColumn { get; private set; }
        public AggregateTopXOrderByDirection OrderByDirection { get; private set; }

        public SpontaneouslyInventedAggregateTopX(int topX, AggregateTopXOrderByDirection orderByDirection, IColumn orderByColumn)
        {
            TopX = topX;
            OrderByDirection = orderByDirection;
            OrderByColumn = orderByColumn;
        }
    }
}