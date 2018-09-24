namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Interface for additional join column pairs required by an IJoin.  This is only applicable if you need to join two tables using multiple columns at once.  E.g. A left join B
    /// on A.x = B.x and A.y=B.y.  ISupplementalJoin is assumed to follow the same direction as the principal IJoin.
    /// </summary>
    public interface ISupplementalJoin
    {

        /// <inheritdoc cref="IJoin.ForeignKey"/>
        ColumnInfo ForeignKey { get; }

        /// <inheritdoc cref="IJoin.PrimaryKey"/>
        ColumnInfo PrimaryKey { get; }

        /// <inheritdoc cref="IJoin.Collation"/>
        string Collation { get; }
        
    }
}