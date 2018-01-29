using System;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See AggregateTopX
    /// </summary>
    public interface IAggregateTopX:IMapsDirectlyToDatabaseTable
    {
        int TopX { get; }
        IColumn OrderByColumn { get; }
        AggregateTopXOrderByDirection OrderByDirection { get; }

    }
}