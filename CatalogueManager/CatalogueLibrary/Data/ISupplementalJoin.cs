namespace CatalogueLibrary.Data
{
    public interface ISupplementalJoin
    {
        ColumnInfo ForeignKey { get; }
        ColumnInfo PrimaryKey { get; }
        string Collation { get; }
        
    }
}