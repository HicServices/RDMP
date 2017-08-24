namespace CatalogueLibrary.Data
{
    public interface ICollectSqlParameters
    {
        ISqlParameter[] GetAllParameters();
    }
}