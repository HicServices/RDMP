using System.Collections.Generic;

namespace CatalogueLibrary.Data
{
    public interface IJoin
    {
        ColumnInfo ForeignKey { get; }
        ColumnInfo PrimaryKey { get; }
        string Collation { get; }
        ExtractionJoinType ExtractionJoinType { get; }
        IEnumerable<ISupplementalJoin> GetSupplementalJoins();
        ExtractionJoinType GetInvertedJoinType();
    }
}
