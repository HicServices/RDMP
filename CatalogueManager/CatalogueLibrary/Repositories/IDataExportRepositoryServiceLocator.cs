namespace CatalogueLibrary.Repositories
{
    /// <summary>
    /// Interface for a class which can find the DataExportRepository database connection string
    /// </summary>
    public interface IDataExportRepositoryServiceLocator
    {
        IDataExportRepository DataExportRepository { get; }
    }
}