namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Interface for all objects which can have one or more sql parameter associated with them.  For example an IFilter (line of WHERE Sql) might have 2 parameters @startDate
    /// and @endDate then GetAllParameters should return the two ISqlParameter objects that contain the DECLARE, Comment and Value setting Sql bits for these parameters.
    /// 
    /// <para>Each ISqlParameter should only ever have a single owner.</para>
    /// </summary>
    public interface ICollectSqlParameters
    {
        /// <summary>
        /// Returns all parameters declared directly against2 the current object.  This does not normally include sub objects existing below the current
        /// object which might have their own <see cref="ISqlParameter"/>.
        /// </summary>
        /// <returns></returns>
        ISqlParameter[] GetAllParameters();
    }
}
