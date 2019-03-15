using CatalogueLibrary.Data;

namespace CatalogueLibrary.Repositories.Managers
{
    public interface IJoinManager
    {
        JoinInfo[] GetAllJoinInfosBetweenColumnInfoSets(ColumnInfo[] set1, ColumnInfo[] set2);
        JoinInfo[] GetAllJoinInfosWhereTableContains(TableInfo tableInfo,JoinInfoType type);
        JoinInfo[] GetAllJoinInfos();
        JoinInfo[] GetAllJoinInfoForColumnInfoWhereItIsAForeignKey(ColumnInfo columnInfo);
        void AddJoinInfo(ColumnInfo ForeignKey, ColumnInfo PrimaryKey, ExtractionJoinType type, string Collation);
    }
}