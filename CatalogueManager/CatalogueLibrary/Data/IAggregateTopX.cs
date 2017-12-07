using System;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See AggregateTopX
    /// </summary>
    public interface IAggregateTopX
    {
        int TopX { get; }
        IColumn OrderByColumn { get; }
        AggregateTopXOrderByDirection OrderByDirection { get; }

    }
}