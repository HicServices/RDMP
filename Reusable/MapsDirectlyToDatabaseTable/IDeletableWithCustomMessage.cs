namespace MapsDirectlyToDatabaseTable
{
    public interface IDeletableWithCustomMessage : IDeleteable
    {
        /// <summary>
        /// describes the effects of deleting the IDeletable e.g. Remove dataset X from configuration Y.  Should not be phrased as a question.
        /// </summary>
        /// <returns></returns>
        string GetDeleteMessage();
    }
}