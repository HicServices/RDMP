namespace CatalogueLibrary.Repositories
{
    /// <summary>
    /// Interface for a class which can find the CatalogueRepository database connection string
    /// </summary>
    public interface ICatalogueRepositoryServiceLocator
    {
        CatalogueRepository CatalogueRepository { get; }
    }
}