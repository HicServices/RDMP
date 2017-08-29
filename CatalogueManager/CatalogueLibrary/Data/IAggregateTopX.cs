using System;

namespace CatalogueLibrary.Data
{
    public interface IAggregateTopX
    {
        int TopX { get; }
        IColumn OrderByColumn { get; }
        AggregateTopXOrderByDirection OrderByDirection { get; }

    }
}