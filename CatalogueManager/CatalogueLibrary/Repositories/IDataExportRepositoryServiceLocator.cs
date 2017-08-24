namespace CatalogueLibrary.Repositories
{
    public interface IDataExportRepositoryServiceLocator
    {
        IDataExportRepository DataExportRepository { get; }
    }
}