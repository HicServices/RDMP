using System.Collections.Generic;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes how to join two tables together.  This is used to during Query Building (See JoinHelper) to build the JOIN section of the query once all required tables
    /// have been identified (See SqlQueryBuilderHelper).  
    /// </summary>
    public interface IJoin
    {
        ColumnInfo ForeignKey { get; }
        ColumnInfo PrimaryKey { get; }
        string Collation { get; }
        ExtractionJoinType ExtractionJoinType { get; }
        
        /// <summary>
        /// If it is nessesary to join on more than one column, use this method to indicate the aditional fk / pk pairs (they must belong to the same TableInfos as the 
        /// main IJoin)
        /// </summary>
        /// <returns></returns>
        IEnumerable<ISupplementalJoin> GetSupplementalJoins();

        /// <summary>
        /// The ExtractionJoinType Property models Left/Right/Inner when the SqlQueryBuilderHelper finds the PrimaryKey TableInfo and needs to join to the ForeignKey table
        /// (the normal situation). However if the ForeignKey TableInfo is required first (either because it is IsPrimaryExtractionTable or because there are other tables
        /// in the query that force a particular join order) then the Join direction needs to be inverted.  Normally this is a matter of swapping Left=>Right and vice versa
        /// but you might instead want to throw NotSupportedException if you are expecting a specific direction (See Lookup)
        /// </summary>
        /// <returns></returns>
        ExtractionJoinType GetInvertedJoinType();
    }
}
