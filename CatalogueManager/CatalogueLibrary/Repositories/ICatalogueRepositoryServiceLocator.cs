namespace CatalogueLibrary.Repositories
{
    public interface ICatalogueRepositoryServiceLocator
    {
        CatalogueRepository CatalogueRepository { get; }
    }
}