namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    public interface INamedTab
    {
        /// <summary>
        /// Provide a name for when your control is presented on a tab control, this should only be called after SetCollection/SetDatabaseObject etc has previously been called
        /// </summary>
        string GetTabName();
    }
}