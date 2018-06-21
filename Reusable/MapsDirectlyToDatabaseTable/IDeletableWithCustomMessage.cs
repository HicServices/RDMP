namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// interface for any <see cref="IDeleteable"/> which wants to describe the unique special snowflake effects of deleting it.  This is primarily intended
    /// for classes which are not nouns and instead describe a relationship e.g. SelectedDataSets which describes the fact that dataset X is included in 
    /// configuration Y.
    /// </summary>
    public interface IDeletableWithCustomMessage : IDeleteable
    {
        /// <summary>
        /// Describes the effects of deleting the IDeletable e.g. Remove dataset X from configuration Y.  Should not be phrased as a question.
        /// </summary>
        /// <returns></returns>
        string GetDeleteMessage();
    }
}