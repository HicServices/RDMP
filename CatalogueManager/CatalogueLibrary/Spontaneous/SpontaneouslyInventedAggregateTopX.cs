using CatalogueLibrary.Data;

namespace CatalogueLibrary.Spontaneous
{
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