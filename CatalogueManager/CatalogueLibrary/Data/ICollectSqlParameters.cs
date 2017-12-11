namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Interface for all objects which can have one or more sql parameter associated with them.  For example an IFilter (line of WHERE Sql) might have 2 parameters @startDate
    /// and @endDate then GetAllParameters should return the two ISqlParameter objects that contain the DECLARE, Comment and Value setting Sql bits for these parameters.
    /// 
    /// Each ISqlParameter should only ever have a single owner.
    /// </summary>
    public interface ICollectSqlParameters
    {
        ISqlParameter[] GetAllParameters();
    }
}