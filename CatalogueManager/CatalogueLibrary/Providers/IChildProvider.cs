namespace CatalogueLibrary.Providers
{
    /// <summary>
    /// Returns children for a given model object (any object in an RDMPCollectionUI).  This should be fast and your IChildProvider should pre load all the objects
    /// and then return them as needed when GetChildren is called.
    /// </summary>
    public interface IChildProvider
    {
        /// <summary>
        /// Returns all children that should hierarchically exist below the <paramref name="model"/> or null if none
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        object[] GetChildren(object model);
    }
}